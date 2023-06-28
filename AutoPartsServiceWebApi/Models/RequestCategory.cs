namespace AutoPartsServiceWebApi.Models
{
    public class RequestCategory
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public virtual UserCommon UserCommon { get; set; }
        public int UserCommonId { get; set; }
    }
}
