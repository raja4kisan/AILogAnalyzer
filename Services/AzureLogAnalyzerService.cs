using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using LogAnalyzer.API.Models;

namespace LogAnalyzer.API.Services;

public class AzureLogAnalyzerService : ILogAnalyzerService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureLogAnalyzerService> _logger;

    public AzureLogAnalyzerService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AzureLogAnalyzerService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LogAnalysisResponse> AnalyzeLogsAsync(string logs, string? logType = null, CancellationToken cancellationToken = default)
    {
        // Forward to Azure OpenAI service
        var service = new AzureOpenAILogAnalyzerService(_httpClientFactory, _configuration, new Logger<AzureOpenAILogAnalyzerService>(new LoggerFactory()));
        return await service.AnalyzeLogsAsync(logs, logType, cancellationToken);
    }
}
