using AutoMapper;
using AutoPartsServiceWebApi.Data;
using AutoPartsServiceWebApi.Dto;
using AutoPartsServiceWebApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoPartsServiceWebApi.Services
{
    public class RequestService : IRequestService
    {
        private readonly AutoDbContext _context;
        private readonly IMapper _mapper;

        public RequestService(AutoDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<RequestDto>>> CreateRequest(CreateRequestDto createRequestDto)
        {
            var userCommon = await _context.UserCommons
                .Include(uc => uc.Requests)
                .FirstOrDefaultAsync(uc => uc.Jwt == createRequestDto.Jwt && uc.Devices.Any(d => d.DeviceId == createRequestDto.DeviceId));

            if (userCommon == null)
            {
                throw new Exception("User not found.");
            }

            var newRequest = _mapper.Map<Request>(createRequestDto);
            newRequest.Active = true;
            newRequest.CreationDate = DateTime.Now;

            userCommon.Requests.Add(newRequest);

            await _context.SaveChangesAsync();

            var activeRequests = userCommon.Requests.Where(r => r.Active).ToList();

            var activeRequestsDto = _mapper.Map<List<RequestDto>>(activeRequests);

            var apiResponse = new ApiResponse<List<RequestDto>>
            {
                Success = true,
                Message = "Request created successfully.",
                Jwt = createRequestDto.Jwt,
                DeviceId = createRequestDto.DeviceId,
                Data = activeRequestsDto
            };

            return apiResponse;
        }


        public async Task<List<Request>> GetActiveRequests(int userId)
        {
            var userCommon = await _context.UserCommons
                .Include(uc => uc.Requests)
                .FirstOrDefaultAsync(uc => uc.Id == userId);

            if (userCommon == null)
            {
                throw new Exception("User not found.");
            }

            return userCommon.Requests.Where(r => r.Active).ToList();
        }

        public async Task<ApiResponse<List<RequestDto>>> GetAvailableRequestsForUser(string jwt, string deviceId)
        {
            var userCommon = await _context.UserCommons
                .Include(uc => uc.RequestCategories)
                .FirstOrDefaultAsync(uc => uc.Jwt == jwt && uc.Devices.Any(d => d.DeviceId == deviceId));

            if (userCommon == null)
            {
                throw new Exception("User not found.");
            }

            // Filter the categories
            var userCategories = userCommon.RequestCategories.Select(rc => rc.CategoryName);

            // Get all the active requests excluding the ones created by the user himself
            var availableRequests = await _context.Requests
                .Where(r => r.Active && r.UserCommonId != userCommon.Id && userCategories.Contains(r.Category))
                .ToListAsync();

            var availableRequestsDto = _mapper.Map<List<RequestDto>>(availableRequests);

            var apiResponse = new ApiResponse<List<RequestDto>>
            {
                Success = true,
                Message = "Available requests fetched successfully.",
                Jwt = jwt,
                DeviceId = deviceId,
                Data = availableRequestsDto
            };

            return apiResponse;
        }

        public async Task<ApiResponse<OfferDto>> CreateOffer(CreateOfferDto createOfferDto)
        {
            var userCommon = await _context.UserCommons
                .FirstOrDefaultAsync(uc => uc.Jwt == createOfferDto.Jwt && uc.Devices.Any(d => d.DeviceId == createOfferDto.DeviceId));

            if (userCommon == null)
            {
                throw new Exception("User not found.");
            }

            var newOffer = new Offer
            {
                Price = createOfferDto.Price,
                Message = createOfferDto.Message,
                CreatedDate = DateTime.UtcNow,
                UserId = userCommon.Id,
                RequestId = createOfferDto.RequestId,
                Active = true,
                Accepted = false
            };

            await _context.Offers.AddAsync(newOffer);
            await _context.SaveChangesAsync();

            var offerDto = _mapper.Map<OfferDto>(newOffer);
            offerDto.User = _mapper.Map<UserCredentialsDto>(userCommon);

            var apiResponse = new ApiResponse<OfferDto>
            {
                Success = true,
                Message = "Offer created successfully.",
                Jwt = createOfferDto.Jwt,
                DeviceId = createOfferDto.DeviceId,
                Data = offerDto
            };

            return apiResponse;
        }


        public async Task<ApiResponse<OfferDto>> AcceptOffer(AcceptOfferDto acceptOfferDto)
        {
            var userCommon = await _context.UserCommons
                .Include(uc => uc.Requests)
                .ThenInclude(r => r.Offers)
                .FirstOrDefaultAsync(uc => uc.Jwt == acceptOfferDto.Jwt && uc.Devices.Any(d => d.DeviceId == acceptOfferDto.DeviceId));

            if (userCommon == null)
            {
                throw new Exception("User not found.");
            }

            var offer = await _context.Offers
                .Include(o => o.Request)
                .FirstOrDefaultAsync(o => o.Id == acceptOfferDto.OfferId);

            if (offer == null)
            {
                throw new Exception("Offer not found.");
            }

            var request = offer.Request;

            // Check if the user is the owner of the request
            if (request.UserCommonId != userCommon.Id)
            {
                throw new Exception("User is not the owner of the request.");
            }

            // Check if the offer is already accepted
            if (!offer.Active)
            {
                throw new Exception("Offer is already inactive.");
            }

            // Accept the offer
            offer.Accepted = true;
            offer.Active = false;
            _context.Offers.Update(offer);

            // Mark other offers as inactive
            var otherOffers = request.Offers.Where(o => o.Id != offer.Id);
            foreach (var otherOffer in otherOffers)
            {
                otherOffer.Active = false;
            }

            _context.Offers.UpdateRange(otherOffers);
            await _context.SaveChangesAsync();

            var offerDto = _mapper.Map<OfferDto>(offer);

            var apiResponse = new ApiResponse<OfferDto>
            {
                Success = true,
                Message = "Offer accepted successfully.",
                Jwt = acceptOfferDto.Jwt,
                DeviceId = acceptOfferDto.DeviceId,
                Data = offerDto
            };

            return apiResponse;
        }

        public async Task<ApiResponse<RequestDto>> GetRequestById(RequestIdDto requestIdDto)
        {
            var userCommon = await _context.UserCommons
                .FirstOrDefaultAsync(uc => uc.Jwt == requestIdDto.Jwt && uc.Devices.Any(d => d.DeviceId == requestIdDto.DeviceId));

            if (userCommon == null)
            {
                throw new Exception("User not found.");
            }

            var request = await _context.Requests
                .Include(r => r.Offers)  
                .FirstOrDefaultAsync(r => r.Id == requestIdDto.Id);

            if (request == null)
            {
                throw new Exception("Request not found.");
            }

            var requestDto = _mapper.Map<RequestDto>(request);  

            var apiResponse = new ApiResponse<RequestDto>
            {
                Success = true,
                Message = "Request fetched successfully.",
                Jwt = requestIdDto.Jwt,
                DeviceId = requestIdDto.DeviceId,
                Data = requestDto
            };

            return apiResponse;
        }


    }
}
