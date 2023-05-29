using AutoPartsServiceWebApi.Models;

namespace AutoPartsServiceWebApi.Dto
{
    public class UserBusinessDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Password { get; set; }
        public List<ServiceDto> Services { get; set; }
    }
}
