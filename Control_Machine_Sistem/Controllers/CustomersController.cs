using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Control_Machine_Sistem.Models;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Microsoft.AspNetCore.Authorization;

namespace Control_Machine_Sistem.Controllers
{
    [Authorize(Roles = "Admin, Técnico, SuperAdmin,Viewer")]
    public class CustomersController : Controller
    {
        private readonly AppDbContext _context;

        public CustomersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Customers
        [Authorize(Roles = "Viewer, Admin, Técnico, SuperAdmin")]
        public async Task<IActionResult> Index(string searchString, int page = 1, int pageSize = 5)
        {
            var customer = from c in _context.Customers select c;

            //Filter by search text if provided
            if (!String.IsNullOrEmpty(searchString))
            {
                customer = customer.Where(s => s.Name!.Contains(searchString) || s.LastName!.Contains(searchString));
            }

            // Get total customers 
            var totalCustomers = await customer.CountAsync();

            // Apply pagination
            var customersPager = await customer
                                         .Skip((page - 1) * pageSize)
                                         .Take(pageSize)
                                         .ToListAsync();

            // Create the paginator with the paginated list
            var pager = new Pager<Customer>(customersPager, totalCustomers, page, pageSize);

            //To maintain the value of the lookup field when the user changes pages
            ViewData["searchString"] = searchString;
            return View(pager);
        }

        // GET: Customers/Details/5
        [Authorize(Roles = "Viewer, Admin, Técnico, SuperAdmin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customers/Create
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,LastName,Cuit,Phone,Email,Address,City,PostalCode,Province,Country")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Edit/5
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,LastName,Cuit,Phone,Email,Address,City,PostalCode,Province,Country")] Customer customer)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.Id))
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
            return View(customer);
        }

        // GET: Customers/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers
            .Include(c => c.Machines)
            .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            if (customer.Machines!.Any())
            {
                TempData["ErrorMessage"] = "No se puede eliminar el cliente porque tiene máquinas asociadas.";
                return RedirectToAction(nameof(Index));
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cliente eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }

        public IActionResult ImportExcel()
        {
            return View();
        }

        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "No se seleccionó un archivo Excel.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    var workbook = new XSSFWorkbook(stream);
                    var sheet = workbook.GetSheetAt(0);

                    var importedCustomers = new List<Customer>();

                    for (int row = 1; row <= sheet.LastRowNum; row++)
                    {
                        var currentRow = sheet.GetRow(row);
                        if (currentRow == null || currentRow.Cells.All(c => c.CellType == CellType.Blank)) continue;

                        var customer = new Customer
                        {
                            Name = GetCellValue(currentRow.GetCell(0)),
                            LastName = GetCellValue(currentRow.GetCell(1)),
                            Cuit = GetCellValue(currentRow.GetCell(2)),
                            Phone = GetCellValue(currentRow.GetCell(3)),
                            Email = GetCellValue(currentRow.GetCell(4)),
                            Address = GetCellValue(currentRow.GetCell(5)),
                            City = GetCellValue(currentRow.GetCell(6)),
                            PostalCode = GetCellValue(currentRow.GetCell(7)),
                            Province = GetCellValue(currentRow.GetCell(8)),
                            Country = GetCellValue(currentRow.GetCell(9))
                        };

                        // Validaciones mínimas antes de agregar
                        if (string.IsNullOrWhiteSpace(customer.Name) || string.IsNullOrWhiteSpace(customer.Phone) || string.IsNullOrWhiteSpace(customer.Email))
                        {
                            TempData["ErrorMessage"] = $"Error en fila {row + 1}: Faltas datos en campos obligatorios.";
                            return RedirectToAction(nameof(Index));
                        }

                        importedCustomers.Add(customer);
                    }

                    if (importedCustomers.Any())
                    {
                        _context.Customers.AddRange(importedCustomers);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = $"Se importaron {importedCustomers.Count} clientes correctamente.";
                    }
                    else
                    {
                        TempData["WarningMessage"] = "No se encontraron clientes válidos para importar.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Hubo un error al procesar el archivo Excel: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private string GetCellValue(ICell cell)
        {
            if (cell == null) return string.Empty;

            try
            {
                switch (cell.CellType)
                {
                    case CellType.String:
                        return cell.StringCellValue?.Trim() ?? "";

                    case CellType.Numeric:
                        if (DateUtil.IsCellDateFormatted(cell))
                        {
                            DateTime? fecha = cell.DateCellValue;
                            return fecha.HasValue ? fecha.Value.ToString("dd/MM/yyyy") : "";

                        }
                        return cell.NumericCellValue.ToString();

                    case CellType.Boolean:
                        return cell.BooleanCellValue.ToString();

                    case CellType.Formula:
                        var evaluator = cell.Sheet.Workbook.GetCreationHelper().CreateFormulaEvaluator();
                        var evaluatedCell = evaluator.EvaluateInCell(cell);
                        return GetCellValue(evaluatedCell);

                    default:
                        return cell.ToString().Trim();
                }
            }
            catch
            {
                return "";
            }
        }

    }
}
