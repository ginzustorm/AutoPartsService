namespace AutoPartsServiceWebApi.Dto
{
    public class OfferDto
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UserId { get; set; }
        public UserCredentialsDto User { get; set; } 
        public int RequestId { get; set; }
        public bool Active { get; set; }
        public bool Accepted { get; set; }
    }


}
