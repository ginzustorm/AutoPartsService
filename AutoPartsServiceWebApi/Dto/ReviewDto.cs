namespace AutoPartsServiceWebApi.Dto
{
    public class ReviewDto
    {
        public int Rating { get; set; }
        public string Comment { get; set; }
        public UserCommonDto User { get; set; }
    }

}
