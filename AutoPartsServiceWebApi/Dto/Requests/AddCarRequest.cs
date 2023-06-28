using AutoPartsServiceWebApi.Dto;

namespace AutoPartsServiceWebApi.Dto.Requests
{
    public class AddCarRequest
    {
        public string Jwt { get; set; }
        public string DeviceId { get; set; }
        public CarDto Data { get; set; }
    }

}
