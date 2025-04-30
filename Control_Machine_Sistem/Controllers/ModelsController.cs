using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Control_Machine_Sistem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        public async Task<IActionResult> Create([Bind("Id,Name,Manuals,CategoryId")] Model model)
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
                    ManualUrls = manualUrls,
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name, CategoryId")] Model model, List<string> ExistingManuals, List<IFormFile> Manuals)
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
