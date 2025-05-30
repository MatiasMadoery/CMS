using Xunit;
using Control_Machine_Sistem.Controllers;
using Control_Machine_Sistem.Models;
using Control_Machine_Sistem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Control_Machine_Sistem.Test.Controllers
{
    public class MaintenanceControllerTests
    {
        // Método auxiliar para obtener un contexto en memoria.
        private AppDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        // Tests para el método GET: /Maintenance/Index        

        // Test para verificar que Index devuelve un ViewResult con una lista de MaintenanceIndexViewModel
        [Fact]
        public async Task Index_ReturnsViewResult_WithMaintenanceIndexViewModels()
        {
            // Arrange
            var context = GetInMemoryContext();

            // Crear datos de prueba: Cliente, Modelo y una Máquina asociada.
            var customer = new Customer { Id = 1, Name = "John", LastName = "Doe" };
            var modelEntity = new Model { Id = 1, Name = "Excavator", CategoryId = 1 };
            context.Customers.Add(customer);
            context.Models.Add(modelEntity);
            var machine = new Machine
            {
                Id = 1,
                CustomerId = customer.Id,
                ModelId = modelEntity.Id,
                // Asignamos las propiedades de navegación para que se puedan acceder en la proyección.
                Customer = customer,
                Model = modelEntity
            };
            context.Machines.Add(machine);
            context.SaveChanges();

            var controller = new MaintenanceController(context);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = Assert.IsAssignableFrom<List<MaintenanceIndexViewModel>>(viewResult.Model);
            Assert.Single(viewModel);
            var item = viewModel.First();
            Assert.Equal(machine.Id, item.MachineId);
            Assert.Equal(modelEntity.Name, item.MachineModel);
            Assert.Equal(customer.Name, item.CustomerName);
        }

       
        // Tests para el método GET: /Maintenance/MachineServices?machineId=5       

        // Test para verificar que se retorna NotFound cuando no existe la máquina solicitada.
        [Fact]
        public async Task MachineServices_ReturnsNotFound_WhenMachineDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new MaintenanceController(context);

            // Act
            var result = await controller.MachineServices(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // Test para verificar que se retorna la vista con la máquina cuando ésta existe.
        [Fact]
        public async Task MachineServices_ReturnsViewResult_WithMachine_WhenMachineExists()
        {
            // Arrange
            var context = GetInMemoryContext();

            // Crear una máquina con la propiedad Services inicializada.
            // Se asume que la propiedad Services existe en la entidad Machine.
            var machine = new Machine
            {
                Id = 1,
                CustomerId = 1,
                ModelId = 1,
                Services = new List<Service>
                {
                    // Se pueden agregar instancias dummy de Service si es necesario.
                    new Service { Id = 1, Observations = "Service A" }
                }
            };
            context.Machines.Add(machine);
            context.SaveChanges();

            var controller = new MaintenanceController(context);

            // Act
            var result = await controller.MachineServices(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedMachine = Assert.IsType<Machine>(viewResult.Model);
            Assert.Equal(machine.Id, returnedMachine.Id);
        }

       
        // Tests para el método GET: /Maintenance/MachineMaintenances?machineId=5        

        // Test para verificar que se retorne NotFound cuando la máquina no existe.
        [Fact]
        public async Task MachineMaintenances_ReturnsNotFound_WhenMachineDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new MaintenanceController(context);

            // Act
            var result = await controller.MachineMaintenances(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // Test para verificar que se retorne la vista con la máquina cuando ésta existe.
        [Fact]
        public async Task MachineMaintenances_ReturnsViewResult_WithMachine_WhenMachineExists()
        {
            // Arrange
            var context = GetInMemoryContext();

            // Crear una máquina con la propiedad OtherMaintenances inicializada.
            // Se asume que OtherMaintenances es la colección que contiene mantenimientos u otra información.
            var machine = new Machine
            {
                Id = 1,
                CustomerId = 1,
                ModelId = 1,
                OtherMaintenances = new List<OtherMaintenance>
                {
                    // Agregar mantenimientos dummy si lo requieres.
                    new OtherMaintenance { Id = 1, Observations = "Maintenance A" }
                }
            };
            context.Machines.Add(machine);
            context.SaveChanges();

            var controller = new MaintenanceController(context);

            // Act
            var result = await controller.MachineMaintenances(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedMachine = Assert.IsType<Machine>(viewResult.Model);
            Assert.Equal(machine.Id, returnedMachine.Id);
        }

        // Test para Index cuando no existen máquinas.
        [Fact]
        public async Task Index_ReturnsEmptyList_WhenNoMachinesExist()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new MaintenanceController(context);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = Assert.IsAssignableFrom<List<MaintenanceIndexViewModel>>(viewResult.Model);
            Assert.Empty(viewModel);
        } 

        // Test para MachineServices cuando la máquina existe pero la colección de Services está vacía
        [Fact]
        public async Task MachineServices_ReturnsViewResult_WithMachineHavingEmptyServices()
        {
            // Arrange
            var context = GetInMemoryContext();

            // Crear una máquina con Services asignado como lista vacía.
            var machine = new Machine
            {
                Id = 1,
                CustomerId = 1,
                ModelId = 1,
                Services = new List<Service>()  // lista vacía
            };
            context.Machines.Add(machine);
            context.SaveChanges();

            var controller = new MaintenanceController(context);

            // Act
            var result = await controller.MachineServices(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedMachine = Assert.IsType<Machine>(viewResult.Model);
            Assert.Equal(machine.Id, returnedMachine.Id);
            Assert.NotNull(returnedMachine.Services);
            Assert.Empty(returnedMachine.Services);
        }

        // Test para MachineMaintenances cuando la máquina existe pero OtherMaintenances está vacía
        [Fact]
        public async Task MachineMaintenances_ReturnsViewResult_WithMachineHavingEmptyOtherMaintenances()
        {
            // Arrange
            var context = GetInMemoryContext();

            // Crear una máquina con OtherMaintenances asignado como lista vacía.
            var machine = new Machine
            {
                Id = 1,
                CustomerId = 1,
                ModelId = 1,
                OtherMaintenances = new List<OtherMaintenance>()  // lista vacía
            };
            context.Machines.Add(machine);
            context.SaveChanges();

            var controller = new MaintenanceController(context);

            // Act
            var result = await controller.MachineMaintenances(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedMachine = Assert.IsType<Machine>(viewResult.Model);
            Assert.Equal(machine.Id, returnedMachine.Id);
            Assert.NotNull(returnedMachine.OtherMaintenances);
            Assert.Empty(returnedMachine.OtherMaintenances);
        }
    }
}