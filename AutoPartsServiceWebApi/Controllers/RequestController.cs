using AutoPartsServiceWebApi.Data;
using AutoPartsServiceWebApi.Dto;
using AutoPartsServiceWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsServiceWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly AutoDbContext _context;

        public RequestController(AutoDbContext context)
        {
            _context = context;
        }

        // 1. CreateRequest
        [HttpPost("{userCommonId}")]
        public async Task<ActionResult<ApiResponse<object>>> CreateRequest(int userCommonId, RequestCreateDto requestDto)
        {
            var user = await _context.UserCommons.FindAsync(userCommonId);
            if (user == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            var request = new Request
            {
                Description = requestDto.Description,
                Header = requestDto.Header,
                Price = requestDto.Price,
                UserCommonId = userCommonId,
                CreationDate = DateTime.UtcNow,
                Active = true
            };

            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Request created successfully."
            };
        }


        // 2. GetActiveRequests
        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Request>>>> GetActiveRequests()
        {
            var requests = await _context.Requests.Where(r => r.Active).ToListAsync();
            return new ApiResponse<IEnumerable<Request>>
            {
                Success = true,
                Message = "Active requests retrieved successfully.",
                Data = requests
            };
        }

        // 3. AcceptRequest
        [HttpPost("accept/{requestId}/{userId}")]
        public async Task<ActionResult<ApiResponse<object>>> AcceptRequest(int requestId, int userId)
        {
            var request = await _context.Requests.FindAsync(requestId);
            if (request == null || !request.Active)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Request not found or already accepted."
                };
            }

            var user = await _context.UserCommons.FindAsync(userId);
            if (user == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            request.Active = false;
            request.AcceptedByUserId = userId;

            await _context.SaveChangesAsync();

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Request accepted successfully."
            };
        }
    }

}
