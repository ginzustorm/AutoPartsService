namespace AutoPartsServiceWebApi.Models
{
    public class LoginSms
    {
        public int Id { get; set; }
        public string DeviceId { get; set; }
        public DateTime CreationDate { get; set; }
        public string SmsCode { get; set; }
        public bool NewUser { get; set; }
        public string PhoneNumber { get; set; }
    }
}
