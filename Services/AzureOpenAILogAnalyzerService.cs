using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using LogAnalyzer.API.Models;

namespace LogAnalyzer.API.Services;

public class AzureOpenAILogAnalyzerService : ILogAnalyzerService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureOpenAILogAnalyzerService> _logger;

    public AzureOpenAILogAnalyzerService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AzureOpenAILogAnalyzerService> logger)
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

            // Call Azure OpenAI API
            var aiResponse = await CallAzureOpenAIAsync(prompt, cancellationToken);

            // Parse and structure the response
            var analysis = ParseAIResponse(aiResponse, quickAnalysis);

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
- Null reference patterns: {quickAnalysis.NullReferenceCount}
- Database issues: {quickAnalysis.DatabaseIssueCount}
- Authentication issues: {quickAnalysis.AuthIssueCount}
- Timeout issues: {quickAnalysis.TimeoutCount}

LOGS TO ANALYZE:
{logs}

CRITICAL: You MUST respond with ONLY a valid JSON object. Do not include any markdown formatting, code blocks, or explanatory text before or after the JSON.

Your response must be a valid JSON object with this exact structure:
{{
  ""summary"": ""Comprehensive overview: What happened? Is it server-side or client-side? Runtime or compile-time? Overall health status (Safe/Warning/Critical)"",
  ""errors"": [
    {{
      ""message"": ""Detailed error description with context"",
      ""lineNumber"": ""line number if available"",
      ""timestamp"": ""timestamp if available"",
      ""category"": ""error category (NullReference/Database/Auth/Network/Timeout/CompileTime/Runtime/ServerError/ClientError)""
    }}
  ],
  ""warnings"": [
    {{
      ""message"": ""Warning description and potential impact"",
      ""lineNumber"": ""line number if available"",
      ""timestamp"": ""timestamp if available"",
      ""category"": ""warning category""
    }}
  ],
  ""info"": [
    {{
      ""message"": ""Important contextual information"",
      ""timestamp"": ""timestamp if available""
    }}
  ],
  ""rootCause"": ""In-depth analysis:
- Primary Issue: What is the main problem?
- Error Type: Server-side or Client-side?
- When It Occurs: Runtime or Compile-time?
- Technical Details: Stack trace analysis, affected components
- Impact: What functionality is broken?
- Cascading Effects: What else might be affected?"",
  ""suggestedFix"": ""Step-by-step solution:

1. Immediate Actions:
   - What to do first
   - Quick workarounds if available

2. Root Cause Fix:
   - Specific code changes needed
   - Configuration updates required
   - Include code examples with proper syntax

3. Prevention:
   - How to prevent this in the future
   - Best practices to implement

4. Validation:
   - How to verify the fix works
   - What to test"",
  ""severity"": ""Info|Low|Medium|High|Critical""
}}

Analysis Guidelines:
1. If logs are CLEAN and SAFE (no errors/warnings): Set summary to 'Logs are clean and safe. No issues detected.' and severity to 'Info'
2. If WARNINGS exist: Provide detailed warning analysis with potential impacts
3. If ERRORS exist: 
   - Categorize as Server-side vs Client-side
   - Identify Runtime vs Compile-time errors
   - Provide specific stack trace analysis
   - Explain cascading failures
4. Be extremely specific and technical
5. Include ALL errors and warnings found
6. Provide actionable, code-specific fixes
7. Use proper categorization (NullReference, Database, Auth, Network, Timeout, CompileTime, Runtime, ServerError, ClientError)
8. If database errors: suggest connection string checks, timeout adjustments, pool settings
9. If null reference: identify the exact variable and suggest null checks or initialization
10. Always specify the error context: where it happens, when it happens, what triggers it

Remember: Output ONLY the JSON object, no other text.";

        return prompt;
    }

    private async Task<string> CallAzureOpenAIAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["AzureOpenAI:ApiKey"];
        var endpoint = _configuration["AzureOpenAI:Endpoint"];
        var deployment = _configuration["AzureOpenAI:Deployment"];
        
        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(deployment))
        {
            throw new InvalidOperationException("Azure OpenAI configuration not complete. Please check ApiKey, Endpoint, and Deployment settings.");
        }

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("api-key", apiKey);

        var requestBody = new
        {
            messages = new[]
            {
                new { role = "system", content = "You are a helpful AI assistant specialized in analyzing application logs and debugging issues." },
                new { role = "user", content = prompt }
            },
            max_completion_tokens = 4000
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Azure OpenAI endpoint format: {endpoint}/openai/deployments/{deployment}/chat/completions?api-version=2024-02-15-preview
        var requestUrl = $"{endpoint.TrimEnd('/')}/openai/deployments/{deployment}/chat/completions?api-version=2024-02-15-preview";
        
        _logger.LogInformation($"Calling Azure OpenAI at: {requestUrl.Replace(apiKey, "***")}");
        
        var response = await client.PostAsync(requestUrl, content, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError($"Azure OpenAI API error: {response.StatusCode} - {errorContent}");
            throw new HttpRequestException($"Azure OpenAI API returned {response.StatusCode}: {errorContent}");
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private LogAnalysisResponse ParseAIResponse(string aiResponse, QuickAnalysis quickAnalysis)
    {
        try
        {
            var responseDoc = JsonDocument.Parse(aiResponse);
            var textResponse = responseDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrEmpty(textResponse))
            {
                throw new InvalidOperationException("Empty response from Azure OpenAI");
            }

            _logger.LogInformation("AI Response received: {Response}", textResponse.Substring(0, Math.Min(500, textResponse.Length)));

            // Clean up the response (remove markdown code blocks if present)
            textResponse = textResponse.Trim();
            if (textResponse.StartsWith("```json"))
            {
                textResponse = textResponse.Substring(7);
            }
            if (textResponse.StartsWith("```"))
            {
                textResponse = textResponse.Substring(3);
            }
            if (textResponse.EndsWith("```"))
            {
                textResponse = textResponse.Substring(0, textResponse.Length - 3);
            }
            textResponse = textResponse.Trim();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };

            var analysis = JsonSerializer.Deserialize<LogAnalysisResponse>(textResponse, options);

            if (analysis == null)
            {
                throw new InvalidOperationException("Failed to parse Azure OpenAI response");
            }

            // Ensure AnalyzedAt is set
            analysis.AnalyzedAt = DateTime.UtcNow;

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Azure OpenAI response. Raw response: {Response}", aiResponse);
            
            // Return a basic analysis based on quick patterns
            return new LogAnalysisResponse
            {
                Summary = "Quick pattern-based analysis (AI parsing failed)",
                Errors = new List<LogEntry>(),
                Warnings = new List<LogEntry>(),
                RootCause = $"Found {quickAnalysis.ErrorCount} errors, {quickAnalysis.WarningCount} warnings",
                SuggestedFix = "Manual review recommended. AI analysis failed to parse.",
                Severity = quickAnalysis.ErrorCount > 0 ? SeverityLevel.High : SeverityLevel.Medium
            };
        }
    }

    private QuickAnalysis QuickPatternAnalysis(string logs)
    {
        var analysis = new QuickAnalysis();

        // Case-insensitive regex patterns
        var errorPatterns = new[] { @"\berror\b", @"\bexception\b", @"\bfailed\b", @"\bfailure\b" };
        var warningPatterns = new[] { @"\bwarn(ing)?\b", @"\bdeprecated\b" };
        
        foreach (var pattern in errorPatterns)
        {
            analysis.ErrorCount += Regex.Matches(logs, pattern, RegexOptions.IgnoreCase).Count;
        }

        foreach (var pattern in warningPatterns)
        {
            analysis.WarningCount += Regex.Matches(logs, pattern, RegexOptions.IgnoreCase).Count;
        }

        // Specific issue detection
        analysis.NullReferenceCount = Regex.Matches(logs, @"null\s*reference|nullpointer|nullreferenceexception", RegexOptions.IgnoreCase).Count;
        analysis.DatabaseIssueCount = Regex.Matches(logs, @"database|sql|connection\s*(timeout|failed)|deadlock", RegexOptions.IgnoreCase).Count;
        analysis.AuthIssueCount = Regex.Matches(logs, @"auth(entication|orization)|unauthorized|forbidden|401|403", RegexOptions.IgnoreCase).Count;
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
