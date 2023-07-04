namespace AutoPartsServiceWebApi.Dto
{
    public class ServiceWithUserDto : ServiceDto
    {
        public string Name { get; set; }
        public decimal AverageScore { get; set; }
        public string UserName { get; set; }
    }
}
