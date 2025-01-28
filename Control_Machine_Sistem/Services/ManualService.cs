namespace Control_Machine_Sistem.Services
{
    public class ManualService
    {
        private readonly IWebHostEnvironment _env;

        public ManualService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string GetUserManualUrl()
        {
            return "/manuals/user/ManualUsuario.pdf";
        }

        public string GetMachineManualUrl(string machineName)
        {
            return $"/manuals/machines/{machineName}/ManualMaquina{machineName}.pdf";
        }

        public string GetServiceManualUrl(string machineName)
        {
            return $"/manuals/service/{machineName}/ManualService{machineName}.pdf";
        }
    }
}
