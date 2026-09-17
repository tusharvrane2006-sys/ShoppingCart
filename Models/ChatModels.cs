namespace HealthcareShoppingCart.Models;

public class ChatMessage
{
    public string Role { get; set; } = "user";
    public string Text { get; set; } = string.Empty;
}

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public string Provider { get; set; } = "google";
    public List<ChatMessage> History { get; set; } = [];
}

public class ChatResponse
{
    public bool Success { get; set; }
    public string Reply { get; set; } = string.Empty;
    public string? Error { get; set; }
    public string Provider { get; set; } = string.Empty;
}
