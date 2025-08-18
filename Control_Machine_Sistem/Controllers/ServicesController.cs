using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Control_Machine_Sistem.Models;
using Control_Machine_Sistem.Services;
using Microsoft.AspNetCore.Authorization;

namespace Control_Machine_Sistem.Controllers
{
    [Authorize(Roles = "Admin")]
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

        // GET: Services/Create?machineId=...&serviceHour=...
        public IActionResult Create(int? machineId, int? serviceHour)
        {
            var service = new Service();

            if (machineId.HasValue)
                service.MachineId = machineId.Value;

            if (serviceHour.HasValue)
                service.ServiceHour = serviceHour.Value;

            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "ChasisNumber", service.MachineId);
            return View(service);
        }


        // POST: Services/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MachineId,WorkHours,RealHours,ServiceHour,ServiceDate,OrderNumber,Observations,ServiceSheet")] Service service, int? machineId)
        {
            if (ModelState.IsValid)
            {
                List<string> serviceSheetUrls = new List<string>();
                if (service.ServiceSheet != null && service.ServiceSheet.Any())
                {
                    foreach (var sheet in service.ServiceSheet)
                    {
                        var fileExtension = Path.GetExtension(sheet.FileName).ToLower();
                        if (fileExtension != ".pdf")
                        {
                            ModelState.AddModelError("ServiceSheet", "Solo se permiten archivos PDF.");
                            return View(service);
                        }
                    }
                    serviceSheetUrls = await FileService.SaveServiceSheetAsync((List<IFormFile>)service.ServiceSheet);
                }


                var newService = new Service
                {
                    MachineId = service.MachineId,
                    WorkHours = service.WorkHours,
                    ServiceHour = service.ServiceHour,
                    ServiceDate = service.ServiceDate,
                    OrderNumber = service.OrderNumber,
                    Observations = service.Observations,
                    ServiceSheetUrls = serviceSheetUrls
                };
                _context.Add(newService);
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

            ViewData["MachineId"] = machineId ?? service.MachineId;
            return View(service);
        }

        // POST: Services/Edit/5?machineId=...
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,MachineId,WorkHours,ServiceDate,OrderNumber,Observations,ServiceSheet")] Service service,
            int? machineId,
            string[] DeletedServiceSheetUrls)
        {
            if (id != service.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {                   
                    var serviceToUpdate = await _context.Services.FindAsync(id);
                    if (serviceToUpdate == null)
                    {
                        return NotFound();
                    }
                   
                    serviceToUpdate.WorkHours = service.WorkHours;
                    serviceToUpdate.ServiceDate = service.ServiceDate;
                    serviceToUpdate.OrderNumber = service.OrderNumber;
                    serviceToUpdate.Observations = service.Observations;
                    
                    if (DeletedServiceSheetUrls != null && DeletedServiceSheetUrls.Any())
                    {
                        foreach (var url in DeletedServiceSheetUrls)
                        {
                            // Elimina de la lista la URL seleccionada
                            serviceToUpdate.ServiceSheetUrls.Remove(url);
                            // Elimina el archivo fisico
                            await FileService.DeleteServiceSheetAsync(url);
                        }
                    }
                    
                    if (service.ServiceSheet != null && service.ServiceSheet.Any())
                    {                        
                        foreach (var file in service.ServiceSheet)
                        {
                            var ext = Path.GetExtension(file.FileName).ToLower();
                            if (ext != ".pdf")
                            {
                                ModelState.AddModelError("ServiceSheet", "Solo se permiten archivos PDF.");
                                return View(service);
                            }
                        }                       
                        var newFileUrls = await FileService.SaveServiceSheetAsync(service.ServiceSheet.ToList());
                        
                        serviceToUpdate.ServiceSheetUrls.AddRange(newFileUrls);
                    }

                    _context.Update(serviceToUpdate);
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
                if (service.ServiceSheetUrls != null && service.ServiceSheetUrls.Any())
                {
                    foreach (var fileUrl in service.ServiceSheetUrls)
                    {                        
                        await FileService.DeleteServiceSheetAsync(fileUrl);
                    }
                }                
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
