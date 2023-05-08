using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPartsServiceWebApi.Models
{
    public class Sms
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public string PhoneNumber { get; set; }

        public string Code { get; set; }

        public DateTime ExpirationDate { get; set; }

        public User User { get; set; } 
    }
}
