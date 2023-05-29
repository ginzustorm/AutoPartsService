namespace AutoPartsServiceWebApi.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }

        public int UserBusinessId { get; set; }
        public UserBusiness UserBusiness { get; set; }
    }
}
