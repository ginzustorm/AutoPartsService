using AutoPartsServiceWebApi.Dto.Requests;
using AutoPartsServiceWebApi.Dto;

namespace AutoPartsServiceWebApi.Services
{
    public interface ICarService
    {
        Task<ApiResponse<List<ResponseCarDto>>> AddCar(AddCarRequest request);
        Task<ApiResponse<List<ResponseCarDto>>> DeleteCar(CarIdDeviceJwtDto request);
        Task<ApiResponse<List<ResponseCarDto>>> GetUserCars(DeviceJwtDto request);
    }

}
