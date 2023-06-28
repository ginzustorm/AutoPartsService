using AutoPartsServiceWebApi.Dto.Requests;
using AutoPartsServiceWebApi.Dto;
using AutoPartsServiceWebApi.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoPartsServiceWebApi.Data;

namespace AutoPartsServiceWebApi.Services
{
    public class CarService : ICarService
    {
        private readonly AutoDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public CarService(AutoDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<ApiResponse<List<ResponseCarDto>>> AddCar(AddCarRequest request)
        {
            var userCommon = await _context.UserCommons
                .Include(u => u.Devices)
                .FirstOrDefaultAsync(u => u.Devices.Any(d => d.DeviceId == request.DeviceId));

            if (userCommon == null || userCommon.Jwt != request.Jwt)
            {
                throw new Exception("Invalid DeviceId or Jwt.");
            }

            var newCar = new Car
            {
                Mark = request.Data.Mark,
                Model = request.Data.Model,
                Color = request.Data.Color,
                StateNumber = request.Data.StateNumber,
                VinNumber = request.Data.VinNumber,
                UserCommonId = userCommon.Id
            };

            _context.Cars.Add(newCar);
            await _context.SaveChangesAsync();

            // Get updated list of cars
            var user = await _context.UserCommons
                .Include(uc => uc.Cars)
                .FirstOrDefaultAsync(uc => uc.Id == userCommon.Id);

            var carDtos = _mapper.Map<List<ResponseCarDto>>(user.Cars);

            var apiResponse = new ApiResponse<List<ResponseCarDto>>
            {
                Success = true,
                Message = "Car added successfully.",
                Jwt = request.Jwt,
                DeviceId = request.DeviceId,
                Data = carDtos
            };

            return apiResponse;
        }

        public async Task<ApiResponse<List<ResponseCarDto>>> DeleteCar(CarIdDeviceJwtDto request)
        {
            var userCommon = await _context.UserCommons
                .Include(uc => uc.Cars)
                .FirstOrDefaultAsync(u => u.Devices.Any(d => d.DeviceId == request.DeviceId));

            if (userCommon == null || userCommon.Jwt != request.Jwt)
            {
                throw new Exception("Invalid DeviceId or Jwt.");
            }

            var car = userCommon.Cars.FirstOrDefault(c => c.Id == request.CarId);
            if (car == null)
            {
                throw new Exception("Car not found.");
            }

            userCommon.Cars.Remove(car);
            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();

            var carDtos = _mapper.Map<List<ResponseCarDto>>(userCommon.Cars);

            var apiResponse = new ApiResponse<List<ResponseCarDto>>
            {
                Success = true,
                Message = "Car deleted successfully.",
                Jwt = request.Jwt,
                DeviceId = request.DeviceId,
                Data = carDtos
            };

            return apiResponse;
        }
    }
}
