using AutoPartsServiceWebApi.Data;
using AutoPartsServiceWebApi.Dto;
using AutoPartsServiceWebApi.Models;
using AutoPartsServiceWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace AutoPartsServiceWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RequestController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRequestService _requestService;

        public RequestController(IUserService userService, IRequestService requestService)
        {
            _userService = userService;
            _requestService = requestService;
        }

        [HttpPost("createRequest")]
        public async Task<ActionResult<ApiResponse<List<RequestDto>>>> CreateRequest(CreateRequestDto createRequestDto)
        {
            try
            {
                var apiResponse = await _requestService.CreateRequest(createRequestDto);
                return Ok(apiResponse);
            }
            catch (Exception e)
            {
                var apiResponse = new ApiResponse<List<RequestDto>>
                {
                    Success = false,
                    Message = e.Message,
                    Jwt = createRequestDto.Jwt,
                    DeviceId = createRequestDto.DeviceId,
                    Data = null
                };

                return Ok(apiResponse);
            }
        }

        [HttpPost("getActiveRequests")]
        public async Task<ActionResult<ApiResponse<List<RequestDto>>>> GetActiveRequests(UserJwtDeviceDto userJwtDevice)
        {
            try
            {
                var apiResponse = await _requestService.GetActiveRequests(userJwtDevice.Jwt, userJwtDevice.DeviceId);
                return Ok(apiResponse);
            }
            catch (Exception e)
            {
                var apiResponse = new ApiResponse<List<RequestDto>>
                {
                    Success = false,
                    Message = e.Message,
                    Jwt = userJwtDevice.Jwt,
                    DeviceId = userJwtDevice.DeviceId,
                    Data = null
                };

                return Ok(apiResponse);
            }
        }

        [HttpPost("createOffer")]
        public async Task<ActionResult<ApiResponse<OfferDto>>> CreateOffer(CreateOfferDto createOfferDto)
        {
            try
            {
                var apiResponse = await _requestService.CreateOffer(createOfferDto);
                return Ok(apiResponse);
            }
            catch (Exception e)
            {
                var apiResponse = new ApiResponse<OfferDto>
                {
                    Success = false,
                    Message = e.Message,
                    Jwt = createOfferDto.Jwt,
                    DeviceId = createOfferDto.DeviceId,
                    Data = null
                };

                return Ok(apiResponse);
            }
        }

        [HttpPost("acceptOffer")]
        public async Task<ActionResult<ApiResponse<OfferDto>>> AcceptOffer(AcceptOfferDto acceptOfferDto)
        {
            try
            {
                var apiResponse = await _requestService.AcceptOffer(acceptOfferDto);
                return Ok(apiResponse);
            }
            catch (Exception e)
            {
                var apiResponse = new ApiResponse<OfferDto>
                {
                    Success = false,
                    Message = e.Message,
                    Jwt = acceptOfferDto.Jwt,
                    DeviceId = acceptOfferDto.DeviceId,
                    Data = null
                };

                return Ok(apiResponse);
            }
        }

        [HttpPost("getAcceptedRequests")]
        public async Task<ActionResult<ApiResponse<List<RequestDto>>>> GetAcceptedRequests(UserJwtDeviceDto userJwtDevice)
        {
            try
            {
                var apiResponse = await _requestService.GetAcceptedRequests(userJwtDevice.Jwt, userJwtDevice.DeviceId);
                return Ok(apiResponse);
            }
            catch (Exception e)
            {
                var apiResponse = new ApiResponse<List<RequestDto>>
                {
                    Success = false,
                    Message = e.Message,
                    Jwt = userJwtDevice.Jwt,
                    DeviceId = userJwtDevice.DeviceId,
                    Data = null
                };

                return Ok(apiResponse);
            }
        }

        [HttpPost("getRequestById")]
        public async Task<ActionResult<ApiResponse<RequestDto>>> GetRequestById(RequestIdDto requestIdDto)
        {
            try
            {
                var apiResponse = await _requestService.GetRequestById(requestIdDto);
                return Ok(apiResponse);
            }
            catch (Exception e)
            {
                var apiResponse = new ApiResponse<RequestDto>
                {
                    Success = false,
                    Message = e.Message,
                    Jwt = requestIdDto.Jwt,
                    DeviceId = requestIdDto.DeviceId,
                    Data = null
                };

                return Ok(apiResponse);
            }
        }

        [HttpPost("closeRequest")]
        public async Task<ActionResult<ApiResponse<RequestDto>>> CloseRequest(CloseRequestDto closeRequestDto)
        {
            try
            {
                var response = await _requestService.CloseRequest(closeRequestDto);
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(new ApiResponse<RequestDto>
                {
                    Success = false,
                    Message = e.Message,
                    Jwt = closeRequestDto.Jwt,
                    DeviceId = closeRequestDto.DeviceId,
                    Data = null
                });
            }
        }

    }
}
