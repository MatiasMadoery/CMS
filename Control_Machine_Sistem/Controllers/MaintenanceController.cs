using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Control_Machine_Sistem.Models;
using Control_Machine_Sistem.ViewModels;

namespace Control_Machine_Sistem.Controllers
{
    public class MaintenanceController : Controller
    {
        private readonly AppDbContext _context;

        public MaintenanceController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Maintenance/Index        
        public async Task<IActionResult> Index()
        {
            var machines = await _context.Machines
                .Include(m => m.Customer)
                .Include(m => m.Model)
                .ToListAsync();
            
            var viewModel = machines.Select(m => new MaintenanceIndexViewModel
            {
                MachineId = m.Id,
                MachineModel = m.Model?.Name,               
                CustomerName = m.Customer?.Name
            }).ToList();

            return View(viewModel);
        }

        // GET: /Maintenance/MachineServices?machineId=5       
        public async Task<IActionResult> MachineServices(int machineId)
        {
            var machine = await _context.Machines
                .Include(m => m.Services)
                .FirstOrDefaultAsync(m => m.Id == machineId);

            if (machine == null)
            {
                return NotFound();
            }
          
            return View(machine);
        }

        // GET: /Maintenance/MachineMaintenances?machineId=5        
        public async Task<IActionResult> MachineMaintenances(int machineId)
        {
            var machine = await _context.Machines
                .Include(m => m.OtherMaintenances)
                .FirstOrDefaultAsync(m => m.Id == machineId);

            if (machine == null)
            {
                return NotFound();
            }
            
            return View(machine);
        }
    }
}
