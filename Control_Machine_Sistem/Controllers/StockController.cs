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

        public async Task<IActionResult> Index(int machinePage = 1, int accessoryPage = 1, string activeTab = "machines")
        {
            int pageSize = 10;

            // Query Máquinas en Stock
            var mQuery = _context.Machines.Include(m => m.Model).Where(m => m.CustomerId == null);
            var mElements = await mQuery.Skip((machinePage - 1) * pageSize).Take(pageSize).ToListAsync();
            var mPager = new Pager<Machine>(mElements, await mQuery.CountAsync(), machinePage, pageSize);

            // Query Accesorios en Stock
            var aQuery = _context.Accessories.Where(a => a.CustomerId == null);
            var aElements = await aQuery.Skip((accessoryPage - 1) * pageSize).Take(pageSize).ToListAsync();
            var aPager = new Pager<Accessory>(aElements, await aQuery.CountAsync(), accessoryPage, pageSize);

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

        public async Task<IActionResult> Vendidos()
        {
            var model = new SoldItemViewModel
            {
                SoldMachines = await _context.Machines
                    .Include(m => m.Model)
                    .Include(m => m.Customer)
                    .Include (m => m.Model.Category)
                    .Where(m => m.CustomerId != null)
                    .ToListAsync(),

                SoldAccessories = await _context.Accessories
                    .Include(a => a.Customer)
                    .Where(a => a.CustomerId != null)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}
