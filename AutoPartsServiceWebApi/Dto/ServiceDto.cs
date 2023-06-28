namespace AutoPartsServiceWebApi.Dto
{
    public class ServiceDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string Category { get; set; }
        public string Avatar { get; set; }
        public decimal AverageScore { get; set; }
    }

    public class ServiceDeviceJwtDto
    {
        public string DeviceId { get; set; }
        public string Jwt { get; set; }
        public AddServiceData Data { get; set; }
    }

    public class ServiceIdDeviceJwtDto
    {
        public string DeviceId { get; set; }
        public int ServiceId { get; set; }
        public string Jwt { get; set; }
        public int Id { get; set; }
    }

    public class ServiceIdDto
    {
        public int Id { get; set; }
    }
}
