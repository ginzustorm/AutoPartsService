namespace AutoPartsServiceWebApi.Services
{
    public interface ISmsService
    {
        string GenerateSmsCode();
        Task SendSmsAsync(string phoneNumber, string smsCode, string sender = "SMS", DateTime? datetime = null, int sms_lifetime = 0, int type = 2);
    }

}
