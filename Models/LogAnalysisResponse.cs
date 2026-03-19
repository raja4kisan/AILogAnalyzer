namespace LogAnalyzer.API.Models;

public class LogAnalysisResponse
{
    public List<LogEntry> Errors { get; set; } = new();
    public List<LogEntry> Warnings { get; set; } = new();
    public List<LogEntry> Info { get; set; } = new();
    public string RootCause { get; set; } = string.Empty;
    public string SuggestedFix { get; set; } = string.Empty;
    public SeverityLevel Severity { get; set; }
    public string Summary { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

public class LogEntry
{
    public string Message { get; set; } = string.Empty;
    public string? LineNumber { get; set; }
    public string? Timestamp { get; set; }
    public string? Category { get; set; }
}

public enum SeverityLevel
{
    Info,
    Low,
    Medium,
    High,
    Critical
}
