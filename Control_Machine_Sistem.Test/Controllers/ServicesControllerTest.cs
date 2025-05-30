using Control_Machine_Sistem.Controllers;
using Control_Machine_Sistem.Models;
using Humanizer.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Control_Machine_Sistem.Test.Controllers
{
    public class ServicesControllerTest
    {
        private DbContextOptions<AppDbContext> _options;

        public ServicesControllerTest()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Semilla de datos
            using var context = new AppDbContext(_options);
            if (!context.Machines.Any())
            {
                var category = new Category {Id = 1, Name = "Category A" };
                var model = new Model { Id = 1, CategoryId = 1, Name = "Model A" };
                var machine = new Models.Machine { Id = 1, ChasisNumber = "ABC123", CustomerId = 1, ModelId = 1 };
                context.Machines.Add(machine);
                context.Services.Add(new Service { Id = 1, MachineId = 1, WorkHours = 100 });
                context.SaveChanges();
            }
        }

        //--------------------------------------GETS------------------------------------------------------------------
        //Verifica que devuelva un ViewResult y que el modelo sea una lista de todos los servicios existentes.
        [Fact]
        public async Task Index_ReturnsViewWithAllServices()
        {
            using var context = new AppDbContext(_options);
            var controller = new ServicesController(context);

            var result = await controller.Index(null) as ViewResult;

            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<List<Service>>(result.Model);
            Assert.True(model.Count > 0);
        }

        //Comprueba que solo se devuelvan servicios cuyo MachineId sea igual a 1.
        [Fact]
        public async Task Index_WithMachineId_FiltersByMachine()
        {
            using var context = new AppDbContext(_options);
            var controller = new ServicesController(context);

            var result = await controller.Index(1) as ViewResult;

            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<List<Service>>(result.Model);
            Assert.All(model, s => Assert.Equal(1, s.MachineId));
        }
        //Asegura que devuelva un ViewResult con el servicio que tiene Id = 1.
        [Fact]
        public async Task Details_ReturnsService_WhenIdExists()
        {
            using var context = new AppDbContext(_options);
            var controller = new ServicesController(context);

            var result = await controller.Details(1, null) as ViewResult;

            Assert.NotNull(result);
            var service = Assert.IsType<Service>(result.Model);
            Assert.Equal(1, service.Id);
        }
        //Verifica que, al no pasar id, el controlador retorne un NotFoundResult.
        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            using var context = new AppDbContext(_options);
            var controller = new ServicesController(context);

            var result = await controller.Details(null, null);

            Assert.IsType<NotFoundResult>(result);
        }
        //Asegura que se retorne un NotFoundResult.
        [Fact]
        public async Task Details_ReturnsNotFound_WhenServiceDoesNotExist()
        {
            using var context = new AppDbContext(_options);
            var controller = new ServicesController(context);

            var result = await controller.Details(999, null);

            Assert.IsType<NotFoundResult>(result);
        }
        //Comprueba que devuelva la vista de creación con un objeto Service precargado en esos valores(MachineId = 1, ServiceHour = 150) y el ViewData["MachineId"] configurado.
        [Fact]
        public void Create_ReturnsViewWithDefaultValues()
        {
            using var context = new AppDbContext(_options);
            var controller = new ServicesController(context);

            var result = controller.Create(1, 150) as ViewResult;

            Assert.NotNull(result);
            var model = Assert.IsType<Service>(result.Model);
            Assert.Equal(1, model.MachineId);
            Assert.Equal(150, model.ServiceHour);
        }
        //Verifica que retorne la vista de edición con el servicio correcto (Id = 1).
        [Fact]
        public async Task Edit_ReturnsViewWithService_WhenIdExists()
        {
            using var context = new AppDbContext(_options);
            var controller = new ServicesController(context);

            var result = await controller.Edit(1, 1) as ViewResult;

            Assert.NotNull(result);
            var model = Assert.IsType<Service>(result.Model);
            Assert.Equal(1, model.Id);
        }
        //Asegura que, al no proporcionar id, se retorne un NotFoundResult.
        [Fact]
        public async Task Edit_ReturnsNotFound_WhenIdIsNull()
        {
            using var context = new AppDbContext(_options);
            var controller = new ServicesController(context);

            var result = await controller.Edit(null, null);

            Assert.IsType<NotFoundResult>(result);
        }
        //Comprueba que el controlador retorne NotFoundResult.
        [Fact]
        public async Task Edit_ReturnsNotFound_WhenServiceNotFound()
        {
            using var context = new AppDbContext(_options);
            var controller = new ServicesController(context);

            var result = await controller.Edit(999, null);

            Assert.IsType<NotFoundResult>(result);
        }
        //Verifica que devuelva la vista de confirmación de borrado con el servicio correcto (Id = 1).
        [Fact]
        public async Task Delete_ReturnsViewWithService_WhenIdExists()
        {
            using var context = new AppDbContext(_options);
            var controller = new ServicesController(context);

            var result = await controller.Delete(1, 1) as ViewResult;

            Assert.NotNull(result);
            var model = Assert.IsType<Service>(result.Model);
            Assert.Equal(1, model.Id);
        }
        //Asegura que, al no pasar id, el controlador retorne NotFoundResult.
        [Fact]
        public async Task Delete_ReturnsNotFound_WhenIdIsNull()
        {
            using var context = new AppDbContext(_options);
            var controller = new ServicesController(context);

            var result = await controller.Delete(null, null);

            Assert.IsType<NotFoundResult>(result);
        }
        //Comprueba que se retorne un NotFoundResult.
        [Fact]
        public async Task Delete_ReturnsNotFound_WhenServiceNotFound()
        {
            using var context = new AppDbContext(_options);
            var controller = new ServicesController(context);

            var result = await controller.Delete(999, null);

            Assert.IsType<NotFoundResult>(result);
        }
        //------------------------------------------------------FIN GETS---------------------------------------------------------------

        //------------------------------------------------------POSTS------------------------------------------------------------------

        //Verifica que se cree correctamente un servicio válido y se redirija al Index del controlador.
        [Fact]
        public async Task Create_Post_RedirectsToIndex_WhenModelIsValid()
        {
            using var context = new AppDbContext(_options);
            var controller = new ServicesController(context);

            var service = new Service
            {
                MachineId = 1,
                WorkHours = 200,
                ServiceHour = 250,
                ServiceDate = DateTime.Now,
                OrderNumber = "ORD123",
                Observations = "Service Test"
            };

            var result = await controller.Create(service, 1) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal(1, result.RouteValues["machineId"]);
        }

        //Verifica que se retorne la vista de creación si el modelo no es válido.
        [Fact]
        public async Task Create_Post_ReturnsView_WhenModelIsInvalid()
        {
            using var context = new AppDbContext(_options);
            var controller = new ServicesController(context);
            controller.ModelState.AddModelError("WorkHours", "Required");

            var service = new Service { MachineId = 1 };

            var result = await controller.Create(service, 1) as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<Service>(result.Model);
        }

        //Comprueba que al editar un servicio válido se redirige al Index.
        [Fact]
        public async Task Edit_Post_RedirectsToIndex_WhenModelIsValid()
        {
            using var context = new AppDbContext(_options);
            var controller = new ServicesController(context);

            var service = context.Services.First();
            service.WorkHours = 300;

            var result = await controller.Edit(service.Id, service, 1, new string[] { }) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
        }

        //Asegura que se retorne NotFound si los Id no coinciden.
        [Fact]
        public async Task Edit_Post_ReturnsNotFound_WhenIdMismatch()
        {
            using var context = new AppDbContext(_options);
            var controller = new ServicesController(context);

            var service = new Service { Id = 999, MachineId = 1, WorkHours = 300 };

            var result = await controller.Edit(1, service, 1, new string[] { });

            Assert.IsType<NotFoundResult>(result);
        }

        //Verifica que se retorne la vista de edición si el modelo no es válido.
        [Fact]
        public async Task Edit_Post_ReturnsView_WhenModelInvalid()
        {
            using var context = new AppDbContext(_options);
            var controller = new ServicesController(context);
            controller.ModelState.AddModelError("WorkHours", "Required");

            var service = context.Services.First();

            var result = await controller.Edit(service.Id, service, 1, new string[] { }) as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<Service>(result.Model);
        }

        //Asegura que al borrar un servicio se redirige al Index y el servicio desaparece de la base.
        [Fact]
        public async Task DeleteConfirmed_RemovesServiceAndRedirects()
        {
            using var context = new AppDbContext(_options);
            var controller = new ServicesController(context);

            var result = await controller.DeleteConfirmed(1, 1) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal(1, result.RouteValues["machineId"]);
            Assert.Null(context.Services.Find(1));
        }

        //------------------------------------------------------FIN POSTS--------------------------------------------------------------


    }
}
