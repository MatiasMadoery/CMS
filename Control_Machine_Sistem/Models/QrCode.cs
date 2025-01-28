namespace Control_Machine_Sistem.Models
{
    public class QrCode
    {
        public string ClientName { get; set; }
        public string MachineModel { get; set; }
        public string UserManualUrl { get; set; }
        public string MachineManualUrl { get; set; }
        public string ServiceDataUrl { get; set; }

        // Properties for printing
        public string QrImageBase64 { get; set; }
        public string QrContentUrl { get; set; }
    }
}
