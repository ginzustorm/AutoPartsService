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

namespace AutoPartsServiceWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly AutoDbContext _context;
        private readonly IConfiguration _configuration;

        public LoginController(AutoDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<LoginSms>>> Post([FromBody] LoginRequestModel loginRequestModel)
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

            return new ApiResponse<LoginSms>
            {
                Success = true,
                Message = "SMS sent.",
                Data = loginSms
            };
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
                    // Initialize the Devices list and add a new Device
                    Devices = new List<Device>
            {
                new Device
                {
                    DeviceId = request.DeviceId
                }
            }
                };

                // Add the new user to the context and save changes
                _context.UserCommons.Add(newUser);
                await _context.SaveChangesAsync();

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "This is a new user.",
                    Data = new { Token = token, User = newUser }
                };
            }
            else
            {
                var userCommon = await _context.UserCommons
                    .Include(uc => uc.Devices)
                    .FirstOrDefaultAsync(uc => uc.PhoneNumber == request.PhoneNumber);

                // If userCommon does not have a device with this DeviceId, then add it
                if (userCommon != null && userCommon.Devices.All(d => d.DeviceId != request.DeviceId))
                {
                    userCommon.Devices.Add(new Device { DeviceId = request.DeviceId });
                    await _context.SaveChangesAsync();
                }

                var userBusiness = await _context.UserBusinesses
                    .Include(ub => ub.Services)
                    .Include(ub => ub.Devices)
                    .FirstOrDefaultAsync(ub => ub.Phone == request.PhoneNumber);

                // If userBusiness does not have a device with this DeviceId, then add it
                if (userBusiness != null && userBusiness.Devices.All(d => d.DeviceId != request.DeviceId))
                {
                    userBusiness.Devices.Add(new Device { DeviceId = request.DeviceId });
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
                        Address = userCommon.Address
                    };
                    return new ApiResponse<object>
                    {
                        Success = true,
                        Message = "User found.",
                        Data = new { Token = token, User = userCommonDTO }
                    };
                }
                else if (userBusiness != null)
                {
                    var userBusinessDTO = new UserBusinessDto
                    {
                        Id = userBusiness.Id,
                        Email = userBusiness.Email,
                        Phone = userBusiness.Phone,
                        RegistrationDate = userBusiness.RegistrationDate,
                        Password = userBusiness.Password,
                        Services = userBusiness.Services.Select(s => new ServiceDto
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Description = s.Description,
                            Price = s.Price
                        }).ToList()
                    };
                    return new ApiResponse<object>
                    {
                        Success = true,
                        Message = "User found.",
                        Data = new { Token = token, User = userBusinessDTO }
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
        public async Task<ActionResult<ApiResponse<object>>> UpdateUser([FromBody] RegisterRequest request)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var principal = tokenHandler.ValidateToken(request.Token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            }, out SecurityToken validatedToken);

            var phoneClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.MobilePhone);
            if (phoneClaim == null || phoneClaim.Value != request.PhoneNumber)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid token or phone number."
                };
            }

            var userCommon = await _context.UserCommons.FirstOrDefaultAsync(uc => uc.PhoneNumber == request.PhoneNumber);
            var userBusiness = await _context.UserBusinesses.FirstOrDefaultAsync(ub => ub.Phone == request.PhoneNumber);

            if (userCommon != null)
            {
                userCommon.Name = request.Name;
                userCommon.Email = request.Email;
                userCommon.Password = request.Password;

                var device = userCommon.Devices?.FirstOrDefault(d => d.DeviceId == request.DeviceId);
                if (device != null)
                {
                    // 
                }

                await _context.SaveChangesAsync();

                var userCommonDto = new UserCommonDto
                {
                    Id = userCommon.Id,
                    Name = userCommon.Name,
                    Email = userCommon.Email,
                    PhoneNumber = userCommon.PhoneNumber,
                    Password = userCommon.Password,
                    RegistrationDate = userCommon.RegistrationDate,
                    Address = userCommon.Address
                };

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "User updated.",
                    Data = userCommonDto
                };
            }
            else if (userBusiness != null)
            {
                userBusiness.Email = request.Email;
                userBusiness.Password = request.Password;

                var device = userBusiness.Devices.FirstOrDefault(d => d.DeviceId == request.DeviceId);
                if (device != null)
                {
                    // 
                }

                await _context.SaveChangesAsync();

                var userBusinessDto = new UserBusinessDto
                {
                    Id = userBusiness.Id,
                    Email = userBusiness.Email,
                    Phone = userBusiness.Phone,
                    Password = userBusiness.Password,
                    RegistrationDate = userBusiness.RegistrationDate
                };

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "User updated.",
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

        [HttpGet("userInfo")]
        public async Task<ActionResult<ApiResponse<object>>> GetUserInfo([FromQuery] string deviceId, [FromQuery] string jwt)
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
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid token."
                };
            }

            var userCommon = await _context.UserCommons
                            .Include(u => u.Devices)
                            .FirstOrDefaultAsync(uc => uc.Devices.Any(d => d.DeviceId == deviceId));

            var userBusiness = await _context.UserBusinesses
                            .Include(ub => ub.Devices)
                            .Include(ub => ub.Services)
                            .FirstOrDefaultAsync(ub => ub.Devices.Any(d => d.DeviceId == deviceId));

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
                    Address = userCommon.Address
                };

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "User found.",
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
                        Price = s.Price
                    }).ToList()
                };

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "User found.",
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


    }
}