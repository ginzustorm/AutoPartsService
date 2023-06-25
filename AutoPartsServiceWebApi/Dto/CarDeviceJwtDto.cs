namespace AutoPartsServiceWebApi.Dto
{
    public class CarDeviceJwtDto
    {
        public string DeviceId { get; set; }
        public string Jwt { get; set; }
        public CarIdDto Data { get; set; }
    }


}
