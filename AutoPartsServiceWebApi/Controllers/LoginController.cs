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
        public async Task<ActionResult<ApiResponse<LoginSms>>> Post([FromBody] string phoneNumber)
        {
            var isUserExists = _context.UserCommons.Any(uc => uc.PhoneNumber == phoneNumber) || _context.UserBusinesses.Any(ub => ub.Phone == phoneNumber);

            var loginSms = new LoginSms
            {
                PhoneNumber = phoneNumber,
                CreationDate = DateTime.UtcNow,
                NewUser = !isUserExists,
                SmsCode = GenerateSmsCode()
            };

            _context.LoginSmses.Add(loginSms);
            await _context.SaveChangesAsync();

            await SendSmsAsync(phoneNumber, loginSms.SmsCode);

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
                .FirstOrDefaultAsync(ls => ls.PhoneNumber == request.PhoneNumber && ls.SmsCode == request.SmsCode);

            if (loginSms == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid phone number or SMS code."
                };
            }

            var token = GenerateJwtToken(request.PhoneNumber);

            if (loginSms.NewUser)
            {
                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "This is a new user.",
                    Data = new { Token = token }
                };
            }
            else
            {
                var userCommon = await _context.UserCommons.FirstOrDefaultAsync(uc => uc.PhoneNumber == request.PhoneNumber);
                var userBusiness = await _context.UserBusinesses
                    .Include(ub => ub.Services) 
                    .FirstOrDefaultAsync(ub => ub.Phone == request.PhoneNumber);

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


        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<object>>> Register([FromBody] RegisterRequest request)
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
            if (userCommon != null || userBusiness != null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "User already exists."
                };
            }

            if (request.UserType == "Common")
            {
                userCommon = new UserCommon
                {
                    Name = request.Name,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Password = request.Password,
                    RegistrationDate = DateTime.UtcNow,
                    Address = new Address
                    {
                        Country = request.Country,
                        Region = request.Region,
                        City = request.City,
                        Street = request.Street
                    },
                };

                _context.UserCommons.Add(userCommon);
                await _context.SaveChangesAsync();

                var device = new Device() { DeviceId = Guid.NewGuid().ToString(), UserCommonId = userCommon.Id, UserBusinessId = null };
                _context.Devices.Add(device);
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
                    Message = "User registered.",
                    Data = userCommonDto
                };
            }
            else if (request.UserType == "Business")
            {
                userBusiness = new UserBusiness
                {
                    Email = request.Email,
                    Phone = request.PhoneNumber,
                    Password = request.Password,
                    RegistrationDate = DateTime.UtcNow,
                };

                _context.UserBusinesses.Add(userBusiness);
                await _context.SaveChangesAsync();

                var device = new Device() { DeviceId = Guid.NewGuid().ToString(), UserBusinessId = userBusiness.Id, UserCommonId = null };
                _context.Devices.Add(device);
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
                    Message = "User registered.",
                    Data = userBusinessDto
                };
            }
            else
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid user type."
                };
            }
        }

        [HttpGet("userInfo")]
        public async Task<ActionResult<ApiResponse<object>>> GetUserInfo([FromQuery] string jwt, [FromQuery] string deviceId)
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

        private async Task SendSmsAsync(string phoneNumber, string message)
        {
            string src = 
                $@"<?xml version=""1.0"" encoding=""UTF-8""?> 
                    <SMS> 
                        <operations> 
                            <operation>SEND</operation> 
                        </operations> 
                        <authentification> 
                            <username>Your AtomPark username here</username> 
                            <password>Your AtomPark password here</password> 
                        </authentification> 
                        <message> 
                            <sender>SMS</sender> 
                            <text>{message}</text> 
                        </message> 
                        <numbers> 
                        <number messageID=""msg11"">{phoneNumber}</number> 
                        </numbers> 
                    </SMS>";

            var httpClient = new HttpClient();

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("XML", src)
            });

            HttpResponseMessage response = await httpClient.PostAsync("http://api.atompark.com/members/sms/xml.php", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("SMS sending failed");
            }
        }

    }
}