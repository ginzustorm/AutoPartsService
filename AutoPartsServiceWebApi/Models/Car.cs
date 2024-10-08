﻿namespace AutoPartsServiceWebApi.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Mark { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public string StateNumber { get; set; }
        public string VinNumber { get; set; }

        public int UserCommonId { get; set; }
        public UserCommon UserCommon { get; set; }
    }
}
