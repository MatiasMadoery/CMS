using ClosedXML.Excel;
using Control_Machine_Sistem.Models;
using Control_Machine_Sistem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static Control_Machine_Sistem.Services.FileService;

namespace Control_Machine_Sistem.Controllers
{
    [Authorize(Roles = "Admin, Técnico, SuperAdmin,Viewer")]
    public class MachinesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IImageStorageService _imageStorageService; // <--- AGREGAR ESTO

        public MachinesController(AppDbContext context, IImageStorageService imageStorageService)
        {
            _context = context;
            _imageStorageService = imageStorageService; // <--- ASIGNAR
        }

        // GET: Machines
        [Authorize(Roles = "Admin, Técnico, SuperAdmin,Viewer")]
        public async Task<IActionResult> Index(string searchString, int? categoryId, int page = 1, int pageSize = 5)
        {
            var machine = _context.Machines!
                          .Include(m => m.Customer)
                          .Include(m => m.Model)
                          .OrderBy(m => m.Id)
                          .AsQueryable();

            //Filter by search text if provided
            if (!String.IsNullOrEmpty(searchString))
            {
                machine = machine.Where(s => s.Customer!.Name!.Contains(searchString) || s.Customer.LastName!.Contains(searchString) ||
                s.Model!.Name!.Contains(searchString));
            }

            //Filter by category
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                machine = machine.Where(s => s.Model!.CategoryId == categoryId.Value);
            }

            var totalMachines = await machine.CountAsync();

            var machinePager = await machine
                           .Skip((page - 1) * pageSize)
                           .Take(pageSize)
                           .ToListAsync();

            var pager = new Pager<Machine>(machinePager, totalMachines, page, pageSize);

            ViewData["searchString"] = searchString;
            ViewData["categoryId"] = categoryId;

            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name", categoryId);

            return View(pager);
        }

        // GET: Machines/Details/5
        [Authorize(Roles = "Admin, Técnico, SuperAdmin,Viewer")]
        public async Task<IActionResult> Details(int? id, bool isStock = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _context.Machines
                .Include(m => m.Customer)
                .Include(m => m.Model)
                .Include(m => m.OwnerHistories)
                .Include(m => m.Ubication)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (machine == null)
            {
                return NotFound();
            }

            ViewBag.IsStock = isStock;
            return View(machine);
        }

        // GET: Machines/Create
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        public IActionResult Create(bool isStock = false)
        {
            ViewBag.IsStock = isStock;
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.ModelId = new SelectList(new List<Model>(), "Id", "Name");
            ViewBag.Customers = new SelectList(_context.Customers.Select(c => new { c.Id, c.Name }), "Id", "Name");
            ViewBag.UbicationId = new SelectList(_context.Ubications.OrderBy(u => u.Name), "Id", "Name");

            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetModelsByCategory(int categoryId)
        {
            var models = await _context.Models
                                       .Where(m => m.CategoryId == categoryId)
                                       .Select(m => new { id = m.Id, name = m.Name })
                                       .ToListAsync();

            return Json(models);
        }

        // POST: Machines/Create
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(104857600)]
        public async Task<IActionResult> Create([Bind("Id,CustomerId,ModelId,ChasisNumber,EngineNumber,DeliveryDate,Documentations,ImageFiles,CheckListFile,ManufactureYear,SerialNumber,UserHours,UbicationId,,ImportNumber,OfficializationDate")] Machine machine, bool isStock = false)
        {
            if (isStock)
            {
                machine.CustomerId = null;
                machine.DeliveryDate = null;
                machine.WarrantyExpirationDate = null;

                ModelState.Remove("CustomerId");
                ModelState.Remove("DeliveryDate");
            }
            else
            {
                if (machine.DeliveryDate.HasValue)
                {
                    machine.WarrantyExpirationDate = machine.DeliveryDate.Value.AddDays(365);
                }
            }

            if (ModelState.IsValid)
            {
                List<string> docUrls = new List<string>();

                if (machine.Documentations != null && machine.Documentations.Any())
                {
                    foreach (var manual in machine.Documentations)
                    {
                        var fileExtension = Path.GetExtension(manual.FileName).ToLower();
                        if (fileExtension != ".pdf")
                        {
                            ModelState.AddModelError("Manuals", "Solo se permiten archivos PDF.");
                            return View(machine);
                        }
                    }
                    docUrls = await FileService.SaveDocAsync(machine.Documentations.ToList(), "documentation/machines");
                }

                // --- NUEVA LÓGICA: PROCESAR IMÁGENES PARA AZURE ---
                List<string> imageUrls = new List<string>();
                if (machine.ImageFiles != null && machine.ImageFiles.Any())
                {
                    foreach (var photo in machine.ImageFiles.Take(4)) // Limitamos a 4 por seguridad
                    {
                        // Subimos a Azure y obtenemos la URL
                        string url = await _imageStorageService.UploadImageAsync(photo, "machine-photos");
                        if (!string.IsNullOrEmpty(url))
                        {
                            imageUrls.Add(url);
                        }
                    }
                }

                // --- NUEVA LÓGICA: PROCESAR CHECKLIST PDF ---
                string checkListUrl = null;
                if (machine.CheckListFile != null)
                {
                    var extension = Path.GetExtension(machine.CheckListFile.FileName).ToLower();
                    if (extension == ".pdf")
                    {
                        // Reutilizamos el servicio de Azure subiendo al contenedor "checklists"
                        checkListUrl = await _imageStorageService.UploadImageAsync(machine.CheckListFile, "checklists");
                    }
                    else
                    {
                        ModelState.AddModelError("CheckListFile", "El Check List debe ser un archivo PDF.");
                        // Recargar ViewBags si hay error...
                        return View(machine);
                    }
                }

                var newMachine = new Machine
                {
                    CustomerId = machine.CustomerId,
                    ModelId = machine.ModelId,
                    ChasisNumber = machine.ChasisNumber,
                    EngineNumber = machine.EngineNumber,
                    DeliveryDate = machine.DeliveryDate,
                    WarrantyExpirationDate = machine.DeliveryDate?.AddDays(365),
                    DocUrls = docUrls,
                    ImageUrls = imageUrls, // <--- ASIGNAMOS LAS URLS DE AZURE
                    CheckListUrl = checkListUrl, // <--- ASIGNAMOS LA URL DE AZURITE
                    ManufactureYear = machine.ManufactureYear,
                    SerialNumber = machine.SerialNumber,
                    UserHours = machine.UserHours,
                    UbicationId = machine.UbicationId,
                    ImportNumber = machine.ImportNumber,
                    OfficializationDate = machine.OfficializationDate
                };
                _context.Add(newMachine);
                await _context.SaveChangesAsync();
                return isStock ? RedirectToAction("Index", "Stock") : RedirectToAction("Vendidos", "Stock");
            }

            ViewBag.IsStock = isStock;
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.ModelId = new SelectList(new List<Model>(), "Id", "Name");
            ViewBag.Customers = new SelectList(_context.Customers.Select(c => new { c.Id, c.Name }), "Id", "Name");
            ViewBag.UbicationId = new SelectList(_context.Ubications, "Id", "Name", machine.UbicationId);
            return View(machine);
        }

        // GET: Machines/Edit/5
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        public async Task<IActionResult> Edit(int? id, bool isStock = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _context.Machines
            .Include(m => m.Customer)
            .FirstOrDefaultAsync(m => m.Id == id);

            if (machine == null)
            {
                return NotFound();
            }

            ViewBag.IsStock = isStock;
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", machine.CustomerId);
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Name", machine.ModelId);
            ViewBag.CustomerName = machine.Customer?.Name;
            ViewBag.UbicationId = new SelectList(_context.Ubications.OrderBy(u => u.Name), "Id", "Name", machine.UbicationId);

            return View(machine);
        }

        // POST: Machines/Edit/5
        [Authorize(Roles = "Admin, Técnico, SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(104857600)]
        public async Task<IActionResult> Edit(
            int id,
          [Bind("Id,CustomerId,ModelId,ChasisNumber,EngineNumber,DeliveryDate,WarrantyExpirationDate,ManufactureYear,SerialNumber,UserHours,UbicationId,ImportNumber,OfficializationDate")] Machine machine,
            List<string> ExistingDocs,
            List<IFormFile> Documentations,
            List<string> DeletedDocs,
            List<string> ExistingImageUrls, // <--- Fotos que se quedaron
            List<string> DeletedImageUrls,  // <--- Fotos marcadas para borrar
            List<IFormFile> ImageFiles,      // <--- Fotos nuevas
            IFormFile? CheckListFile, // <--- Nuevo  CheckList
            bool DeleteChecklist = false, // <--- Marcar para eliminar CheckList sin reemplazo
            bool isStock = false
            )
        {
            if (id != machine.Id)
            {
                return NotFound();
            }

            if (isStock)
            {
                machine.CustomerId = null;
                machine.DeliveryDate = null;
                ModelState.Remove("CustomerId");
                ModelState.Remove("DeliveryDate");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingMachine = await _context.Machines
                        .Include(m => m.Customer)
                        .Include(m => m.OwnerHistories)
                        .FirstOrDefaultAsync(m => m.Id == id);

                    if (existingMachine == null)
                    {
                        return NotFound();
                    }

                    if (!isStock && existingMachine.CustomerId != machine.CustomerId && existingMachine.Customer != null)
                    {
                        _context.OwnerHistories.Add(new OwnerHistory
                        {
                            MachineId = existingMachine.Id,
                            PreviousOwner = existingMachine.Customer.Name,
                            ChangeDate = DateTime.Now
                        });
                    }

                    existingMachine.CustomerId = machine.CustomerId;
                    existingMachine.ModelId = machine.ModelId;
                    existingMachine.ChasisNumber = machine.ChasisNumber;
                    existingMachine.EngineNumber = machine.EngineNumber;
                    existingMachine.DeliveryDate = machine.DeliveryDate;
                    existingMachine.WarrantyExpirationDate = machine.DeliveryDate?.AddDays(365);
                    existingMachine.ManufactureYear = machine.ManufactureYear;
                    existingMachine.SerialNumber = machine.SerialNumber;
                    existingMachine.UserHours = machine.UserHours;
                    existingMachine.UbicationId = machine.UbicationId;
                    existingMachine.ImportNumber = machine.ImportNumber;
                    existingMachine.OfficializationDate = machine.OfficializationDate;

                    List<string> docUrls = ExistingDocs ?? new List<string>();

                    if (DeletedDocs != null && DeletedDocs.Any())
                    {
                        foreach (var url in DeletedDocs)
                        {
                            var fileName = Path.GetFileName(url);
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "documentation", "machines", fileName);

                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                        }

                        docUrls = docUrls.Except(DeletedDocs).ToList();
                    }

                    if (Documentations != null && Documentations.Any())
                    {
                        foreach (var document in Documentations)
                        {
                            var fileExtension = Path.GetExtension(document.FileName).ToLower();
                            if (fileExtension != ".pdf")
                            {
                                ModelState.AddModelError("Manuals", "Solo se permiten archivos PDF.");
                                return View(machine);
                            }
                        }
                        docUrls.AddRange(await FileService.SaveDocAsync(Documentations));
                    }

                    existingMachine.DocUrls = docUrls;

                    // --- GESTIÓN DE IMÁGENES (Azure Storage) ---
                    List<string> imageUrls = ExistingImageUrls ?? new List<string>();

                    // 1. Borrar de Azure las fotos eliminadas
                    if (DeletedImageUrls != null && DeletedImageUrls.Any())
                    {
                        foreach (var url in DeletedImageUrls)
                        {
                            await _imageStorageService.DeleteImageAsync(url, "machine-photos");
                        }
                        imageUrls = imageUrls.Except(DeletedImageUrls).ToList();
                    }

                    // 2. Subir fotos nuevas a Azure
                    if (ImageFiles != null && ImageFiles.Any())
                    {
                        foreach (var photo in ImageFiles)
                        {
                            string newUrl = await _imageStorageService.UploadImageAsync(photo, "machine-photos");
                            if (!string.IsNullOrEmpty(newUrl)) imageUrls.Add(newUrl);
                        }
                    }

                    // Limitamos a 4 por si acaso
                    existingMachine.ImageUrls = imageUrls.Take(4).ToList();

                    // --- GESTIÓN DE CHECKLIST (Azure Storage) ---

                    if (CheckListFile != null) // Caso 1: El usuario subió un archivo nuevo (reemplaza o crea)
                    {
                        var extension = Path.GetExtension(CheckListFile.FileName).ToLower();
                        if (extension != ".pdf")
                        {
                            ModelState.AddModelError("CheckListFile", "El Check List debe ser un archivo PDF.");
                            // Recargar datos necesarios para la vista
                            ViewData["UbicationId"] = new SelectList(_context.Ubications, "Id", "Name", machine.UbicationId);
                            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Name", machine.ModelId);
                            return View(machine);
                        }

                        // 1. Borrar el anterior si existe
                        if (!string.IsNullOrEmpty(existingMachine.CheckListUrl))
                        {
                            await _imageStorageService.DeleteImageAsync(existingMachine.CheckListUrl, "checklists");
                        }

                        // 2. Subir el nuevo
                        existingMachine.CheckListUrl = await _imageStorageService.UploadImageAsync(CheckListFile, "checklists");
                    }
                    else if (DeleteChecklist) // Caso 2: El usuario marcó "Eliminar" y no subió nada nuevo
                    {
                        if (!string.IsNullOrEmpty(existingMachine.CheckListUrl))
                        {
                            await _imageStorageService.DeleteImageAsync(existingMachine.CheckListUrl, "checklists");
                            existingMachine.CheckListUrl = null; // Limpiamos la referencia en la DB
                        }
                    }

                    _context.Update(existingMachine);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MachineExists(machine.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return isStock ? RedirectToAction("Index", "Stock") : RedirectToAction("Vendidos", "Stock");
            }

            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id", machine.CustomerId);
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Id", machine.ModelId);
            ViewBag.IsStock = isStock;
            return View(machine);
        }

        // GET: Machines/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public async Task<IActionResult> Delete(int? id, bool isStock = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _context.Machines
                .Include(m => m.Customer)
                .Include(m => m.Model)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (machine == null)
            {
                return NotFound();
            }

            ViewBag.IsStock = isStock;
            return View(machine);
        }

        // POST: Machines/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, bool isStock = false)
        {
            var machine = await _context.Machines.FindAsync(id);
            if (machine != null)
            {
                if (machine.DocUrls != null && machine.DocUrls.Any())
                {
                    foreach (var fileUrl in machine.DocUrls)
                    {
                        await FileService.DeleteDocumentationFileAsync(fileUrl);
                    }
                }

                // --- Borrar Fotos de Azure ---
                if (machine.ImageUrls != null)
                {
                    foreach (var url in machine.ImageUrls)
                        await _imageStorageService.DeleteImageAsync(url, "machine-photos");
                }

                // --- BORRAR CHECKLIST DE AZURE ---
                if (!string.IsNullOrEmpty(machine.CheckListUrl))
                {
                    await _imageStorageService.DeleteImageAsync(machine.CheckListUrl, "checklists");
                }

                _context.Machines.Remove(machine);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Máquina eliminada correctamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se encontró la máquina a eliminar.";
            }

            return isStock ? RedirectToAction("Index", "Stock") : RedirectToAction("Vendidos", "Stock");
        }

        private bool MachineExists(int id)
        {
            return _context.Machines.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<JsonResult> GetCustomers(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return Json(new List<object>());
            }

            var customers = await _context.Customers
                                          .Where(c => c.Name!.ToLower().Contains(term.ToLower()))
                                          .Select(c => new
                                          {
                                              id = c.Id,
                                              text = c.Name
                                          })
                                          .Take(10)
                                          .ToListAsync();

            Console.WriteLine($"Clientes encontrados: {customers.Count}");
            return Json(customers);
        }

        public async Task<IActionResult> ExportarExcel()
        {
            var machines = await _context.Machines!
                .Include(m => m.Model)
                    .ThenInclude(mod => mod.Category)
                .Include(m => m.Ubication)
                .OrderBy(m => m.Id)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Stock de Maquinaria");

                string[] headers = {
            "Categoría", "Modelo", "N° Importación", "Fecha Oficialización",
            "N° Chasis", "N° Motor", "N° Serie", "Año Fabricación",
            "Horas de Uso", "Ubicación"
        };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                int row = 2;
                foreach (var item in machines)
                {
                    worksheet.Cell(row, 1).Value = item.Model?.Category?.Name ?? "N/A";
                    worksheet.Cell(row, 2).Value = item.Model?.Name ?? "N/A";
                    worksheet.Cell(row, 3).Value = item.ImportNumber;
                    worksheet.Cell(row, 4).Value = item.OfficializationDate.ToShortDateString();
                    worksheet.Cell(row, 5).Value = item.ChasisNumber;
                    worksheet.Cell(row, 6).Value = item.EngineNumber;
                    worksheet.Cell(row, 7).Value = item.SerialNumber;
                    worksheet.Cell(row, 8).Value = item.ManufactureYear;
                    worksheet.Cell(row, 9).Value = item.UserHours;
                    worksheet.Cell(row, 10).Value = item.Ubication?.Name ?? "N/A";
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