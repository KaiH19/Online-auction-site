using System.ComponentModel.DataAnnotations;

namespace AuctionApp.Models.DTOs
{
    public class RegisterDto
    {
        [Required, EmailAddress]
        public required string Email { get; init; }

        [Required, MinLength(6)]
        public required string Password { get; init; }

        public string? Role { get; init; }
    }

    public class LoginDto
    {
        [Required, EmailAddress]
        public required string Email { get; init; }

        [Required]
        public required string Password { get; init; }
    }
}

