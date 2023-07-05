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
        public AddressDto Address { get; set; }
        public List<CarDto> Cars { get; set; }
        public List<ServiceDto> Services { get; set; }
        public string Avatar { get; set; }
        // public ICollection<RequestCategoryDto> RequestCategories { get; set; }
        public List<RequestDto> Requests { get; set; }
    }
}
