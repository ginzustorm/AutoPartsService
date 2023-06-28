namespace AutoPartsServiceWebApi.Dto
{
    public class LoginSmsDto
    {
        public string PhoneNumber { get; set; }
        public string DeviceId { get; set; }
        public DateTime CreationDate { get; set; }
        public bool NewUser { get; set; }
        public string SmsCode { get; set; }
    }
}
