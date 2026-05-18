using Control_Machine_Sistem.Models;
using Control_Machine_Sistem.Models.Control_Machine_Sistem.Models;
using Control_Machine_Sistem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Control_Machine_Sistem.Controllers
{
    [Authorize(Roles = "Admin, Técnico, SuperAdmin, Viewer")]
    public class AccessoriesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IImageStorageService _imageStorageService;

        public AccessoriesController(AppDbContext context, IImageStorageService imageStorageService)
        {
            _context = context;
            _imageStorageService = imageStorageService;
        }

        // GET: Accessories
        [Authorize(Roles = "Admin, Técnico, SuperAdmin, Viewer")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 5, int? productId = null)
        {
            // CORRECCIÓN: Ahora incluimos AccessoryModel para llegar a la Categoría indirectamente
            var query = _context.Accessories
                .Include(a => a.AccessoryModel)
                    .ThenInclude(am => am!.Category)
                .Include(a => a.Ubication)
                .AsQueryable();

            // CORRECCIÓN: El filtrado por producto/categoría ahora se hace a través de AccessoryModel
            if (productId.HasValue && productId > 0)
            {
                query = query.Where(a => a.AccessoryModel!.CategoryId == productId);
            }

            var totalAccessories = await query.CountAsync();

            var accessoriesPage = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pager = new Pager<Accessory>(accessoriesPage, totalAccessories, page, pageSize);

            var categoriasAccesorios = await _context.Categories
                .Where(c => c.Type == CategoryType.Accessory)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Categories = new SelectList(categoriasAccesorios, "Id", "Name");
            ViewBag.SelectedCategory = productId;

            return View(pager);
        }

        // GET: Accessories/Details/5
        [Authorize(Roles = "Admin, Técnico, SuperAdmin, Viewer")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // CORRECCIÓN: Incluimos AccessoryModel y su Categoría
            var accessory = await _context.Accessories
                .Include(a => a.AccessoryModel)
                    .ThenInclude(am => am!.Category)
                .Include(a => a.Ubication)
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (accessory == null) return NotFound();

            return View(accessory);
        }

        // GET: Accessories/Create
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        public IActionResult Create()
        {
            ViewBag.UbicationId = new SelectList(_context.Ubications, "Id", "Name");
            CargarModelosAccesorios(); // <--- NUEVO: Cargamos los modelos del catálogo
            return View();
        }

        // POST: Accessories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        public async Task<IActionResult> Create([Bind("Id,AccessoryModelId,UbicationId,ImageFiles")] Accessory accessory)
        {
            accessory.CustomerId = null;
            accessory.SaleDate = null;
            ModelState.Remove("CustomerId");
            ModelState.Remove("SaleDate");

            if (ModelState.IsValid)
            {
                List<string> imageUrls = new List<string>();
                if (accessory.ImageFiles != null && accessory.ImageFiles.Any())
                {
                    foreach (var photo in accessory.ImageFiles.Take(4))
                    {
                        string url = await _imageStorageService.UploadImageAsync(photo, "accessory-photos");
                        if (!string.IsNullOrEmpty(url)) imageUrls.Add(url);
                    }
                }
                accessory.ImageUrls = imageUrls;

                _context.Add(accessory);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Ubications = new SelectList(_context.Ubications, "Id", "Name", accessory.UbicationId);
            CargarModelosAccesorios(accessory.AccessoryModelId);
            return View(accessory);
        }

        // GET: Accessories/Edit/5
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        public async Task<IActionResult> Edit(int? id, bool isStock = true) // <-- Parámetro opcional añadido
        {
            if (id == null) return NotFound();

            var accessory = await _context.Accessories.FindAsync(id);
            if (accessory == null) return NotFound();

            // CORRECCIÓN: Cambiamos a UbicationId para que coincida con el asp-items de la vista
            ViewBag.UbicationId = new SelectList(_context.Ubications, "Id", "Name", accessory.UbicationId);

            // Si la vista no es de stock (es vendido), también necesitas cargar los clientes:
            if (!isStock)
            {
                ViewBag.Customers = new SelectList(_context.Customers, "Id", "Name", accessory.CustomerId);
            }

            // Pasamos el estado de las pestañas y tipo a la vista
            ViewBag.IsStock = isStock;
            ViewBag.ActiveTab = "accessories";

            CargarModelosAccesorios(accessory.AccessoryModelId); // Asegúrate que dentro asigne a "ViewBag.AccessoryModelId"

            return View(accessory);
        }

        // POST: Accessories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AccessoryModelId,UbicationId")] Accessory accessory, List<string> ExistingImageUrls, List<string> DeletedImageUrls, List<IFormFile> ImageFiles)
        {
            if (id != accessory.Id) return NotFound();

            ModelState.Remove("CustomerId");
            ModelState.Remove("SaleDate");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingAccessory = await _context.Accessories.FirstOrDefaultAsync(a => a.Id == id);
                    if (existingAccessory == null) return NotFound();


                    // CORRECCIÓN: Mapeo de propiedades físicas actualizadas
                    existingAccessory.AccessoryModelId = accessory.AccessoryModelId;
                    existingAccessory.UbicationId = accessory.UbicationId;

                    List<string> imageUrls = ExistingImageUrls ?? new List<string>();

                    if (DeletedImageUrls != null && DeletedImageUrls.Any())
                    {
                        foreach (var url in DeletedImageUrls)
                        {
                            await _imageStorageService.DeleteImageAsync(url, "accessory-photos");
                        }
                        imageUrls = imageUrls.Except(DeletedImageUrls).ToList();
                    }

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

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Ubications = new SelectList(_context.Ubications, "Id", "Name", accessory.UbicationId);
            CargarModelosAccesorios(accessory.AccessoryModelId);
            return View(accessory);
        }

        // GET: Accessories/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public async Task<IActionResult> Delete(int? id, bool isStock = true)
        {
            if (id == null) return NotFound();

            var accessory = await _context.Accessories
                .Include(a => a.AccessoryModel)
                    .ThenInclude(am => am!.Category)
                .Include(a => a.Ubication)
                .Include(a => a.Customer) // Te agrego este Include por si borras un accesorio ya vendido
                .FirstOrDefaultAsync(m => m.Id == id);

            if (accessory == null) return NotFound();

            // NUEVO: Guardamos el estado para que la vista sepa a dónde regresar
            ViewBag.IsStock = isStock;
            ViewBag.ActiveTab = "accessories";
            return View(accessory);
        }

        // POST: Accessories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, SuperAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var accessory = await _context.Accessories.FindAsync(id);
            if (accessory != null)
            {
                if (accessory.ImageUrls != null && accessory.ImageUrls.Any())
                {
                    foreach (var url in accessory.ImageUrls)
                    {
                        await _imageStorageService.DeleteImageAsync(url, "accessory-photos");
                    }
                }

                _context.Accessories.Remove(accessory);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AccessoryExists(int id)
        {
            return _context.Accessories.Any(e => e.Id == id);
        }

        // NUEVOS MÉTODOS AUXILIARES: Reemplaza CargarCategoriasAccesorios por los modelos disponibles
        private void CargarModelosAccesorios(int? selectedId = null)
        {
            var modelos = _context.AccessoryModels
                .OrderBy(m => m.Name)
                .ToList();

            ViewBag.AccessoryModelId = new SelectList(modelos, "Id", "Name", selectedId);
        }
    }
}