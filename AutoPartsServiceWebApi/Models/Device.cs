namespace AutoPartsServiceWebApi.Models
{
    public class Device
    {
        public int Id { get; set; }
        public string DeviceId { get; set; }

        public int? UserCommonId { get; set; }
        public UserCommon UserCommon { get; set; }

        public int? UserBusinessId { get; set; }
        public UserBusiness UserBusiness { get; set; }
    }

}
