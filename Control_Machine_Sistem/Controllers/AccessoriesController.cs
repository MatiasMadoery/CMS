using ClosedXML.Excel;
using Control_Machine_Sistem.Models;
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
        [HttpGet("Accessories/Details/{id}")] //Forzamos la ruta base explícita
        [Authorize(Roles = "Admin, Técnico, SuperAdmin, Viewer")]
        public async Task<IActionResult> Details(int? id, [FromQuery] bool isStock = true) //Indica que isStock viene del QueryString (?isStock=true)
        {
            if (id == null) return NotFound();

            var accessory = await _context.Accessories
                .Include(a => a.AccessoryModel)
                    .ThenInclude(am => am!.Category)
                .Include(a => a.Ubication)
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (accessory == null) return NotFound();

            ViewBag.IsStock = isStock;

            return View(accessory);
        }

        // GET: Accessories/Create
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        public IActionResult Create()
        {
            var accessoriesCategories = _context.Categories
                .Where(c => c.Type == CategoryType.Accessory)
                .OrderBy(c => c.Name)
                .ToList();

            ViewBag.Categories = new SelectList(accessoriesCategories, "Id", "Name");
            ViewBag.UbicationId = new SelectList(_context.Ubications, "Id", "Name");
            CargarModelosAccesorios(); // Carga los modelos del catálogo

            ViewBag.AccessoryModelId = new SelectList(Enumerable.Empty<SelectListItem>());
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetAccessoriesModelsByCategory(int categoryId)
        {
            var models = await _context.AccessoryModels
                                       .Where(a => a.CategoryId == categoryId)
                                       .Select(a => new { id = a.Id, name = a.Name })
                                       .ToListAsync();

            return Json(models);
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

                return RedirectToAction("Index", "Stock", new { activeTab = "accessories" });
            }

            ViewBag.Ubications = new SelectList(_context.Ubications, "Id", "Name", accessory.UbicationId);
            CargarModelosAccesorios(accessory.AccessoryModelId);
            return View(accessory);
        }

        // GET: Accessories/Edit/5
        [HttpGet]
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        public async Task<IActionResult> Edit(int? id, bool isStock = true)
        {
            if (id == null) return NotFound();

            var accessory = await _context.Accessories.FindAsync(id);
            if (accessory == null) return NotFound();

            ViewBag.UbicationId = new SelectList(_context.Ubications, "Id", "Name", accessory.UbicationId);

            if (!isStock)
            {
                ViewBag.Customers = new SelectList(_context.Customers, "Id", "Name", accessory.CustomerId);
            }

            ViewBag.IsStock = isStock;
            ViewBag.ActiveTab = "accessories";

            CargarModelosAccesorios(accessory.AccessoryModelId);

            return View(accessory);
        }

        // POST: Accessories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,AccessoryModelId,UbicationId")] Accessory accessory,
            List<string> ExistingImageUrls,
            List<string> DeletedImageUrls,
            List<IFormFile> ImageFiles,
            bool origenStock) // Renombramos el parámetro a 'origenStock' para romper la ambigüedad con el GET
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

                // REDIRECCIÓN usando el nuevo parámetro
                if (!origenStock)
                {
                    return RedirectToAction("Vendidos", "Stock", new { activeTab = "accessories" });
                }

                return RedirectToAction("Index", "Stock", new { activeTab = "accessories" });
            }

            ViewBag.Ubications = new SelectList(_context.Ubications, "Id", "Name", accessory.UbicationId);
            CargarModelosAccesorios(accessory.AccessoryModelId);
            ViewBag.IsStock = origenStock;
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
                .Include(a => a.Customer)
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
            return RedirectToAction("Index", "Stock", new { activeTab = "accessories" });
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

        public async Task<IActionResult> ExportarExcelAccessories()
        {
            var accessories = await _context.Accessories!
                .Include(a => a.AccessoryModel)
                    .ThenInclude(a => a.Category)
                .Include(m => m.Ubication)
                .OrderBy(m => m.Id)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Stock de Accesorios");

                string[] headers = {
            "Accesorio", "Modelo", "Capacidad de Carga", "Sucursal",
        };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                int row = 2;
                foreach (var item in accessories)
                {
                    worksheet.Cell(row, 1).Value = item.AccessoryModel?.Category?.Name ?? "N/A";
                    worksheet.Cell(row, 2).Value = item.AccessoryModel?.Name ?? "N/A";
                    worksheet.Cell(row, 3).Value = item.AccessoryModel?.LoadCapacity;
                    worksheet.Cell(row, 4).Value = item.Ubication?.Name ?? "N/A";
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Reporte_Stock_{DateTime.Now:yyyyMMdd}.xlsx"
                    );
                }
            }
        }
    }
}