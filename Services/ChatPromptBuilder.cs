using HealthcareShoppingCart.Models;

namespace HealthcareShoppingCart.Services;

public static class ChatPromptBuilder
{
    public static string BuildSystemPrompt(IProductService productService)
    {
        var productSummary = string.Join("\n",
            productService.GetAllProducts().Select(p => $"- {p.Name} (${p.Price:F2}): {p.Description}"));

        return
            "You are a Polite E-commerce Assistant for HealthCare Cart. " +
            "Tone: empathetic, professional, courteous, and helpful. Use simple, accessible English.\n\n" +
            "STRICT RULE: You must ONLY discuss e-commerce topics for this store. " +
            "Allowed topics: product catalogs and details, order placement and tracking, " +
            "cancellations and returns, payment methods and billing, shipping and delivery, " +
            "discounts and coupons, shopping cart management, and user accounts.\n\n" +
            "NEVER answer questions about: entertainment, Bollywood, movies, celebrities, sports, " +
            "politics, news, weather, jokes, coding, homework, medical diagnosis, general wellness advice, " +
            "or any topic not directly related to shopping on this website.\n\n" +
            "For ANY off-topic question, do not provide the answer. Reply ONLY with:\n" +
            "\"I would love to help you with that, but I am currently only trained to assist with shopping, " +
            "orders, and e-commerce questions. Please let me know if you have a question about our products or your order!\"\n\n" +
            "Greet the user warmly at the start of a conversation. " +
            "Use courtesy words such as 'please', 'thank you', and 'I would be happy to help'. " +
            "Keep responses scannable, short, and clear.\n\n" +
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
