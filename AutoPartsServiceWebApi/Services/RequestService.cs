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
                .FirstOrDefaultAsync(uc => uc.Jwt == createRequestDto.Jwt && uc.Devices.Any(d => d.DeviceId == createRequestDto.DeviceId));

            if (userCommon == null)
            {
                throw new Exception("User not found.");
            }

            var newRequest = _mapper.Map<Request>(createRequestDto);
            newRequest.Active = true;
            newRequest.CreationDate = DateTime.Now;

            // Check if Requests is null
            if (userCommon.Requests == null)
            {
                userCommon.Requests = new List<Request>();
            }

            userCommon.Requests.Add(newRequest);
            await _context.SaveChangesAsync();

            var activeRequests = await _context.Requests
                .Include(r => r.Offers)
                .Where(r => r.UserCommonId == userCommon.Id && r.Active)
                .OrderByDescending(r => r.CreationDate)
                .ToListAsync();

            var activeRequestsDto = activeRequests.Select(ar => {
                var dto = _mapper.Map<RequestDto>(ar);
                dto.Close = ar.Offers.Any(o => o.Accepted);
                return dto;
            }).ToList();

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

        public async Task<ApiResponse<List<RequestDto>>> GetActiveRequests(string jwt, string deviceId)
        {
            try
            {
                var userCommon = await _context.UserCommons
                    .Include(uc => uc.Devices)
                    .FirstOrDefaultAsync(uc => uc.Jwt == jwt && uc.Devices.Any(d => d.DeviceId == deviceId));

                if (userCommon == null)
                {
                    return new ApiResponse<List<RequestDto>>
                    {
                        Success = false,
                        Message = "User not found.",
                        Jwt = jwt,
                        DeviceId = deviceId,
                        Data = null
                    };
                }

                var activeRequests = await _context.Requests
                    .Include(r => r.Offers)
                    .Where(r => r.Active
                        && (r.Offers == null || !r.Offers.Any(o => o.Accepted)))
                    .OrderByDescending(r => r.CreationDate)
                    .ToListAsync();

                var requestDtos = activeRequests.Select(ar => {
                    var dto = _mapper.Map<RequestDto>(ar);
                    dto.Close = ar.Offers.Any(o => o.Accepted);
                    return dto;
                }).ToList();

                return new ApiResponse<List<RequestDto>>
                {
                    Success = true,
                    Message = "Active requests retrieved successfully.",
                    Jwt = jwt,
                    DeviceId = deviceId,
                    Data = requestDtos
                };
            }
            catch (Exception e)
            {
                return new ApiResponse<List<RequestDto>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving active requests: {e.Message}",
                    Jwt = jwt,
                    DeviceId = deviceId,
                    Data = null
                };
            }
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

        public async Task<ApiResponse<List<RequestDto>>> GetAcceptedRequests(string jwt, string deviceId)
        {
            try
            {
                var userCommon = await _context.UserCommons
                    .Include(uc => uc.Devices)
                    .FirstOrDefaultAsync(uc => uc.Jwt == jwt && uc.Devices.Any(d => d.DeviceId == deviceId));

                if (userCommon == null)
                {
                    return new ApiResponse<List<RequestDto>>
                    {
                        Success = false,
                        Message = "User not found.",
                        Jwt = jwt,
                        DeviceId = deviceId,
                        Data = null
                    };
                }

                var allRequests = await _context.Requests
                    .Include(r => r.Offers)
                    .Where(r => r.Offers.Any(o => o.Accepted))
                    .OrderByDescending(r => r.CreationDate)
                    .ToListAsync();

                var requestDtos = allRequests.Select(ar => {
                    var dto = _mapper.Map<RequestDto>(ar);
                    dto.Close = ar.Offers.Any(o => o.Accepted);
                    return dto;
                }).ToList();

                return new ApiResponse<List<RequestDto>>
                {
                    Success = true,
                    Message = "Accepted requests retrieved successfully.",
                    Jwt = jwt,
                    DeviceId = deviceId,
                    Data = requestDtos
                };
            }
            catch (Exception e)
            {
                return new ApiResponse<List<RequestDto>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving accepted requests: {e.Message}",
                    Jwt = jwt,
                    DeviceId = deviceId,
                    Data = null
                };
            }
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
            requestDto.Close = request.Offers.Any(o => o.Accepted);

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

        public async Task<ApiResponse<RequestDto>> CloseRequest(CloseRequestDto closeRequestDto)
        {
            var userCommon = await _context.UserCommons
                .Include(uc => uc.Requests)
                .FirstOrDefaultAsync(uc => uc.Jwt == closeRequestDto.Jwt && uc.Devices.Any(d => d.DeviceId == closeRequestDto.DeviceId));

            if (userCommon == null)
            {
                throw new Exception("User not found.");
            }

            var request = await _context.Requests
                .Include(r => r.Offers)
                .FirstOrDefaultAsync(r => r.Id == closeRequestDto.RequestId);

            if (request == null)
            {
                throw new Exception("Request not found.");
            }

            // Check if the user is the owner of the request or has an accepted offer
            if (request.UserCommonId != userCommon.Id && !request.Offers.Any(o => o.Accepted && o.UserId == userCommon.Id))
            {
                throw new Exception("User is not the owner of the request and does not have an accepted offer.");
            }

            // Check if the request is already inactive
            if (!request.Active)
            {
                throw new Exception("Request is already inactive.");
            }

            // Find the accepted offer
            var acceptedOffer = request.Offers.FirstOrDefault(o => o.Accepted);

            if (acceptedOffer != null)
            {
                // Set the accepted offer's active status to false
                acceptedOffer.Active = false;
                _context.Offers.Update(acceptedOffer);
            }

            // Close the request
            request.Active = false;
            _context.Requests.Update(request);
            await _context.SaveChangesAsync();

            var requestDto = _mapper.Map<RequestDto>(request);

            var apiResponse = new ApiResponse<RequestDto>
            {
                Success = true,
                Message = "Request closed successfully.",
                Jwt = closeRequestDto.Jwt,
                DeviceId = closeRequestDto.DeviceId,
                Data = requestDto
            };

            return apiResponse;
        }

        public async Task<ApiResponse<List<RequestDto>>> GetMyCreatedRequests(string jwt, string deviceId)
        {
            try
            {
                var userCommon = await _context.UserCommons
                    .Include(uc => uc.Requests)
                    .ThenInclude(r => r.Offers)
                    .FirstOrDefaultAsync(uc => uc.Jwt == jwt && uc.Devices.Any(d => d.DeviceId == deviceId));

                if (userCommon == null)
                {
                    return new ApiResponse<List<RequestDto>>
                    {
                        Success = false,
                        Message = "User not found.",
                        Jwt = jwt,
                        DeviceId = deviceId,
                        Data = null
                    };
                }

                var activeRequests = userCommon.Requests
                    .Where(r => r.Active)
                    .OrderByDescending(r => r.CreationDate)
                    .ToList();

                var inactiveRequests = userCommon.Requests
                    .Where(r => !r.Active)
                    .OrderByDescending(r => r.CreationDate)
                    .ToList();

                var allRequests = activeRequests.Concat(inactiveRequests).ToList();

                var requestDtos = allRequests.Select(ar => {
                    var dto = _mapper.Map<RequestDto>(ar);
                    dto.Close = ar.Offers.Any(o => o.Accepted);
                    return dto;
                }).ToList();

                return new ApiResponse<List<RequestDto>>
                {
                    Success = true,
                    Message = "User's created requests retrieved successfully.",
                    Jwt = jwt,
                    DeviceId = deviceId,
                    Data = requestDtos
                };
            }
            catch (Exception e)
            {
                return new ApiResponse<List<RequestDto>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving user's created requests: {e.Message}",
                    Jwt = jwt,
                    DeviceId = deviceId,
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<RequestDto>> DeleteRequest(RequestIdDto requestIdDto)
        {
            var userCommon = await _context.UserCommons
                .Include(uc => uc.Requests)
                .FirstOrDefaultAsync(uc => uc.Jwt == requestIdDto.Jwt && uc.Devices.Any(d => d.DeviceId == requestIdDto.DeviceId));

            if (userCommon == null)
            {
                throw new Exception("User not found.");
            }

            var request = userCommon.Requests.FirstOrDefault(r => r.Id == requestIdDto.Id);

            if (request == null)
            {
                throw new Exception("Request not found.");
            }

            _context.Requests.Remove(request);
            await _context.SaveChangesAsync();

            var requestDto = _mapper.Map<RequestDto>(request);

            var apiResponse = new ApiResponse<RequestDto>
            {
                Success = true,
                Message = "Request deleted successfully.",
                Jwt = requestIdDto.Jwt,
                DeviceId = requestIdDto.DeviceId,
                Data = null
            };

            return apiResponse;
        }

    }
}
