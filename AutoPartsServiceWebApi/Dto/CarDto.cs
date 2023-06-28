namespace AutoPartsServiceWebApi.Dto
{
    public class CarDto
    {
        public string Mark { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public string StateNumber { get; set; }
        public string VinNumber { get; set; }
    }

    public class CarIdDeviceJwtDto
    {
        public int CarId { get; set; }
        public string DeviceId { get; set; }
        public string Jwt { get; set; }
    }

    public class CarIdDto
    {
        public int Id { get; set; }
    }

    public class CarDeviceJwtDto
    {
        public string DeviceId { get; set; }
        public string Jwt { get; set; }
        public CarIdDto Data { get; set; }
    }

}
