using Control_Machine_Sistem.Models;
using Control_Machine_Sistem.Models.Control_Machine_Sistem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Control_Machine_Sistem.Services; // Para que reconozca tu clase Pager

namespace Control_Machine_Sistem.Controllers
{
    [Authorize(Roles = "Admin, Técnico, SuperAdmin,Viewer")]
    public class AccessoryModelsController : Controller
    {
        private readonly AppDbContext _context;

        public AccessoryModelsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AccessoryModels
        [Authorize(Roles = "Admin, Técnico, SuperAdmin,Viewer")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, int? categoryId = null)
        {
            // Inicializamos la consulta incluyendo la categoría
            var query = _context.AccessoryModels.Include(a => a.Category).AsQueryable();

            // Aplicamos el filtro por ID de categoría si fue seleccionado
            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(m => m.CategoryId == categoryId);
            }

            // Calculamos el total de elementos antes de paginar
            var totalAccessories = await query.CountAsync();

            // Aplicamos la paginación eficiente
            var accessoriesPager = await query
                .OrderBy(m => m.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Modificado: Usamos tu componente Pager genérico igual que en Models
            var pager = new Pager<AccessoryModel>(accessoriesPager, totalAccessories, page, pageSize);

            // FILTRADO CLAVE: Solo cargamos categorías cuyo tipo sea Accesorio
            ViewBag.Categories = new SelectList(_context.Categories.Where(c => c.Type == CategoryType.Accessory), "Id", "Name");
            ViewBag.SelectedCategory = categoryId;

            return View(pager);
        }

        // GET: AccessoryModels/Details/5
        [Authorize(Roles = "Admin, Técnico, SuperAdmin,Viewer")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var accessoryModel = await _context.AccessoryModels
                .Include(a => a.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (accessoryModel == null)
            {
                return NotFound();
            }

            return View(accessoryModel);
        }

        // GET: AccessoryModels/Create
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        public IActionResult Create()
        {
            // Solo mostramos categorías que correspondan a Accesorios
            var categoriasAccesorios = _context.Categories
                .Where(c => c.Type == CategoryType.Accessory)
                .OrderBy(c => c.Name)
                .ToList();

            ViewBag.Categories = new SelectList(categoriasAccesorios, "Id", "Name");
            return View();
        }

        // POST: AccessoryModels/Create
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,LoadCapacity,CategoryId")] AccessoryModel accessoryModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(accessoryModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var categoriasAccesorios = _context.Categories
                .Where(c => c.Type == CategoryType.Accessory)
                .OrderBy(c => c.Name)
                .ToList();

            ViewBag.Categories = new SelectList(categoriasAccesorios, "Id", "Name", accessoryModel.CategoryId);
            return View(accessoryModel);
        }

        // GET: AccessoryModels/Edit/5
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var accessoryModel = await _context.AccessoryModels.FindAsync(id);
            if (accessoryModel == null)
            {
                return NotFound();
            }

            var categoriasAccesorios = _context.Categories
                .Where(c => c.Type == CategoryType.Accessory)
                .OrderBy(c => c.Name)
                .ToList();

            ViewBag.Categories = new SelectList(categoriasAccesorios, "Id", "Name", accessoryModel.CategoryId);
            return View(accessoryModel);
        }

        // POST: AccessoryModels/Edit/5
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,LoadCapacity,CategoryId")] AccessoryModel accessoryModel)
        {
            if (id != accessoryModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(accessoryModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccessoryModelExists(accessoryModel.Id))
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

            var categoriasAccesorios = _context.Categories
                .Where(c => c.Type == CategoryType.Accessory)
                .OrderBy(c => c.Name)
                .ToList();

            ViewBag.Categories = new SelectList(categoriasAccesorios, "Id", "Name", accessoryModel.CategoryId);
            return View(accessoryModel);
        }

        // GET: AccessoryModels/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var accessoryModel = await _context.AccessoryModels
                .Include(a => a.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (accessoryModel == null)
            {
                return NotFound();
            }

            return View(accessoryModel);
        }

        // POST: AccessoryModels/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var accessoryModel = await _context.AccessoryModels.FindAsync(id);
            if (accessoryModel != null)
            {
                _context.AccessoryModels.Remove(accessoryModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccessoryModelExists(int id)
        {
            return _context.AccessoryModels.Any(e => e.Id == id);
        }
    }
}