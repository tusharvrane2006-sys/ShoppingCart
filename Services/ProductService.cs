using HealthcareShoppingCart.Models;

namespace HealthcareShoppingCart.Services;

public class ProductService : IProductService
{
    private static readonly List<Product> Products =
    [
        new()
        {
            Id = 1,
            Name = "Digital Blood Pressure Monitor",
            Description = "Accurate upper-arm blood pressure monitor with large LCD display and irregular heartbeat detection.",
            Price = 49.99m,
            ImageUrl = "/images/diego-mejia-um_A5BvI-D8-unsplash.jpg",
            Category = "Monitoring"
        },
        new()
        {
            Id = 2,
            Name = "Vitamin D3 Supplements",
            Description = "High-potency Vitamin D3 softgels to support bone health and immune function. 120 count bottle.",
            Price = 18.50m,
            ImageUrl = "/images/ela-de-pure-LlC_feChWqc-unsplash.jpg",
            Category = "Supplements"
        },
        new()
        {
            Id = 3,
            Name = "Premium First Aid Kit",
            Description = "Comprehensive 100-piece first aid kit for home, office, or travel emergencies.",
            Price = 34.99m,
            ImageUrl = "/images/elsa-olofsson-6Iq2T0DN7ds-unsplash.jpg",
            Category = "First Aid"
        },
        new()
        {
            Id = 4,
            Name = "Pregnancy Test Kit",
            Description = "Early detection pregnancy test with over 99% accuracy. Pack of 3 tests included.",
            Price = 12.99m,
            ImageUrl = "/images/reproductive-health-supplies-coalition-WNiVc2rIO88-unsplash.jpg",
            Category = "Diagnostics"
        },
        new()
        {
            Id = 5,
            Name = "Digital Thermometer",
            Description = "Fast-read digital thermometer with fever alert. Suitable for oral, underarm, and rectal use.",
            Price = 15.99m,
            ImageUrl = "/images/saad-ali-ML8DsUzq9Qw-unsplash.jpg",
            Category = "Monitoring"
        },
        new()
        {
            Id = 6,
            Name = "Hand Sanitizer Pack",
            Description = "Hospital-grade hand sanitizer gel, 70% alcohol. Pack of 4 travel-size bottles.",
            Price = 9.99m,
            ImageUrl = "/images/the-good-hygenie-co-tghc-ilqIt9KyCVE-unsplash.jpg",
            Category = "Hygiene"
        },
        new()
        {
            Id = 7,
            Name = "Herbal Wellness Tea Box",
            Description = "Organic herbal tea assortment for relaxation and wellness. 30 tea bags, caffeine-free.",
            Price = 22.50m,
            ImageUrl = "/images/wander-fleur-qnnYhq71fLU-unsplash.jpg",
            Category = "Wellness"
        },
        new()
        {
            Id = 8,
            Name = "Pulse Oximeter",
            Description = "Fingertip pulse oximeter measuring SpO2 and heart rate. OLED display with carrying case.",
            Price = 29.99m,
            ImageUrl = "/images/zoshua-colah-M9IA1xd6yB8-unsplash.jpg",
            Category = "Monitoring"
        }
    ];

    public List<Product> GetAllProducts() => Products;

    public Product? GetProductById(int id) => Products.FirstOrDefault(p => p.Id == id);
}
