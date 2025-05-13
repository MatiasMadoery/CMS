using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Control_Machine_Sistem.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Control_Machine_Sistem.Controllers
{
    public class ServicesController : Controller
    {
        private readonly AppDbContext _context;

        public ServicesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Services?machineId=...
        public async Task<IActionResult> Index(int? machineId)
        {
            IQueryable<Service> query = _context.Services.Include(s => s.Machine);
            
            if (machineId.HasValue)
            {
                query = query.Where(s => s.MachineId == machineId.Value);
                ViewData["MachineId"] = machineId.Value;
            }

            var services = await query.ToListAsync();
            return View(services);
        }

        // GET: Services/Details/5?machineId=...
        public async Task<IActionResult> Details(int? id, int? machineId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.Machine)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (service == null)
            {
                return NotFound();
            }
            
            ViewData["MachineId"] = machineId;
            return View(service);
        }

        // GET: Services/Create?machineId=...
        public IActionResult Create(int? machineId)
        {            
            var service = new Service();
            if (machineId.HasValue)
            {
                service.MachineId = machineId.Value;
            }
            
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "ChasisNumber", service.MachineId);
            return View(service);
        }

        // POST: Services/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MachineId,WorkHours,ServiceDate,OrderNumber,Observations")] Service service, int? machineId)
        {
            if (ModelState.IsValid)
            {
                _context.Add(service);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index), new { machineId = service.MachineId });
            }
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "ChasisNumber", service.MachineId);
            return View(service);
        }

        // GET: Services/Edit/5?machineId=...
        public async Task<IActionResult> Edit(int? id, int? machineId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "ChasisNumber", service.MachineId);
            ViewData["MachineContext"] = machineId; 
            return View(service);
        }

        // POST: Services/Edit/5?machineId=...
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MachineId,WorkHours,ServiceDate,OrderNumber,Observations")] Service service, int? machineId)
        {
            if (id != service.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }                
                return RedirectToAction(nameof(Index), new { machineId = service.MachineId });
            }
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "ChasisNumber", service.MachineId);
            return View(service);
        }

        // GET: Services/Delete/5?machineId=...
        public async Task<IActionResult> Delete(int? id, int? machineId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.Machine)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (service == null)
            {
                return NotFound();
            }
            
            ViewData["MachineId"] = machineId;
            return View(service);
        }

        // POST: Services/Delete/5?machineId=...
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int? machineId)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
            }

            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index), new { machineId = machineId });
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.Id == id);
        }
    }
}
