using Xunit;
using Control_Machine_Sistem.Controllers;
using Control_Machine_Sistem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Control_Machine_Sistem.Test.Controllers
{
    // Implementación de un servicio "fake" para simular la autenticación en los tests
    public class FakeAuthenticationService : IAuthenticationService
    {
        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string scheme)
            => Task.FromResult(AuthenticateResult.NoResult());

        public Task ChallengeAsync(HttpContext context, string scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignInAsync(HttpContext context, string scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignOutAsync(HttpContext context, string scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;
    }

    // Implementación "fake" para simular TempData
    public class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object?> LoadTempData(HttpContext context)
        {
            return new Dictionary<string, object?>();
        }

        public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
        {
            // No hacemos nada
        }
    }

    public class UsersLoginControllerTest
    {
        // Método auxiliar para crear un AppDbContext en memoria
        private AppDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                            .UseInMemoryDatabase(databaseName: "TestDb")
                            .Options;
            return new AppDbContext(options);
        }

        //**Test para la Acción GET del Login cuando el Usuario ya está Autenticado**
        //- Propósito:
        //Verificar que si el usuario ya está autenticado, al acceder a la acción GET del login se produzca una redirección a Home/Index.
        //- Cómo se hizo:
        //-Arrange: Se crea un UsersLoginController y se configura su HttpContext.User asignándole un ClaimsPrincipal con una identidad válida. Esto simula que el usuario ya está logueado.
        //- Act: Se llama al método Login() (la acción GET).
        //- Assert: Se verifica que la respuesta sea de tipo RedirectToActionResult y que indique redirigir a la acción "Index" del controlador "Home".
        [Fact]
        public void Login_Get_WhenUserIsAuthenticated_RedirectsToHome()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new UsersLoginController(context);

            // Simulamos que el usuario ya está autenticado
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = controller.Login();

            // Assert: se espera un RedirectToActionResult a Home/Index
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        //**Test para la Acción POST del Login con Credenciales Válidas**
        //- Propósito:
        //Verificar que al enviar un LoginDto con credenciales correctas (donde la contraseña ingresada coincide, vía BCrypt, con la del usuario en la base de datos), el controlador:
        //-Llama internamente a SignInAsync para autenticar al usuario.
        //- Redirige a Home/Index.
        //- Cómo se hizo:
        //-Arrange:
        //-Se crea el contexto en memoria y se inserta un usuario de prueba en la base de datos, con la contraseña hasheada usando BCrypt.
        //- Se configura el ControllerContext con un ActionContext completo (incluyendo un DefaultHttpContext, RouteData y un ControllerActionDescriptor).
        //- Se inyecta un ServiceProvider en el HttpContext.RequestServices que incluye tanto el FakeAuthenticationService como el IUrlHelperFactory (usando UrlHelperFactory).
        //- Se prepara un objeto LoginDto con el nombre y la contraseña correctos.
        //- Act: Se llama a la acción Login(LoginDto loginDto).
        //- Assert: Se comprueba que el resultado sea un RedirectToActionResult a Home/Index.
        [Fact]
        public async Task Login_Post_WithValidCredentials_RedirectsToHome()
        {
            // Arrange: Crear un contexto en memoria
            var context = GetInMemoryContext();
            context.Users.RemoveRange(context.Users);
            context.SaveChanges();

            // Crear un usuario de prueba con contraseña hasheada
            var testUser = new User
            {
                Name = "TestUser",
                Password = BCrypt.Net.BCrypt.HashPassword("TestPassword"),
                Rol = "Admin"
            };
            context.Users.Add(testUser);
            context.SaveChanges();

            var controller = new UsersLoginController(context);

            // Crear un HttpContext, RouteData y un ControllerActionDescriptor para simular la ejecución en un entorno real
            var httpContext = new DefaultHttpContext();
            var routeData = new Microsoft.AspNetCore.Routing.RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            // Asignar el ActionContext al ControllerContext
            controller.ControllerContext = new ControllerContext(actionContext);

            // Configurar el ServiceProvider con los servicios necesarios
            var services = new ServiceCollection();
            services.AddSingleton<IAuthenticationService, FakeAuthenticationService>();
            services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();
            var serviceProvider = services.BuildServiceProvider();
            controller.ControllerContext.HttpContext.RequestServices = serviceProvider;

            // Preparar el LoginDto con credenciales correctas
            var loginDto = new LoginDto
            {
                Name = "TestUser",
                Password = "TestPassword"
            };

            // Act
            var result = await controller.Login(loginDto);

            // Assert: Se debe devolver un RedirectToActionResult a Home/Index
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        //**Test para la Acción POST del Login con Credenciales Inválidas**
        //- Propósito:
        //Verificar que si se envían credenciales erróneas (por ejemplo, contraseña incorrecta), la acción POST:
        //-No autentica al usuario.
        //- Agrega un error al ModelState.
        //- Retorna la vista del login para que se pueda mostrar el mensaje de error.
        //- Cómo se hizo:
        //-Arrange:
        //-Se configura nuevamente el contexto en memoria y se inserta un usuario de prueba.
        //- Se prepara un LoginDto con el mismo nombre del usuario pero una contraseña equivocada.
        //- Se configura un HttpContext y se inyecta un ServiceProvider con los servicios necesarios como en el caso anterior.
        //- Act: Se llama a la acción Login(LoginDto loginDto).
        //- Assert: Se verifica que el resultado sea un ViewResult y se comprueba que el ModelState esté marcado como no válido (con al menos un error).
        [Fact]
        public async Task Login_Post_WithInvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            var context = GetInMemoryContext();
            // Limpia la base de datos
            context.Users.RemoveRange(context.Users);
            context.SaveChanges();

            // Crear un usuario de prueba
            var testUser = new User
            {
                Name = "TestUser",
                Password = BCrypt.Net.BCrypt.HashPassword("TestPassword"),
                Rol = "Admin"
            };
            context.Users.Add(testUser);
            context.SaveChanges();

            var controller = new UsersLoginController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Preparamos un loginDto con contraseña errónea
            var loginDto = new LoginDto
            {
                Name = "TestUser",
                Password = "WrongPassword"
            };

            // Act
            var result = await controller.Login(loginDto);

            // Assert: se espera que el resultado sea una View y que ModelState contenga errores
            var viewResult = Assert.IsType<ViewResult>(result);
            // Se agregó un error al ModelState en el controller si las credenciales no coinciden
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ErrorCount > 0);
        }

        //**Test para la Acción POST del Login cuando el ModelState es inválido**
        //- Propósito:
        //Verificar que si el modelo recibido es inválido (por ejemplo, falta un dato obligatorio), la acción POST retorne la vista del login sin procesar la autenticación.
        //- Cómo se hizo:
        //-Arrange:
        //-Se configura un ControllerContext con datos válidos.
        //-Se registra un ServiceProvider con los servicios necesarios.
        //-Se agrega manualmente un error al ModelState (para simular datos inválidos).
        //-Se prepara un LoginDto con datos inválidos.
        //- Act: Se llama a la acción Login(LoginDto loginDto).
        //- Assert: Se verifica que se retorne un ViewResult y que el objeto modelo devuelto sea el mismo que se envió.
        [Fact]
        public async Task Login_Post_WhenModelStateIsInvalid_ReturnsView()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new UsersLoginController(context);

            // Configurar un ActionContext válido
            var httpContext = new DefaultHttpContext();
            var routeData = new Microsoft.AspNetCore.Routing.RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
            controller.ControllerContext = new ControllerContext(actionContext);

            // Registrar los servicios necesarios, incluyendo los relacionados a TempData
            var services = new ServiceCollection();
            services.AddSingleton<IAuthenticationService, FakeAuthenticationService>();
            services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();
            services.AddSingleton<ITempDataProvider, FakeTempDataProvider>();
            services.AddSingleton<ITempDataDictionaryFactory, TempDataDictionaryFactory>();
            controller.ControllerContext.HttpContext.RequestServices = services.BuildServiceProvider();

            // Agregar un error al ModelState para simular un modelo inválido
            controller.ModelState.AddModelError("Name", "El nombre es obligatorio");

            var loginDto = new LoginDto
            {
                Name = "", // Simula dato inválido
                Password = "AlgunaContraseña"
            };

            // Act
            var result = await controller.Login(loginDto);

            // Assert: se retorna un ViewResult y el modelo es el mismo que se envió
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(loginDto, viewResult.Model);
        }

        //**Test para la Acción Logout**
        //- Propósito:
        //Verificar que al llamar a la acción Logout, el controlador efectúe la desconexión (llamando a SignOutAsync internamente) y redirija a la acción Login.
        //- Cómo se hizo:
        //-Arrange:
        //-Se configura un ControllerContext completo (con HttpContext, RouteData y ControllerActionDescriptor).
        //- Se inyecta un ServiceProvider con los servicios necesarios para la autenticación y el URL helper.
        //- Act: Se llama a la acción Logout().
        //- Assert: Se verifica que el resultado sea un RedirectToActionResult redirigiendo a la acción "Login".
        [Fact]
        public async Task Logout_LoggedUser_RedirectsToLogin()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new UsersLoginController(context);

            // Configurar un ActionContext completo
            var httpContext = new DefaultHttpContext();
            var routeData = new Microsoft.AspNetCore.Routing.RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
            controller.ControllerContext = new ControllerContext(actionContext);

            // Registrar los servicios necesarios
            var services = new ServiceCollection();
            services.AddSingleton<IAuthenticationService, FakeAuthenticationService>();
            services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();
            controller.ControllerContext.HttpContext.RequestServices = services.BuildServiceProvider();

            // Act
            var result = await controller.Logout();

            // Assert: se espera un RedirectToActionResult que redirija a la acción "Login"
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        //**Test para la Acción AccessDenied**
        //- Propósito:
        //Verificar que la acción AccessDenied simplemente retorne la vista correspondiente.
        //- Cómo se hizo:
        //-Arrange: Se crea una instancia del controller.
        //- Act: Se llama a la acción AccessDenied().
        //- Assert: Se verifica que el resultado sea un ViewResult.
        [Fact]
        public void AccessDenied_ReturnsViewResult()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new UsersLoginController(context);

            // Act
            var result = controller.AccessDenied();

            // Assert: se espera que el resultado sea un ViewResult
            var viewResult = Assert.IsType<ViewResult>(result);
        }

        //**Test para la Acción GET del Login cuando el Usuario NO está autenticado**
        //- Propósito:
        //Verificar que si el usuario NO está autenticado, al acceder a la acción GET del login se retorne la vista del login.
        //- Cómo se hizo:
        //-Arrange: Se crea un UsersLoginController sin asignar un usuario al HttpContext.
        //- Act: Se llama al método Login() (la acción GET).
        //- Assert: Se verifica que la respuesta sea de tipo ViewResult.
        [Fact]
        public void Login_Get_WhenUserIsNotAuthenticated_ReturnsView()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new UsersLoginController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext() // Sin usuario asignado
            };

            // Act
            var result = controller.Login();

            // Assert: se espera un ViewResult
            var viewResult = Assert.IsType<ViewResult>(result);
        }

        //**Test para la Acción POST del Login cuando el usuario no existe**
        //- Propósito:
        //Verificar que si se envía un LoginDto con un nombre de usuario que no existe en la base de datos, la acción POST:
        //-No autentica al usuario.
        //- Agrega un error al ModelState.
        //- Retorna la vista del login para que se muestre un mensaje de error.
        //- Cómo se hizo:
        //-Arrange:
        //-Se configura un contexto en memoria sin usuarios o asegurando que no exista el usuario.
        //- Se prepara un LoginDto con un nombre de usuario que no se encuentre.
        //- Se configura un HttpContext y se inyecta un ServiceProvider con los servicios necesarios.
        //- Act: Se llama a la acción Login(LoginDto loginDto).
        //- Assert: Se verifica que el resultado sea un ViewResult y que el ModelState tenga errores.
        [Fact]
        public async Task Login_Post_WhenUserDoesNotExist_ReturnsViewWithError()
        {
            // Arrange
            var context = GetInMemoryContext();
            // Aseguramos que no haya usuarios en la base de datos
            context.Users.RemoveRange(context.Users);
            context.SaveChanges();

            var controller = new UsersLoginController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Preparar un LoginDto con un usuario que no existe
            var loginDto = new LoginDto
            {
                Name = "NonExistingUser",
                Password = "AnyPassword"
            };

            // Act
            var result = await controller.Login(loginDto);

            // Assert: se espera que el resultado sea un ViewResult y ModelState contenga errores
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ErrorCount > 0);
        }
    }
}



