namespace Control_Machine_Sistem.Models
{
    public class QrCode
    {
        public string? ClientName { get; set; }
        public string? MachineModel { get; set; }
        public string? ManualUrl { get; set; }

        // Properties for printing
        public string? QrImageBase64 { get; set; }
        public string? QrContentUrl { get; set; }
    }
}
