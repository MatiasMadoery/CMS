using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Control_Machine_Sistem.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Control_Machine_Sistem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OtherMaintenancesController : Controller
    {
        private readonly AppDbContext _context;

        public OtherMaintenancesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: OtherMaintenances?machineId=...
        public async Task<IActionResult> Index(int? machineId)
        {
            IQueryable<OtherMaintenance> query = _context.OtherMaintenances.Include(o => o.Machine);
                        
            if (machineId.HasValue)
            {
                query = query.Where(o => o.MachineId == machineId.Value);
                ViewData["MachineId"] = machineId.Value;
            }

            var list = await query.ToListAsync();
            return View(list);
        }

        // GET: OtherMaintenances/Details/5?machineId=...
        public async Task<IActionResult> Details(int? id, int? machineId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var otherMaintenance = await _context.OtherMaintenances
                .Include(o => o.Machine)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (otherMaintenance == null)
            {
                return NotFound();
            }
           
            ViewData["MachineId"] = machineId;
            return View(otherMaintenance);
        }

        // GET: OtherMaintenances/Create?machineId=...
        public IActionResult Create(int? machineId)
        {
            var otherMaintenance = new OtherMaintenance();
            if (machineId.HasValue)
            {                
                otherMaintenance.MachineId = machineId.Value;
            }
          
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Id", otherMaintenance.MachineId);
            return View(otherMaintenance);
        }

        // POST: OtherMaintenances/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MachineId,WorkHours,ServiceDate,OrderNumber,Observations")] OtherMaintenance otherMaintenance, int? machineId)
        {
            if (ModelState.IsValid)
            {
                _context.Add(otherMaintenance);
                await _context.SaveChangesAsync();              
                return RedirectToAction(nameof(Index), new { machineId = otherMaintenance.MachineId });
            }
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Id", otherMaintenance.MachineId);
            return View(otherMaintenance);
        }

        // GET: OtherMaintenances/Edit/5?machineId=...
        public async Task<IActionResult> Edit(int? id, int? machineId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var otherMaintenance = await _context.OtherMaintenances.FindAsync(id);
            if (otherMaintenance == null)
            {
                return NotFound();
            }
            
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Id", otherMaintenance.MachineId);           
            ViewData["MachineContext"] = machineId;
            return View(otherMaintenance);
        }

        // POST: OtherMaintenances/Edit/5?machineId=...
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MachineId,WorkHours,ServiceDate,OrderNumber,Observations")] OtherMaintenance otherMaintenance, int? machineId)
        {
            if (id != otherMaintenance.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(otherMaintenance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OtherMaintenanceExists(otherMaintenance.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
                return RedirectToAction(nameof(Index), new { machineId = otherMaintenance.MachineId });
            }
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Id", otherMaintenance.MachineId);
            return View(otherMaintenance);
        }

        // GET: OtherMaintenances/Delete/5?machineId=...
        public async Task<IActionResult> Delete(int? id, int? machineId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var otherMaintenance = await _context.OtherMaintenances
                .Include(o => o.Machine)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (otherMaintenance == null)
            {
                return NotFound();
            }

            ViewData["MachineId"] = machineId;
            return View(otherMaintenance);
        }

        // POST: OtherMaintenances/Delete/5?machineId=...
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int? machineId)
        {
            var otherMaintenance = await _context.OtherMaintenances.FindAsync(id);
            if (otherMaintenance != null)
            {
                _context.OtherMaintenances.Remove(otherMaintenance);
            }

            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index), new { machineId = machineId });
        }

        private bool OtherMaintenanceExists(int id)
        {
            return _context.OtherMaintenances.Any(e => e.Id == id);
        }
    }
}
