using System;

namespace Patrakov.Models;

public class AppUser
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Address { get; set; }
    public DateTime? CreatedAt { get; set; }
    public bool? IsActive { get; set; }
}