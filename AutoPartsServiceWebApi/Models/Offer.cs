﻿namespace AutoPartsServiceWebApi.Models
{
    public class Offer
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UserId { get; set; }
        public UserCommon User { get; set; }
        public int RequestId { get; set; }
        public Request Request { get; set; }
        public bool Active { get; set; }
        public bool Accepted { get; set; }
    }

}
