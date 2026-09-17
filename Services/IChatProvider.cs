using HealthcareShoppingCart.Models;

namespace HealthcareShoppingCart.Services;

public interface IChatProvider
{
    string Name { get; }
    Task<ChatResponse> SendMessageAsync(string message, List<ChatMessage> history);
}
