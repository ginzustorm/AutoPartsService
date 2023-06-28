using AutoPartsServiceWebApi.Data;
using AutoPartsServiceWebApi.Dto;
using AutoPartsServiceWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using AutoPartsServiceWebApi.Dto.Requests;
using AutoPartsServiceWebApi.Services;

namespace AutoPartsServiceWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AutoDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ICarService _carService;
        private readonly IReviewService _reviewService;
        private readonly IServiceService _serviceService;

        public UserController(AutoDbContext context, IConfiguration configuration, ICarService carService, IReviewService reviewService, IServiceService serviceService)
        {
            _context = context;
            _configuration = configuration;
            _carService = carService;
            _reviewService = reviewService;
            _serviceService = serviceService;
        }

        [HttpPost("addCar")]
        public async Task<ActionResult<ApiResponse<List<ResponseCarDto>>>> AddCar([FromBody] AddCarRequest request)
        {
            try
            {
                var apiResponse = await _carService.AddCar(request);
                return Ok(apiResponse);
            }
            catch (Exception e)
            {
                return Ok(new ApiResponse<List<ResponseCarDto>>
                {
                    Success = false,
                    Message = e.Message,
                    Jwt = request.Jwt,
                    DeviceId = request.DeviceId,
                    Data = null
                });
            }
        }

        [HttpPost("deleteCar")]
        public async Task<ActionResult<ApiResponse<List<ResponseCarDto>>>> DeleteCar([FromBody] CarIdDeviceJwtDto request)
        {
            try
            {
                var apiResponse = await _carService.DeleteCar(request);
                return Ok(apiResponse);
            }
            catch (Exception e)
            {
                return Ok(new ApiResponse<List<ResponseCarDto>>
                {
                    Success = false,
                    Message = e.Message,
                    Jwt = request.Jwt,
                    DeviceId = request.DeviceId,
                    Data = null
                });
            }
        }



        [HttpPost("addService")]
        public async Task<ActionResult<ApiResponse<List<ServiceDto>>>> AddService([FromBody] ServiceDeviceJwtDto request)
        {
            try
            {
                var apiResponse = await _serviceService.AddService(request);
                return Ok(apiResponse);
            }
            catch (Exception e)
            {
                return Ok(new ApiResponse<List<ServiceDto>>
                {
                    Success = false,
                    Message = e.Message,
                    Jwt = request.Jwt,
                    DeviceId = request.DeviceId,
                    Data = null
                });
            }
        }

        [HttpPost("deleteService")]
        public async Task<ActionResult<ApiResponse<List<ServiceDto>>>> DeleteService([FromBody] ServiceIdDeviceJwtDto request)
        {
            try
            {
                var apiResponse = await _serviceService.DeleteService(request);
                return Ok(apiResponse);
            }
            catch (Exception e)
            {
                return Ok(new ApiResponse<List<ServiceDto>>
                {
                    Success = false,
                    Message = e.Message,
                    Jwt = request.Jwt,
                    DeviceId = request.DeviceId,
                    Data = null
                });
            }
        }

        [HttpPost("myServices")]
        public async Task<ActionResult<ApiResponse<List<ServiceDto>>>> MyServices([FromBody] DeviceJwtDto request)
        {
            var apiResponse = await _serviceService.GetUserServices(request);
            if (!apiResponse.Success)
            {
                return Ok(apiResponse);
            }

            return Ok(apiResponse);
        }


        [HttpPost("allServices")]
        public async Task<ActionResult<ApiResponse<List<ServiceWithUserDto>>>> AllServices([FromBody] OptionalCategoryDto request)
        {
            var services = await _serviceService.GetAllServices(request);

            return Ok(new ApiResponse<List<ServiceWithUserDto>>
            {
                Success = true,
                Message = "Services fetched successfully.",
                Data = services
            });
        }


        [HttpPost("addReview")]
        public async Task<ActionResult<ApiResponse<List<ReviewDto>>>> AddReview([FromBody] AddReviewRequest request)
        {
            var apiResponse = await _reviewService.AddReview(request);
            if (!apiResponse.Success)
            {
                return Ok(apiResponse);
            }
            return Ok(apiResponse);
        }
    }
}
