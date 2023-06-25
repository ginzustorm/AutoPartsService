namespace AutoPartsServiceWebApi.Dto
{
    public class ServiceDeviceJwtDto
    {
        public string DeviceId { get; set; }
        public string Jwt { get; set; }
        public AddServiceData Data { get; set; }
    }

}
