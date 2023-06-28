using AutoMapper;
using AutoPartsServiceWebApi.Data;
using AutoPartsServiceWebApi.Dto;
using AutoPartsServiceWebApi.Dto.Requests;
using AutoPartsServiceWebApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AutoPartsServiceWebApi.Services
{
    public class UserService : IUserService
    {
        private readonly AutoDbContext _context; 
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public UserService(AutoDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }


        public bool CheckIfUserExists(string phoneNumber)
        {
            return _context.UserCommons.Any(uc => uc.PhoneNumber == phoneNumber) ||
                   _context.UserBusinesses.Any(ub => ub.Phone == phoneNumber);
        }

        public async Task CreateLoginSms(LoginSmsDto loginSmsDto)
        {
            var loginSms = _mapper.Map<LoginSms>(loginSmsDto);
            _context.LoginSmses.Add(loginSms);
            await _context.SaveChangesAsync();
        }

        public string GenerateJwtToken(string phoneNumber)
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

        public async Task<LoginSms> ValidateLoginSms(string phoneNumber, string smsCode, string deviceId)
        {
            var loginSms = await _context.LoginSmses
                .Where(ls => ls.PhoneNumber == phoneNumber && ls.SmsCode == smsCode && ls.DeviceId == deviceId)
                .OrderByDescending(ls => ls.Id).FirstOrDefaultAsync();
            return loginSms;
        }

        public async Task<UserCommon> GetUserOrCreateNew(AuthenticateRequest request, string token)
        {
            var userCommon = await _context.UserCommons
                .Include(uc => uc.Devices)
                .Include(uc => uc.Address)
                .FirstOrDefaultAsync(uc => uc.PhoneNumber == request.PhoneNumber);

            if (userCommon == null)
            {
                userCommon = new UserCommon
                {
                    PhoneNumber = request.PhoneNumber,
                    RegistrationDate = DateTime.UtcNow,
                    Devices = new List<Device>
            {
                new Device
                {
                    DeviceId = request.DeviceId
                }
            },
                    Jwt = token
                };
                _context.UserCommons.Add(userCommon);
            }
            else
            {
                if (userCommon.Devices.All(d => d.DeviceId != request.DeviceId))
                {
                    userCommon.Devices.Add(new Device { DeviceId = request.DeviceId });
                }
                userCommon.Jwt = token;
            }
            await _context.SaveChangesAsync();

            return userCommon;
        }

        public UserCommonDto PrepareUserDto(UserCommon userCommon)
        {
            var userCommonDTO = _mapper.Map<UserCommonDto>(userCommon);

            if (userCommon.Address != null)
            {
                userCommonDTO.Address = _mapper.Map<AddressDto>(userCommon.Address);
            }
            return userCommonDTO;
        }

        public async Task<UserCommon> UpdateUserCommonDetails(UpdateUserRequest request, byte[] avatarData = null)
        {
            var userCommon = await _context.UserCommons
                .Include(uc => uc.Address)
                .FirstOrDefaultAsync(uc => uc.Jwt == request.Jwt && uc.Devices.Any(d => d.DeviceId == request.DeviceId));

            if (userCommon != null)
            {
                userCommon.Name = request.Data.Name;
                userCommon.Email = request.Data.Email;
                userCommon.Password = request.Data.Password;

                if (avatarData != null)
                {
                    var avatarUrl = SaveImage(avatarData);

                    userCommon.Avatar = avatarUrl;
                }

                await _context.SaveChangesAsync();
            }

            return userCommon;
        }

        public async Task UpdateUserAddress(UserCommon userCommon, UpdateAddressDto addressDto)
        {
            if (userCommon.Address == null)
            {
                userCommon.Address = new Address();
            }

            userCommon.Address.Country = addressDto.Country;
            userCommon.Address.Region = addressDto.Region;
            userCommon.Address.City = addressDto.City;
            userCommon.Address.Street = addressDto.Street;
            userCommon.Address.UserCommonId = userCommon.Id;

            _context.Addresses.Update(userCommon.Address);
            await _context.SaveChangesAsync();
        }

        public async Task<UserCommonDto> GetUserWithAssociatedEntities(string jwt, string deviceId)
        {
            var userCommon = await _context.UserCommons
                .Include(u => u.Devices)
                .Include(u => u.Address)
                .Include(u => u.Cars)
                .Include(u => u.Services)
                .Include(u => u.RequestCategories)
                .Include(u => u.Requests)
                .FirstOrDefaultAsync(uc => uc.Jwt == jwt && uc.Devices.Any(d => d.DeviceId == deviceId));

            if (userCommon == null)
            {
                return null;
            }

            return _mapper.Map<UserCommonDto>(userCommon);
        }

        public string SaveImage(byte[] imageData)
        {
            var fileName = $"{Guid.NewGuid()}_image.jpg";  // create a unique name for the file
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

            // Save the file to the server
            System.IO.File.WriteAllBytes(path, imageData);

            return $"/images/{fileName}";  // return the URL to the image
        }

        public async Task<ApiResponse<UserCommon>> ValidateUser(string deviceId, string jwt)
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

        public async Task<UserCommon> GetUserByDeviceJwt(DeviceJwtDto request)
        {
            return await _context.UserCommons
                .Include(u => u.Devices)
                .FirstOrDefaultAsync(u => u.Devices.Any(d => d.DeviceId == request.DeviceId) && u.Jwt == request.Jwt);
        }
    }
}
