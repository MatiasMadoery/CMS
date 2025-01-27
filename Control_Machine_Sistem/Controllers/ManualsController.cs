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
    public class ManualsController : Controller
    {
        private readonly AppDbContext _context;

        public ManualsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Manuals
        public async Task<IActionResult> Index()
        {
            return View(await _context.Manual.ToListAsync());
        }

        // GET: Manuals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manual = await _context.Manual
                .FirstOrDefaultAsync(m => m.Id == id);
            if (manual == null)
            {
                return NotFound();
            }

            return View(manual);
        }

        // GET: Manuals/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Manuals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MachineId,ManualUrl")] Manual manual)
        {
            if (ModelState.IsValid)
            {
                _context.Add(manual);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(manual);
        }

        // GET: Manuals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manual = await _context.Manual.FindAsync(id);
            if (manual == null)
            {
                return NotFound();
            }
            return View(manual);
        }

        // POST: Manuals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MachineId,ManualUrl")] Manual manual)
        {
            if (id != manual.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(manual);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ManualExists(manual.Id))
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
            return View(manual);
        }

        // GET: Manuals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manual = await _context.Manual
                .FirstOrDefaultAsync(m => m.Id == id);
            if (manual == null)
            {
                return NotFound();
            }

            return View(manual);
        }

        // POST: Manuals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var manual = await _context.Manual.FindAsync(id);
            if (manual != null)
            {
                _context.Manual.Remove(manual);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ManualExists(int id)
        {
            return _context.Manual.Any(e => e.Id == id);
        }
    }
}
