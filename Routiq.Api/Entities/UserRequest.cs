using System.ComponentModel.DataAnnotations;

namespace Routiq.Api.Entities;

public class UserRequest
{
    public int Id { get; set; }
    
    [Required]
    public string PassportCountry { get; set; } = string.Empty;
    
    public decimal TotalBudget { get; set; }
    
    public int DurationDays { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
