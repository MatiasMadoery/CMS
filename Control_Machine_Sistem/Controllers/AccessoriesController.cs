using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Control_Machine_Sistem.Models;

namespace Control_Machine_Sistem.Controllers
{
    public class AccessoriesController : Controller
    {
        private readonly AppDbContext _context;

        public AccessoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Accessories
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Accessories.Include(a => a.Customer);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Accessories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var accessory = await _context.Accessories
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (accessory == null)
            {
                return NotFound();
            }

            return View(accessory);
        }

        // GET: Accessories/Create
        public IActionResult Create(bool isStock = false)
        {
            ViewBag.IsStock = isStock;
            ViewBag.Customers = new SelectList(_context.Customers.Select(c => new { c.Id, c.Name }), "Id", "Name");
            return View();
        }

        // POST: Accessories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Model,LoadCapacity,SaleDate,CustomerId")] Accessory accessory, bool isStock = false)
        {
            if (isStock)
            {
                accessory.CustomerId = null;
                accessory.SaleDate = null;
                ModelState.Remove("CustomerId");
            }

            if (ModelState.IsValid)
            {
                _context.Add(accessory);
                await _context.SaveChangesAsync();

                return isStock ? RedirectToAction("Index", "Stock") : RedirectToAction(nameof(Index));
            }

            ViewBag.IsStock = isStock;
            ViewBag.Customers = new SelectList(_context.Customers.Select(c => new { c.Id, c.Name }), "Id", "Name", accessory.CustomerId);
            return View(accessory);
        }

        // GET: Accessories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var accessory = await _context.Accessories.FindAsync(id);
            if (accessory == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Cuit", accessory.CustomerId);
            return View(accessory);
        }

        // POST: Accessories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Model,LoadCapacity,SaleDate,CustomerId")] Accessory accessory)
        {
            if (id != accessory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(accessory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccessoryExists(accessory.Id))
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
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Cuit", accessory.CustomerId);
            return View(accessory);
        }

        // GET: Accessories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var accessory = await _context.Accessories
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (accessory == null)
            {
                return NotFound();
            }

            return View(accessory);
        }

        // POST: Accessories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var accessory = await _context.Accessories.FindAsync(id);
            if (accessory != null)
            {
                _context.Accessories.Remove(accessory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccessoryExists(int id)
        {
            return _context.Accessories.Any(e => e.Id == id);
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
