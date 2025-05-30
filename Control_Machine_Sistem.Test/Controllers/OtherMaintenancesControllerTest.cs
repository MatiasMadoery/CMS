using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Control_Machine_Sistem.Controllers;
using Control_Machine_Sistem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Control_Machine_Sistem.Test.Controllers
{
    public class OtherMaintenancesControllerTests
    {
        // Método auxiliar para obtener un contexto en memoria.
        private AppDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }
             
        // GET: OtherMaintenances/Index?machineId=...    

        // Caso 1: Sin parámetro machineId (se devuelven todos los registros)
        [Fact]
        public async Task Index_ReturnsAllRecords_WhenMachineIdNotProvided()
        {
            // Arrange
            var context = GetInMemoryContext();
            context.OtherMaintenances.Add(new OtherMaintenance
            {
                Id = 1,
                MachineId = 1,
                WorkHours = "10",
                ServiceDate = DateTime.Now,
                OrderNumber = "ORD1",
                Observations = "Observación 1"
            });
            context.OtherMaintenances.Add(new OtherMaintenance
            {
                Id = 2,
                MachineId = 2,
                WorkHours = "20",
                ServiceDate = DateTime.Now,
                OrderNumber = "ORD2",
                Observations = "Observación 2"
            });
            await context.SaveChangesAsync();
            var controller = new OtherMaintenancesController(context);

            // Act
            var result = await controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<OtherMaintenance>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        // Caso 2: Con machineId proporcionado (se filtra por ese valor)
        [Fact]
        public async Task Index_ReturnsFilteredRecords_WhenMachineIdIsProvided()
        {
            // Arrange
            var context = GetInMemoryContext();
            context.OtherMaintenances.Add(new OtherMaintenance
            {
                Id = 1,
                MachineId = 1,
                WorkHours = "10",
                ServiceDate = DateTime.Now,
                OrderNumber = "ORD1",
                Observations = "Observación 1"
            });
            context.OtherMaintenances.Add(new OtherMaintenance
            {
                Id = 2,
                MachineId = 2,
                WorkHours = "20",
                ServiceDate = DateTime.Now,
                OrderNumber = "ORD2",
                Observations = "Observación 2"
            });
            await context.SaveChangesAsync();
            var controller = new OtherMaintenancesController(context);

            // Act: Solicitamos registros para machineId = 1
            var result = await controller.Index(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<OtherMaintenance>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal(1, model.First().MachineId);
            Assert.Equal(1, viewResult.ViewData["MachineId"]);
        }

        // ____________________________
        // GET: OtherMaintenances/Details/5?machineId=...
        // ____________________________

        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new OtherMaintenancesController(context);

            // Act
            var result = await controller.Details(null, 1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenRecordNotFound()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new OtherMaintenancesController(context);

            // Act
            var result = await controller.Details(1, 1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsViewResult_WithOtherMaintenance()
        {
            // Arrange
            var context = GetInMemoryContext();
            var maintenance = new OtherMaintenance
            {
                Id = 1,
                MachineId = 1,
                WorkHours = "10",
                ServiceDate = DateTime.Now,
                OrderNumber = "ORD1",
                Observations = "Observación 1"
            };
            context.OtherMaintenances.Add(maintenance);
            await context.SaveChangesAsync();
            var controller = new OtherMaintenancesController(context);

            // Act
            var result = await controller.Details(1, 99); // machineId de ejemplo

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedMaintenance = Assert.IsType<OtherMaintenance>(viewResult.Model);
            Assert.Equal(maintenance.Id, returnedMaintenance.Id);
            Assert.Equal(99, viewResult.ViewData["MachineId"]);
        }

        // ____________________________
        // GET: OtherMaintenances/Create?machineId=...
        // ____________________________

        [Fact]
        public void Create_ReturnsViewResult_WithPrepopulatedMachineId()
        {
            // Arrange
            var context = GetInMemoryContext();
            // Agregamos una máquina válida: se asignan los valores requeridos (CustomerId y ModelId)
            context.Machines.Add(new Machine
            {
                Id = 5,
                CustomerId = 1,
                ModelId = 1,
                ChasisNumber = "CH001",
                EngineNumber = "EN001",
                DeliveryDate = DateTime.Now,
                WarrantyExpirationDate = DateTime.Now.AddYears(1)
            });
            // Es posible que debas agregar también el registro para Customer y Model:
            context.Customers.Add(new Customer { Id = 1, Name = "John", LastName = "Doe" });
            context.Models.Add(new Model { Id = 1, Name = "Excavator", CategoryId = 1 });
            context.SaveChanges();

            var controller = new OtherMaintenancesController(context);

            // Act
            var result = controller.Create(5);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<OtherMaintenance>(viewResult.Model);
            Assert.Equal(5, model.MachineId);
            Assert.IsType<SelectList>(viewResult.ViewData["MachineId"]);
        }

        // ____________________________
        // POST: OtherMaintenances/Create
        // ____________________________

        [Fact]
        public async Task Create_Post_RedirectsToIndex_WhenModelStateIsValid()
        {
            // Arrange
            var context = GetInMemoryContext();

            // Agregamos las entidades requeridas.
            context.Customers.Add(new Customer { Id = 1, Name = "John", LastName = "Doe" });
            context.Models.Add(new Model { Id = 1, Name = "Excavator", CategoryId = 1 });

            // Ahora agregamos la máquina asegurándonos de asignar las claves foráneas requeridas.
            context.Machines.Add(new Machine
            {
                Id = 1,
                CustomerId = 1,
                ModelId = 1,
                ChasisNumber = "CH001",
                EngineNumber = "EN001",
                DeliveryDate = DateTime.Now,
                WarrantyExpirationDate = DateTime.Now.AddYears(1)
            });
            await context.SaveChangesAsync();

            var controller = new OtherMaintenancesController(context);

            var otherMaintenance = new OtherMaintenance
            {
                MachineId = 1,
                WorkHours = "50",  // Asegúrate de usar el tipo correcto, en este caso string.
                ServiceDate = DateTime.Now,
                OrderNumber = "ORD123",
                Observations = "Observaciones de prueba"
            };

            // Act
            var result = await controller.Create(otherMaintenance, 1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(1, redirectResult.RouteValues["machineId"]);
            Assert.Single(context.OtherMaintenances);
        }

        [Fact]
        public async Task Create_Post_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new OtherMaintenancesController(context);
            controller.ModelState.AddModelError("Error", "Simulated error");

            var otherMaintenance = new OtherMaintenance
            {
                MachineId = 1,
                WorkHours = "50",
                ServiceDate = DateTime.Now,
                OrderNumber = "ORD123",
                Observations = "Observaciones de prueba"
            };

            // Act
            var result = await controller.Create(otherMaintenance, 1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(otherMaintenance, viewResult.Model);
            Assert.IsType<SelectList>(viewResult.ViewData["MachineId"]);
        }

        // ____________________________
        // GET: OtherMaintenances/Edit/5?machineId=...
        // ____________________________

        [Fact]
        public async Task Edit_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new OtherMaintenancesController(context);

            // Act
            var result = await controller.Edit(null, 1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_ReturnsNotFound_WhenOtherMaintenanceNotFound()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new OtherMaintenancesController(context);

            // Act
            var result = await controller.Edit(1, 1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_ReturnsViewResult_WithOtherMaintenance()
        {
            // Arrange
            var context = GetInMemoryContext();

            // Agregamos las entidades requeridas para cumplir las restricciones en Machine.
            context.Customers.Add(new Customer { Id = 1, Name = "John", LastName = "Doe" });
            context.Models.Add(new Model { Id = 1, Name = "Excavator", CategoryId = 1 });

            // Agregamos la máquina válida, asignando CustomerId y ModelId.
            context.Machines.Add(new Machine
            {
                Id = 1,
                CustomerId = 1,    // Asignado
                ModelId = 1,       // Asignado
                ChasisNumber = "CH001",
                EngineNumber = "EN001",
                DeliveryDate = DateTime.Now,
                WarrantyExpirationDate = DateTime.Now.AddYears(1)
            });

            // Agregamos el registro OtherMaintenance vinculado a la máquina 1.
            var otherMaintenance = new OtherMaintenance
            {
                Id = 1,
                MachineId = 1,
                WorkHours = "30", // tipo string
                ServiceDate = DateTime.Now,
                OrderNumber = "ORD321",
                Observations = "Observación edit"
            };
            context.OtherMaintenances.Add(otherMaintenance);

            await context.SaveChangesAsync();

            var controller = new OtherMaintenancesController(context);

            // Act
            var result = await controller.Edit(1, 99);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<OtherMaintenance>(viewResult.Model);
            Assert.Equal(otherMaintenance.Id, model.Id);
            Assert.Equal(99, viewResult.ViewData["MachineContext"]);
            Assert.IsType<SelectList>(viewResult.ViewData["MachineId"]);
        }

        // ____________________________
        // POST: OtherMaintenances/Edit/5?machineId=...
        // ____________________________

        [Fact]
        public async Task Edit_Post_ReturnsNotFound_WhenRouteIdDoesNotMatchModelId()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new OtherMaintenancesController(context);
            var otherMaintenance = new OtherMaintenance
            {
                Id = 1,
                MachineId = 1,
                WorkHours = "30",
                ServiceDate = DateTime.Now,
                OrderNumber = "ORD321",
                Observations = "Observación edit"
            };

            // Act
            var result = await controller.Edit(2, otherMaintenance, 1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_RedirectsToIndex_WhenModelStateIsValid()
        {
            // Arrange
            var context = GetInMemoryContext();

            // Agregar las entidades requeridas para la integridad referencial.
            context.Customers.Add(new Customer { Id = 1, Name = "John", LastName = "Doe" });
            context.Models.Add(new Model { Id = 1, Name = "Excavator", CategoryId = 1 });

            // Agregar una máquina válida, asignando los valores obligatorios: CustomerId y ModelId.
            context.Machines.Add(new Machine
            {
                Id = 1,
                CustomerId = 1,    // Valor requerido
                ModelId = 1,       // Valor requerido
                ChasisNumber = "CH001",
                EngineNumber = "EN001",
                DeliveryDate = DateTime.Now,
                WarrantyExpirationDate = DateTime.Now.AddYears(1)
            });

            // Agregar el registro en OtherMaintenances vinculado a la máquina anterior.
            var otherMaintenance = new OtherMaintenance
            {
                Id = 1,
                MachineId = 1,
                WorkHours = "30",  // Asignar como texto, dado que WorkHours es string
                ServiceDate = DateTime.Now,
                OrderNumber = "ORD321",
                Observations = "Observación edit"
            };
            context.OtherMaintenances.Add(otherMaintenance);
            await context.SaveChangesAsync();

            var controller = new OtherMaintenancesController(context);

            // Simular modificación
            otherMaintenance.WorkHours = "40";

            // Act
            var result = await controller.Edit(1, otherMaintenance, 1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(1, redirectResult.RouteValues["machineId"]);

            // Verificar que se actualizó el registro
            var updated = await context.OtherMaintenances.FindAsync(1);
            Assert.Equal("40", updated.WorkHours);
        }

        [Fact]
        public async Task Edit_Post_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            // Arrange
            var context = GetInMemoryContext();

            // Agregar entidades necesarias para la máquina.
            context.Customers.Add(new Customer { Id = 1, Name = "John", LastName = "Doe" });
            context.Models.Add(new Model { Id = 1, Name = "Excavator", CategoryId = 1 });

            // Agregar la máquina válida a la base (se requiere para el SelectList)
            context.Machines.Add(new Machine
            {
                Id = 1,
                CustomerId = 1,          // Valor requerido.
                ModelId = 1,             // Valor requerido.
                ChasisNumber = "CH001",
                EngineNumber = "EN001",
                DeliveryDate = DateTime.Now,
                WarrantyExpirationDate = DateTime.Now.AddYears(1)
            });

            // Agregar un OtherMaintenance vinculado a dicha máquina.
            var otherMaintenance = new OtherMaintenance
            {
                Id = 1,
                MachineId = 1,
                WorkHours = "30", // Asignar como string
                ServiceDate = DateTime.Now,
                OrderNumber = "ORD321",
                Observations = "Observación edit"
            };
            context.OtherMaintenances.Add(otherMaintenance);
            await context.SaveChangesAsync();

            var controller = new OtherMaintenancesController(context);
            // Simular error en el ModelState
            controller.ModelState.AddModelError("Error", "Simulated error");

            // Act
            var result = await controller.Edit(1, otherMaintenance, 1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(otherMaintenance, viewResult.Model);
            Assert.IsType<SelectList>(viewResult.ViewData["MachineId"]);
        }

        // ____________________________
        // GET: OtherMaintenances/Delete/5?machineId=...
        // ____________________________

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new OtherMaintenancesController(context);

            // Act
            var result = await controller.Delete(null, 1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenOtherMaintenanceNotFound()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new OtherMaintenancesController(context);

            // Act
            var result = await controller.Delete(1, 1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsViewResult_WithOtherMaintenance()
        {
            // Arrange
            var context = GetInMemoryContext();

            // Agregar entidades requeridas para la máquina
            context.Customers.Add(new Customer { Id = 1, Name = "John", LastName = "Doe" });
            context.Models.Add(new Model { Id = 1, Name = "Excavator", CategoryId = 1 });

            // Agregar una máquina (con las propiedades obligatorias CustomerId y ModelId asignadas)
            context.Machines.Add(new Machine
            {
                Id = 1,
                CustomerId = 1,
                ModelId = 1,
                ChasisNumber = "CH001",
                EngineNumber = "EN001",
                DeliveryDate = DateTime.Now,
                WarrantyExpirationDate = DateTime.Now.AddYears(1)
            });

            // Agregar la entidad OtherMaintenance relacionada a la máquina 1
            var otherMaintenance = new OtherMaintenance
            {
                Id = 1,
                MachineId = 1,
                WorkHours = "30",
                ServiceDate = DateTime.Now,
                OrderNumber = "ORD321",
                Observations = "Observación delete"
            };
            context.OtherMaintenances.Add(otherMaintenance);
            await context.SaveChangesAsync();

            var controller = new OtherMaintenancesController(context);

            // Act
            var result = await controller.Delete(1, 99);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<OtherMaintenance>(viewResult.Model);
            Assert.Equal(otherMaintenance.Id, model.Id);
            Assert.Equal(99, viewResult.ViewData["MachineId"]);
        }

        // ____________________________
        // POST: OtherMaintenances/Delete/5?machineId=...
        // ____________________________

        [Fact]
        public async Task DeleteConfirmed_RemovesRecord_AndRedirectsToIndex()
        {
            // Arrange
            var context = GetInMemoryContext();
            var otherMaintenance = new OtherMaintenance
            {
                Id = 1,
                MachineId = 1,
                WorkHours = "30",
                ServiceDate = DateTime.Now,
                OrderNumber = "ORD321",
                Observations = "Observación delete"
            };
            context.OtherMaintenances.Add(otherMaintenance);
            await context.SaveChangesAsync();
            var controller = new OtherMaintenancesController(context);

            // Act
            var result = await controller.DeleteConfirmed(1, 1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(1, redirectResult.RouteValues["machineId"]);
            var record = await context.OtherMaintenances.FindAsync(1);
            Assert.Null(record);
        }
    }
}
