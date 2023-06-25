﻿namespace AutoPartsServiceWebApi.Dto
{
    public class ServiceDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string Category { get; set; }
        public string Avatar { get; set; }
        public decimal AverageScore { get; set; }
    }
}
