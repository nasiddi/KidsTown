using System.ComponentModel.DataAnnotations;

namespace Database.EfCore;

public class User
{
    public int Id { get; set; }

    [StringLength(maximumLength: 150)] public string Username { get; set; } = null!;

    [StringLength(maximumLength: 150)] public string PasswordHash { get; set; } = null!;
}