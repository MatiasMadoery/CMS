using Microsoft.AspNetCore.Mvc;
using Control_Machine_Sistem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using QRCoder;
using Control_Machine_Sistem.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Control_Machine_Sistem.Controllers
{
    public class QrCodesController : Controller
    {
        private readonly AppDbContext _context;

        public QrCodesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: QrCodes/GenerateQr/5
        [HttpGet]
        public async Task<IActionResult> GenerateQr(int machineId)
        {
            var machine = await _context.Machines
                .Include(m => m.Customer)
                .Include(m => m.Model)
                .Include(m => m.Services)  // Incluimos los Services para poder trabajar con ellos
                .FirstOrDefaultAsync(m => m.Id == machineId);

            if (machine == null || machine.Model == null || machine.Customer == null)
            {
                return NotFound();
            }

            var customer = machine.Customer;

            // Obtenemos manuales y documentación igual que antes
            var manualUrls = machine.Model.ManualUrls ?? new List<string>();
            var manualUrl = manualUrls.Any()
                ? string.Join(", ", manualUrls)
                : "No hay manuales disponibles";

            var docUrls = machine.DocUrls ?? new List<string>();
            var docUrl = docUrls.Any()
                ? string.Join(", ", docUrls)
                : "No hay documentación disponible";

            var qrContentUrl = Url.Action("Details", "QrCodes", new { machineId }, Request.Scheme);
            var qrImageBase64 = GenerateQrCodeAsBase64(qrContentUrl!);
            
            var serviceUrls = new List<string>();
            if (machine.Services != null)
            {
                foreach (var service in machine.Services)
                {
                    if (service.ServiceSheetUrls != null && service.ServiceSheetUrls.Any())
                    {
                        serviceUrls.AddRange(service.ServiceSheetUrls);
                    }
                }
            }
            var serviceUrl = serviceUrls.Any()
                ? string.Join(", ", serviceUrls)
                : "No hay datos de service disponibles.";        

            
            var model = new QrCode
            {
                ClientName = customer.FullName,
                MachineModel = machine.Model.Name,
                ManualUrl = manualUrl,
                DocUrl = docUrl,
                ServiceUrl = serviceUrl,
                QrContentUrl = qrContentUrl,
                QrImageBase64 = qrImageBase64,             
                MachineId = machineId     
            };

            return View(model);
        }


        // Customer and machine selection page
        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customers.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToListAsync();

            var machines = await _context.Machines
                .Include(m => m.Customer)
                .Include(m => m.Model)
                .Include(m => m.Services)
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = $"{m.Customer!.Name} - {m.Model!.Name}"
                }).ToListAsync();

            var model = new QrCodeIndexViewModel
            {
                Clients = customers,
                Machines = machines
            };

            return View(model);
        }

        // QR generation
        [HttpPost]
        public async Task<IActionResult> postGenerateQr(int machineId)
        {
            var machine = await _context.Machines
                .Include(m => m.Customer)
                .Include(m => m.Model)
                .Include(m => m.Services)
                .FirstOrDefaultAsync(m => m.Id == machineId);

            if (machine == null || machine.Model == null || machine.Customer == null)
            {
                return NotFound();
            }

            var customer = machine.Customer;
            var manualUrls = machine.Model.ManualUrls ?? new List<string>();
            var manualUrl = manualUrls.Any()
                ? string.Join(", ", manualUrls)
                : "No hay manuales disponibles";

            var docUrls = machine.DocUrls ?? new List<string>();
            var docUrl = docUrls.Any()
                ? string.Join(", ", docUrls)
                : "No hay documentación disponible";

            var qrContentUrl = Url.Action("Details", "QrCodes", new { machineId }, Request.Scheme);
            var qrImageBase64 = GenerateQrCodeAsBase64(qrContentUrl!);

            var serviceUrls = new List<string>();
            if (machine.Services != null)
            {
                foreach (var service in machine.Services)
                {
                    if (service.ServiceSheetUrls != null && service.ServiceSheetUrls.Any())
                    {
                        serviceUrls.AddRange(service.ServiceSheetUrls);
                    }
                }
            }

            var serviceUrl = serviceUrls.Any()
                ? string.Join(", ", serviceUrls)
                : "No hay datos de service disponibles.";           

            var model = new QrCode
            {
                ClientName = customer.FullName,
                MachineModel = machine.Model.Name,
                ManualUrl = manualUrl,
                DocUrl = docUrl,
                ServiceUrl = serviceUrl,
                QrContentUrl = qrContentUrl,
                QrImageBase64 = qrImageBase64,
                MachineId = machineId,
            };

            return View(model);
        }

        private string GenerateQrCodeAsBase64(string content)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);

            return $"data:image/png;base64,{Convert.ToBase64String(qrCodeImage)}";
        }

        // Action to view the details of the scanned QR
        public async Task<IActionResult> Details(int machineId)
        {
            var machine = await _context.Machines
                .Include(m => m.Customer)
                .Include(m => m.Model)
                .Include(m => m.Services)
                .FirstOrDefaultAsync(m => m.Id == machineId);

            if (machine == null || machine.Model == null || machine.Customer == null)
            {
                return NotFound();
            }

            var manualUrls = machine.Model.ManualUrls;

            var manualUrl = manualUrls?.Any() == true
            ? string.Join(", ", manualUrls)
            : "No hay manuales disponibles";

            var docUrls = machine.DocUrls ?? new List<string>();

            var docUrl = docUrls?.Any() == true
                ? string.Join(", ", docUrls)
                : "No hay documentación disponible";

            var spareKitUrls = machine.Model.SpareKitsUrls ?? new List<string>();

            var spareKitUrl = spareKitUrls.Any()
                ? string.Join(",", spareKitUrls)
                : "No hay repuestos disponibles";

            var serviceUrls = new List<string>();
            if (machine.Services != null)
            {
                foreach (var service in machine.Services)
                {
                    if (service.ServiceSheetUrls != null && service.ServiceSheetUrls.Any())
                    {
                        serviceUrls.AddRange(service.ServiceSheetUrls);
                    }
                }
            }

            var serviceUrl = serviceUrls.Any()
                ? string.Join(", ", serviceUrls)
                : "No hay datos de service disponibles.";

            var model = new QrCode
            {
                ClientName = machine.Customer.FullName,
                MachineModel = machine.Model.Name,
                ManualUrl = manualUrl,
                DocUrl = docUrl,
                SpareKitsUrl = spareKitUrl,
                ServiceUrl = serviceUrl,
                MachineId = machineId,
                DeliveryDate = machine.DeliveryDate,                
            };

            return View(model);
        }

        // Acción para mostrar los manuales
        public IActionResult ManualList(string manualUrls, string spareKitUrls)
        {
            var manuals = (manualUrls ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(url => new FileDownloadViewModel
                {
                    OriginalName = Path.GetFileName(url),
                    DisplayName = Path.GetFileName(url).Split('_').Last()
                }).ToList();

            var spareKits = (spareKitUrls ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(url => new FileDownloadViewModel
                {
                    OriginalName = Path.GetFileName(url),
                    DisplayName = Path.GetFileName(url).Split('_').Last()
                }).ToList();

            var model = new ModelDownloadsViewModel
            {
                Manuals = manuals,
                SpareKits = spareKits
            };

            return View("ManualList", model);
        }


        public IActionResult DocumentList(string urls)
        {
            var urlArray = urls.Split(',');

            if (!urlArray.Any())
            {
                return NotFound();
            }

            var documents = urlArray.Select(url => new DocumentationViewModel
            {

                OriginalName = Path.GetFileName(url),

                DisplayName = Path.GetFileName(url).Split('_').Last()
            }).ToList();      
                      
            return View("DocumentList", documents);
        }


        public IActionResult ServiceList(string urls)
        {
            if (string.IsNullOrWhiteSpace(urls))
            {                
                return NotFound("No se han proporcionado datos del servicio técnico.");
            }

            var urlArray = urls.Split(',')
                                .Select(u => u.Trim())
                                .Where(u => !string.IsNullOrEmpty(u))
                                .ToArray();

            if (!urlArray.Any())
            {
                return NotFound("No se han encontrado datos del servicio técnico.");
            }

            var serviceDocuments = urlArray.Select(url => new DocumentationViewModel
            {
                OriginalName = Path.GetFileName(url),
                DisplayName = Path.GetFileName(url).Split('_').Last()
            }).ToList();

            return View("ServiceList", serviceDocuments);
        }

        public IActionResult DownloadService(string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "documentation", "serviceSheets", fileName);

            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine($"Archivo no encontrado en la ruta: {path}");
                return NotFound("Archivo no encontrado");
            }

            var contenido = System.IO.File.ReadAllBytes(path);

            return File(contenido, "application/pdf", fileName);
        }

        public IActionResult DownloadManual(string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "documentation", "manuals", fileName);

            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine($"Archivo no encontrado en la ruta: {path}");
                return NotFound("Archivo no encontrado");
            }

            var contenido = System.IO.File.ReadAllBytes(path);

            return File(contenido, "application/pdf", fileName);
        }

        public IActionResult DownloadDocument(string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "documentation", "machines", fileName);

            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine($"Archivo no encontrado en la ruta: {path}");
                return NotFound("Archivo no encontrado");
            }

            var contenido = System.IO.File.ReadAllBytes(path);

            return File(contenido, "application/pdf", fileName);
        }

        public IActionResult DownloadSpareKit(string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "documentation", "spareKits", fileName);

            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine($"Archivo no encontrado en la ruta: {path}");
                return NotFound("Archivo no encontrado");
            }

            var contenido = System.IO.File.ReadAllBytes(path);

            return File(contenido, "application/pdf", fileName);
        }

        public IActionResult DownloadServiceSheet(string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "documentation", "serviceSheets", fileName);

            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine($"Archivo no encontrado en la ruta: {path}");
                return NotFound("Archivo no encontrado");
            }

            var contenido = System.IO.File.ReadAllBytes(path);

            return File(contenido, "application/pdf", fileName);
        }
        public async Task<IActionResult> ServiceDetails(int machineId)
        {
            var services = await _context.Services
                .Where(s => s.MachineId == machineId)
                .ToListAsync();

            if (!services.Any())
            {
                return NotFound("No hay servicios registrados para esta máquina.");
            }

            ViewData["MachineId"] = machineId; 
            return View(services);
        }

    }
}