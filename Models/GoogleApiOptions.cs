namespace HealthcareShoppingCart.Models;

public class GoogleApiOptions
{
    public const string SectionName = "GoogleApi";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com";
    public string Model { get; set; } = "gemini-3.6-flash";
}
