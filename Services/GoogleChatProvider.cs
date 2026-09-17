using System.Text;
using System.Text.Json;
using HealthcareShoppingCart.Models;
using Microsoft.Extensions.Options;

namespace HealthcareShoppingCart.Services;

public class GoogleChatProvider : IChatProvider
{
    public string Name => "google";

    private readonly HttpClient _httpClient;
    private readonly GoogleApiOptions _options;
    private readonly IProductService _productService;

    public GoogleChatProvider(
        HttpClient httpClient,
        IOptions<GoogleApiOptions> options,
        IProductService productService)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _productService = productService;
    }

    public async Task<ChatResponse> SendMessageAsync(string message, List<ChatMessage> history)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new ChatResponse
            {
                Success = false,
                Error = "Google API key is not configured."
            };
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            return new ChatResponse
            {
                Success = false,
                Error = "Please enter a message."
            };
        }

        var systemPrompt = ChatPromptBuilder.BuildSystemPrompt(_productService);

        var contents = history
            .Where(h => !string.IsNullOrWhiteSpace(h.Text))
            .Select(h => new
            {
                role = h.Role == "model" ? "model" : "user",
                parts = new[] { new { text = h.Text } }
            })
            .ToList();

        contents.Add(new
        {
            role = "user",
            parts = new[] { new { text = message } }
        });

        var requestBody = new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            contents
        };

        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var url = $"{baseUrl}/v1beta/models/{_options.Model}:generateContent";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-goog-api-key", _options.ApiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        try
        {
            using var response = await _httpClient.SendAsync(request);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ChatResponse
                {
                    Success = false,
                    Error = TryGetApiError(responseJson) ?? $"Google API returned {(int)response.StatusCode}."
                };
            }

            using var document = JsonDocument.Parse(responseJson);
            var reply = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return new ChatResponse
            {
                Success = true,
                Reply = reply ?? "I could not generate a response. Please try again.",
                Provider = Name
            };
        }
        catch (Exception ex)
        {
            return new ChatResponse
            {
                Success = false,
                Error = $"Unable to reach Google API: {ex.Message}"
            };
        }
    }

    private static string? TryGetApiError(string responseJson)
    {
        try
        {
            using var document = JsonDocument.Parse(responseJson);
            if (document.RootElement.TryGetProperty("error", out var error) &&
                error.TryGetProperty("message", out var message))
            {
                return message.GetString();
            }
        }
        catch
        {
            // Ignore parse errors.
        }

        return null;
    }
}
