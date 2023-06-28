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
using AutoPartsServiceWebApi.Tools;
using AutoPartsServiceWebApi.Services;
using AutoPartsServiceWebApi.Dto.Requests;

namespace AutoPartsServiceWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly AutoDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly IUserService _userService;
        private readonly ISmsService _smsService;

        public LoginController(AutoDbContext context, IConfiguration configuration, IWebHostEnvironment environment, IUserService userService, ISmsService smsService)
        {
            _context = context;
            _configuration = configuration;
            _environment = environment;
            _userService = userService;
            _smsService = smsService;
        }

        [HttpPost]
        public async Task<ActionResult> SendLoginSms([FromBody] LoginRequestDto loginRequestDto)
        {
            try
            {
                var isUserExists = _userService.CheckIfUserExists(loginRequestDto.PhoneNumber);

                var loginSms = new LoginSmsDto
                {
                    PhoneNumber = loginRequestDto.PhoneNumber,
                    DeviceId = loginRequestDto.DeviceId,
                    CreationDate = DateTime.UtcNow,
                    NewUser = !isUserExists,
                    SmsCode = _smsService.GenerateSmsCode()
                };

                await _userService.CreateLoginSms(loginSms);
                await _smsService.SendSmsAsync(loginRequestDto.PhoneNumber, loginSms.SmsCode);

                return Ok(new { Message = "SMS sent." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while sending SMS.");
            }
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<ApiResponse<object>>> Authenticate([FromBody] AuthenticateRequest request)
        {
            var loginSms = await _userService.ValidateLoginSms(request.PhoneNumber, request.SmsCode, request.DeviceId);

            if (loginSms == null)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid phone number, SMS code, or device id."
                });
            }

            var token = _userService.GenerateJwtToken(request.PhoneNumber);
            var userCommon = await _userService.GetUserOrCreateNew(request, token);

            var userCommonDTO = _userService.PrepareUserDto(userCommon);

            if (string.IsNullOrEmpty(userCommon.Name))
            {
                userCommonDTO = new UserCommonDto(); 
                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "User is new.",
                    Jwt = token,
                    Data = userCommonDTO
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


        [HttpPost("updateUser")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateUser([FromBody] UpdateUserRequest request)
        {
            // Updating user details
            var userCommon = await _userService.UpdateUserCommonDetails(request);

            if (userCommon == null)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found."
                });
            }

            // If avatar URL is provided, update user's avatar URL
            if (!string.IsNullOrEmpty(request.Data.Avatar))
            {
                userCommon.Avatar = request.Data.Avatar;
                await _context.SaveChangesAsync();
            }

            if (request.Data.Address != null)
            {
                await _userService.UpdateUserAddress(userCommon, request.Data.Address);
            }

            var userCommonDTO = _userService.PrepareUserDto(userCommon);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "User updated.",
                Jwt = request.Jwt,
                Data = userCommonDTO
            });
        }


        [HttpPost("uploadImage")]
        public ActionResult<ApiResponse<string>> UploadImage([FromBody] ImageUploadDto imageUploadDto)
        {
            if (imageUploadDto.ImageData == null || imageUploadDto.ImageData.Length == 0)
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "No image data provided."
                };

            // Save image to wwwroot/images directory and get the URL
            var imageUrl = SaveImage(imageUploadDto.ImageData);

            return new ApiResponse<string>
            {
                Success = true,
                Message = "Image uploaded.",
                Data = imageUrl
            };
        }

        private string SaveImage(byte[] imageData)
        {
            var fileName = $"{Guid.NewGuid()}_image.jpg";  // create a unique name for the file
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

            // Save the file to the server
            System.IO.File.WriteAllBytes(path, imageData);

            return $"/images/{fileName}";  // return the URL to the image
        }



        [HttpPost("userInfo")]
        public async Task<ActionResult<ApiResponse<object>>> GetUserInfo([FromBody] UserRequest request)
        {
            var userCommonDto = await _userService.GetUserWithAssociatedEntities(request.Jwt, request.DeviceId);

            if (userCommonDto == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found."
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
    }
}