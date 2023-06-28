namespace AutoPartsServiceWebApi.Dto.Requests
{
    public class AuthenticateRequest
    {
        public string PhoneNumber { get; set; }
        public string SmsCode { get; set; }
        public string DeviceId { get; set; }
    }

}
