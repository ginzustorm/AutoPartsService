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

namespace AutoPartsServiceWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AutoDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(AutoDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("addCar")]
        public async Task<ActionResult<ApiResponse<List<ResponseCarDto>>>> AddCar([FromBody] AddCarRequest request)
        {
            var validationResponse = await ValidateUser(request.DeviceId, request.Jwt);
            if (!validationResponse.Success)
            {
                return BadRequest(validationResponse);
            }

            var newCar = new Car
            {
                Mark = request.Data.Mark,
                Model = request.Data.Model,
                Color = request.Data.Color,
                StateNumber = request.Data.StateNumber,
                VinNumber = request.Data.VinNumber,
                UserCommonId = validationResponse.Data.Id
            };

            _context.Cars.Add(newCar);
            await _context.SaveChangesAsync();

            // Get updated list of cars
            var user = await _context.UserCommons
                .Include(uc => uc.Cars)
                .FirstOrDefaultAsync(uc => uc.Id == validationResponse.Data.Id);

            var carDtos = user.Cars.Select(car => new ResponseCarDto
            {
                Id = car.Id,
                Mark = car.Mark,
                Model = car.Model,
                Color = car.Color,
                StateNumber = car.StateNumber,
                VinNumber = car.VinNumber
            }).ToList();

            return Ok(new ApiResponse<List<ResponseCarDto>>
            {
                Success = true,
                Message = "Car added successfully.",
                Data = carDtos,
                Jwt = request.Jwt
            });
        }



        [HttpPost("deleteCar")]
        public async Task<ActionResult<ApiResponse<List<ResponseCarDto>>>> DeleteCar([FromBody] CarIdDeviceJwtDto request)
        {
            var validationResponse = await ValidateUser(request.DeviceId, request.Jwt);
            if (!validationResponse.Success)
            {
                return new ApiResponse<List<ResponseCarDto>>
                {
                    Success = false,
                    Message = validationResponse.Message,
                    Jwt = request.Jwt,
                    Data = null
                };
            }

            var car = validationResponse.Data.Cars.FirstOrDefault(c => c.Id == request.CarId);
            if (car == null)
            {
                return new ApiResponse<List<ResponseCarDto>>
                {
                    Success = false,
                    Message = "Car not found.",
                    Jwt = request.Jwt,
                    Data = null
                };
            }

            validationResponse.Data.Cars.Remove(car);
            _context.Cars.Remove(car);

            await _context.SaveChangesAsync();

            var updatedCarList = validationResponse.Data.Cars.Select(c => new ResponseCarDto
            {
                Id = c.Id,
                Mark = c.Mark,
                Model = c.Model,
                Color = c.Color,
                StateNumber = c.StateNumber,
                VinNumber = c.VinNumber
            }).ToList();

            return new ApiResponse<List<ResponseCarDto>>
            {
                Success = true,
                Message = "Car deleted successfully.",
                Jwt = request.Jwt,
                Data = updatedCarList
            };
        }


        [HttpPost("addService")]
        public async Task<ActionResult<ApiResponse<List<ServiceDto>>>> AddService([FromBody] ServiceDeviceJwtDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.DeviceId) || string.IsNullOrEmpty(request.Jwt) || request.Data == null)
                {
                    return new ApiResponse<List<ServiceDto>>
                    {
                        Success = false,
                        Message = "Invalid request.",
                        Jwt = request.Jwt,
                        Data = null
                    };
                }

                var validationResponse = await ValidateUser(request.DeviceId, request.Jwt);
                if (!validationResponse.Success || validationResponse.Data == null)
                {
                    return new ApiResponse<List<ServiceDto>>
                    {
                        Success = false,
                        Message = validationResponse?.Message ?? "Failed to validate user.",
                        Jwt = request.Jwt,
                        Data = null
                    };
                }

                if (_context?.Services == null)
                {
                    return new ApiResponse<List<ServiceDto>>
                    {
                        Success = false,
                        Message = "Services context is not initialized.",
                        Jwt = request.Jwt,
                        Data = null
                    };
                }

                var newService = new Service
                {
                    Name = request.Data.Name,
                    Description = request.Data.Description,
                    Price = Convert.ToDecimal(request.Data.Price),
                    Category = request.Data.Category,
                    Avatar = Convert.FromBase64String(request.Data.Avatar),
                    UserCommonId = validationResponse.Data.Id,
                    AverageScore = 0
                };

                if (validationResponse.Data.Services == null)
                {
                    validationResponse.Data.Services = new List<Service>();
                }

                validationResponse.Data.Services.Add(newService);
                _context.Services.Add(newService);
                await _context.SaveChangesAsync();

                var updatedServiceList = validationResponse.Data.Services.Select(s => new ServiceDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Price = Convert.ToDouble(s.Price),
                    Category = s.Category,
                    Avatar = Convert.ToBase64String(s.Avatar),
                    AverageScore = (decimal)s.AverageScore
                }).ToList();

                return new ApiResponse<List<ServiceDto>>
                {
                    Success = true,
                    Message = "Service added successfully.",
                    Jwt = request.Jwt,
                    Data = updatedServiceList
                };
            }
            catch (Exception ex)
            {

                return new ApiResponse<List<ServiceDto>>
                {
                    Success = false,
                    Message = "An error occurred while processing your request.",
                    Jwt = request.Jwt,
                    Data = null
                };
            }
        }



        [HttpPost("deleteService")]
        public async Task<ActionResult<ApiResponse<List<ServiceDto>>>> DeleteService([FromBody] ServiceIdDeviceJwtDto request)
        {
            var validationResponse = await ValidateUser(request.DeviceId, request.Jwt);
            if (!validationResponse.Success)
            {
                return BadRequest(validationResponse);
            }

            var service = validationResponse.Data.Services.FirstOrDefault(s => s.Id == request.Id);
            if (service == null)
            {
                return NotFound(new ApiResponse<List<ServiceDto>>
                {
                    Success = false,
                    Message = "Service not found."
                });
            }

            validationResponse.Data.Services.Remove(service);
            _context.Services.Remove(service);

            await _context.SaveChangesAsync();

            var remainingServices = validationResponse.Data.Services.Select(s => new ServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Price = Convert.ToDouble(s.Price),
                Category = s.Category,
                Avatar = Convert.ToBase64String(s.Avatar),
                AverageScore = (decimal)s.AverageScore
            }).ToList();

            return Ok(new ApiResponse<List<ServiceDto>>
            {
                Success = true,
                Message = "Service deleted successfully.",
                Jwt = request.Jwt,
                Data = remainingServices
            });
        }



        [HttpPost("myServices")]
        public async Task<ActionResult<ApiResponse<List<ServiceDto>>>> MyServices([FromBody] DeviceJwtDto request)
        {
            var validationResponse = await ValidateUser(request.DeviceId, request.Jwt);
            if (!validationResponse.Success)
            {
                return BadRequest(validationResponse);
            }

            var myServices = validationResponse.Data.Services.Select(s => new ServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Price = (double)s.Price,
                Category = s.Category,
                Avatar = Convert.ToBase64String(s.Avatar),
                AverageScore = (decimal)s.AverageScore
            }).ToList();

            return Ok(new ApiResponse<List<ServiceDto>>
            {
                Success = true,
                Message = "Services fetched successfully.",
                Jwt = request.Jwt,
                Data = myServices
            });
        }

        [HttpPost("allServices")]
        public async Task<ActionResult<ApiResponse<List<ServiceWithUserDto>>>> AllServices([FromBody] OptionalCategoryDto request)
        {
            IQueryable<Service> query = _context.Services.Include(s => s.UserCommon);

            if (!string.IsNullOrEmpty(request.Category))
            {
                query = query.Where(s => s.Category == request.Category);
            }

            var allServices = await query.Select(s => new ServiceWithUserDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Price = (double)s.Price,
                Category = s.Category,
                Avatar = Convert.ToBase64String(s.Avatar),
                AverageScore = (decimal)s.AverageScore
            }).ToListAsync();

            return Ok(new ApiResponse<List<ServiceWithUserDto>>
            {
                Success = true,
                Message = "Services fetched successfully.",
                Data = allServices
            });
        }

        [HttpPost("addReview")]
        public async Task<ActionResult<ApiResponse<List<ReviewDto>>>> AddReview([FromBody] AddReviewRequest request)
        {
            var validationResponse = await ValidateUser(request.DeviceId, request.Jwt);
            if (!validationResponse.Success)
            {
                return BadRequest(validationResponse);
            }

            var service = await _context.Services.Include(s => s.Reviews)
                .FirstOrDefaultAsync(s => s.Id == request.ServiceId);

            if (service == null)
            {
                return NotFound(new ApiResponse<List<ReviewDto>>
                {
                    Success = false,
                    Message = "Service not found."
                });
            }

            var newReview = new Review
            {
                Rating = request.Data.Rating,
                Comment = request.Data.Comment,
                ServiceId = service.Id
            };

            service.Reviews.Add(newReview);

            // Recalculate the average score
            service.AverageScore = service.Reviews.Average(r => r.Rating);

            await _context.SaveChangesAsync();

            var updatedReviews = service.Reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                Rating = r.Rating,
                Comment = r.Comment
            }).ToList();

            return Ok(new ApiResponse<List<ReviewDto>>
            {
                Success = true,
                Message = "Review added successfully.",
                Jwt = request.Jwt,
                Data = updatedReviews
            });
        }


        private async Task<ApiResponse<UserCommon>> ValidateUser(string deviceId, string jwt)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var principal = tokenHandler.ValidateToken(jwt, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            }, out SecurityToken validatedToken);

            var phoneClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.MobilePhone);
            if (phoneClaim == null)
            {
                return new ApiResponse<UserCommon>
                {
                    Success = false,
                    Message = "Invalid token."
                };
            }

            var user = await _context.UserCommons
                .Include(uc => uc.Cars)
                .Include(uc => uc.Services)
                .FirstOrDefaultAsync(uc => uc.PhoneNumber == phoneClaim.Value && uc.Devices.Any(d => d.DeviceId == deviceId));

            if (user == null)
            {
                return new ApiResponse<UserCommon>
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            return new ApiResponse<UserCommon>
            {
                Success = true,
                Message = "User validated.",
                Data = user
            };
        }




    }
}
