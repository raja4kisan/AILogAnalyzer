using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using LogAnalyzer.API.Models;

namespace LogAnalyzer.API.Services;

public class GeminiLogAnalyzerService : ILogAnalyzerService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiLogAnalyzerService> _logger;
    private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

    public GeminiLogAnalyzerService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<GeminiLogAnalyzerService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LogAnalysisResponse> AnalyzeLogsAsync(string logs, string? logType = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Pre-analyze for common patterns
            var quickAnalysis = QuickPatternAnalysis(logs);

            // Build comprehensive prompt
            var prompt = BuildAnalysisPrompt(logs, logType, quickAnalysis);

            // Call Gemini API with cancellation support
            var geminiResponse = await CallGeminiApiAsync(prompt, cancellationToken);

            // Parse and structure the response
            var analysis = ParseGeminiResponse(geminiResponse, quickAnalysis);

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing logs");
            return new LogAnalysisResponse
            {
                Summary = "Error occurred during analysis",
                RootCause = ex.Message,
                Severity = SeverityLevel.Critical
            };
        }
    }

    private string BuildAnalysisPrompt(string logs, string? logType, QuickAnalysis quickAnalysis)
    {
        var logTypeHint = string.IsNullOrEmpty(logType) ? "" : $"Log Type: {logType}\n";
        
        var prompt = $@"You are a senior software engineer and debugging expert. Analyze the following application logs and provide a detailed, structured analysis.

{logTypeHint}
DETECTED PATTERNS:
- Errors found: {quickAnalysis.ErrorCount}
- Warnings found: {quickAnalysis.WarningCount}

LOGS TO ANALYZE:
{logs}

Respond with ONLY a valid JSON object in this exact structure:
{{
  ""summary"": ""Comprehensive overview"",
  ""errors"": [],
  ""warnings"": [],
  ""info"": [],
  ""rootCause"": ""In-depth analysis"",
  ""suggestedFix"": ""Step-by-step solution"",
  ""severity"": ""Info|Low|Medium|High|Critical""
}}";

        return prompt;
    }

    private async Task<string> CallGeminiApiAsync(string prompt, CancellationToken cancellationToken)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("Gemini API key not configured");

        var client = _httpClientFactory.CreateClient();
        
        var requestBody = new
        {
            contents = new[] {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var requestUrl = $"{GeminiApiUrl}?key={apiKey}";
        
        var response = await client.PostAsync(requestUrl, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Gemini API error: {Status} - {Error}", response.StatusCode, errorContent);
            throw new HttpRequestException($"Gemini API returned {response.StatusCode}");
        }

        return await response.Content.ReadAsStringAsync();
    }

    private LogAnalysisResponse ParseGeminiResponse(string geminiResponse, QuickAnalysis quickAnalysis)
    {
        try
        {
            var geminiResult = JsonSerializer.Deserialize<JsonDocument>(geminiResponse);
            var textResponse = geminiResult?.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";

            textResponse = textResponse.Trim().Trim('`').Replace("```json", "").Replace("```", "").Trim();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };

            var analysis = JsonSerializer.Deserialize<LogAnalysisResponse>(textResponse, options);
            if (analysis != null)
            {
                analysis.AnalyzedAt = DateTime.UtcNow;
                return analysis;
            }

            throw new InvalidOperationException("Failed to parse response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Gemini response");
            return new LogAnalysisResponse
            {
                Summary = "Quick pattern-based analysis (AI parsing failed)",
                Errors = new List<LogEntry>(),
                Warnings = new List<LogEntry>(),
                RootCause = $"Found {quickAnalysis.ErrorCount} errors, {quickAnalysis.WarningCount} warnings",
                SuggestedFix = "Manual review recommended",
                Severity = quickAnalysis.ErrorCount > 0 ? SeverityLevel.High : SeverityLevel.Medium
            };
        }
    }

    private QuickAnalysis QuickPatternAnalysis(string logs)
    {
        var analysis = new QuickAnalysis();
        var errorPatterns = new[] { @"\berror\b", @"\bexception\b", @"\bfailed\b" };
        var warningPatterns = new[] { @"\bwarn(ing)?\b", @"\bdeprecated\b" };
        
        foreach (var pattern in errorPatterns)
            analysis.ErrorCount += Regex.Matches(logs, pattern, RegexOptions.IgnoreCase).Count;

        foreach (var pattern in warningPatterns)
            analysis.WarningCount += Regex.Matches(logs, pattern, RegexOptions.IgnoreCase).Count;

        return analysis;
    }

    private class QuickAnalysis
    {
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
    }
}
