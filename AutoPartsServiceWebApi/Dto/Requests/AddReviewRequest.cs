namespace AutoPartsServiceWebApi.Dto.Requests
{
    public class AddReviewRequest
    {
        public string DeviceId { get; set; }
        public string Jwt { get; set; }
        public int ServiceId { get; set; }
        public ReviewDto Data { get; set; }
    }
}
