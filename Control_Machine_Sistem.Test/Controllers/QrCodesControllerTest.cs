using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Control_Machine_Sistem.Controllers;
using Control_Machine_Sistem.Models;
using Control_Machine_Sistem.ViewModels;  // Asumimos que QrCode se encuentra aquí
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Control_Machine_Sistem.Test.Controllers
{
    public class QrCodesControllerTests
    {
        private AppDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        // --- Test 1: Cuando no existe la máquina ---
        [Fact]
        public async Task GenerateQr_ReturnsNotFound_WhenMachineDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new QrCodesController(context);

            // Configuramos el UrlHelper y Request.Scheme
            controller.Url = new FakeUrlHelper();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Scheme = "http";

            // Act
            var result = await controller.GenerateQr(99);  // machineId inexistente

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // --- Test 2: Cuando la máquina existe pero Model es null ---
        [Fact]
        public async Task GenerateQr_ReturnsNotFound_WhenModelIsNull()
        {
            // Arrange
            var context = GetInMemoryContext();

            // Insertar Customer pero no Model
            var customer = new Customer { Id = 1, Name = "John", LastName = "Doe"};
            context.Customers.Add(customer);

            // Inserto una máquina con Model = null
            context.Machines.Add(new Machine
            {
                Id = 1,
                CustomerId = customer.Id,
                ModelId = 0,  // Valor inválido
                Customer = customer,
                Model = null,
                DocUrls = new List<string> { "doc1.pdf" },
                // Completar campos obligatorios
                ChasisNumber = "CH001",
                EngineNumber = "EN001",
                DeliveryDate = DateTime.Now,
                WarrantyExpirationDate = DateTime.Now.AddYears(1)
            });
            await context.SaveChangesAsync();

            var controller = new QrCodesController(context);
            controller.Url = new FakeUrlHelper();
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            controller.ControllerContext.HttpContext.Request.Scheme = "http";

            // Act
            var result = await controller.GenerateQr(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // --- Test 3: Cuando la máquina existe pero Customer es null ---
        [Fact]
        public async Task GenerateQr_ReturnsNotFound_WhenCustomerIsNull()
        {
            // Arrange
            var context = GetInMemoryContext();

            // Insertar Model pero no Customer
            var modelEntity = new Model { Id = 1, Name = "Excavator", CategoryId = 1, ManualUrls = new List<string> { "manual1.pdf" } };
            context.Models.Add(modelEntity);

            // Insertar máquina con Customer null
            context.Machines.Add(new Machine
            {
                Id = 1,
                CustomerId = 0,
                ModelId = modelEntity.Id,
                Customer = null,
                Model = modelEntity,
                DocUrls = new List<string> { "doc1.pdf" },
                ChasisNumber = "CH001",
                EngineNumber = "EN001",
                DeliveryDate = DateTime.Now,
                WarrantyExpirationDate = DateTime.Now.AddYears(1)
            });
            await context.SaveChangesAsync();

            var controller = new QrCodesController(context);
            controller.Url = new FakeUrlHelper();
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            controller.ControllerContext.HttpContext.Request.Scheme = "http";

            // Act
            var result = await controller.GenerateQr(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // --- Test 4: Escenario válido ---
        [Fact]
        public async Task GenerateQr_ReturnsViewResult_WithQrCodeModel()
        {
            // Arrange
            var context = GetInMemoryContext();

            // Agregar Customer y Model (con ManualUrls completos)
            var customer = new Customer { Id = 1, Name = "John", LastName = "Doe"};
            var modelEntity = new Model
            {
                Id = 1,
                Name = "Excavator",
                CategoryId = 1,
                ManualUrls = new List<string> { "manual1.pdf", "manual2.pdf" }
            };
            context.Customers.Add(customer);
            context.Models.Add(modelEntity);

            // Definir un Service con ServiceSheetUrls
            var service = new Service
            {
                Id = 1,
                ServiceSheetUrls = new List<string> { "service1.pdf", "service2.pdf" }
            };

            // Agregar la máquina completa
            var machine = new Machine
            {
                Id = 1,
                CustomerId = customer.Id,
                ModelId = modelEntity.Id,
                Customer = customer,
                Model = modelEntity,
                DocUrls = new List<string> { "doc1.pdf", "doc2.pdf" },
                Services = new List<Service> { service },
                ChasisNumber = "CH001",
                EngineNumber = "EN001",
                DeliveryDate = DateTime.Now,
                WarrantyExpirationDate = DateTime.Now.AddYears(1)
            };
            context.Machines.Add(machine);
            await context.SaveChangesAsync();

            var controller = new QrCodesController(context);
            controller.Url = new FakeUrlHelper();
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            controller.ControllerContext.HttpContext.Request.Scheme = "http";

            // Act
            var result = await controller.GenerateQr(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var qrModel = Assert.IsType<QrCode>(viewResult.Model);

            // Verificamos los valores del modelo
            Assert.Equal("John Doe", qrModel.ClientName);
            Assert.Equal("Excavator", qrModel.MachineModel);
            // Se debe concatenar manuales y documentación como se espera:
            Assert.Equal("manual1.pdf, manual2.pdf", qrModel.ManualUrl);
            Assert.Equal("doc1.pdf, doc2.pdf", qrModel.DocUrl);
            // Service URL: se concatena la lista de ServiceSheetUrls
            Assert.Equal("service1.pdf, service2.pdf", qrModel.ServiceUrl);
            // El QrContentUrl se genera usando nuestro FakeUrlHelper:
            Assert.Equal("http://localhost/QrCodes/Details?machineId=1", qrModel.QrContentUrl);
            // Y se espera que la imagen en base64 tenga algún contenido (no vacío)
            Assert.False(string.IsNullOrEmpty(qrModel.QrImageBase64));
            Assert.Equal(1, qrModel.MachineId);
        }

        // Escenario 1: Sin datos en la base
        [Fact]
        public async Task Index_ReturnsViewResult_WithEmptyLists_WhenNoDataExists()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new QrCodesController(context);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<QrCodeIndexViewModel>(viewResult.Model);
            Assert.Empty(model.Clients);
            Assert.Empty(model.Machines);
        }

        // Escenario 2: Con datos disponibles, se deben llenar Clients y Machines
        [Fact]
        public async Task Index_ReturnsViewResult_WithPopulatedClientsAndMachines()
        {
            // Arrange
            var context = GetInMemoryContext();

            // Insertar un cliente
            var customer = new Customer { Id = 1, Name = "John", LastName = "Doe" };
            context.Customers.Add(customer);

            // Insertar un modelo
            var modelEntity = new Model { Id = 1, Name = "Excavator", CategoryId = 1 };
            context.Models.Add(modelEntity);

            // Insertar una máquina asociada (con Customer y Model)
            context.Machines.Add(new Machine
            {
                Id = 1,
                CustomerId = customer.Id,
                ModelId = modelEntity.Id,
                Customer = customer,
                Model = modelEntity,
                ChasisNumber = "CH001",
                EngineNumber = "EN001",
                DeliveryDate = DateTime.Now,
                WarrantyExpirationDate = DateTime.Now.AddYears(1),
                Services = new List<Service>()
            });
            await context.SaveChangesAsync();

            var controller = new QrCodesController(context);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = Assert.IsType<QrCodeIndexViewModel>(viewResult.Model);

            // Verificar Clients
            Assert.Single(viewModel.Clients);
            var clientItem = viewModel.Clients[0];
            Assert.Equal("1", clientItem.Value);
            Assert.Equal("John", clientItem.Text);

            // Verificar Machines (Texto: "Customer.Name - Model.Name")
            Assert.Single(viewModel.Machines);
            var machineItem = viewModel.Machines[0];
            Assert.Equal("1", machineItem.Value);
            Assert.Equal("John - Excavator", machineItem.Text);
        }

        // Escenario 1: Cuando no se encuentra la máquina (o alguno de sus elementos requeridos), se debe retornar NotFound.
        [Fact]
        public async Task postGenerateQr_ReturnsNotFound_WhenMachineDoesNotExistOrIncomplete()
        {
            // Arrange
            var context = GetInMemoryContext();
            // No insertamos ninguna máquina para forzar NotFound.
            var controller = new QrCodesController(context);
            controller.Url = new FakeUrlHelper();
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            controller.ControllerContext.HttpContext.Request.Scheme = "http";

            // Act
            var result = await controller.postGenerateQr(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }


        // Escenario 2: Cuando la máquina existe y tiene datos completos, se debe retornar un ViewResult con un modelo QrCode poblado.
        [Fact]
        public async Task postGenerateQr_ReturnsViewResult_WithQrCodeModel()
        {
            // Arrange
            var context = GetInMemoryContext();

            // Crear entidades requeridas: Customer, Model y asignar datos a la máquina.
            var customer = new Customer
            {
                Id = 1,
                Name = "John",
                LastName = "Doe"                
            };
            var modelEntity = new Model
            {
                Id = 1,
                Name = "Excavator",
                CategoryId = 1,
                ManualUrls = new List<string> { "manual1.pdf", "manual2.pdf" }
            };
            // Crear una máquina completa, incluyendo DocUrls y asignando Services con ServiceSheetUrls.
            var service = new Service
            {
                Id = 1,
                ServiceSheetUrls = new List<string> { "service1.pdf", "service2.pdf" }
            };
            var machine = new Machine
            {
                Id = 1,
                CustomerId = customer.Id,
                ModelId = modelEntity.Id,
                Customer = customer,
                Model = modelEntity,
                DocUrls = new List<string> { "doc1.pdf", "doc2.pdf" },
                Services = new List<Service> { service },
                ChasisNumber = "CH001",
                EngineNumber = "EN001",
                DeliveryDate = DateTime.Now,
                WarrantyExpirationDate = DateTime.Now.AddYears(1)
            };

            context.Customers.Add(customer);
            context.Models.Add(modelEntity);
            context.Machines.Add(machine);
            await context.SaveChangesAsync();

            var controller = new QrCodesController(context);
            controller.Url = new FakeUrlHelper();
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            controller.ControllerContext.HttpContext.Request.Scheme = "http";

            // Act
            var result = await controller.postGenerateQr(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var qrModel = Assert.IsType<QrCode>(viewResult.Model);

            // Verificar valores en el modelo
            Assert.Equal("John Doe", qrModel.ClientName);
            Assert.Equal("Excavator", qrModel.MachineModel);
            Assert.Equal("manual1.pdf, manual2.pdf", qrModel.ManualUrl);
            Assert.Equal("doc1.pdf, doc2.pdf", qrModel.DocUrl);
            Assert.Equal("service1.pdf, service2.pdf", qrModel.ServiceUrl);
            Assert.Equal("http://localhost/QrCodes/Details?machineId=1", qrModel.QrContentUrl);
            Assert.StartsWith("data:image/png;base64,", qrModel.QrImageBase64);
            Assert.Equal(1, qrModel.MachineId);
        }

        // Escenario 1: Retorna NotFound si no se encuentra la máquina.
        [Fact]
        public async Task Details_ReturnsNotFound_WhenMachineDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new QrCodesController(context);

            // Act
            var result = await controller.Details(99); // machineId inexistente

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // Escenario 2: Retorna NotFound si la máquina existe pero alguno de sus elementos requeridos (Model o Customer) es nulo.
        [Fact]
        public async Task Details_ReturnsNotFound_WhenModelOrCustomerIsNull()
        {
            // Arrange
            var context = GetInMemoryContext();

            // Crear un Customer pero no se asigna un Model válido
            var customer = new Customer
            {
                Id = 1,
                Name = "John",
                LastName = "Doe"
            };
            context.Customers.Add(customer);

            // Insertar una máquina con Model nulo
            context.Machines.Add(new Machine
            {
                Id = 1,
                CustomerId = customer.Id,
                ModelId = 0, // valor que indique ausencia; se dejará sin asignar relación
                Customer = customer,
                Model = null,
                ChasisNumber = "CH001",
                EngineNumber = "EN001",
                DeliveryDate = DateTime.Now,
                WarrantyExpirationDate = DateTime.Now.AddYears(1)
            });
            await context.SaveChangesAsync();

            var controller = new QrCodesController(context);

            // Act
            var result = await controller.Details(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // Escenario 3: Retorna ViewResult con un modelo QrCode correctamente poblado cuando la máquina existe y tiene todos los datos.
        [Fact]
        public async Task Details_ReturnsViewResult_WithQrCodeModel()
        {
            // Arrange
            var context = GetInMemoryContext();

            // Crear las entidades requeridas
            var customer = new Customer
            {
                Id = 1,
                Name = "John",
                LastName = "Doe"                
            };
            var modelEntity = new Model
            {
                Id = 1,
                Name = "Excavator",
                CategoryId = 1,
                ManualUrls = new List<string> { "manual1.pdf", "manual2.pdf" },
                SpareKitsUrls = new List<string> { "spare1.pdf", "spare2.pdf" }
            };
            var service = new Service
            {
                Id = 1,
                ServiceSheetUrls = new List<string> { "service1.pdf", "service2.pdf" }
            };

            context.Customers.Add(customer);
            context.Models.Add(modelEntity);
            context.Machines.Add(new Machine
            {
                Id = 1,
                CustomerId = customer.Id,
                ModelId = modelEntity.Id,
                Customer = customer,
                Model = modelEntity,
                DocUrls = new List<string> { "doc1.pdf", "doc2.pdf" },
                Services = new List<Service> { service },
                ChasisNumber = "CH001",
                EngineNumber = "EN001",
                DeliveryDate = new DateTime(2020, 1, 1),
                WarrantyExpirationDate = new DateTime(2021, 1, 1)
            });
            await context.SaveChangesAsync();

            var controller = new QrCodesController(context);

            // Act
            var result = await controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var qrModel = Assert.IsType<QrCode>(viewResult.Model);

            Assert.Equal("John Doe", qrModel.ClientName);                   // Desde Customer.FullName
            Assert.Equal("Excavator", qrModel.MachineModel);                 // Desde Model.Name
            Assert.Equal("manual1.pdf, manual2.pdf", qrModel.ManualUrl);       // ManualUrls concatenados
            Assert.Equal("doc1.pdf, doc2.pdf", qrModel.DocUrl);                // DocUrls concatenados
            Assert.Equal("spare1.pdf,spare2.pdf", qrModel.SpareKitsUrl);       // SpareKitsUrls concatenados (sin espacio tras la coma)
            Assert.Equal("service1.pdf, service2.pdf", qrModel.ServiceUrl);     // ServiceSheetUrls concatenados
            Assert.Equal(1, qrModel.MachineId);
            Assert.Equal(new DateTime(2020, 1, 1), qrModel.DeliveryDate);
        }

        // Escenario 1: Con URLs válidas para manuales y repuestos.
        // Se espera que se creen listas con un elemento cada una, y que se extraiga el nombre de archivo y
        // el "display" (última parte luego del guion bajo).
        [Fact]
        public void ManualList_ReturnsViewResult_WithPopulatedModelDownloads()
        {
            // Arrange
            var context = GetInMemoryContext(); // Asume que este método ya existe para obtener un AppDbContext in-memory.
            var controller = new QrCodesController(context);

            // Ejemplo de entrada:
            // En este caso, se asume que los valores vienen con una ruta y se usa Path.GetFileName.
            // "folder/manual_Instruction.pdf" --> OriginalName "manual_Instruction.pdf", DisplayName "Instruction.pdf"
            // "folder/sparekit_Info.pdf"     --> OriginalName "sparekit_Info.pdf",     DisplayName "Info.pdf"
            string manualUrls = "folder/manual_Instruction.pdf";
            string spareKitUrls = "folder/sparekit_Info.pdf";

            // Act
            var result = controller.ManualList(manualUrls, spareKitUrls);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("ManualList", viewResult.ViewName);
            var model = Assert.IsType<ModelDownloadsViewModel>(viewResult.Model);

            Assert.Single(model.Manuals);
            var manual = model.Manuals[0];
            Assert.Equal("manual_Instruction.pdf", manual.OriginalName);
            Assert.Equal("Instruction.pdf", manual.DisplayName);

            Assert.Single(model.SpareKits);
            var spareKit = model.SpareKits[0];
            Assert.Equal("sparekit_Info.pdf", spareKit.OriginalName);
            Assert.Equal("Info.pdf", spareKit.DisplayName);
        }

        // Escenario 2: Cuando no se proporcionan URLs (null o cadena vacía),
        // se espera que ambas listas (Manuals y SpareKits) queden vacías.
        [Fact]
        public void ManualList_ReturnsViewResult_WithEmptyLists_WhenUrlsAreNullOrEmpty()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new QrCodesController(context);

            // Act
            var result = controller.ManualList(null, "");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("ManualList", viewResult.ViewName);
            var model = Assert.IsType<ModelDownloadsViewModel>(viewResult.Model);

            Assert.Empty(model.Manuals);
            Assert.Empty(model.SpareKits);
        }

        // Escenario 1: Cuando se pasan URLs válidas, se debe retornar un ViewResult con una lista de
        // DocumentationViewModel donde cada elemento se llena usando Path.GetFileName y extrae la parte después del guion bajo.
        [Fact]
        public void DocumentList_ReturnsViewResult_WithPopulatedDocuments()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new QrCodesController(context);

            // Ejemplo: se pasan dos rutas separadas por coma.
            // Para "folder/doc_manual.pdf":
            // Path.GetFileName("folder/doc_manual.pdf") => "doc_manual.pdf"
            // "doc_manual.pdf".Split('_') => [ "doc", "manual.pdf" ] => Last: "manual.pdf"
            // Para "folder/doc_revision.pdf": se espera "doc_revision.pdf" y "revision.pdf".
            string urls = "folder/doc_manual.pdf,folder/doc_revision.pdf";

            // Act
            var result = controller.DocumentList(urls);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("DocumentList", viewResult.ViewName);
            var documents = Assert.IsAssignableFrom<List<DocumentationViewModel>>(viewResult.Model);
            Assert.Equal(2, documents.Count);

            var doc1 = documents[0];
            Assert.Equal("doc_manual.pdf", doc1.OriginalName);
            Assert.Equal("manual.pdf", doc1.DisplayName);

            var doc2 = documents[1];
            Assert.Equal("doc_revision.pdf", doc2.OriginalName);
            Assert.Equal("revision.pdf", doc2.DisplayName);
        }

        // Escenario 2: Cuando se pasa una cadena vacía, 
        // .Split(',') retorna un arreglo con un elemento (string vacío), 
        // por lo que se espera que se genere una lista con un elemento cuyos valores sean vacíos.
        [Fact]
        public void DocumentList_ReturnsViewResult_WithEmptyDocument_WhenInputIsEmpty()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new QrCodesController(context);

            string urls = "";

            // Act
            var result = controller.DocumentList(urls);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("DocumentList", viewResult.ViewName);
            var documents = Assert.IsAssignableFrom<List<DocumentationViewModel>>(viewResult.Model);
            // Al hacer Split, obtenemos un arreglo con un elemento (""), por lo que se crea un único elemento.
            Assert.Single(documents);
            var doc = documents[0];
            Assert.Equal("", doc.OriginalName);
            Assert.Equal("", doc.DisplayName);
        }

        // Escenario 1: Cuando se pasa una cadena nula o que contiene sólo espacios en blanco,
        // se debe retornar NotFound con el mensaje "No se han proporcionado datos del servicio técnico."
        [Fact]
        public void ServiceList_ReturnsNotFound_WhenInputIsNullOrWhiteSpace()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new QrCodesController(context);

            // Act: Usamos null para simular la ausencia de datos
            var result = controller.ServiceList(null);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No se han proporcionado datos del servicio técnico.", notFoundResult.Value);
        }

        // Escenario 2: Cuando se pasa una cadena que, una vez aplicada la división y el Trim,
        // resulta en un arreglo vacío, se debe retornar NotFound con el mensaje "No se han encontrado datos del servicio técnico."
        [Fact]
        public void ServiceList_ReturnsNotFound_WhenNoUrlsFoundAfterTrimming()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new QrCodesController(context);
            // Este string, al dividirlo y aplicar Trim, dará como resultado un arreglo vacío.
            string urls = "   ,   ";

            // Act
            var result = controller.ServiceList(urls);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No se han encontrado datos del servicio técnico.", notFoundResult.Value);
        }

        // Escenario 3: Cuando se pasan URLs válidas, se debe retornar un ViewResult con el nombre de vista "ServiceList"
        // y con el modelo (lista de DocumentationViewModel) poblado correctamente según la lógica del controller.
        [Fact]
        public void ServiceList_ReturnsViewResult_WithServiceDocuments()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new QrCodesController(context);

            string urls = "folder/service_A_info.pdf, folder/service_B_manual.pdf";

            // Act
            var result = controller.ServiceList(urls);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("ServiceList", viewResult.ViewName);
            var documents = Assert.IsAssignableFrom<List<DocumentationViewModel>>(viewResult.Model);
            Assert.Equal(2, documents.Count);

            // Para el primer documento:
            var doc1 = documents[0];
            // Path.GetFileName("folder/service_A_info.pdf") -> "service_A_info.pdf"
            // "service_A_info.pdf".Split('_') -> ["service", "A", "info.pdf"], último elemento: "info.pdf"
            Assert.Equal("service_A_info.pdf", doc1.OriginalName);
            Assert.Equal("info.pdf", doc1.DisplayName);

            // Para el segundo documento:
            var doc2 = documents[1];
            // "folder/service_B_manual.pdf" -> "service_B_manual.pdf"
            // Split('_') → ["service", "B", "manual.pdf"], último elemento: "manual.pdf"
            Assert.Equal("service_B_manual.pdf", doc2.OriginalName);
            Assert.Equal("manual.pdf", doc2.DisplayName);
        }

        // ------------------------------
        // DownloadService
        // ------------------------------

        [Fact]
        public void DownloadService_ReturnsNotFound_WhenFileDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new QrCodesController(context);
            string fileName = "nonexistent.pdf";
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "documentation", "serviceSheets", fileName);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            // Act
            var result = controller.DownloadService(fileName);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Archivo no encontrado", notFoundResult.Value);
        }

        [Fact]
        public void DownloadService_ReturnsFileResult_WhenFileExists()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new QrCodesController(context);
            string fileName = "testservice.pdf";
            string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "documentation", "serviceSheets");
            Directory.CreateDirectory(dirPath);
            string filePath = Path.Combine(dirPath, fileName);
            byte[] dummyContent = new byte[] { 1, 2, 3, 4 };
            System.IO.File.WriteAllBytes(filePath, dummyContent);

            // Act
            var result = controller.DownloadService(fileName);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal(fileName, fileResult.FileDownloadName);
            Assert.Equal(dummyContent, fileResult.FileContents);

            // Clean up
            System.IO.File.Delete(filePath);
        }

        // ------------------------------
        // DownloadManual
        // ------------------------------

        [Fact]
        public void DownloadManual_ReturnsNotFound_WhenFileDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new QrCodesController(context);
            string fileName = "nonexistent.pdf";
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "documentation", "manuals", fileName);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            // Act
            var result = controller.DownloadManual(fileName);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Archivo no encontrado", notFoundResult.Value);
        }

        [Fact]
        public void DownloadManual_ReturnsFileResult_WhenFileExists()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new QrCodesController(context);
            string fileName = "testmanual.pdf";
            string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "documentation", "manuals");
            Directory.CreateDirectory(dirPath);
            string filePath = Path.Combine(dirPath, fileName);
            byte[] dummyContent = new byte[] { 10, 20, 30 };
            System.IO.File.WriteAllBytes(filePath, dummyContent);

            // Act
            var result = controller.DownloadManual(fileName);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal(fileName, fileResult.FileDownloadName);
            Assert.Equal(dummyContent, fileResult.FileContents);

            // Clean up
            System.IO.File.Delete(filePath);
        }

        // ------------------------------
        // DownloadDocument
        // ------------------------------

        [Fact]
        public void DownloadDocument_ReturnsNotFound_WhenFileDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new QrCodesController(context);
            string fileName = "nonexistent.pdf";
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "documentation", "machines", fileName);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            // Act
            var result = controller.DownloadDocument(fileName);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Archivo no encontrado", notFoundResult.Value);
        }

        [Fact]
        public void DownloadDocument_ReturnsFileResult_WhenFileExists()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new QrCodesController(context);
            string fileName = "testdocument.pdf";
            string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "documentation", "machines");
            Directory.CreateDirectory(dirPath);
            string filePath = Path.Combine(dirPath, fileName);
            byte[] dummyContent = new byte[] { 5, 6, 7 };
            System.IO.File.WriteAllBytes(filePath, dummyContent);

            // Act
            var result = controller.DownloadDocument(fileName);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal(fileName, fileResult.FileDownloadName);
            Assert.Equal(dummyContent, fileResult.FileContents);

            // Clean up
            System.IO.File.Delete(filePath);
        }

        // ------------------------------
        // DownloadSpareKit
        // ------------------------------

        [Fact]
        public void DownloadSpareKit_ReturnsNotFound_WhenFileDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new QrCodesController(context);
            string fileName = "nonexistent.pdf";
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "documentation", "spareKits", fileName);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            // Act
            var result = controller.DownloadSpareKit(fileName);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Archivo no encontrado", notFoundResult.Value);
        }

        [Fact]
        public void DownloadSpareKit_ReturnsFileResult_WhenFileExists()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new QrCodesController(context);
            string fileName = "testsparekit.pdf";
            string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "documentation", "spareKits");
            Directory.CreateDirectory(dirPath);
            string filePath = Path.Combine(dirPath, fileName);
            byte[] dummyContent = new byte[] { 8, 9, 10 };
            System.IO.File.WriteAllBytes(filePath, dummyContent);

            // Act
            var result = controller.DownloadSpareKit(fileName);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal(fileName, fileResult.FileDownloadName);
            Assert.Equal(dummyContent, fileResult.FileContents);

            // Clean up
            System.IO.File.Delete(filePath);
        }

        // ------------------------------
        // DownloadServiceSheet
        // ------------------------------

        [Fact]
        public void DownloadServiceSheet_ReturnsNotFound_WhenFileDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new QrCodesController(context);
            string fileName = "nonexistent.pdf";
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "documentation", "serviceSheets", fileName);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            // Act
            var result = controller.DownloadServiceSheet(fileName);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Archivo no encontrado", notFoundResult.Value);
        }

        [Fact]
        public void DownloadServiceSheet_ReturnsFileResult_WhenFileExists()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new QrCodesController(context);
            string fileName = "testservicesheet.pdf";
            string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "documentation", "serviceSheets");
            Directory.CreateDirectory(dirPath);
            string filePath = Path.Combine(dirPath, fileName);
            byte[] dummyContent = new byte[] { 11, 12, 13 };
            System.IO.File.WriteAllBytes(filePath, dummyContent);

            // Act
            var result = controller.DownloadServiceSheet(fileName);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal(fileName, fileResult.FileDownloadName);
            Assert.Equal(dummyContent, fileResult.FileContents);

            // Clean up
            System.IO.File.Delete(filePath);
        }

        [Fact]
        public async Task ServiceDetails_ReturnsNotFound_WhenNoServicesForMachine()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new QrCodesController(context);

            // No se insertan servicios para machineId = 1
            // Act
            var result = await controller.ServiceDetails(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No hay servicios registrados para esta máquina.", notFoundResult.Value);
        }

        [Fact]
        public async Task ServiceDetails_ReturnsViewResult_WithServicesList()
        {
            // Arrange
            var context = GetInMemoryContext();
            // Insertar servicios para machineId = 1
            var service1 = new Service { Id = 1, MachineId = 1, ServiceSheetUrls = new List<string> { "service1.pdf" } };
            var service2 = new Service { Id = 2, MachineId = 1, ServiceSheetUrls = new List<string> { "service2.pdf" } };
            context.Services.AddRange(service1, service2);
            await context.SaveChangesAsync();

            var controller = new QrCodesController(context);

            // Act
            var result = await controller.ServiceDetails(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(1, viewResult.ViewData["MachineId"]);
            var services = Assert.IsAssignableFrom<List<Service>>(viewResult.Model);
            Assert.Equal(2, services.Count);
        }
    }
}