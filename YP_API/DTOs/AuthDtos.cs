namespace YP_API.DTOs
{
    public class RegisterDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<int> AllergyIds { get; set; } = new List<int>();
    }

    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public List<string> Allergies { get; set; } = new List<string>();
        public string Token { get; set; }
    }
}
