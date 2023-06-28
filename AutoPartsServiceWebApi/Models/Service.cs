namespace AutoPartsServiceWebApi.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public string Avatar { get; set; }
        public double AverageScore { get; set; }
        public int UserCommonId { get; set; }
        public UserCommon UserCommon { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}
