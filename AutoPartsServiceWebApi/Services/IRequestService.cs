using AutoPartsServiceWebApi.Dto;
using AutoPartsServiceWebApi.Models;

namespace AutoPartsServiceWebApi.Services
{
    public interface IRequestService
    {
        Task<ApiResponse<List<RequestDto>>> CreateRequest(CreateRequestDto createRequestDto);
        Task<List<Request>> GetActiveRequests(int userId);
        Task<ApiResponse<List<RequestDto>>> GetAvailableRequestsForUser(string jwt, string deviceId);
        Task<ApiResponse<OfferDto>> CreateOffer(CreateOfferDto createOfferDto);
        Task<ApiResponse<OfferDto>> AcceptOffer(AcceptOfferDto acceptOfferDto);

    }
}
