namespace AutoPartsServiceWebApi.Models
{
    public enum DocumentType
    {
        RegistrationCertificate,
        DriversLicense,
        ResolutionNumber
    }

    public class DocumentCheck
    {
        public DocumentType DocumentType { get; set; }

        // For Registration Certificate
        public string CertificateNumber { get; set; }
        public string StateNumber { get; set; }

        // For Drivers License
        public string DocumentNumber { get; set; }

        // For Resolution Number
        public string UinAccruals { get; set; }
    }

}
