using System.Text.Json.Serialization;

namespace LogAnalyzer.API.Models;

public class AzureOpenAIRequest
{
    [JsonPropertyName("messages")]
    public List<AzureMessage> Messages { get; set; } = new();

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; } = 4000;

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.5;
}

public class AzureMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

public class AzureOpenAIResponse
{
    [JsonPropertyName("choices")]
    public List<AzureChoice> Choices { get; set; } = new();
}

public class AzureChoice
{
    [JsonPropertyName("message")]
    public AzureMessage Message { get; set; } = new();
}
