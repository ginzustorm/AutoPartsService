namespace AutoPartsServiceWebApi.Dto
{
    public class CloseRequestDto
    {
        public string Jwt { get; set; }
        public string DeviceId { get; set; }
        public int RequestId { get; set; }
    }
}
