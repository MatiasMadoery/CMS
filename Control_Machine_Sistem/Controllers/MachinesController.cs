using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Control_Machine_Sistem.Models;
using Control_Machine_Sistem.Services;
using Microsoft.AspNetCore.Authorization;

namespace Control_Machine_Sistem.Controllers
{
    [Authorize(Roles = "Admin, Técnico, SuperAdmin,Viewer")]
    public class MachinesController : Controller
    {
        private readonly AppDbContext _context;

        public MachinesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Machines
        [Authorize(Roles = "Admin, Técnico, SuperAdmin,Viewer")]
        public async Task<IActionResult> Index(string searchString,int? categoryId, int page = 1, int pageSize = 5)
        {
            var machine = _context.Machines!
                          .Include(m => m.Customer)
                          .Include(m => m.Model)
                          .OrderBy(m => m.Id)
                          .AsQueryable();

            //Filter by search text if provided
            if (!String.IsNullOrEmpty(searchString))
            {
                machine = machine.Where(s => s.Customer!.Name!.Contains(searchString) || s.Customer.LastName!.Contains(searchString) ||
                s.Model!.Name!.Contains(searchString));
            }

            //Filter by category
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                machine = machine.Where(s => s.Model!.CategoryId == categoryId.Value);
            }

            var totalMachines = await machine.CountAsync();

            var machinePager = await machine
                           .Skip((page - 1) * pageSize)
                           .Take(pageSize)
                           .ToListAsync();

            var pager = new Pager<Machine>(machinePager, totalMachines, page, pageSize);

            ViewData["searchString"] = searchString;
            ViewData["categoryId"] = categoryId;

            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id" , "Name", categoryId);

            return View(pager);
        }

        // GET: Machines/Details/5
        [Authorize(Roles = "Admin, Técnico, SuperAdmin,Viewer")]
        public async Task<IActionResult> Details(int? id, bool isStock = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _context.Machines
                .Include(m => m.Customer)
                .Include(m => m.Model)
                .Include(m => m.OwnerHistories)
                .Include(m => m.Ubication)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (machine == null)
            {
                return NotFound();
            }

            ViewBag.IsStock = isStock;
            return View(machine);
        }

        // GET: Machines/Create
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        public IActionResult Create(bool isStock = false)
        {
            ViewBag.IsStock = isStock;
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.ModelId = new SelectList(new List<Model>(), "Id", "Name");
            ViewBag.Customers = new SelectList(_context.Customers.Select(c => new { c.Id, c.Name}), "Id", "Name");
            ViewBag.UbicationId = new SelectList(_context.Ubications.OrderBy(u => u.Name), "Id", "Name");

            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetModelsByCategory(int categoryId)
        {
            var models = await _context.Models
                                       .Where(m => m.CategoryId == categoryId)
                                       .Select(m => new { id = m.Id, name = m.Name })
                                       .ToListAsync();

            return Json(models);
        }


        // POST: Machines/Create
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(104857600)]
        public async Task<IActionResult> Create([Bind("Id,CustomerId,ModelId,ChasisNumber,EngineNumber,DeliveryDate,Documentations,ManufactureYear,SerialNumber,UserHours,UbicationId")] Machine machine, bool isStock = false)
        {
            if (isStock)
            {
                machine.CustomerId = null;
                machine.DeliveryDate = null;
                machine.WarrantyExpirationDate = null;


                ModelState.Remove("CustomerId");
                ModelState.Remove("DeliveryDate");
            }
            else
            {
                if (machine.DeliveryDate.HasValue)
                {
                    machine.WarrantyExpirationDate = machine.DeliveryDate.Value.AddDays(365);
                }
            }

            if (ModelState.IsValid)
            {
                List<string> docUrls = new List<string>();

                if (machine.Documentations != null && machine.Documentations.Any())
                {
                    foreach (var manual in machine.Documentations)
                    {
                        var fileExtension = Path.GetExtension(manual.FileName).ToLower();
                        if (fileExtension != ".pdf")
                        {
                            ModelState.AddModelError("Manuals", "Solo se permiten archivos PDF.");
                            return View(machine);
                        }
                    }
                    docUrls = await FileService.SaveDocAsync(machine.Documentations.ToList(), "documentation/machines");
                }


                var newMachine = new Machine
                {
                    CustomerId = machine.CustomerId,
                    ModelId = machine.ModelId,
                    ChasisNumber = machine.ChasisNumber,
                    EngineNumber = machine.EngineNumber,
                    DeliveryDate = machine.DeliveryDate,
                    WarrantyExpirationDate = machine.DeliveryDate?.AddDays(365),
                    DocUrls = docUrls,
                    ManufactureYear = machine.ManufactureYear,
                    SerialNumber = machine.SerialNumber,
                    UserHours = machine.UserHours,
                    UbicationId = machine.UbicationId
                };
                _context.Add(newMachine);
                await _context.SaveChangesAsync();


                return isStock ? RedirectToAction("Index", "Stock") : RedirectToAction("Vendidos", "Stock");
            }

            ViewBag.IsStock = isStock;
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.ModelId = new SelectList(new List<Model>(), "Id", "Name");
            ViewBag.Customers = new SelectList(_context.Customers.Select(c => new { c.Id, c.Name }), "Id", "Name");
            ViewBag.UbicationId = new SelectList(_context.Ubications, "Id", "Name", machine.UbicationId);
            return View(machine);
        }


        // GET: Machines/Edit/5
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        public async Task<IActionResult> Edit(int? id, bool isStock = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _context.Machines
            .Include(m => m.Customer)
            .FirstOrDefaultAsync(m => m.Id == id);

            if (machine == null)
            {
                return NotFound();
            }

            ViewBag.IsStock = isStock;
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", machine.CustomerId);
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Name", machine.ModelId);
            ViewBag.CustomerName = machine.Customer?.Name;
            ViewBag.UbicationId = new SelectList(_context.Ubications.OrderBy(u => u.Name), "Id", "Name", machine.UbicationId);

            return View(machine);
        }

        // POST: Machines/Edit/5
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(104857600)]
        public async Task<IActionResult> Edit(
            int id, 
            [Bind("Id,CustomerId,ModelId,ChasisNumber,EngineNumber,DeliveryDate,WarrantyExpirationDate,ManufactureYear,SerialNumber,UserHours,UbicationId")] Machine machine, 
            List<string> ExistingDocs, 
            List<IFormFile> Documentations, 
            List<string> DeletedDocs,
            bool isStock = false
            )
        {
            if (id != machine.Id)
            {
                return NotFound();
            }

            if (isStock)
            {
                machine.CustomerId = null;
                machine.DeliveryDate = null;
                ModelState.Remove("CustomerId");
                ModelState.Remove("DeliveryDate");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingMachine = await _context.Machines
                        .Include(m => m.Customer)
                        .Include(m => m.OwnerHistories)
                        .FirstOrDefaultAsync(m => m.Id == id);

                    if (existingMachine == null)
                    {
                        return NotFound();
                    }

                    if (!isStock && existingMachine.CustomerId != machine.CustomerId && existingMachine.Customer != null)
                    {
                        _context.OwnerHistories.Add(new OwnerHistory
                        {
                            MachineId = existingMachine.Id,
                            PreviousOwner = existingMachine.Customer.Name,
                            ChangeDate = DateTime.Now
                        });
                    }


                    existingMachine.CustomerId = machine.CustomerId;
                    existingMachine.ModelId = machine.ModelId;
                    existingMachine.ChasisNumber = machine.ChasisNumber;
                    existingMachine.EngineNumber = machine.EngineNumber;
                    existingMachine.DeliveryDate = machine.DeliveryDate;
                    existingMachine.WarrantyExpirationDate = machine.DeliveryDate?.AddDays(365);
                    existingMachine.ManufactureYear = machine.ManufactureYear;
                    existingMachine.SerialNumber = machine.SerialNumber;
                    existingMachine.UserHours = machine.UserHours;
                    existingMachine.UbicationId = machine.UbicationId;

                    List<string> docUrls = ExistingDocs ?? new List<string>();

                    if (DeletedDocs != null && DeletedDocs.Any())
                    {
                        foreach (var url in DeletedDocs)
                        {
                            var fileName = Path.GetFileName(url);
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "documentation", "machines", fileName);

                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                        }

                        docUrls = docUrls.Except(DeletedDocs).ToList();
                    }

                    if (Documentations != null && Documentations.Any())
                    {
                        foreach (var document in Documentations)
                        {
                            var fileExtension = Path.GetExtension(document.FileName).ToLower();
                            if (fileExtension != ".pdf")
                            {
                                ModelState.AddModelError("Manuals", "Solo se permiten archivos PDF.");
                                return View(machine);
                            }
                        }
                        docUrls.AddRange(await FileService.SaveDocAsync(Documentations));
                    }

                    existingMachine.DocUrls = docUrls;

                    _context.Update(existingMachine);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MachineExists(machine.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }

                }
                return isStock ? RedirectToAction("Index", "Stock") : RedirectToAction("Vendidos", "Stock");
            }

            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id", machine.CustomerId);
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Id", machine.ModelId);
            ViewBag.IsStock = isStock;
            return View(machine);
        }


        // GET: Machines/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public async Task<IActionResult> Delete(int? id, bool isStock = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _context.Machines
                .Include(m => m.Customer)
                .Include(m => m.Model)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (machine == null)
            {
                return NotFound();
            }

            ViewBag.IsStock = isStock;
            return View(machine);
        }

        // POST: Machines/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, bool isStock = false)
        {
            var machine = await _context.Machines.FindAsync(id);
            if (machine != null)
            {                
                if (machine.DocUrls != null && machine.DocUrls.Any())
                {
                    foreach (var fileUrl in machine.DocUrls)
                    {
                        await FileService.DeleteDocumentationFileAsync(fileUrl);
                    }
                }
                
                _context.Machines.Remove(machine);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Máquina eliminada correctamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se encontró la máquina a eliminar.";
            }

            return isStock ? RedirectToAction("Index", "Stock") : RedirectToAction("Vendidos", "Stock");
        }

        private bool MachineExists(int id)
        {
            return _context.Machines.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<JsonResult> GetCustomers(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return Json(new List<object>());
            }

            var customers = await _context.Customers
                                          .Where(c => c.Name!.ToLower().Contains(term.ToLower()))
                                          .Select(c => new
                                          {
                                              id = c.Id,
                                              text = c.Name
                                          })
                                          .Take(10)
                                          .ToListAsync();

            Console.WriteLine($"Clientes encontrados: {customers.Count}");
            return Json(customers);
        }

    }
}
