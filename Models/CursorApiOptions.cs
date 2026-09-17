namespace HealthcareShoppingCart.Models;

public class CursorApiOptions
{
    public const string SectionName = "CursorApi";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.cursor.com";
    public string ModelId { get; set; } = "composer-2.5";
    public bool FastMode { get; set; } = false;
    public int PollIntervalMs { get; set; } = 1500;
    public int MaxWaitSeconds { get; set; } = 90;
}
