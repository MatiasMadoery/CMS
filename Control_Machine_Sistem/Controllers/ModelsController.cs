using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Control_Machine_Sistem.Models;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using NPOI.OpenXml4Net.OPC.Internal;
using Control_Machine_Sistem.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http.Metadata;


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
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, int? categoryId = null)

        {
            var models = _context.Models.Include(m => m.Category).AsQueryable();

            if (categoryId.HasValue && categoryId > 0)
            {
                models = models.Where(m => m.CategoryId == categoryId);
            }

            // Get total models 
            var totalModels = await models.CountAsync();

            // Apply pagination
            var modelsPager = await models
                                         .Skip((page - 1) * pageSize)
                                         .Take(pageSize)
                                         .ToListAsync();


            var pager = new Pager<Model>(modelsPager, totalModels, page, pageSize);

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.SelectedCategory = categoryId;

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
                .Include(m => m.Category)
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
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Models/Create     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Manuals,SpareKits,CategoryId")] Model model)
        {
            if (ModelState.IsValid)
            {
                List<string> spareKitsUrls = new List<string>();
                List<string> manualUrls = new List<string>();
                if (model.Manuals != null && model.Manuals.Any())
                {
                    foreach (var manual in model.Manuals)
                    {
                        var fileExtension = Path.GetExtension(manual.FileName).ToLower();
                        if (fileExtension != ".pdf")
                        {
                            ModelState.AddModelError("Manuals", "Solo se permiten archivos PDF.");
                            return View(model);
                        }
                    }
                    manualUrls = await FileService.SaveManualsAsync((List<IFormFile>)model.Manuals);
                }

                if (model.SpareKits != null && model.SpareKits.Any())
                {
                    foreach (var spareKit in model.SpareKits)
                    {
                        var fileExtension = Path.GetExtension(spareKit.FileName).ToLower();
                        if (fileExtension != ".pdf")
                        {
                            ModelState.AddModelError("SpareKits", "Solo se permiten archivos PDF.");
                            return View(model);
                        }
                    }
                    spareKitsUrls = await FileService.SaveSpareKitsAsync((List<IFormFile>)model.SpareKits);
                }


                var newModel = new Model
                {
                    Name = model.Name,
                    ManualUrls = manualUrls,
                    SpareKitsUrls = spareKitsUrls,
                    CategoryId = model.CategoryId
                };

                _context.Models.Add(newModel);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Nombre");

            return View(model);
        }

        // GET: Models/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _context.Models
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (model == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);

            return View(model);
        }

        //// POST: Models/Edit/5        
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CategoryId")] Model model, List<string> ExistingManuals, List<IFormFile> Manuals, List<string> DeletedManuals, List<string> ExistingSpareKits, List<IFormFile> SpareKits, List<string> DeletedSpareKits)

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

                    existingModel.Name = model.Name;
                    existingModel.CategoryId = model.CategoryId;

                    List<string> manualUrls = ExistingManuals ?? new List<string>();
                    List<string> spareKitsUrls = ExistingSpareKits ?? new List<string>();

                    if (DeletedManuals != null && DeletedManuals.Any())
                    {
                        foreach (var url in DeletedManuals)
                        {
                            var fileName = Path.GetFileName(url);
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "documentation", "manuals", fileName);

                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                        }


                        manualUrls = manualUrls.Except(DeletedManuals).ToList();
                    }

                    if (DeletedSpareKits != null && DeletedSpareKits.Any())
                    {
                        foreach (var url in DeletedSpareKits)
                        {
                            var fileName = Path.GetFileName(url);
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "documentation", "spareKits", fileName);

                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                        }


                        spareKitsUrls = spareKitsUrls.Except(DeletedSpareKits).ToList();
                    }

                    if (Manuals != null && Manuals.Any())
                    {
                        foreach (var manual in Manuals)
                        {
                            var fileExtension = Path.GetExtension(manual.FileName).ToLower();
                            if (fileExtension != ".pdf")
                            {
                                ModelState.AddModelError("Manuals", "Solo se permiten archivos PDF.");
                                return View(model);
                            }
                        }
                        manualUrls.AddRange(await FileService.SaveManualsAsync(Manuals));

                    }

                    if (SpareKits != null && SpareKits.Any())
                    {
                        foreach (var spareKit in SpareKits)
                        {
                            var fileExtension = Path.GetExtension(spareKit.FileName).ToLower();
                            if (fileExtension != ".pdf")
                            {
                                ModelState.AddModelError("SpareKits", "Solo se permiten archivos PDF.");
                                return View(model);
                            }
                        }
                        spareKitsUrls.AddRange(await FileService.SaveSpareKitsAsync(SpareKits));

                    }

                    existingModel.ManualUrls = manualUrls;
                    existingModel.SpareKitsUrls = spareKitsUrls;

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

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);

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
                .Include(m => m.Category)
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
            
            if (model.Machines != null && model.Machines.Any())
            {
                TempData["ErrorMessage"] = "No se puede eliminar el modelo porque tiene máquinas asociadas.";
                return RedirectToAction(nameof(Index));
            }
          
            if (model.ManualUrls != null && model.ManualUrls.Any())
            {
                foreach (var fileUrl in model.ManualUrls)
                {
                    await FileService.DeleteManualFileAsync(fileUrl);
                }
            }
          
            if (model.SpareKitsUrls != null && model.SpareKitsUrls.Any())
            {
                foreach (var fileUrl in model.SpareKitsUrls)
                {
                    await FileService.DeleteSpareKitFileAsync(fileUrl);
                }
            }
            
            _context.Models.Remove(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool ModelExists(int id)
        {
            return _context.Models.Any(e => e.Id == id);
        }
    }
}
