namespace Control_Machine_Sistem.ViewModels
{
    public class MaintenanceIndexViewModel
    {
        //Machine Data
        public int MachineId { get; set; }
        public string? MachineModel { get; set; }
        public string? MachineCategory { get; set; }
        public string? ChasisNumber { get; set; }
        public string? EngineNumber { get; set; }

        //Customer Data
        public string? CustomerName { get; set; }
    }
}
