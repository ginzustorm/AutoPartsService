namespace AutoPartsServiceWebApi.Models
{
    public class Sms
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public DateTime ExpirationDate { get; set; }

        public int UserCommonId { get; set; }
        public UserCommon UserCommon { get; set; }

        public int UserBusinessId { get; set; }
        public UserBusiness UserBusiness { get; set; }
    }

}
