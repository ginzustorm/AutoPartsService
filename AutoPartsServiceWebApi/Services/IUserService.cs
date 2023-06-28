using AutoPartsServiceWebApi.Models;
using AutoPartsServiceWebApi.Dto;
using System.Threading.Tasks;
using AutoPartsServiceWebApi.Dto.Requests;

namespace AutoPartsServiceWebApi.Services
{
    public interface IUserService
    {
        bool CheckIfUserExists(string phoneNumber);
        Task CreateLoginSms(LoginSmsDto loginSmsDto);
        string GenerateJwtToken(string phoneNumber);
        Task<LoginSms> ValidateLoginSms(string phoneNumber, string smsCode, string deviceId);
        Task<UserCommon> GetUserOrCreateNew(AuthenticateRequest request, string token);
        UserCommonDto PrepareUserDto(UserCommon userCommon);
        Task<UserCommon> UpdateUserCommonDetails(UpdateUserRequest request, byte[] avatarData = null);
        Task UpdateUserAddress(UserCommon userCommon, UpdateAddressDto addressDto);
        Task<UserCommonDto> GetUserWithAssociatedEntities(string jwt, string deviceId);
        string SaveImage(byte[] imageData);
        Task<ApiResponse<UserCommon>> ValidateUser(string deviceId, string jwt);
        Task<UserCommon> GetUserByDeviceJwt(DeviceJwtDto request);  
    }
}
