namespace AutoPartsServiceWebApi.Models
{
    public class RegisterRequest
    {
        public string Token { get; set; }
        public string PhoneNumber { get; set; }
        public string UserType { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string DeviceId { get; set; }  
    }
}
