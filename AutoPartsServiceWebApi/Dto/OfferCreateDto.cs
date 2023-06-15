namespace AutoPartsServiceWebApi.Dto
{
    public class OfferCreateDto
    {
        public int UserId { get; set; }
        public string Message { get; set; }
        public decimal CounterPrice { get; set; }
    }

}
