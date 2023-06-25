namespace AutoPartsServiceWebApi.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ServiceId { get; set; }
        public Service Service { get; set; }
    }

}
