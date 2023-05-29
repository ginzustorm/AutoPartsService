namespace AutoPartsServiceWebApi.Models
{
    public class LoginRequest
    {
        public string PhoneNumber { get; set; }
        public string SmsCode { get; set; }
    }

}