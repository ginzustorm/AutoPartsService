using AutoPartsServiceWebApi.Models;

namespace AutoPartsServiceWebApi.Dto
{
    public class UserCommonDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Password { get; set; }
        public Address Address { get; set; }
        public List<Car> Cars { get; set; }
        public string Avatar { get; set; }
    }
}
