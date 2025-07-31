public class RegisterDto
{
    public required string Email { get; set; }
    public string Password { get; set; }
}

// Dtos/LoginDto.cs
public class LoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}
