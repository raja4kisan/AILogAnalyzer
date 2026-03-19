namespace LogAnalyzer.API.Models;

/// <summary>
/// Enhanced error response with detailed information
/// </summary>
public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public IEnumerable<string> Details { get; set; } = Array.Empty<string>();
    public DateTime Timestamp { get; set; }
    public string? RequestId { get; set; }
    public int? RetryAfter { get; set; }
}

/// <summary>
/// Extended log analysis response with metadata
/// </summary>
public class LogAnalysisResponseExtended
{
    public LogAnalysisResponse Analysis { get; set; } = new();
    public bool FromCache { get; set; }
    public TimeSpan AnalysisDuration { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public AnalysisStatistics? Statistics { get; set; }
}

/// <summary>
/// Statistics about the analyzed logs
/// </summary>
public class AnalysisStatistics
{
    public int TotalErrors { get; set; }
    public int TotalWarnings { get; set; }
    public int TotalInfo { get; set; }
    public int LogSizeBytes { get; set; }
    public int LogLineCount { get; set; }
}

/// <summary>
/// Validation result for log input
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public int LogSize { get; set; }
    public TimeSpan EstimatedAnalysisTime { get; set; }
    public string DetectedLogType { get; set; } = string.Empty;
}

/// <summary>
/// History item for past analyses
/// </summary>
public class AnalysisHistoryItem
{
    public SeverityLevel Severity { get; set; }
    public string Summary { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
}

/// <summary>
/// System-wide statistics
/// </summary>
public class SystemStatistics
{
    public int TotalAnalysesInCache { get; set; }
    public double CacheHitRate { get; set; }
    public TimeSpan AverageAnalysisTime { get; set; }
    public int SupportedLogTypes { get; set; }
    public string ApiVersion { get; set; } = string.Empty;
    public string ServiceStatus { get; set; } = string.Empty;
}

/// <summary>
/// Health check response with diagnostics
/// </summary>
public class HealthCheckResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Version { get; set; } = string.Empty;
    public Dictionary<string, string> Checks { get; set; } = new();
}

/// <summary>
/// Detailed log type information
/// </summary>
public class LogTypeInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string[] Examples { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Cached analysis entry
/// </summary>
public class CachedAnalysis
{
    public LogAnalysisResponse Analysis { get; set; } = new();
    public DateTime CachedAt { get; set; }
}
