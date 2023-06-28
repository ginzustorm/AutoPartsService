using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AutoPartsServiceWebApi
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Jwt { get; set; }
        public string DeviceId { get; set; }
        public T Data { get; set; }
    }
}
