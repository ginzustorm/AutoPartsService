namespace AutoPartsServiceWebApi.Dto
{
    public class EditUserCommonDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public AddressDto Address { get; set; }
    }

}
