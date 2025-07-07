using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Control_Machine_Sistem.Models;
using Control_Machine_Sistem.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Control_Machine_Sistem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MaintenanceController : Controller
    {
        private readonly AppDbContext _context;

        public MaintenanceController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Maintenance/Index        
        public async Task<IActionResult> Index(string searchString, int page = 1, int pageSize = 5)
        {
            var machines = _context.Machines
                .Include(m => m.Customer)
                .Include(m => m.Model)
                .AsQueryable(); 
            
            if (!String.IsNullOrEmpty(searchString))
            {
                machines = machines.Where(m =>
                    m.Customer!.Name!.Contains(searchString) ||
                     m.Customer!.LastName!.Contains(searchString) ||
                    m.Model!.Name!.Contains(searchString));
            }
            
            var totalMachines = await machines.CountAsync();
            
            var pagedMachines = await machines
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Convertir a ViewModel
            var viewModel = pagedMachines.Select(m => new MaintenanceIndexViewModel
            {
                MachineId = m.Id,
                MachineModel = m.Model?.Name,
                CustomerName = m.Customer?.Name
            }).ToList();
           
            var pager = new Pager<MaintenanceIndexViewModel>(viewModel, totalMachines, page, pageSize);
            
            ViewData["searchString"] = searchString;

            return View(pager);
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
