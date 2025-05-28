using Xunit;
using Control_Machine_Sistem.Controllers;
using Control_Machine_Sistem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Control_Machine_Sistem.Test.Controllers
{
    public class CategoriesControllerTests
    {
        // Método auxiliar para crear un AppDbContext en memoria
        private AppDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;
            return new AppDbContext(options);
        }

        //**Test para la Acción GET del Index**
        //- Propósito:
        //Verificar que al acceder a la acción Index, se retorna la lista de categorías disponibles en la base de datos.
        //- Cómo se hizo:
        //-Arrange: Se crean categorías en un contexto en memoria.
        //- Act: Se llama a la acción Index().
        //- Assert: Se verifica que la respuesta es una vista y contiene la lista de categorías esperada.
        [Fact]
        public async Task Index_ReturnsCategoryList()
        {
            // Arrange
            var context = GetInMemoryContext();
            context.Categories.Add(new Category { Id = 1, Name = "Categoría 1" });
            context.Categories.Add(new Category { Id = 2, Name = "Categoría 2" });
            context.SaveChanges();

            var controller = new CategoriesController(context);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Category>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        //**Test para la Acción GET de Details cuando la categoría existe**
        //- Propósito:
        //Verificar que al solicitar los detalles de una categoría existente, se retorna la vista con el modelo correcto.
        [Fact]
        public async Task Details_WhenCategoryExists_ReturnsViewWithCategory()
        {
            // Arrange
            var context = GetInMemoryContext();
            var category = new Category { Id = 1, Name = "Categoría Test" };
            context.Categories.Add(category);
            context.SaveChanges();

            var controller = new CategoriesController(context);

            // Act
            var result = await controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Category>(viewResult.Model);
            Assert.Equal("Categoría Test", model.Name);
        }

        //**Test para la Acción GET de Details cuando la categoría no existe**
        [Fact]
        public async Task Details_WhenCategoryDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new CategoriesController(context);

            // Act
            var result = await controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        //**Test para la Acción POST de Create con datos válidos**
        [Fact]
        public async Task Create_Post_WithValidData_RedirectsToIndex()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new CategoriesController(context);
            var category = new Category { Id = 1, Name = "Nueva Categoría" };

            // Act
            var result = await controller.Create(category);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        //**Test para la Acción POST de Create con datos inválidos**
        [Fact]
        public async Task Create_Post_WithInvalidData_ReturnsViewWithModel()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new CategoriesController(context);
            controller.ModelState.AddModelError("Name", "El nombre es obligatorio");

            var category = new Category { Id = 1, Name = "" }; // Nombre vacío

            // Act
            var result = await controller.Create(category);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(category, viewResult.Model);
        }

        //**Test para la Acción POST de Edit con ID inválido**
        [Fact]
        public async Task Edit_Post_WhenIdMismatch_ReturnsNotFound()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new CategoriesController(context);
            var category = new Category { Id = 1, Name = "Categoría Editada" };

            // Act
            var result = await controller.Edit(2, category);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        //**Test para la Acción POST de Delete cuando la categoría existe**
        [Fact]
        public async Task DeleteConfirmed_WhenCategoryExists_RedirectsToIndex()
        {
            // Arrange
            var context = GetInMemoryContext();
            var category = new Category { Id = 1, Name = "Categoría a eliminar" };
            context.Categories.Add(category);
            context.SaveChanges();

            var controller = new CategoriesController(context);

            // Act
            var result = await controller.DeleteConfirmed(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        //**Test para la Acción POST de Delete cuando la categoría no existe**
        [Fact]
        public async Task DeleteConfirmed_WhenCategoryDoesNotExist_RedirectsToIndex()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new CategoriesController(context);

            // Act
            var result = await controller.DeleteConfirmed(999);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
    }
}
