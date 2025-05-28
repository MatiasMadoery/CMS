using Control_Machine_Sistem.Controllers;
using Control_Machine_Sistem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control_Machine_Sistem.Test.Controllers
{
    public class ModelsControllerTest
    {
        private DbContextOptions<AppDbContext> _options;

        public ModelsControllerTest()
        {
            // Cada test tendrá una base única con Guid
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }


        //-----------------------------------------------------------GETS-----------------------------------------------------------
        [Fact]
        public async Task Index_ReturnsViewResult_WithPaginatedModels()
        {
            //Base de datos en memoria y agrega 3 clientes
            using (var context = new AppDbContext(_options))
            {
                context.Categories.AddRange(
                    new Category { Id=1, Name="Categoria A"},
                    new Category { Id = 2, Name = "Categoria B" }
                );

                context.Models.AddRange(
                    new Model { Id = 1, Name = "Modelo A", CategoryId = 1 },
                    new Model { Id = 2, Name = "Modelo B", CategoryId = 2 },
                    new Model { Id = 3, Name = "Modelo C", CategoryId = 1 }
                );

                context.SaveChanges();
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new ModelsController(context);

                //Se llama index primera pagina (1), mostrar 2 elementos por pag (2). Se espera el ViewResult con el Pager
                var result = await controller.Index(1, 2) as ViewResult;
                //Verifica que la vista se haya devuelto correctamente.
                Assert.NotNull(result);
                //Verifica que el modelo pasado a la vista sea del tipo correcto.
                var model = Assert.IsAssignableFrom<Pager<Model>>(result.Model);
                //Confirma que el Pager<Model> contiene 2 elementos, lo cual es lo correcto para la primera página (con 3 elementos totales y tamaño de página 2).
                Assert.Equal(2, model.Elements.Count());
                //Confirma que los elementos que se encuentran en la primera página son los correctos.
                Assert.Contains(model.Elements, m => m.Name == "Modelo A");
                Assert.Contains(model.Elements, m => m.Name == "Modelo B");
            }
        }

        //Testea que al pedir la segunda página, se devuelvan los modelos correctos.
        [Fact]
        public async Task Index_ReturnsCorrectModels_ForSecondPage()
        {
            using (var context = new AppDbContext(_options))
            {
                context.Categories.Add(new Category { Id = 1, Name = "Cat" });

                context.Models.AddRange(
                    new Model { Id = 1, Name = "Model 1", CategoryId = 1 },
                    new Model { Id = 2, Name = "Model 2", CategoryId = 1 },
                    new Model { Id = 3, Name = "Model 3", CategoryId = 1 }
                );

                context.SaveChanges();
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new ModelsController(context);

                var result = await controller.Index(page: 2, pageSize: 2) as ViewResult;

                Assert.NotNull(result);
                var model = Assert.IsAssignableFrom<Pager<Model>>(result.Model);
                Assert.Single(model.Elements);
                Assert.Equal("Model 3", model.Elements.First().Name);
            }
        }

        //Testea que el filtro por categoría funcione.
        [Fact]
        public async Task Index_ReturnsFilteredModels_ByCategory()
        {
            using (var context = new AppDbContext(_options))
            {
                context.Categories.AddRange(
                    new Category { Id = 1, Name = "Cat A" },
                    new Category { Id = 2, Name = "Cat B" }
                );

                context.Models.AddRange(
                    new Model { Id = 1, Name = "Model A", CategoryId = 1 },
                    new Model { Id = 2, Name = "Model B", CategoryId = 2 },
                    new Model { Id = 3, Name = "Model C", CategoryId = 1 }
                );

                context.SaveChanges();
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new ModelsController(context);

                var result = await controller.Index(categoryId: 1) as ViewResult;

                Assert.NotNull(result);
                var model = Assert.IsAssignableFrom<Pager<Model>>(result.Model);
                Assert.Equal(2, model.Elements.Count());
                Assert.All(model.Elements, m => Assert.Equal(1, m.CategoryId));
            }
        }

        //Test cuando no hay coincidencias para categoryId
        [Fact]
        public async Task Index_ReturnsEmptyList_WhenCategoryIdDoesNotMatchAnyModel()
        {
            using (var context = new AppDbContext(_options))
            {
                context.Categories.Add(new Category { Id = 1, Name = "Cat A" });

                context.Models.Add(new Model { Id = 1, Name = "Model A", CategoryId = 1 });

                context.SaveChanges();
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new ModelsController(context);

                var result = await controller.Index(categoryId: 99) as ViewResult;

                Assert.NotNull(result);
                var model = Assert.IsAssignableFrom<Pager<Model>>(result.Model);
                Assert.Empty(model.Elements);
            }
        }

        //Testea que devuelve el ViewResult correcto cuando el Id es válido
        [Fact]
        public async Task Details_ReturnsViewResult_WithModel_WhenIdIsValid()
        {
            using (var context = new AppDbContext(_options))
            {
                context.Categories.Add(new Category { Id = 1, Name = "Cat A" });
                context.Models.Add(new Model { Id = 1, Name = "Model A", CategoryId = 1 });
                context.SaveChanges();
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new ModelsController(context);
                var result = await controller.Details(1) as ViewResult;

                Assert.NotNull(result);
                var model = Assert.IsAssignableFrom<Model>(result.Model);
                Assert.Equal(1, model.Id);
            }
        }

        //Testea que devuelva el NotFound  cuando el Id es null
        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            var controller = new ModelsController(new AppDbContext(_options));
            var result = await controller.Details(null);
            Assert.IsType<NotFoundResult>(result);
        }

        //Testea que devuelva el NotFound  cuando el Id es inválido
        [Fact]
        public async Task Details_ReturnsNotFound_WhenModelNotFound()
        {
            using (var context = new AppDbContext(_options))
            {
                context.Categories.Add(new Category { Id = 1, Name = "Cat A" });
                context.Models.Add(new Model { Id = 1, Name = "Model A", CategoryId = 1 });
                context.SaveChanges();
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new ModelsController(context);
                var result = await controller.Details(99);
                Assert.IsType<NotFoundResult>(result);
            }
        }

        //Testea que devuelva la vista para crear Models
        [Fact]
        public void Create_ReturnsViewResult()
        {
            var controller = new ModelsController(new AppDbContext(_options));
            var result = controller.Create() as ViewResult;
            Assert.NotNull(result);
        }

        //Testea que retorne el ViewResult correcto cuando el ID es válido
        [Fact]
        public async Task Edit_ReturnsViewResult_WithModel_WhenIdIsValid()
        {
            using (var context = new AppDbContext(_options))
            {
                context.Categories.Add(new Category { Id = 1, Name = "Cat A" });
                context.Models.Add(new Model { Id = 1, Name = "Model A", CategoryId = 1 });
                context.SaveChanges();
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new ModelsController(context);
                var result = await controller.Edit(1) as ViewResult;

                Assert.NotNull(result);
                var model = Assert.IsAssignableFrom<Model>(result.Model);
                Assert.Equal(1, model.Id);
            }
        }

        //Testea que retorne NotFound cuando el ID es null
        [Fact]
        public async Task Edit_ReturnsNotFound_WhenIdIsNull()
        {
            var controller = new ModelsController(new AppDbContext(_options));
            var result = await controller.Edit(null);
            Assert.IsType<NotFoundResult>(result);
        }

        //Testea que retorne NotFound cuando el ID es inválido
        [Fact]
        public async Task Edit_ReturnsNotFound_WhenModelNotFound()
        {
            using (var context = new AppDbContext(_options))
            {
                context.Categories.Add(new Category { Id = 1, Name = "Cat A" });
                context.Models.Add(new Model { Id = 1, Name = "Model A", CategoryId = 1 });
                context.SaveChanges();
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new ModelsController(context);
                var result = await controller.Edit(99);
                Assert.IsType<NotFoundResult>(result);
            }
        }

        //Testea que devuelva la vista correcta cuando el ID es válido
        [Fact]
        public async Task Delete_ReturnsViewResult_WithModel_WhenIdIsValid()
        {
            using (var context = new AppDbContext(_options))
            {
                context.Categories.Add(new Category { Id = 1, Name = "Cat A" });
                context.Models.Add(new Model { Id = 1, Name = "Model A", CategoryId = 1 });
                context.SaveChanges();
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new ModelsController(context);
                var result = await controller.Delete(1) as ViewResult;

                Assert.NotNull(result);
                var model = Assert.IsAssignableFrom<Model>(result.Model);
                Assert.Equal(1, model.Id);
            }
        }

        //Testea que retorne NotFound cuando el ID es null
        [Fact]
        public async Task Delete_ReturnsNotFound_WhenIdIsNull()
        {
            var controller = new ModelsController(new AppDbContext(_options));
            var result = await controller.Delete(null);
            Assert.IsType<NotFoundResult>(result);
        }

        //Testea que retorne NotFound cuando el ID es inválido
        [Fact]
        public async Task Delete_ReturnsNotFound_WhenModelNotFound()
        {
            using (var context = new AppDbContext(_options))
            {
                context.Categories.Add(new Category { Id = 1, Name = "Cat A" });
                context.Models.Add(new Model { Id = 1, Name = "Model A", CategoryId = 1 });
                context.SaveChanges();
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new ModelsController(context);
                var result = await controller.Delete(99);
                Assert.IsType<NotFoundResult>(result);
            }
        }
        //-------------------------------------------------FIN GETS-------------------------------------------------------------------

        //-------------------------------------------------POSTS----------------------------------------------------------------------
        [Fact]
        public async Task Create_Post_RedirectsToIndex_WhenModelIsValid()
        {
            using (var context = new AppDbContext(_options))
            {
                var controller = new ModelsController(context);

                var newModel = new Model { Name = "Nuevo Modelo", CategoryId = 1 };

                var result = await controller.Create(newModel) as RedirectToActionResult;

                Assert.NotNull(result);
                Assert.Equal("Index", result.ActionName);

                // Verificamos que se haya agregado el modelo a la base
                Assert.Equal(1, context.Models.Count());
                Assert.Equal("Nuevo Modelo", context.Models.First().Name);
            }
        }

        [Fact]
        public async Task Create_Post_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            using (var context = new AppDbContext(_options))
            {
                var controller = new ModelsController(context);
                controller.ModelState.AddModelError("Name", "El nombre es requerido");

                var newModel = new Model { Name = "", CategoryId = 1 };

                var result = await controller.Create(newModel) as ViewResult;

                Assert.NotNull(result);
                var model = Assert.IsAssignableFrom<Model>(result.Model);
                Assert.Equal(newModel, model);
            }
        }

        [Fact]
        public async Task Edit_Post_RedirectsToIndex_WhenModelIsValid()
        {
            using (var context = new AppDbContext(_options))
            {
                // Agregamos un modelo existente
                context.Models.Add(new Model { Id = 1, Name = "Original", CategoryId = 1 });
                context.SaveChanges();
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new ModelsController(context);

                var editedModel = new Model { Id = 1, Name = "Editado", CategoryId = 1 };

                var result = await controller.Edit(
                    1,
                    editedModel,
                    new List<string>(),  // ExistingManuals
                    null,               // Manuals
                    new List<string>(),  // DeletedManuals
                    new List<string>(),  // ExistingSpareKits
                    null,               // SpareKits
                    new List<string>()   // DeletedSpareKits
                ) as RedirectToActionResult;

                Assert.NotNull(result);
                Assert.Equal("Index", result.ActionName);

                var modelInDb = context.Models.Find(1);
                Assert.Equal("Editado", modelInDb.Name);
            }
        }

        [Fact]
        public async Task Edit_Post_ReturnsNotFound_WhenIdDoesNotMatchModelId()
        {
            var controller = new ModelsController(new AppDbContext(_options));

            var result = await controller.Edit(
                1,
                new Model { Id = 2, Name = "X", CategoryId = 1 },
                new List<string>(),
                null,
                new List<string>(),
                new List<string>(),
                null,
                new List<string>()
            );

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            using (var context = new AppDbContext(_options))
            {
                context.Models.Add(new Model { Id = 1, Name = "Original", CategoryId = 1 });
                context.SaveChanges();
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new ModelsController(context);
                controller.ModelState.AddModelError("Name", "Requerido");

                var editedModel = new Model { Id = 1, Name = "", CategoryId = 1 };

                var result = await controller.Edit(
                    1,
                    editedModel,
                    new List<string>(),
                    null,
                    new List<string>(),
                    new List<string>(),
                    null,
                    new List<string>()
                ) as ViewResult;

                Assert.NotNull(result);
                var model = Assert.IsAssignableFrom<Model>(result.Model);
                Assert.Equal(editedModel, model);
            }
        }

        [Fact]
        public async Task DeleteConfirmed_RemovesModel_AndRedirectsToIndex()
        {
            using (var context = new AppDbContext(_options))
            {
                context.Models.Add(new Model { Id = 1, Name = "Para borrar", CategoryId = 1 });
                context.SaveChanges();
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new ModelsController(context);

                var result = await controller.DeleteConfirmed(1) as RedirectToActionResult;

                Assert.NotNull(result);
                Assert.Equal("Index", result.ActionName);
                Assert.Empty(context.Models);
            }
        }

        [Fact]
        public async Task Edit_Post_AddsManualsAndSpareKits_WhenProvided()
        {
            // Preparar contexto y datos iniciales
            using var context = new AppDbContext(_options);
            context.Models.Add(new Model { Id = 1, Name = "Original", CategoryId = 1 });
            context.SaveChanges();

            var controller = new ModelsController(context);

            // Mockear IFormFile para manuales
            var manualMock = new Mock<IFormFile>();
            manualMock.Setup(f => f.FileName).Returns("manual.pdf");
            manualMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[] { 1, 2, 3 }));
            manualMock.Setup(f => f.Length).Returns(3);
            manualMock.Setup(f => f.ContentType).Returns("application/pdf");

            var manuals = new List<IFormFile> { manualMock.Object };

            // Mockear IFormFile para spare kits
            var spareKitMock = new Mock<IFormFile>();
            spareKitMock.Setup(f => f.FileName).Returns("sparekit.pdf");
            spareKitMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[] { 4, 5, 6 }));
            spareKitMock.Setup(f => f.Length).Returns(3);
            spareKitMock.Setup(f => f.ContentType).Returns("application/pdf");

            var spareKits = new List<IFormFile> { spareKitMock.Object };

            // Ejecutar acción Edit con parámetros exactos (mayúsculas incluidas)
            var result = await controller.Edit(1,
                                              new Model { Id = 1, Name = "Editado", CategoryId = 1 },
                                              ExistingManuals: new List<string>(),
                                              Manuals: manuals,
                                              DeletedManuals: new List<string>(),
                                              ExistingSpareKits: new List<string>(),
                                              SpareKits: spareKits,
                                              DeletedSpareKits: new List<string>()) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);

            // Validar que el nombre se actualizó
            var updatedModel = await context.Models.FindAsync(1);
            Assert.Equal("Editado", updatedModel.Name);

        }
        [Fact]
        public async Task DeleteConfirmed_ReturnsNotFound_WhenIdDoesNotExist()
        {
            using var context = new AppDbContext(_options);
            var controller = new ModelsController(context);

            var result = await controller.DeleteConfirmed(99);

            Assert.IsType<NotFoundResult>(result);
        }

        //----------------------------------------------FIN POSTS----------------------------------------------------------


    }
}
