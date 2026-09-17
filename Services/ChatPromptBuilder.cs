using HealthcareShoppingCart.Models;

namespace HealthcareShoppingCart.Services;

public static class ChatPromptBuilder
{
    public static string BuildSystemPrompt(IProductService productService)
    {
        var productSummary = string.Join("\n",
            productService.GetAllProducts().Select(p => $"- {p.Name} (${p.Price:F2}): {p.Description}"));

        return
            "You are a friendly healthcare shopping assistant for HealthCare Cart. " +
            "Help users find products, explain healthcare items, answer cart and checkout questions, " +
            "and give general wellness guidance. Keep replies concise and helpful.\n\n" +
            "Available products:\n" + productSummary;
    }

    public static string BuildCursorPrompt(string message, List<ChatMessage> history, string systemPrompt)
    {
        if (history.Count == 0)
        {
            return $"{systemPrompt}\n\nUser question: {message}";
        }

        var conversation = string.Join("\n",
            history.Select(h => $"{(h.Role == "model" ? "Assistant" : "User")}: {h.Text}"));

        return $"{systemPrompt}\n\nConversation so far:\n{conversation}\n\nUser question: {message}";
    }
}
