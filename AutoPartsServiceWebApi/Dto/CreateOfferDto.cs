namespace AutoPartsServiceWebApi.Dto
{
    public class CreateOfferDto
    {
        public decimal Price { get; set; }
        public string Message { get; set; }
        public string Jwt { get; set; }
        public string DeviceId { get; set; }
        public int RequestId { get; set; }
    }

}
