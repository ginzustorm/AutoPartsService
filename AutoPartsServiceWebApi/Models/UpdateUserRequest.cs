﻿ namespace AutoPartsServiceWebApi.Models
{
    public class UpdateUserRequest
    {
        public string Jwt { get; set; }
        public string DeviceId { get; set; }
        public RegisterRequest Data { get; set; }
    }

    public class AddressDto
    {
        public string? Country { get; set; }
        public string? Region { get; set; }
        public string City { get; set; }
        public string? Street { get; set; }
    }
}
