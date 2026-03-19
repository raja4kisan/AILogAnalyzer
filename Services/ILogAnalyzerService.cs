using LogAnalyzer.API.Models;
using System.Threading;

namespace LogAnalyzer.API.Services;

public interface ILogAnalyzerService
{
    Task<LogAnalysisResponse> AnalyzeLogsAsync(string logs, string? logType = null, CancellationToken cancellationToken = default);
}
