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
    public class ModelsController : Controller
    {
        private readonly AppDbContext _context;

        public ModelsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Models
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var models = from m in _context.Models select m;

            // Get total models 
            var totalModels = await models.CountAsync();

            // Apply pagination
            var modelsPager = await models
                                         .Skip((page - 1) * pageSize)
                                         .Take(pageSize)
                                         .ToListAsync();

            // Create the paginator with the paginated list
            var pager = new Pager<Model>(modelsPager, totalModels, page, pageSize);

            //To maintain the value of the lookup field when the user changes pages           
            return View(pager);
        }

        // GET: Models/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _context.Models
                .FirstOrDefaultAsync(m => m.Id == id);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // GET: Models/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Models/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Manuals")] Model model)
        {
            if (ModelState.IsValid)
            {
                List<string> manualUrls = new List<string>();

                if (model.Manuals != null && model.Manuals.Any())
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "manuals", "models");
                    Directory.CreateDirectory(uploadsFolder);

                    foreach (var manual in model.Manuals)
                    {
                        if (manual.Length > 0)
                        {
                            string uniqueFileName = $"{Guid.NewGuid()}_{manual.FileName}";
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await manual.CopyToAsync(fileStream);
                            }

                            manualUrls.Add($"/manuals/models/{uniqueFileName}");
                        }
                    }
                }

                var newModel = new Model
                {
                    Name = model.Name,
                    ManualUrls = manualUrls
                };

                _context.Models.Add(newModel);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Models/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _context.Models.FindAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        //// POST: Models/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Manual")] Model model)
        //{
        //    if (id != model.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(model);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!ModelExists(model.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(model);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Model model, List<string> ExistingManuals, List<IFormFile> Manuals)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingModel = await _context.Models.FindAsync(id);

                    if (existingModel == null)
                    {
                        return NotFound();
                    }

                    // Actualizamos las propiedades del modelo existente
                    existingModel.Name = model.Name;

                    List<string> manualUrls = ExistingManuals ?? new List<string>();

                    // Si se cargan nuevos archivos, los añadimos a la lista de manualUrls
                    if (Manuals != null && Manuals.Any())
                    {
                        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "manuals", "models");
                        Directory.CreateDirectory(uploadsFolder);

                        foreach (var manual in Manuals)
                        {
                            if (manual.Length > 0)
                            {
                                string uniqueFileName = $"{Guid.NewGuid()}_{manual.FileName}";
                                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await manual.CopyToAsync(fileStream);
                                }

                                manualUrls.Add($"/manuals/models/{uniqueFileName}");
                            }
                        }
                    }

                    existingModel.ManualUrls = manualUrls;

                    _context.Update(existingModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ModelExists(model.Id))
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
            return View(model);
        }




        // GET: Models/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _context.Models
                .FirstOrDefaultAsync(m => m.Id == id);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: Models/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var model = await _context.Models
        .Include(m => m.Machines)
        .FirstOrDefaultAsync(m => m.Id == id);

            if (model == null)
            {
                return NotFound();
            }

            if (model.Machines!.Any())
            {
                TempData["ErrorMessage"] = "No se puede eliminar el modelo porque tiene máquinas asociadas.";
                return RedirectToAction(nameof(Index));
            }

            _context.Models.Remove(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Modelo eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private bool ModelExists(int id)
        {
            return _context.Models.Any(e => e.Id == id);
        }
    }
}
