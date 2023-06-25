namespace AutoPartsServiceWebApi.Models
{
    public class RegisterRequest
    {
        public string? PhoneNumber { get; set; }
        public string? UserType { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public AddressDto? Address { get; set; }
        public string? Avatar { get; set; } 
    }
}
