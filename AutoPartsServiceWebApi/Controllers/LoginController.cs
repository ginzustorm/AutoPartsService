using AutoPartsServiceWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using AutoPartsServiceWebApi.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using AutoPartsServiceWebApi.Dto;
using System.Net;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;

namespace AutoPartsServiceWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly AutoDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public LoginController(AutoDbContext context, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _context = context;
            _configuration = configuration;
            _environment = environment;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] LoginRequestModel loginRequestModel)
        {
            var isUserExists = _context.UserCommons.Any(uc => uc.PhoneNumber == loginRequestModel.PhoneNumber) || _context.UserBusinesses.Any(ub => ub.Phone == loginRequestModel.PhoneNumber);

            var loginSms = new LoginSms
            {
                PhoneNumber = loginRequestModel.PhoneNumber,
                DeviceId = loginRequestModel.DeviceId,
                CreationDate = DateTime.UtcNow,
                NewUser = !isUserExists,
                SmsCode = GenerateSmsCode()
            };

            _context.LoginSmses.Add(loginSms);
            await _context.SaveChangesAsync();

            await SendSmsAsync(loginRequestModel.PhoneNumber, loginSms.SmsCode);

            return Ok(new { Message = "SMS sent." });
        }



        [HttpPost("authenticate")]
        public async Task<ActionResult<ApiResponse<object>>> Authenticate([FromBody] LoginRequest request)
        {
            var loginSms = await _context.LoginSmses
                .FirstOrDefaultAsync(ls => ls.PhoneNumber == request.PhoneNumber && ls.SmsCode == request.SmsCode && ls.DeviceId == request.DeviceId);

            if (loginSms == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid phone number, SMS code, or device id."
                };
            }

            var token = GenerateJwtToken(request.PhoneNumber);

            if (loginSms.NewUser)
            {
                // Create a new UserCommon
                var newUser = new UserCommon
                {
                    PhoneNumber = request.PhoneNumber,
                    RegistrationDate = DateTime.UtcNow,
                    Devices = new List<Device>
            {
                new Device
                {
                    DeviceId = request.DeviceId
                }
            }
                };

                _context.UserCommons.Add(newUser);
                await _context.SaveChangesAsync();

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "This is a new user.",
                    Jwt = token,
                    DeviceId = request.DeviceId,
                    Data = null
                };
            }
            else
            {
                var userCommon = await _context.UserCommons
                    .Include(uc => uc.Devices)
                    .Include(uc => uc.Address)
                    .FirstOrDefaultAsync(uc => uc.PhoneNumber == request.PhoneNumber);

                if (userCommon != null && userCommon.Devices.All(d => d.DeviceId != request.DeviceId))
                {
                    userCommon.Devices.Add(new Device { DeviceId = request.DeviceId });
                    await _context.SaveChangesAsync();
                }

                if (userCommon != null)
                {
                    var userCommonDTO = new UserCommonDto
                    {
                        Id = userCommon.Id,
                        Name = userCommon.Name,
                        Email = userCommon.Email,
                        PhoneNumber = userCommon.PhoneNumber,
                        RegistrationDate = userCommon.RegistrationDate,
                        Password = userCommon.Password,
                        Avatar = userCommon.Avatar
                    };

                    // Check if userCommon.Address is not null
                    if (userCommon.Address != null)
                    {
                        userCommonDTO.Address = new AutoPartsServiceWebApi.Dto.AddressDto
                        {
                            Country = userCommon.Address.Country,
                            Region = userCommon.Address.Region,
                            City = userCommon.Address.City,
                            Street = userCommon.Address.Street
                        };
                    }

                    return new ApiResponse<object>
                    {
                        Success = true,
                        Message = "User found.",
                        Jwt = token,
                        Data = userCommonDTO
                    };
                }
                else
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not found."
                    };
                }
            }
        }

        [HttpPost("updateUser")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateUser([FromBody] UpdateUserRequest request)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var principal = tokenHandler.ValidateToken(request.Jwt, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            }, out SecurityToken validatedToken);

            var phoneClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.MobilePhone);
            if (phoneClaim == null || phoneClaim.Value != request.Data.PhoneNumber)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid token or phone number."
                };
            }

            var userCommon = await _context.UserCommons.Include(uc => uc.Address).FirstOrDefaultAsync(uc => uc.PhoneNumber == request.Data.PhoneNumber);
            if (userCommon != null)
            {
                userCommon.Name = request.Data.Name;
                userCommon.Email = request.Data.Email;
                userCommon.Password = request.Data.Password;

                // Address update
                if (request.Data.Address != null)
                {
                    if (userCommon.Address == null)
                    {
                        userCommon.Address = new Address();
                    }

                    userCommon.Address.Country = request.Data.Address.Country;
                    userCommon.Address.Region = request.Data.Address.Region;
                    userCommon.Address.City = request.Data.Address.City;
                    userCommon.Address.Street = request.Data.Address.Street;
                    userCommon.Address.UserCommonId = userCommon.Id;

                    _context.Addresses.Update(userCommon.Address);
                }

                // Check if the Avatar field is not null or empty and then convert it to byte array and save
                if (!string.IsNullOrEmpty(request.Data.Avatar))
                {
                    string fileName = $"{userCommon.Id}_avatar.jpg";
                    string imageUrl = SaveImage(request.Data.Avatar, fileName);
                    userCommon.Avatar = imageUrl;
                }

                var device = userCommon.Devices?.FirstOrDefault(d => d.DeviceId == request.DeviceId);
                if (device != null)
                {
                    // 
                }

                await _context.SaveChangesAsync();

                return new ApiResponse<string>
                {
                    Success = true,
                    Message = "User updated.",
                    Jwt = request.Jwt
                };
            }
            else
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "User not found."
                };
            }
        }



        [HttpPost("userInfo")]
        public async Task<ActionResult<ApiResponse<object>>> GetUserInfo([FromBody] UserRequest request)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var principal = tokenHandler.ValidateToken(request.Jwt, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            }, out SecurityToken validatedToken);

            var phoneClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.MobilePhone);
            if (phoneClaim == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid token."
                };
            }

            var userCommon = await _context.UserCommons
                                .Include(u => u.Devices)
                                .Include(u => u.Address)
                                .Include(u => u.Cars) // fetch UserCommon's Cars
                                .Include(u => u.Services) // fetch UserCommon's Services
                                .FirstOrDefaultAsync(uc => uc.Devices.Any(d => d.DeviceId == request.DeviceId));

            var userBusiness = await _context.UserBusinesses
                                .Include(ub => ub.Devices)
                                .Include(ub => ub.Services)
                                .FirstOrDefaultAsync(ub => ub.Devices.Any(d => d.DeviceId == request.DeviceId));

            if (userCommon != null)
            {
                var userCommonDto = new UserCommonDto
                {
                    Id = userCommon.Id,
                    Name = userCommon.Name,
                    Email = userCommon.Email,
                    PhoneNumber = userCommon.PhoneNumber,
                    Password = userCommon.Password,
                    RegistrationDate = userCommon.RegistrationDate,
                    Avatar = userCommon.Avatar,
                    Cars = userCommon.Cars?.Select(c => new CarDto
                    {
                        Mark = c.Mark,
                        Model = c.Model,
                        Color = c.Color,
                        StateNumber = c.StateNumber,
                        VinNumber = c.VinNumber
                    }).ToList(),
                    Services = userCommon.Services?.Select(s => new ServiceDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description,
                        Price = Convert.ToDouble(s.Price),
                        Category = s.Category,
                        Avatar = Convert.ToBase64String(s.Avatar),
                        AverageScore = (decimal)s.AverageScore
                    }).ToList()
                };

                if (userCommon.Address != null)
                {
                    userCommonDto.Address = new AutoPartsServiceWebApi.Dto.AddressDto
                    {
                        Country = userCommon.Address.Country,
                        Region = userCommon.Address.Region,
                        City = userCommon.Address.City,
                        Street = userCommon.Address.Street
                    };
                }

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "User found.",
                    Jwt = request.Jwt,
                    Data = userCommonDto
                };
            }
            else if (userBusiness != null)
            {
                var userBusinessDto = new UserBusinessDto
                {
                    Id = userBusiness.Id,
                    Email = userBusiness.Email,
                    Phone = userBusiness.Phone,
                    Password = userBusiness.Password,
                    RegistrationDate = userBusiness.RegistrationDate,
                    Services = userBusiness.Services.Select(s => new ServiceDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description,
                        Price = (double)s.Price,
                        Category = s.Category,
                        Avatar = Convert.ToBase64String(s.Avatar),
                        AverageScore = (decimal)s.AverageScore
                    }).ToList(),
                    Avatar = userBusiness.Avatar
                };

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "User found.",
                    Jwt = request.Jwt,
                    Data = userBusinessDto
                };
            }
            else
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found."
                };
            }
        }


        public class UserRequest
        {
            public string DeviceId { get; set; }
            public string Jwt { get; set; }
        }


        private string GenerateSmsCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private string GenerateJwtToken(string phoneNumber)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.MobilePhone, phoneNumber)
        };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task SendSmsAsync(string phoneNumber, string message, string sender = "SMS", DateTime? datetime = null, int sms_lifetime = 0, int type = 2)
        {
            var datetimeParam = datetime.HasValue ? datetime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "";

            string url = $"https://api.atompark.com/sms/3.0/sendSMS?key=publicKey&sum=controlSum&sender={sender}&text={message}&phone={phoneNumber}&datetime={datetimeParam}&sms_lifetime={sms_lifetime}&type={type}";

            var httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("SMS sending failed");
            }

            string resultContent = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<dynamic>(resultContent);

            if (result.result == null)
            {
                throw new Exception("Error during SMS sending. No result received.");
            }
        }

        private string SaveImage(string base64Image, string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

            byte[] imageBytes = Convert.FromBase64String(base64Image);

            System.IO.File.WriteAllBytes(path, imageBytes);

            return $"/images/{fileName}";
        }

    }
}