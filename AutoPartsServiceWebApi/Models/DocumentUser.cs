namespace AutoPartsServiceWebApi.Models
{
    public class DocumentUser
    {
        public int Id { get; set; }
        public DocumentType DocumentType { get; set; }  
        public string CertificateNumber { get; set; }
        public string StateNumber { get; set; }
        public string DocumentNumber { get; set; }
        public string UinAccruals { get; set; }
        public int UserCommonId { get; set; }  
        public UserCommon UserCommon { get; set; }  
    }

}
