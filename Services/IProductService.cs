using HealthcareShoppingCart.Models;

namespace HealthcareShoppingCart.Services;

public interface IProductService
{
    List<Product> GetAllProducts();
    Product? GetProductById(int id);
}
