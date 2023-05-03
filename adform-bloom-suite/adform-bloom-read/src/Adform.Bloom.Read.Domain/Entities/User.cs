using System.ComponentModel.DataAnnotations;

namespace Adform.Bloom.Read.Domain.Entities;

public class User : BaseEntity
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required] 
    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public string Locale { get; set; } = string.Empty;
    public string? FirstName { get; set; } // Mocked empty field in DDP
    public string? LastName { get; set; } // Mocked empty field in DDP
    public string? Company { get; set; } // Mocked empty field in DDP
    public string? Title { get; set; } // Mocked empty field in DDP
    public bool? TwoFaEnabled { get; set; }
    public bool? SecurityNotifications { get; set; }
    public int? Status { get; set; }
    public int Type { get; set; }
}