namespace AutoPartsServiceWebApi.Models
{
    public class UserCommon
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string PhoneNumber { get; set; }
        public Request? Request { get; set; }
        public DateTime RegistrationDate { get; set; }
        public List<Device>? Devices { get; set; }
        public string? Password { get; set; }
        public Address? Address { get; set; }
        public List<Sms>? SmsList { get; set; }
        public List<Car>? Cars { get; set; }
        public List<DocumentUser>? Documents { get; set; }
        public string? Avatar { get; set; }
        public ICollection<Offer> Offers { get; set; }
    }
}
