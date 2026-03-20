using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using LogAnalyzer.API.Models;

namespace LogAnalyzer.API.Services;

public class XAILogAnalyzerService : ILogAnalyzerService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<XAILogAnalyzerService> _logger;

    public XAILogAnalyzerService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<XAILogAnalyzerService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LogAnalysisResponse> AnalyzeLogsAsync(string logs, string? logType = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting log analysis with xAI (Grok)");

            // Pre-analyze for common patterns
            var quickAnalysis = QuickPatternAnalysis(logs);

            // Build comprehensive prompt
            var prompt = BuildAnalysisPrompt(logs, logType, quickAnalysis);

            // Call xAI API
            var aiResponse = await CallXAIAsync(prompt, cancellationToken);

            // Parse and structure the response
            var analysis = ParseAIResponse(aiResponse, quickAnalysis);

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing logs with xAI");
            return new LogAnalysisResponse
            {
                Summary = "Error occurred during analysis",
                RootCause = ex.Message,
                Severity = SeverityLevel.Critical,
                AnalyzedAt = DateTime.UtcNow
            };
        }
    }

    private string BuildAnalysisPrompt(string logs, string? logType, QuickAnalysis quickAnalysis)
    {
        var logTypeHint = string.IsNullOrEmpty(logType) ? "" : $"Log Type: {logType}\n";
        
        return $@"You are a senior software engineer and debugging expert. Analyze the following application logs and provide a detailed, structured analysis.

{logTypeHint}
DETECTED PATTERNS:
- Errors found: {quickAnalysis.ErrorCount}
- Warnings found: {quickAnalysis.WarningCount}
- Null reference patterns: {quickAnalysis.NullReferenceCount}
- Database issues: {quickAnalysis.DatabaseIssueCount}
- Authentication issues: {quickAnalysis.AuthIssueCount}
- Timeout issues: {quickAnalysis.TimeoutCount}

LOGS TO ANALYZE:
{logs}

CRITICAL: You MUST respond with ONLY a valid JSON object. Do not include any markdown formatting, code blocks, or explanatory text.

Your response must be a valid JSON object with this exact structure:
{{
  ""summary"": ""Comprehensive overview: What happened? Is it server-side or client-side? Overall health status."",
  ""errors"": [
    {{
      ""message"": ""Detailed error description"",
      ""lineNumber"": ""line number if visible"",
      ""timestamp"": ""timestamp if visible"",
      ""category"": ""error category""
    }}
  ],
  ""warnings"": [
    {{
      ""message"": ""Warning description"",
      ""lineNumber"": ""line number"",
      ""category"": ""warning category""
    }}
  ],
  ""info"": [
    {{
      ""message"": ""Contextual info"",
      ""timestamp"": ""timestamp""
    }}
  ],
  ""rootCause"": ""In-depth technical analysis of the primary issue."",
  ""suggestedFix"": ""Step-by-step solution with code examples."",
  ""severity"": ""Info|Low|Medium|High|Critical""
}}

Output ONLY the JSON object.";
    }

    private async Task<string> CallXAIAsync(string prompt, CancellationToken cancellationToken)
    {
        var apiKey = _configuration["xAI:ApiKey"];
        var baseUrl = _configuration["xAI:BaseUrl"] ?? "https://api.x.ai/v1";
        var model = _configuration["xAI:Model"] ?? "grok-4-1-fast";
        
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("xAI API Key is not configured.");
        }

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var requestBody = new
        {
            model = model,
            messages = new[]
            {
                new { role = "system", content = "You are a professional log analysis assistant. Provide analysis in pure JSON format." },
                new { role = "user", content = prompt }
            },
            stream = false,
            temperature = 0
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{baseUrl.TrimEnd('/')}/chat/completions", content, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("xAI API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
            throw new HttpRequestException($"xAI API returned {response.StatusCode}");
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private LogAnalysisResponse ParseAIResponse(string aiResponse, QuickAnalysis quickAnalysis)
    {
        var responseDoc = JsonDocument.Parse(aiResponse);
        var textResponse = responseDoc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrEmpty(textResponse))
            throw new InvalidOperationException("Empty response from xAI");

        // Clean markdown blocks if any
        textResponse = textResponse.Trim();
        if (textResponse.StartsWith("```json")) textResponse = textResponse.Substring(7);
        else if (textResponse.StartsWith("```")) textResponse = textResponse.Substring(3);
        if (textResponse.EndsWith("```")) textResponse = textResponse.Substring(0, textResponse.Length - 3);
        textResponse = textResponse.Trim();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var analysis = JsonSerializer.Deserialize<LogAnalysisResponse>(textResponse, options);

        if (analysis == null) throw new InvalidOperationException("Failed to deserialize xAI response");

        analysis.AnalyzedAt = DateTime.UtcNow;
        return analysis;
    }

    private QuickAnalysis QuickPatternAnalysis(string logs)
    {
        var analysis = new QuickAnalysis();
        analysis.ErrorCount = Regex.Matches(logs, @"\berror\b|\bexception\b|\bfailed\b", RegexOptions.IgnoreCase).Count;
        analysis.WarningCount = Regex.Matches(logs, @"\bwarn(ing)?\b", RegexOptions.IgnoreCase).Count;
        analysis.NullReferenceCount = Regex.Matches(logs, @"null\s*reference|nullpointer", RegexOptions.IgnoreCase).Count;
        analysis.DatabaseIssueCount = Regex.Matches(logs, @"database|sql|connection", RegexOptions.IgnoreCase).Count;
        analysis.AuthIssueCount = Regex.Matches(logs, @"auth|unauthorized|forbidden", RegexOptions.IgnoreCase).Count;
        analysis.TimeoutCount = Regex.Matches(logs, @"timeout|timed\s*out", RegexOptions.IgnoreCase).Count;
        return analysis;
    }

    private class QuickAnalysis
    {
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int NullReferenceCount { get; set; }
        public int DatabaseIssueCount { get; set; }
        public int AuthIssueCount { get; set; }
        public int TimeoutCount { get; set; }
    }
}
