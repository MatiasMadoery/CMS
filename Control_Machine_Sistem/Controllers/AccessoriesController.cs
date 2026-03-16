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

        // GET: Accessories (Historial general)
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Accessories.Include(a => a.Customer);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Accessories/Details/5
        public async Task<IActionResult> Details(int? id, bool isStock = false)
        {
            if (id == null) return NotFound();

            var accessory = await _context.Accessories
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (accessory == null) return NotFound();

            ViewBag.IsStock = isStock;
            return View(accessory);
        }

        // GET: Accessories/Create
        public IActionResult Create(bool isStock = false)
        {
            ViewBag.IsStock = isStock;
            ViewBag.Customers = new SelectList(_context.Customers, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Model,LoadCapacity,SaleDate,CustomerId")] Accessory accessory, bool isStock = false)
        {
            if (isStock)
            {
                accessory.CustomerId = null;
                accessory.SaleDate = null;
                ModelState.Remove("CustomerId");
                ModelState.Remove("SaleDate");
            }

            if (ModelState.IsValid)
            {
                _context.Add(accessory);
                await _context.SaveChangesAsync();

                return isStock ? RedirectToAction("Index", "Stock") : RedirectToAction("Vendidos", "Stock");
            }

            ViewBag.IsStock = isStock;
            ViewBag.Customers = new SelectList(_context.Customers, "Id", "Name", accessory.CustomerId);
            return View(accessory);
        }

        // GET: Accessories/Edit/5
        public async Task<IActionResult> Edit(int? id, bool isStock = false)
        {
            if (id == null) return NotFound();

            var accessory = await _context.Accessories.FindAsync(id);
            if (accessory == null) return NotFound();

            ViewBag.IsStock = isStock;
            ViewBag.Customers = new SelectList(_context.Customers, "Id", "Name", accessory.CustomerId);
            return View(accessory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Model,LoadCapacity,SaleDate,CustomerId")] Accessory accessory, bool isStock = false)
        {
            if (id != accessory.Id) return NotFound();

            if (isStock)
            {
                accessory.CustomerId = null;
                accessory.SaleDate = null;
                ModelState.Remove("CustomerId");
                ModelState.Remove("SaleDate");
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
                    if (!AccessoryExists(accessory.Id)) return NotFound();
                    else throw;
                }

                return isStock ? RedirectToAction("Index", "Stock") : RedirectToAction("Vendidos", "Stock");
            }

            ViewBag.IsStock = isStock;
            ViewBag.Customers = new SelectList(_context.Customers, "Id", "Name", accessory.CustomerId);
            return View(accessory);
        }

        // GET: Accessories/Delete/5
        public async Task<IActionResult> Delete(int? id, bool isStock = false)
        {
            if (id == null) return NotFound();

            var accessory = await _context.Accessories
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (accessory == null) return NotFound();

            ViewBag.IsStock = isStock;
            return View(accessory);
        }

        // POST: Accessories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, bool isStock = false)
        {
            var accessory = await _context.Accessories.FindAsync(id);
            if (accessory != null)
            {
                _context.Accessories.Remove(accessory);
                await _context.SaveChangesAsync();
            }

            return isStock ? RedirectToAction("Index", "Stock") : RedirectToAction("Vendidos", "Stock");
        }

        private bool AccessoryExists(int id)
        {
            return _context.Accessories.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<JsonResult> GetCustomers(string term)
        {
            if (string.IsNullOrEmpty(term)) return Json(new List<object>());

            var customers = await _context.Customers
                .Where(c => c.Name!.ToLower().Contains(term.ToLower()))
                .Select(c => new { id = c.Id, text = c.Name })
                .Take(10)
                .ToListAsync();

            return Json(customers);
        }
    }
}
