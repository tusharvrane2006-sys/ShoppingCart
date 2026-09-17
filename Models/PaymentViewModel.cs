using System.ComponentModel.DataAnnotations;

namespace HealthcareShoppingCart.Models;

public class PaymentViewModel
{
    public List<CartItem> CartItems { get; set; } = new();
    public decimal TotalAmount { get; set; }

    [Required(ErrorMessage = "Cardholder name is required")]
    [Display(Name = "Cardholder Name")]
    public string CardholderName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Card number is required")]
    [Display(Name = "Card Number")]
    [RegularExpression(@"^\d{16}$", ErrorMessage = "Enter a valid 16-digit card number")]
    public string CardNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Expiry date is required")]
    [Display(Name = "Expiry Date")]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Use MM/YY format")]
    public string ExpiryDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "CVV is required")]
    [RegularExpression(@"^\d{3,4}$", ErrorMessage = "Enter a valid CVV")]
    public string CVV { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    public string Email { get; set; } = string.Empty;
}
