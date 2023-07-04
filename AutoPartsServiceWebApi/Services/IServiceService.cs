using AutoPartsServiceWebApi.Dto;

namespace AutoPartsServiceWebApi.Services
{
    public interface IServiceService
    {
        Task<ApiResponse<List<ServiceDto>>> AddService(ServiceDeviceJwtDto request);
        Task<ApiResponse<List<ServiceDto>>> DeleteService(ServiceIdDeviceJwtDto request);
        Task<ApiResponse<List<ServiceDto>>> GetUserServices(DeviceJwtDto request);
        Task<List<ServiceWithUserDto>> GetAllServices(OptionalCategoryDto optionalCategoryDto);
        Task<ApiResponse<ServiceDto>> GetServiceById(ServiceDtoId request);
    }
}
