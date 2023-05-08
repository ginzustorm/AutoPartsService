using System.ComponentModel.DataAnnotations;

namespace AutoPartsServiceWebApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string PhoneNumber { get; set; }
        public string DeviceId { get; set; }
        public Role? Role { get; set; }
        public DateTime RegistrationDate { get; set; }
        public virtual ICollection<Sms> SmsRecords { get; set; }
        public int? AddressId { get; set; }
        public Address? Address { get; set; }
    }

    public enum Role
    {
        Buyer,
        Seller
    }
}
