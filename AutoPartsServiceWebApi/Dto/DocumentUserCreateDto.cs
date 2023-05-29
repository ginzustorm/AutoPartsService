using AutoPartsServiceWebApi.Models;

namespace AutoPartsServiceWebApi.Dto
{
    public class DocumentUserCreateDto
    {
        public DocumentType DocumentType { get; set; }
        public string CertificateNumber { get; set; }
        public string StateNumber { get; set; }
        public string DocumentNumber { get; set; }
        public string UinAccruals { get; set; }
    }
}
