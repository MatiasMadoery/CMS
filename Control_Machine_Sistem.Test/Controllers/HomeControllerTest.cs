using Xunit;
using Control_Machine_Sistem.Controllers;
using Control_Machine_Sistem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Diagnostics;

namespace Control_Machine_Sistem.Test.Controllers
{
    public class HomeControllerTests
    {
        //**Test para la Acción GET del Index**
        //- Propósito:
        //Verificar que al llamar a la acción Index se retorna un ViewResult.
        //- Cómo se hizo:
        //-Arrange: Se crea una instancia de HomeController, inyectando un ILogger simulado.
        //- Act: Se llama a la acción Index().
        //- Assert: Se verifica que el resultado sea un ViewResult.
        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Arrange
            var logger = Mock.Of<ILogger<HomeController>>();
            var controller = new HomeController(logger);

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
        }

        //**Test para la Acción GET de Privacy**
        //- Propósito:
        //Verificar que al llamar a la acción Privacy se retorna un ViewResult.
        //- Cómo se hizo:
        //-Arrange: Se crea una instancia de HomeController con un ILogger simulado.
        //- Act: Se llama a la acción Privacy().
        //- Assert: Se verifica que el resultado sea un ViewResult.
        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            // Arrange
            var logger = Mock.Of<ILogger<HomeController>>();
            var controller = new HomeController(logger);

            // Act
            var result = controller.Privacy();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
        }

        //**Test para la Acción Error**
        //- Propósito:
        //Verificar que al llamar a la acción Error se retorna un ViewResult que contiene un ErrorViewModel con el RequestId correcto.
        //- Cómo se hizo:
        //-Arrange:
        //   Se crea una instancia de HomeController con un ILogger simulado y se configura el HttpContext con un TraceIdentifier específico.
        //- Act: Se llama a la acción Error().
        //- Assert: Se verifica que el resultado sea un ViewResult y que el modelo sea un ErrorViewModel con RequestId igual al TraceIdentifier.
        [Fact]
        public void Error_ReturnsViewResult_WithErrorViewModel()
        {
            // Arrange
            var logger = Mock.Of<ILogger<HomeController>>();
            var controller = new HomeController(logger);

            // Configurar un HttpContext de prueba con TraceIdentifier definido
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "test-trace-identifier";
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            // Dado que Activity.Current es probablemente nulo en el test, se espera que RequestId sea igual a HttpContext.TraceIdentifier
            Assert.Equal("test-trace-identifier", model.RequestId);
        }
    }
}
