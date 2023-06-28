namespace AutoPartsServiceWebApi.Dto
{
    public class AcceptOfferDto
    {
        public string Jwt { get; set; }
        public string DeviceId { get; set; }
        public int OfferId { get; set; }
    }
}
