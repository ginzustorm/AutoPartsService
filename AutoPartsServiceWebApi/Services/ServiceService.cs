﻿using AutoMapper;
using AutoPartsServiceWebApi.Data;
using AutoPartsServiceWebApi.Dto;
using AutoPartsServiceWebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsServiceWebApi.Services
{
    public class ServiceService : IServiceService
    {
        private readonly AutoDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public ServiceService(AutoDbContext context, IMapper mapper, IConfiguration configuration, IUserService userService)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _userService = userService;
        }

        public async Task<ApiResponse<List<ServiceDto>>> AddService(ServiceDeviceJwtDto request)
        {
            var userCommon = await _context.UserCommons
                .Include(uc => uc.Services)
                .FirstOrDefaultAsync(uc => uc.Devices.Any(d => d.DeviceId == request.DeviceId));

            if (userCommon == null || userCommon.Jwt != request.Jwt)
            {
                throw new Exception("Invalid DeviceId or Jwt.");
            }

            var newService = new Service
            {
                Name = request.Data.Name,
                Description = request.Data.Description,
                Price = (decimal)request.Data.Price,
                Category = request.Data.Category,
                Avatar = request.Data.Avatar,  
                UserCommonId = userCommon.Id,
                AverageScore = 0
            };

            await _context.Services.AddAsync(newService);
            await _context.SaveChangesAsync();

            var serviceDtos = _mapper.Map<List<ServiceDto>>(userCommon.Services);

            var apiResponse = new ApiResponse<List<ServiceDto>>
            {
                Success = true,
                Message = "Service added successfully.",
                Jwt = request.Jwt,
                DeviceId = request.DeviceId,
                Data = serviceDtos
            };

            return apiResponse;
        }


        public async Task<ApiResponse<List<ServiceDto>>> DeleteService(ServiceIdDeviceJwtDto request)
        {
            var userCommon = await _context.UserCommons
                .Include(uc => uc.Services)
                .FirstOrDefaultAsync(uc => uc.Devices.Any(d => d.DeviceId == request.DeviceId));

            if (userCommon == null || userCommon.Jwt != request.Jwt)
            {
                throw new Exception("Invalid DeviceId or Jwt.");
            }

            var service = userCommon.Services.FirstOrDefault(s => s.Id == request.ServiceId);

            if (service == null)
            {
                throw new Exception("Service not found.");
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            var serviceDtos = _mapper.Map<List<ServiceDto>>(userCommon.Services);

            var apiResponse = new ApiResponse<List<ServiceDto>>
            {
                Success = true,
                Message = "Service deleted successfully.",
                Jwt = request.Jwt,
                DeviceId = request.DeviceId,
                Data = serviceDtos
            };

            return apiResponse;
        }

        public async Task<ApiResponse<List<ServiceDto>>> GetUserServices(DeviceJwtDto request)
        {
            var userCommon = await _userService.GetUserByDeviceJwt(request);
            if (userCommon == null)
            {
                return new ApiResponse<List<ServiceDto>>
                {
                    Success = false,
                    Message = "Invalid DeviceId or Jwt.",
                };
            }

            var services = _context.Services.Where(s => s.UserCommonId == userCommon.Id);
            var serviceDtos = _mapper.Map<List<ServiceDto>>(services);

            return new ApiResponse<List<ServiceDto>>
            {
                Success = true,
                Message = "Services fetched successfully.",
                Jwt = request.Jwt,
                Data = serviceDtos
            };
        }

        public async Task<List<ServiceWithUserDto>> GetAllServices(OptionalCategoryDto optionalCategoryDto)
        {
            IQueryable<Service> query = _context.Services.Include(s => s.UserCommon);

            if (!string.IsNullOrEmpty(optionalCategoryDto.Category))
            {
                query = query.Where(s => s.Category == optionalCategoryDto.Category);
            }

            var services = await query.ToListAsync();
            var serviceDtos = _mapper.Map<List<ServiceWithUserDto>>(services);

            return serviceDtos;
        }

        public async Task<ApiResponse<ServiceDto>> GetServiceById(ServiceDtoId request)
        {
            var deviceJwtDto = new DeviceJwtDto
            {
                Jwt = request.Jwt,
                DeviceId = request.DeviceId
            };

            var userCommon = await _userService.GetUserByDeviceJwt(deviceJwtDto);

            if (userCommon == null)
            {
                return new ApiResponse<ServiceDto>
                {
                    Success = false,
                    Message = "Invalid DeviceId or Jwt.",
                };
            }

            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == request.ServiceId);
            if (service == null)
            {
                return new ApiResponse<ServiceDto>
                {
                    Success = false,
                    Message = "Service not found.",
                };
            }

            var serviceDto = _mapper.Map<ServiceDto>(service);

            return new ApiResponse<ServiceDto>
            {
                Success = true,
                Message = "Service fetched successfully.",
                Jwt = request.Jwt,
                Data = serviceDto
            };
        }


    }

}
