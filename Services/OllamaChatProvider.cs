using System.Text;
using System.Text.Json;
using HealthcareShoppingCart.Models;
using Microsoft.Extensions.Options;

namespace HealthcareShoppingCart.Services;

public class OllamaChatProvider : IChatProvider
{
    public string Name => "ollama";

    private readonly HttpClient _httpClient;
    private readonly OllamaApiOptions _options;
    private readonly IProductService _productService;

    public OllamaChatProvider(
        HttpClient httpClient,
        IOptions<OllamaApiOptions> options,
        IProductService productService)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _productService = productService;
    }

    public async Task<ChatResponse> SendMessageAsync(string message, List<ChatMessage> history)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return new ChatResponse
            {
                Success = false,
                Error = "Please enter a message."
            };
        }

        var systemPrompt = ChatPromptBuilder.BuildSystemPrompt(_productService);
        var messages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        foreach (var item in history.Where(h => !string.IsNullOrWhiteSpace(h.Text)))
        {
            messages.Add(new
            {
                role = item.Role == "model" ? "assistant" : "user",
                content = item.Text
            });
        }

        messages.Add(new { role = "user", content = message });

        var requestBody = new
        {
            model = _options.Model,
            messages,
            stream = false
        };

        var url = $"{_options.BaseUrl.TrimEnd('/')}/api/chat";

        try
        {
            using var response = await _httpClient.PostAsync(
                url,
                new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));

            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ChatResponse
                {
                    Success = false,
                    Error = TryGetApiError(responseJson) ??
                            $"Ollama API returned {(int)response.StatusCode}. Make sure Ollama is running."
                };
            }

            using var document = JsonDocument.Parse(responseJson);
            var reply = document.RootElement
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return new ChatResponse
            {
                Success = true,
                Reply = reply ?? "Ollama returned an empty response.",
                Provider = Name
            };
        }
        catch (HttpRequestException)
        {
            return new ChatResponse
            {
                Success = false,
                Error = "Unable to reach Ollama. Make sure it is running at http://localhost:11434."
            };
        }
        catch (Exception ex)
        {
            return new ChatResponse
            {
                Success = false,
                Error = $"Ollama error: {ex.Message}"
            };
        }
    }

    private static string? TryGetApiError(string responseJson)
    {
        try
        {
            using var document = JsonDocument.Parse(responseJson);
            if (document.RootElement.TryGetProperty("error", out var error))
            {
                return error.GetString();
            }
        }
        catch
        {
            // Ignore parse errors.
        }

        return null;
    }
}
