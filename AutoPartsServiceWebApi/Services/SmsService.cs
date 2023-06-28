using AutoPartsServiceWebApi.Tools;

namespace AutoPartsServiceWebApi.Services
{

    public class SmsService : ISmsService
    {
        public string GenerateSmsCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public async Task SendSmsAsync(string phoneNumber, string smsCode, string sender = "SMS", DateTime? datetime = null, int sms_lifetime = 0, int type = 2)
        {
            SMSC sMSC = new SMSC();
            sMSC.send_sms(phoneNumber, "Код: " + smsCode);
        }
    }
}
