namespace AutoPartsServiceWebApi.Models
{
    public class Offer
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public decimal CounterPrice { get; set; }
        public int UserId { get; set; }
        public UserCommon UserCommon { get; set; }
        public int RequestId { get; set; }
        public Request Request { get; set; }
    }
}
