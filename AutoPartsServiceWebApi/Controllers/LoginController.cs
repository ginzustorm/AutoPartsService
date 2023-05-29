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
        public async Task<ActionResult> Post([FromBody] string phoneNumber)
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

            return Ok(loginSms);
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult> Authenticate([FromBody] LoginRequest request)
        {
            var loginSms = await _context.LoginSmses
                .FirstOrDefaultAsync(ls => ls.PhoneNumber == request.PhoneNumber && ls.SmsCode == request.SmsCode);

            if (loginSms == null)
            {
                return BadRequest("Invalid phone number or SMS code.");
            }

            var token = GenerateJwtToken(request.PhoneNumber);

            if (loginSms.NewUser)
            {
                return Ok(new { Token = token, Message = "This is a new user." });
            }
            else
            {
                var userCommon = await _context.UserCommons.FirstOrDefaultAsync(uc => uc.PhoneNumber == request.PhoneNumber);
                var userBusiness = await _context.UserBusinesses
                    .Include(ub => ub.Services) // Include Services in the query
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
                    return Ok(new { Token = token, User = userCommonDTO });
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
                    return Ok(new { Token = token, User = userBusinessDTO });
                }
                else
                {
                    return BadRequest("User not found.");
                }
            }
        }


        [HttpPost("register")]
        public async Task<ActionResult<object>> Register([FromBody] RegisterRequest request)
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
                return BadRequest("Invalid token or phone number.");
            }

            var userCommon = await _context.UserCommons.FirstOrDefaultAsync(uc => uc.PhoneNumber == request.PhoneNumber);
            var userBusiness = await _context.UserBusinesses.FirstOrDefaultAsync(ub => ub.Phone == request.PhoneNumber);
            if (userCommon != null || userBusiness != null)
            {
                return BadRequest("User already exists.");
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

                return Ok(userCommonDto);
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

                return Ok(userBusinessDto);
            }
            else
            {
                return BadRequest("Invalid user type.");
            }
        }

        [HttpGet("userInfo")]
        public async Task<IActionResult> GetUserInfo([FromQuery] string jwt, [FromQuery] string deviceId)
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
                return BadRequest("Invalid token.");
            }

            var userCommon = await _context.UserCommons
                            .Include(u => u.Devices)
                            .FirstOrDefaultAsync(uc => uc.Devices.Any(d => d.DeviceId == deviceId));

            var userBusiness = await _context.UserBusinesses
                            .Include(ub => ub.Devices)
                            .Include(ub => ub.Services) // Include Services in the query
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

                return Ok(userCommonDto);
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

                return Ok(userBusinessDto);
            }
            else
            {
                return NotFound("User not found.");
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
    }
}