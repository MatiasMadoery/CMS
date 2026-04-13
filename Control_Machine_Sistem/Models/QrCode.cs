namespace Control_Machine_Sistem.Models
{
    public class QrCode
    {
        public string? ClientName { get; set; }
        public string? MachineModel { get; set; }
        public string? ManualUrl { get; set; }
        public string? DocUrl { get; set; }
        public string? SpareKitsUrl { get; set; }
        public string? ServiceUrl { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public int MachineId { get; set; }
        public string? ChasisNumber { get; set; }

        // Properties for printing
        public string? QrImageBase64 { get; set; }

        public string? QrContentUrl { get; set; }
    }
}