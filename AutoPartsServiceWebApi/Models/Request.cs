using System;

namespace AutoPartsServiceWebApi.Models
{
    public class Request
    {
        public int Id { get; set; }
        public DateTime CreationDate { get; set; }
        public string Description { get; set; }
        public string Header { get; set; }
        public decimal Price { get; set; }
        public bool Active { get; set; }
        public int UserCommonId { get; set; }
        public UserCommon UserCommon { get; set; }
        public virtual ICollection<Offer> Offers { get; set; }

    }

}
