using AutoMapper;
using AutoPartsServiceWebApi.Data;
using AutoPartsServiceWebApi.Dto.Requests;
using AutoPartsServiceWebApi.Dto;
using AutoPartsServiceWebApi.Models;
using AutoPartsServiceWebApi.Services;
using AutoPartsServiceWebApi;
using Microsoft.EntityFrameworkCore;

public class ReviewService : IReviewService
{
    private readonly AutoDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    public ReviewService(AutoDbContext context, IMapper mapper, IUserService userService)
    {
        _context = context;
        _mapper = mapper;
        _userService = userService;
    }

    public async Task<ApiResponse<List<ReviewDto>>> AddReview(AddReviewRequest request)
    {
        var deviceJwtDto = new DeviceJwtDto
        {
            DeviceId = request.DeviceId,
            Jwt = request.Jwt
        };

        var userCommon = await _userService.GetUserByDeviceJwt(deviceJwtDto);
        if (userCommon == null)
        {
            return new ApiResponse<List<ReviewDto>>
            {
                Success = false,
                Message = "Invalid DeviceId or Jwt.",
            };
        }

        var service = await _context.Services
            .Include(s => s.Reviews)
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId);

        if (service == null)
        {
            return new ApiResponse<List<ReviewDto>>
            {
                Success = false,
                Message = "Service not found."
            };
        }

        var newReview = _mapper.Map<Review>(request.Data);
        newReview.ServiceId = service.Id;
        service.Reviews.Add(newReview);
        service.AverageScore = service.Reviews.Average(r => r.Rating);

        await _context.SaveChangesAsync();

        var updatedReviews = _mapper.Map<List<ReviewDto>>(service.Reviews);

        return new ApiResponse<List<ReviewDto>>
        {
            Success = true,
            Message = "Review added successfully.",
            Jwt = request.Jwt,
            Data = updatedReviews
        };
    }
}
