using AutoPartsServiceWebApi.Dto;
using AutoPartsServiceWebApi.Models;

namespace AutoPartsServiceWebApi.Services
{
    public interface IRequestService
    {
        Task<ApiResponse<List<RequestDto>>> CreateRequest(CreateRequestDto createRequestDto);
        Task<ApiResponse<List<RequestDto>>> GetActiveRequests(string jwt, string deviceId);
        Task<ApiResponse<OfferDto>> CreateOffer(CreateOfferDto createOfferDto);
        Task<ApiResponse<OfferDto>> AcceptOffer(AcceptOfferDto acceptOfferDto);
        Task<ApiResponse<RequestDto>> GetRequestById(RequestIdDto requestIdDto);
        Task<ApiResponse<List<RequestDto>>> GetAcceptedRequests(string jwt, string deviceId);
        Task<ApiResponse<RequestDto>> CloseRequest(CloseRequestDto closeRequestDto);
        Task<ApiResponse<List<RequestDto>>> GetMyCreatedRequests(string jwt, string deviceId);
    }
}
