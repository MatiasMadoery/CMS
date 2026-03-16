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
    public class UbicationsController : Controller
    {
        private readonly AppDbContext _context;

        public UbicationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Ubications
        public async Task<IActionResult> Index()
        {
            return View(await _context.Ubications.ToListAsync());
        }

        // GET: Ubications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ubication = await _context.Ubications
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ubication == null)
            {
                return NotFound();
            }

            return View(ubication);
        }

        // GET: Ubications/Create
        public IActionResult Create(bool isStock = false)
        {
            ViewBag.IsStock = isStock;
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.Customers = new SelectList(_context.Customers, "Id", "Name");

            // Cargamos las sucursales aquí
            ViewBag.UbicationId = new SelectList(_context.Ubications.OrderBy(u => u.Name), "Id", "Name");

            return View();
        }

        // POST: Ubications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address")] Ubication ubication)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ubication);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ubication);
        }

        // GET: Ubications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ubication = await _context.Ubications.FindAsync(id);
            if (ubication == null)
            {
                return NotFound();
            }
            return View(ubication);
        }

        // POST: Ubications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address")] Ubication ubication)
        {
            if (id != ubication.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ubication);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UbicationExists(ubication.Id))
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
            return View(ubication);
        }

        // GET: Ubications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ubication = await _context.Ubications
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ubication == null)
            {
                return NotFound();
            }

            return View(ubication);
        }

        // POST: Ubications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ubication = await _context.Ubications.FindAsync(id);
            if (ubication != null)
            {
                _context.Ubications.Remove(ubication);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UbicationExists(int id)
        {
            return _context.Ubications.Any(e => e.Id == id);
        }
    }
}
