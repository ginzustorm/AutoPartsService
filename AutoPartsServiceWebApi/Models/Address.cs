﻿namespace AutoPartsServiceWebApi.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string? Country { get; set; }
        public string? Region { get; set; }
        public string City { get; set; }
        public string? Street { get; set; }
        public int? UserCommonId { get; set; } 
        public UserCommon UserCommon { get; set; } 
    }
}
