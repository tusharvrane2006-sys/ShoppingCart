using HealthcareShoppingCart.Models;

namespace HealthcareShoppingCart.Services;

public class ChatService : IChatService
{
    private readonly IEnumerable<IChatProvider> _providers;
    private readonly ChatOptions _options;

    public ChatService(IEnumerable<IChatProvider> providers, Microsoft.Extensions.Options.IOptions<ChatOptions> options)
    {
        _providers = providers;
        _options = options.Value;
    }

    public IReadOnlyList<ChatProviderOption> GetAvailableProviders() =>
        _providers.Select(p => new ChatProviderOption
        {
            Id = p.Name,
            Label = GetProviderLabel(p.Name)
        }).ToList();

    private static string GetProviderLabel(string provider) => provider switch
    {
        "cursor" => "Cursor Agent",
        "google" => "Google Gemini",
        "ollama" => "Ollama (Local)",
        _ => provider
    };

    public async Task<ChatResponse> SendMessageAsync(string provider, string message, List<ChatMessage> history)
    {
        var selectedProvider = _providers.FirstOrDefault(p =>
            string.Equals(p.Name, provider, StringComparison.OrdinalIgnoreCase));

        selectedProvider ??= _providers.FirstOrDefault(p =>
            string.Equals(p.Name, _options.DefaultProvider, StringComparison.OrdinalIgnoreCase));

        if (selectedProvider is null)
        {
            return new ChatResponse
            {
                Success = false,
                Error = "No chat provider is configured."
            };
        }

        var response = await selectedProvider.SendMessageAsync(message, history);
        response.Provider = selectedProvider.Name;
        return response;
    }
}
