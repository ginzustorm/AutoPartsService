﻿namespace AutoPartsServiceWebApi.Dto
{
    public class RequestDto
    {
        public int Id { get; set; }
        public DateTime CreationDate { get; set; }
        public string Description { get; set; }
        public string Header { get; set; }
        public decimal Price { get; set; }
        public bool Active { get; set; }
        // public string Category { get; set; }
        public List<OfferDto> Offers { get; set; }
        public bool Close { get; set; }
    }
}
