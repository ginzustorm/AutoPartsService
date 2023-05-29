namespace AutoPartsServiceWebApi.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public ServiceType ServiceType { get; set; } 
        public int UserBusinessId { get; set; }
        public UserBusiness UserBusiness { get; set; }
    }

    public enum ServiceType
    {
        SellingParts,
        ElectronicFixing,
        RunningGearFixing
    }
}
