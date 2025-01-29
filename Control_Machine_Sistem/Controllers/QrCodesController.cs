﻿using Microsoft.AspNetCore.Mvc;
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

            var machines = await _context.Machines
                .Include(m => m.Customer)
                .Include(m => m.Model)
                .Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = $"{m.Customer.Name} - {m.Model.Name}"
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
        public async Task<IActionResult> GenerateQr(int machineId)
        {
            var machine = await _context.Machines
                .Include(m => m.Customer)
                .Include(m => m.Model)
                .FirstOrDefaultAsync(m => m.Id == machineId);

            if (machine == null || machine.Model == null || machine.Customer == null)
            {
                return NotFound();
            }

            var customer = machine.Customer; 
            var manualUrls = machine.Model.ManualUrls ?? new List<string>();
            var manualUrl = manualUrls.Any()
                ? string.Join(", ", manualUrls)
                : "No manual available";

            var qrContentUrl = Url.Action("Details", "QrCodes", new { machineId }, Request.Scheme);

            var qrImageBase64 = GenerateQrCodeAsBase64(qrContentUrl);

            var model = new QrCode
            {
                ClientName = customer.Name,
                MachineModel = machine.Model.Name,
                ManualUrl = manualUrl,
                QrContentUrl = qrContentUrl,
                QrImageBase64 = qrImageBase64
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
                .FirstOrDefaultAsync(m => m.Id == machineId);

            if (machine == null || machine.Model == null || machine.Customer == null)
            {
                return NotFound();
            }

            var manualUrls = machine.Model.ManualUrls;

            var manualUrl = manualUrls?.Any() == true
            ? string.Join(", ", manualUrls)
            : "No manual available";

            var model = new QrCode
            {
                ClientName = machine.Customer.Name,
                MachineModel = machine.Model.Name,
                ManualUrl = manualUrl
            };

            return View(model);
        }

    }
}
