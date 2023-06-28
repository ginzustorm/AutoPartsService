using AutoPartsServiceWebApi.Models;

namespace AutoPartsServiceWebApi.Dto
{
    public class CreateRequestDto
    {
        public string Description { get; set; }
        public string Header { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public string? Jwt { get; set; }
        public string DeviceId { get; set; }
    }

}
