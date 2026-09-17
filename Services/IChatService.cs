using HealthcareShoppingCart.Models;

namespace HealthcareShoppingCart.Services;

public interface IChatService
{
    IReadOnlyList<ChatProviderOption> GetAvailableProviders();
    Task<ChatResponse> SendMessageAsync(string provider, string message, List<ChatMessage> history);
}
