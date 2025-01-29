using Microsoft.AspNetCore.Mvc;
using Control_Machine_Sistem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using QRCoder;
using Control_Machine_Sistem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Control_Machine_Sistem.Controllers
{
    public class QrCodesController : Controller
    {
        private readonly AppDbContext _context;

        public QrCodesController(AppDbContext context)
        {
            _context = context;
        }

        // Customer and machine selection page
        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customers.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToListAsync();

            var machines = await _context.Machines.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Model
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
        public async Task<IActionResult> GenerateQr(int customerId, int machineId)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
            var machine = await _context.Machines.FirstOrDefaultAsync(m => m.Id == machineId);

            if (customer == null || machine == null)
            {
                return NotFound();
            }

            var model = new QrCode
            {
                ClientName = customer.Name,
                UserManualUrl = Url.Content("~/manuals/user/Usuario.pdf"),
                MachineManualUrl = Url.Content($"~/manuals/machines/{machine.ManualPath}"),
                ServiceDataUrl = Url.Content($"~/manuals/servicesData/{machine.ServiceManualPath}"),
                QrContentUrl = Url.Action("Details", "QrCodes", new { customerId, machineId }, Request.Scheme),
                QrImageBase64 = GenerateQrCodeAsBase64(Url.Action("Details", "QrCodes", new { customerId, machineId }, Request.Scheme))
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
        public async Task<IActionResult> Details(int customerId, int machineId)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
            var machine = await _context.Machines.FirstOrDefaultAsync(m => m.Id == machineId);

            if (customer == null || machine == null)
            {
                return NotFound();
            }

            var model = new QrCode
            {
                ClientName = customer.Name,
                UserManualUrl = Url.Content("~/manuals/user/Usuario.pdf"),
                MachineManualUrl = Url.Content($"~/manuals/machines/{machine.ManualPath}"),
                ServiceDataUrl = Url.Content($"~/manuals/servicesData/{machine.ServiceManualPath}")
            };

            return View(model);
        }

    }
}
