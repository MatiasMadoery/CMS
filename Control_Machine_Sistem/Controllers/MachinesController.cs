using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Control_Machine_Sistem.Models;
using Control_Machine_Sistem.ViewModels;
using System.Drawing.Printing;

namespace Control_Machine_Sistem.Controllers
{
    public class MachinesController : Controller
    {
        private readonly AppDbContext _context;

        public MachinesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Machines
        public async Task<IActionResult> Index(string searchString, int page = 1, int pageSize = 5)
        {
            //var appDbContext = _context.Machines.Include(m => m.Customer).Include(m => m.Model);
            //return View(await appDbContext.ToListAsync());
            var machine = from m in _context.Machines!
                          .Include(m => m.Customer)
                          .Include(m => m.Model)
                          select m;

            //Filter by search text if provided
            if (!String.IsNullOrEmpty(searchString))
            {
                machine = machine.Where(s => s.Customer!.Name!.Contains(searchString) || s.Customer.LastName!.Contains(searchString) || 
                s.Model!.Name!.Contains(searchString));
            }

            // Get total machines 
            var totalMachines = await machine.CountAsync();

            // Apply pagination
            var machinePager = await machine
                           .Skip((page - 1) * pageSize)
                           .Take(pageSize)
                           .ToListAsync();

            // Create the paginator with the paginated list
            var pager = new Pager<Machine>(machinePager, totalMachines, page, pageSize);

            //To maintain the value of the lookup field when the user changes pages
            ViewData["searchString"] = searchString;
            return View(pager);
        }

        // GET: Machines/Details/5
        public async Task<IActionResult> Details(int? id)
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

            return View(machine);
        }

        // GET: Machines/Create
        public IActionResult Create()
        {
            //ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id");
            //ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Id");

            var customers = _context.Customers.Select(c => new
            {
                Id = c.Id,
                FullName = c.FullName
            }).ToList();
            ViewData["CustomerId"] = new SelectList(customers, "Id", "FullName");

            var models = _context.Models.Select(m => new
            {
                Id = m.Id,
                Name = m.Name
            }).ToList();
            ViewData["ModelId"] = new SelectList(models, "Id", "Name");

            return View();
        }

        // POST: Machines/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CustomerId,ModelId,ChasisNumber,EngineNumber,DeliveryDate,WarrantyExpirationDate,Documentations")] Machine machine)
        {
            if (ModelState.IsValid)
            {
                List<string> docUrls = new List<string>();

                if (machine.Documentations != null && machine.Documentations.Any())
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentations", "machines");
                    Directory.CreateDirectory(uploadsFolder);

                    foreach(var documentation in machine.Documentations)
                    {
                        if(documentation.Length > 0)
                        {
                            string uniqueFileName = $"{Guid.NewGuid()}_{documentation.FileName}";
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await documentation.CopyToAsync(fileStream);
                            }

                            docUrls.Add($"/documentations/machines/{uniqueFileName}");
                        }
                    }
                }

                var newMachine = new Machine
                {
                    CustomerId = machine.CustomerId,
                    ModelId = machine.ModelId,
                    ChasisNumber = machine.ChasisNumber,
                    EngineNumber = machine.EngineNumber,
                    DeliveryDate = machine.DeliveryDate,
                    WarrantyExpirationDate = machine.WarrantyExpirationDate,
                    DocUrls = docUrls,
                };
                _context.Add(newMachine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id", machine.CustomerId);
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Id", machine.ModelId);
            return View(machine);
        }

        // GET: Machines/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var machine = await _context.Machines.FindAsync(id);

            if (machine == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id", machine.CustomerId);
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Id", machine.ModelId);
            return View(machine);
        }

        // POST: Machines/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,CustomerId,ModelId,ChasisNumber,EngineNumber,DeliveryDate,WarrantyExpirationDate,Documentations")] Machine machine)
        {
            if (id != machine.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(machine);
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id", machine.CustomerId);
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Id", machine.ModelId);
            return View(machine);
        }

        // GET: Machines/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

            return View(machine);
        }

        // POST: Machines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var machine = await _context.Machines.FindAsync(id);
            if (machine != null)
            {
                _context.Machines.Remove(machine);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Máquina eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private bool MachineExists(int id)
        {
            return _context.Machines.Any(e => e.Id == id);
        }

    }
}
