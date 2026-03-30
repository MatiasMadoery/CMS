using Control_Machine_Sistem.Models;
using Control_Machine_Sistem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Control_Machine_Sistem.Controllers
{
    public class StockController : Controller
    {
        private readonly AppDbContext _context;
        public StockController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? ubicationId, int? categoryId, int? modelId, int machinePage = 1, int accessoryPage = 1, string activeTab = "machines")
        {
            // Persistencia para la vista
            ViewBag.ActiveTab = activeTab;
            ViewBag.SelectedUbication = ubicationId;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SelectedModel = modelId;

            // Carga de Selects para filtros
            ViewBag.Ubications = new SelectList(await _context.Ubications.ToListAsync(), "Id", "Name", ubicationId);
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", categoryId);

            if (categoryId.HasValue)
            {
                ViewBag.Models = new SelectList(await _context.Models.Where(m => m.CategoryId == categoryId).ToListAsync(), "Id", "Name", modelId);
            }

            int pageSize = 10;

            // Query Máquinas en Stock
            var mQuery = _context.Machines
                .Include(m => m.Model)
                .Include(m => m.Ubication)
                .Where(m => m.CustomerId == null);

            if (ubicationId.HasValue) mQuery = mQuery.Where(m => m.UbicationId == ubicationId);
            if (categoryId.HasValue) mQuery = mQuery.Where(m => m.Model.CategoryId == categoryId);
            if (modelId.HasValue) mQuery = mQuery.Where(m => m.ModelId == modelId);

            var mCount = await mQuery.CountAsync();
            var mElements = await mQuery.Skip((machinePage - 1) * pageSize).Take(pageSize).ToListAsync();
            var mPager = new Pager<Machine>(mElements, mCount, machinePage, pageSize);

            // Query Accesorios en Stock
            var aQuery = _context.Accessories
                .Include(m => m.Ubication)
                .Where(a => a.CustomerId == null);

            if (ubicationId.HasValue)
            {
                aQuery = aQuery.Where(a => a.UbicationId == ubicationId);
            }

            var aCount = await aQuery.CountAsync();
            var aElements = await aQuery.Skip((accessoryPage - 1) * pageSize).Take(pageSize).ToListAsync();
            var aPager = new Pager<Accessory>(aElements, aCount, accessoryPage, pageSize);

            var viewModel = new StockViewModel
            {
                Machines = mPager,
                Accessories = aPager,
                ActiveTab = activeTab
            };

            return View(viewModel);
        }

        // Acción para procesar la venta
        [HttpPost]
        public async Task<IActionResult> Sell(int id, string type, int customerId)
        {
            if (type == "Machine")
            {
                var machine = await _context.Machines.FindAsync(id);
                if (machine != null)
                {
                    machine.CustomerId = customerId;
                    machine.DeliveryDate = DateTime.Now;
                }
            }
            else
            {
                var accessory = await _context.Accessories.FindAsync(id);
                if (accessory != null)
                {
                    accessory.CustomerId = customerId;
                    accessory.SaleDate = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "ProductosVendidos");
        }


        // GET: Stock/Vender?id=5&type=Machine
        public async Task<IActionResult> Vender(int id, string type)
        {
            string productName = "";

            if (type == "Machine")
            {
                var machine = await _context.Machines.Include(m => m.Model).FirstOrDefaultAsync(m => m.Id == id);
                if (machine == null) return NotFound();
                productName = $"{machine.Model?.Name} - Chasis: {machine.ChasisNumber}";
            }
            else
            {
                var accessory = await _context.Accessories.FindAsync(id);
                if (accessory == null) return NotFound();
                productName = accessory.Model;
            }

            ViewBag.Id = id;
            ViewBag.Type = type;
            ViewBag.ProductName = productName;
            // Cargamos los clientes para el dropdown
            ViewBag.Customers = new SelectList(_context.Customers.OrderBy(c => c.Name), "Id", "Name");

            return View();
        }

        // POST: Stock/Vender
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vender(int id, string type, int customerId, DateTime deliveryDate)
        {
            if (type == "Machine")
            {
                var machine = await _context.Machines.FindAsync(id);
                if (machine != null)
                {
                    machine.CustomerId = customerId;
                    machine.DeliveryDate = deliveryDate;
                    machine.WarrantyExpirationDate = deliveryDate.AddDays(365);
                }
            }
            else
            {
                var accessory = await _context.Accessories.FindAsync(id);
                if (accessory != null)
                {
                    accessory.CustomerId = customerId;
                    accessory.SaleDate = deliveryDate;
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Stock");
        }

        public async Task<IActionResult> Vendidos(int? ubicationId, int? categoryId, int? modelId, int machinePage = 1, int accessoryPage = 1, string activeTab = "machines")
        {
            // Persistencia para los filtros en la vista
            ViewBag.ActiveTab = activeTab;
            ViewBag.SelectedUbication = ubicationId;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SelectedModel = modelId;

            // Carga de Selects para los filtros
            ViewBag.Ubications = new SelectList(await _context.Ubications.ToListAsync(), "Id", "Name", ubicationId);
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", categoryId);

            if (categoryId.HasValue)
            {
                ViewBag.Models = new SelectList(await _context.Models.Where(m => m.CategoryId == categoryId).ToListAsync(), "Id", "Name", modelId);
            }

            int pageSize = 10;

            // --- QUERY MÁQUINAS VENDIDAS ---
            var mQuery = _context.Machines
                .Include(m => m.Model)
                    .ThenInclude(mod => mod.Category)
                .Include(m => m.Ubication)
                .Include(m => m.Customer)
                .Where(m => m.CustomerId != null); // Solo vendidas

            if (ubicationId.HasValue) mQuery = mQuery.Where(m => m.UbicationId == ubicationId);
            if (categoryId.HasValue) mQuery = mQuery.Where(m => m.Model.CategoryId == categoryId);
            if (modelId.HasValue) mQuery = mQuery.Where(m => m.ModelId == modelId);

            var mCount = await mQuery.CountAsync();
            var mElements = await mQuery
                .OrderByDescending(m => m.DeliveryDate) // Ordenar por fecha de entrega
                .Skip((machinePage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // --- QUERY ACCESORIOS VENDIDOS ---
            var aQuery = _context.Accessories
                .Include(a => a.Ubication)
                .Include(a => a.Customer)
                .Where(a => a.CustomerId != null); // Solo vendidos

            if (ubicationId.HasValue) aQuery = aQuery.Where(a => a.UbicationId == ubicationId);

            var aCount = await aQuery.CountAsync();
            var aElements = await aQuery
                .OrderByDescending(a => a.SaleDate) // Ordenar por fecha de venta
                .Skip((accessoryPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new SoldItemViewModel
            {
                SoldMachines = new Pager<Machine>(mElements, mCount, machinePage, pageSize),
                SoldAccessories = new Pager<Accessory>(aElements, aCount, accessoryPage, pageSize),
                ActiveTab = activeTab
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<JsonResult> GetModelsByCategory(int categoryId)
        {
            var models = await _context.Models
                .Where(m => m.CategoryId == categoryId)
                .OrderBy(m => m.Name)
                .Select(m => new { id = m.Id, name = m.Name })
                .ToListAsync();

            return Json(models);
        }
    }
}
