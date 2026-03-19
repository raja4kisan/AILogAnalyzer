namespace LogAnalyzer.API.Models;

public class LogAnalysisRequest
{
    public string Logs { get; set; } = string.Empty;
    public string? LogType { get; set; }
}
