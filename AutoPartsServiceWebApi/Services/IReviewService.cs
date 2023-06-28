using AutoPartsServiceWebApi.Dto.Requests;
using AutoPartsServiceWebApi.Dto;

namespace AutoPartsServiceWebApi.Services
{
    public interface IReviewService
    {
        Task<ApiResponse<List<ReviewDto>>> AddReview(AddReviewRequest request);
    }

}
