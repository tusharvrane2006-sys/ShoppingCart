namespace HealthcareShoppingCart.Models;

public class OllamaApiOptions
{
    public const string SectionName = "OllamaApi";

    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "llama3.2:3b";
}
