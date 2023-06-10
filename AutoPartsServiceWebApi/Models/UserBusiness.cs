namespace AutoPartsServiceWebApi.Models
{
    public class UserBusiness
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime RegistrationDate { get; set; }
        public List<Device> Devices { get; set; }
        public string Password { get; set; }
        public List<Sms> SmsList { get; set; }
        //public Service Service { get; set; }
        public List<Review> Reviews { get; set; }
        public int Rating { get; set; }
        public List<Service> Services { get; set; }
        public string Avatar { get; set; }
    }
}
