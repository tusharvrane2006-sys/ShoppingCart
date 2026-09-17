using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using HealthcareShoppingCart.Models;
using Microsoft.Extensions.Options;

namespace HealthcareShoppingCart.Services;

public class CursorChatProvider : IChatProvider
{
    public string Name => "cursor";

    private const string CursorAgentSessionKey = "CursorChatAgentId";
    private static readonly HashSet<string> TerminalStatuses =
        ["FINISHED", "ERROR", "CANCELLED", "EXPIRED"];

    private readonly HttpClient _httpClient;
    private readonly CursorApiOptions _options;
    private readonly IProductService _productService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CursorChatProvider(
        HttpClient httpClient,
        IOptions<CursorApiOptions> options,
        IProductService productService,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _productService = productService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ChatResponse> SendMessageAsync(string message, List<ChatMessage> history)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new ChatResponse
            {
                Success = false,
                Error = "Cursor API key is not configured."
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

        try
        {
            var systemPrompt = ChatPromptBuilder.BuildSystemPrompt(_productService);
            var promptText = ChatPromptBuilder.BuildCursorPrompt(message, history, systemPrompt);
            var session = _httpContextAccessor.HttpContext?.Session;
            var existingAgentId = session?.GetString(CursorAgentSessionKey);

            string runId;
            if (string.IsNullOrWhiteSpace(existingAgentId))
            {
                var created = await CreateAgentAsync(promptText);
                if (!created.Success)
                {
                    return created.Response;
                }

                existingAgentId = created.AgentId;
                runId = created.RunId;
                session?.SetString(CursorAgentSessionKey, existingAgentId);
            }
            else
            {
                var followUp = await CreateFollowUpRunAsync(existingAgentId, message);
                if (!followUp.Success)
                {
                    if (followUp.ShouldResetAgent)
                    {
                        session?.Remove(CursorAgentSessionKey);
                    }

                    return followUp.Response;
                }

                runId = followUp.RunId;
            }

            var result = await PollRunAsync(existingAgentId, runId);
            result.Provider = Name;
            return result;
        }
        catch (Exception ex)
        {
            return new ChatResponse
            {
                Success = false,
                Error = $"Unable to reach Cursor API: {ex.Message}"
            };
        }
    }

    private async Task<(bool Success, string AgentId, string RunId, ChatResponse Response)> CreateAgentAsync(string promptText)
    {
        var requestBody = new
        {
            prompt = new { text = promptText },
            model = new
            {
                id = _options.ModelId,
                @params = _options.FastMode
                    ? new[] { new { id = "fast", value = "true" } }
                    : Array.Empty<object>()
            }
        };

        using var response = await SendAuthorizedRequestAsync(
            HttpMethod.Post,
            $"{_options.BaseUrl.TrimEnd('/')}/v1/agents",
            requestBody);

        var responseJson = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            return (false, string.Empty, string.Empty, new ChatResponse
            {
                Success = false,
                Error = TryGetApiError(responseJson, response.StatusCode)
            });
        }

        using var document = JsonDocument.Parse(responseJson);
        var agentId = document.RootElement.GetProperty("agent").GetProperty("id").GetString() ?? string.Empty;
        var runId = document.RootElement.GetProperty("run").GetProperty("id").GetString() ?? string.Empty;

        return (true, agentId, runId, new ChatResponse());
    }

    private async Task<(bool Success, string RunId, bool ShouldResetAgent, ChatResponse Response)> CreateFollowUpRunAsync(
        string agentId,
        string message)
    {
        var requestBody = new
        {
            prompt = new { text = message }
        };

        using var response = await SendAuthorizedRequestAsync(
            HttpMethod.Post,
            $"{_options.BaseUrl.TrimEnd('/')}/v1/agents/{agentId}/runs",
            requestBody);

        var responseJson = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            var shouldReset = response.StatusCode == System.Net.HttpStatusCode.NotFound;
            return (false, string.Empty, shouldReset, new ChatResponse
            {
                Success = false,
                Error = TryGetApiError(responseJson, response.StatusCode)
            });
        }

        using var document = JsonDocument.Parse(responseJson);
        var runId = document.RootElement.GetProperty("run").GetProperty("id").GetString() ?? string.Empty;
        return (true, runId, false, new ChatResponse());
    }

    private async Task<ChatResponse> PollRunAsync(string agentId, string runId)
    {
        var deadline = DateTime.UtcNow.AddSeconds(_options.MaxWaitSeconds);

        while (DateTime.UtcNow < deadline)
        {
            using var response = await SendAuthorizedRequestAsync(
                HttpMethod.Get,
                $"{_options.BaseUrl.TrimEnd('/')}/v1/agents/{agentId}/runs/{runId}",
                null);

            var responseJson = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return new ChatResponse
                {
                    Success = false,
                    Error = TryGetApiError(responseJson, response.StatusCode)
                };
            }

            using var document = JsonDocument.Parse(responseJson);
            var status = document.RootElement.GetProperty("status").GetString() ?? string.Empty;

            if (TerminalStatuses.Contains(status))
            {
                if (status == "FINISHED")
                {
                    var result = document.RootElement.TryGetProperty("result", out var resultElement)
                        ? resultElement.GetString()
                        : null;

                    return new ChatResponse
                    {
                        Success = true,
                        Reply = result ?? "Cursor agent finished without a text response."
                    };
                }

                return new ChatResponse
                {
                    Success = false,
                    Error = $"Cursor agent run ended with status {status}."
                };
            }

            await Task.Delay(_options.PollIntervalMs);
        }

        return new ChatResponse
        {
            Success = false,
            Error = "Cursor agent is still running. Please try again in a moment."
        };
    }

    private async Task<HttpResponseMessage> SendAuthorizedRequestAsync(
        HttpMethod method,
        string url,
        object? body)
    {
        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ApiKey}:")));

        if (body is not null)
        {
            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");
        }

        return await _httpClient.SendAsync(request);
    }

    private static string TryGetApiError(string responseJson, System.Net.HttpStatusCode statusCode)
    {
        try
        {
            using var document = JsonDocument.Parse(responseJson);
            if (document.RootElement.TryGetProperty("message", out var message))
            {
                return message.GetString() ?? $"Cursor API returned {(int)statusCode}.";
            }

            if (document.RootElement.TryGetProperty("error", out var error))
            {
                if (error.ValueKind == JsonValueKind.String)
                {
                    return error.GetString() ?? $"Cursor API returned {(int)statusCode}.";
                }

                if (error.TryGetProperty("message", out var nestedMessage))
                {
                    return nestedMessage.GetString() ?? $"Cursor API returned {(int)statusCode}.";
                }
            }
        }
        catch
        {
            // Ignore parse errors.
        }

        if (statusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return "Cursor API authentication failed. Use a valid Cloud Agents API key from Cursor Dashboard -> API Keys.";
        }

        return $"Cursor API returned {(int)statusCode}.";
    }
}
