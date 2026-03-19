using Microsoft.AspNetCore.Mvc;
using LogAnalyzer.API.Models;
using LogAnalyzer.API.Services;

namespace LogAnalyzer.API.Controllers;

/// <summary>
/// AI-powered log analysis controller with comprehensive features
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LogAnalyzerController : ControllerBase
{
    private readonly ILogAnalyzerService _logAnalyzerService;
    private readonly ILogger<LogAnalyzerController> _logger;
    private readonly IConfiguration _configuration;

    // In-memory cache for recent analyses (consider using Redis for production)
    private static readonly Dictionary<string, CachedAnalysis> _analysisCache = new();
    private static readonly SemaphoreSlim _cacheLock = new(1, 1);
    private static int _totalRequests = 0;
    private static int _cacheHits = 0;

    public LogAnalyzerController(
        ILogAnalyzerService logAnalyzerService,
        ILogger<LogAnalyzerController> logger,
        IConfiguration configuration)
    {
        _logAnalyzerService = logAnalyzerService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Analyzes application logs using AI and provides detailed diagnostics
    /// </summary>
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(LogAnalysisResponseExtended), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LogAnalysisResponseExtended>> AnalyzeLogs(
        [FromBody] LogAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        Interlocked.Increment(ref _totalRequests);
        
        // Enhanced validation
        var validationResult = ValidateRequest(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Invalid request: {Errors}", string.Join(", ", validationResult.Errors));
            return BadRequest(new ErrorResponse 
            { 
                Error = "Validation failed",
                Details = validationResult.Errors,
                Timestamp = DateTime.UtcNow
            });
        }

        try
        {
            // Check cache first
            var cacheKey = GenerateCacheKey(request.Logs, request.LogType);
            var cachedResult = await GetCachedAnalysis(cacheKey);
            
            if (cachedResult != null)
            {
                Interlocked.Increment(ref _cacheHits);
                _logger.LogInformation("Returning cached analysis");
                
                return Ok(new LogAnalysisResponseExtended
                {
                    Analysis = cachedResult,
                    FromCache = true,
                    AnalysisDuration = TimeSpan.Zero,
                    RequestId = Guid.NewGuid().ToString(),
                    Statistics = CalculateStatistics(cachedResult, request.Logs)
                });
            }

            // Perform analysis
            var result = await _logAnalyzerService.AnalyzeLogsAsync(request.Logs, request.LogType, cancellationToken);
            await CacheAnalysis(cacheKey, result);

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("Analysis completed in {Duration}ms", duration.TotalMilliseconds);

            return Ok(new LogAnalysisResponseExtended
            {
                Analysis = result,
                FromCache = false,
                AnalysisDuration = duration,
                RequestId = Guid.NewGuid().ToString(),
                Statistics = CalculateStatistics(result, request.Logs)
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Request cancelled");
            return StatusCode(499, new ErrorResponse { Error = "Request cancelled", Timestamp = DateTime.UtcNow });
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("429"))
        {
            _logger.LogWarning("Rate limit exceeded");
            return StatusCode(429, new ErrorResponse { Error = "Rate limit exceeded", RetryAfter = 60, Timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AnalyzeLogs");
            return StatusCode(500, new ErrorResponse { Error = "Analysis failed", Details = new[] { ex.Message }, Timestamp = DateTime.UtcNow });
        }
    }

    [HttpPost("validate")]
    public IActionResult ValidateLogs([FromBody] LogAnalysisRequest request)
    {
        var validation = ValidateRequest(request);
        return Ok(new ValidationResult
        {
            IsValid = validation.IsValid,
            Errors = validation.Errors,
            LogSize = request.Logs?.Length ?? 0,
            EstimatedAnalysisTime = EstimateAnalysisTime(request.Logs?.Length ?? 0),
            DetectedLogType = DetectLogType(request.Logs ?? "")
        });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        await _cacheLock.WaitAsync();
        try
        {
            var history = _analysisCache
                .OrderByDescending(x => x.Value.CachedAt)
                .Take(10)
                .Select(x => new AnalysisHistoryItem
                {
                    Severity = x.Value.Analysis.Severity,
                    Summary = TruncateString(x.Value.Analysis.Summary, 100),
                    AnalyzedAt = x.Value.Analysis.AnalyzedAt,
                    ErrorCount = x.Value.Analysis.Errors?.Count ?? 0,
                    WarningCount = x.Value.Analysis.Warnings?.Count ?? 0
                })
                .ToList();
            return Ok(history);
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    [HttpGet("stats")]
    public IActionResult GetStatistics()
    {
        return Ok(new SystemStatistics
        {
            TotalAnalysesInCache = _analysisCache.Count,
            CacheHitRate = CalculateCacheHitRate(),
            AverageAnalysisTime = TimeSpan.FromSeconds(25),
            SupportedLogTypes = 15,
            ApiVersion = "2.0",
            ServiceStatus = "Healthy"
        });
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        var hasConfig = !string.IsNullOrEmpty(_configuration["AzureOpenAI:ApiKey"]);
        var health = new HealthCheckResponse
        {
            Status = hasConfig ? "healthy" : "degraded",
            Timestamp = DateTime.UtcNow,
            Version = "2.0.0",
            Checks = new Dictionary<string, string>
            {
                ["AzureOpenAI"] = hasConfig ? "configured" : "missing",
                ["Cache"] = $"{_analysisCache.Count} items",
                ["Memory"] = $"{GC.GetTotalMemory(false) / 1024 / 1024} MB",
                ["Requests"] = _totalRequests.ToString()
            }
        };
        return hasConfig ? Ok(health) : StatusCode(503, health);
    }

    [HttpGet("supported-log-types")]
    public IActionResult GetSupportedLogTypes()
    {
        var logTypes = new[]
        {
            new LogTypeInfo { Id = "android", Name = "Android Build", Description = "Gradle build errors", Category = "Mobile", Icon = "??" },
            new LogTypeInfo { Id = "kotlin", Name = "Kotlin", Description = "Kotlin compilation errors", Category = "Mobile", Icon = "??" },
            new LogTypeInfo { Id = "java", Name = "Java", Description = "Java compilation errors", Category = "Mobile", Icon = "?" },
            new LogTypeInfo { Id = "ndk", Name = "Android Native", Description = "NDK/JNI errors", Category = "Mobile", Icon = "??" },
            new LogTypeInfo { Id = "reactnative", Name = "React Native", Description = "React Native errors", Category = "Mobile", Icon = "??" },
            new LogTypeInfo { Id = "dotnet", Name = ".NET Application", Description = "ASP.NET Core logs", Category = "Backend", Icon = "??" },
            new LogTypeInfo { Id = "nodejs", Name = "Node.js", Description = "Node.js logs", Category = "Backend", Icon = "??" },
            new LogTypeInfo { Id = "python", Name = "Python", Description = "Python logs", Category = "Backend", Icon = "??" },
            new LogTypeInfo { Id = "docker", Name = "Docker", Description = "Container logs", Category = "Infrastructure", Icon = "??" },
            new LogTypeInfo { Id = "kubernetes", Name = "Kubernetes", Description = "K8s logs", Category = "Infrastructure", Icon = "??" },
            new LogTypeInfo { Id = "nginx", Name = "Nginx", Description = "Nginx logs", Category = "Infrastructure", Icon = "??" },
            new LogTypeInfo { Id = "apache", Name = "Apache", Description = "Apache logs", Category = "Infrastructure", Icon = "??" },
            new LogTypeInfo { Id = "database", Name = "Database", Description = "Database logs", Category = "Database", Icon = "??" },
            new LogTypeInfo { Id = "general", Name = "General", Description = "Any logs", Category = "Other", Icon = "??" }
        };
        return Ok(logTypes);
    }

    [HttpDelete("cache")]
    public async Task<IActionResult> ClearCache()
    {
        await _cacheLock.WaitAsync();
        try
        {
            var count = _analysisCache.Count;
            _analysisCache.Clear();
            _logger.LogInformation("Cleared {Count} items from cache", count);
            return Ok(new { message = $"Cleared {count} analyses", timestamp = DateTime.UtcNow });
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    // Helper methods
    private (bool IsValid, List<string> Errors) ValidateRequest(LogAnalysisRequest request)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request?.Logs))
            errors.Add("Logs cannot be empty");
        else if (request.Logs.Length < 10)
            errors.Add("Logs too short (min 10 characters)");
        else if (request.Logs.Length > 50000)
            errors.Add($"Logs too large (max 50,000, got {request.Logs.Length:N0})");
        return (errors.Count == 0, errors);
    }

    private string GenerateCacheKey(string logs, string? logType)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes($"{logs}|{logType ?? "auto"}"));
        return Convert.ToBase64String(hashBytes);
    }

    private async Task<LogAnalysisResponse?> GetCachedAnalysis(string key)
    {
        await _cacheLock.WaitAsync();
        try
        {
            if (_analysisCache.TryGetValue(key, out var cached) && DateTime.UtcNow - cached.CachedAt < TimeSpan.FromMinutes(5))
                return cached.Analysis;
            return null;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private async Task CacheAnalysis(string key, LogAnalysisResponse analysis)
    {
        await _cacheLock.WaitAsync();
        try
        {
            if (_analysisCache.Count >= 100)
            {
                var oldest = _analysisCache.OrderBy(x => x.Value.CachedAt).First().Key;
                _analysisCache.Remove(oldest);
            }
            _analysisCache[key] = new CachedAnalysis { Analysis = analysis, CachedAt = DateTime.UtcNow };
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private AnalysisStatistics CalculateStatistics(LogAnalysisResponse analysis, string logs)
    {
        return new AnalysisStatistics
        {
            TotalErrors = analysis.Errors?.Count ?? 0,
            TotalWarnings = analysis.Warnings?.Count ?? 0,
            TotalInfo = analysis.Info?.Count ?? 0,
            LogSizeBytes = System.Text.Encoding.UTF8.GetByteCount(logs),
            LogLineCount = logs.Split('\n').Length
        };
    }

    private string DetectLogType(string logs)
    {
        if (logs.Contains("gradle", StringComparison.OrdinalIgnoreCase)) return "android";
        if (logs.Contains(".kt:", StringComparison.OrdinalIgnoreCase)) return "kotlin";
        if (logs.Contains(".java:", StringComparison.OrdinalIgnoreCase)) return "java";
        if (logs.Contains("NullReferenceException", StringComparison.OrdinalIgnoreCase)) return "dotnet";
        if (logs.Contains("TypeError", StringComparison.OrdinalIgnoreCase)) return "nodejs";
        if (logs.Contains("Traceback", StringComparison.OrdinalIgnoreCase)) return "python";
        if (logs.Contains("docker", StringComparison.OrdinalIgnoreCase)) return "docker";
        return "general";
    }

    private TimeSpan EstimateAnalysisTime(int logSize)
    {
        var baseTime = 15 + (logSize / 10000) * 5;
        return TimeSpan.FromSeconds(Math.Min(baseTime, 45));
    }

    private double CalculateCacheHitRate()
    {
        return _totalRequests == 0 ? 0.0 : (double)_cacheHits / _totalRequests;
    }

    private string TruncateString(string? str, int maxLength)
    {
        if (string.IsNullOrEmpty(str)) return string.Empty;
        return str.Length <= maxLength ? str : str.Substring(0, maxLength) + "...";
    }
}
