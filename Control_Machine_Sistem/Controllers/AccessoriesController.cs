using Control_Machine_Sistem.Models;
using Control_Machine_Sistem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Control_Machine_Sistem.Controllers
{
    public class AccessoriesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IImageStorageService _imageStorageService;

        public AccessoriesController(AppDbContext context, IImageStorageService imageStorageService)
        {
            _context = context;
            _imageStorageService = imageStorageService;
        }

        // GET: Accessories (Historial general)
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Accessories.Include(a => a.Customer).Include(a => a.Ubication);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Accessories/Details/5
        public async Task<IActionResult> Details(int? id, bool isStock = false, string activeTab = "machines")
        {
            if (id == null) return NotFound();

            var accessory = await _context.Accessories
                .Include(a => a.Customer)
                .Include(a => a.Ubication)
                .Include(a => a.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (accessory == null) return NotFound();

            ViewBag.IsStock = isStock;
            ViewBag.ActiveTab = activeTab;
            CargarCategoriasAccesorios();
            return View(accessory);
        }

        // GET: Accessories/Create
        public IActionResult Create(bool isStock = false, string activeTab = "machines")
        {
            ViewBag.IsStock = isStock;
            ViewBag.ActiveTab = activeTab;
            ViewBag.Customers = new SelectList(_context.Customers, "Id", "Name");
            ViewBag.Ubications = new SelectList(_context.Ubications, "Id", "Name");
            CargarCategoriasAccesorios();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Model,LoadCapacity,SaleDate,CustomerId,UbicationId,ImageFiles,CategoryId")] Accessory accessory, bool isStock = false, string activeTab = "machines")
        {
            if (isStock)
            {
                accessory.CustomerId = null;
                accessory.SaleDate = null;
                ModelState.Remove("CustomerId");
                ModelState.Remove("SaleDate");
            }

            if (accessory.CategoryId <= 0)
            {
                ModelState.AddModelError("CategoryId", "Debe seleccionar una categoría válida.");
            }

            if (ModelState.IsValid)
            {
                // --- NUEVA LÓGICA: PROCESAR IMÁGENES PARA AZURE ---
                List<string> imageUrls = new List<string>();
                if (accessory.ImageFiles != null && accessory.ImageFiles.Any())
                {
                    foreach (var photo in accessory.ImageFiles.Take(4))
                    {
                        // CORRECCIÓN: Nombre de contenedor consistente "accessory-photos"
                        string url = await _imageStorageService.UploadImageAsync(photo, "accessory-photos");
                        if (!string.IsNullOrEmpty(url)) imageUrls.Add(url);
                    }
                }
                accessory.ImageUrls = imageUrls;

                _context.Add(accessory);
                await _context.SaveChangesAsync();

                return isStock
                    ? RedirectToAction("Index", "Stock", new { activeTab = activeTab })
                    : RedirectToAction("Vendidos", "Stock", new { activeTab = activeTab });
            }

            ViewBag.IsStock = isStock;
            ViewBag.ActiveTab = activeTab;
            ViewBag.Customers = new SelectList(_context.Customers, "Id", "Name", accessory.CustomerId);
            ViewBag.Ubications = new SelectList(_context.Ubications, "Id", "Name", accessory.UbicationId);
            CargarCategoriasAccesorios(accessory.CategoryId);
            return View(accessory);
        }

        // GET: Accessories/Edit/5
        public async Task<IActionResult> Edit(int? id, bool isStock = false, string activeTab = "machines")
        {
            if (id == null) return NotFound();

            var accessory = await _context.Accessories.FindAsync(id);
            if (accessory == null) return NotFound();

            ViewBag.IsStock = isStock;
            ViewBag.ActiveTab = activeTab;
            ViewBag.Customers = new SelectList(_context.Customers, "Id", "Name", accessory.CustomerId);
            ViewBag.Ubications = new SelectList(_context.Ubications, "Id", "Name", accessory.UbicationId);
            CargarCategoriasAccesorios(accessory.CategoryId);
            return View(accessory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Model,LoadCapacity,SaleDate,CustomerId,UbicationId,CategoryId")] Accessory accessory,
            List<string> ExistingImageUrls,
            List<string> DeletedImageUrls,
            List<IFormFile> ImageFiles,
            bool isStock = false,
            string activeTab = "machines")
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
                    var existingAccessory = await _context.Accessories.FirstOrDefaultAsync(a => a.Id == id);
                    if (existingAccessory == null) return NotFound();

                    // Mapeo manual (Igual que en Machines)
                    //existingAccessory.Model = accessory.Model;
                    existingAccessory.LoadCapacity = accessory.LoadCapacity;
                    existingAccessory.UbicationId = accessory.UbicationId;
                    existingAccessory.CategoryId = accessory.CategoryId;
                    existingAccessory.CustomerId = isStock ? null : accessory.CustomerId;
                    existingAccessory.SaleDate = isStock ? null : accessory.SaleDate;

                    List<string> imageUrls = ExistingImageUrls ?? new List<string>();

                    // 1. Borrar de Azure
                    if (DeletedImageUrls != null && DeletedImageUrls.Any())
                    {
                        foreach (var url in DeletedImageUrls)
                            await _imageStorageService.DeleteImageAsync(url, "accessory-photos");

                        imageUrls = imageUrls.Except(DeletedImageUrls).ToList();
                    }

                    // 2. Subir nuevas
                    if (ImageFiles != null && ImageFiles.Any())
                    {
                        foreach (var photo in ImageFiles)
                        {
                            string newUrl = await _imageStorageService.UploadImageAsync(photo, "accessory-photos");
                            if (!string.IsNullOrEmpty(newUrl)) imageUrls.Add(newUrl);
                        }
                    }

                    existingAccessory.ImageUrls = imageUrls.Take(4).ToList();

                    _context.Update(existingAccessory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccessoryExists(accessory.Id)) return NotFound();
                    else throw;
                }

                return isStock
                    ? RedirectToAction("Index", "Stock", new { activeTab = activeTab })
                    : RedirectToAction("Vendidos", "Stock", new { activeTab = activeTab });
            }

            ViewBag.IsStock = isStock;
            ViewBag.ActiveTab = activeTab;
            ViewBag.Customers = new SelectList(_context.Customers, "Id", "Name", accessory.CustomerId);
            ViewBag.Ubications = new SelectList(_context.Ubications, "Id", "Name", accessory.UbicationId);
            CargarCategoriasAccesorios(accessory.CategoryId);
            return View(accessory);
        }

        // GET: Accessories/Delete/5
        public async Task<IActionResult> Delete(int? id, bool isStock = false, string activeTab = "machines")
        {
            if (id == null) return NotFound();

            var accessory = await _context.Accessories
                .Include(a => a.Customer)
                .Include(a => a.Ubication)
                .Include(a => a.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (accessory == null) return NotFound();

            ViewBag.IsStock = isStock;
            ViewBag.ActiveTab = activeTab;
            CargarCategoriasAccesorios(accessory.CategoryId);
            return View(accessory);
        }

        // POST: Accessories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, bool isStock = false, string activeTab = "machines")
        {
            var accessory = await _context.Accessories.FindAsync(id);
            if (accessory != null)
            {
                // Borrar fotos de la nube antes de borrar de la DB
                if (accessory.ImageUrls != null)
                {
                    foreach (var url in accessory.ImageUrls)
                        await _imageStorageService.DeleteImageAsync(url, "accessory-photos");
                }

                _context.Accessories.Remove(accessory);
                await _context.SaveChangesAsync();
            }
            ViewBag.ActiveTab = activeTab;
            return isStock
                    ? RedirectToAction("Index", "Stock", new { activeTab = activeTab })
                    : RedirectToAction("Vendidos", "Stock", new { activeTab = activeTab });
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

        private void CargarCategoriasAccesorios(int? selectedId = null)
        {
            var categorias = _context.Categories
                .Where(c => c.Type == CategoryType.Accessory)
                .OrderBy(c => c.Name)
                .ToList();

            ViewBag.CategoryId = new SelectList(categorias, "Id", "Name", selectedId);
        }
    }
}