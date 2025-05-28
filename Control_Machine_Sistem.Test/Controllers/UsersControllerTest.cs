using Xunit;
using Control_Machine_Sistem.Controllers;
using Control_Machine_Sistem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using BCrypt.Net;

namespace Control_Machine_Sistem.Test.Controllers
{
    public class UsersControllerTests
    {
        // Método auxiliar para crear un AppDbContext en memoria con una base de datos única en cada test
        private AppDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                            .Options;
            return new AppDbContext(options);
        }

        //**Test para la Acción GET del Index**
        //- Propósito:
        //Verificar que al llamar a la acción Index se retorne la lista de usuarios disponibles en la base de datos.
        //- Cómo se hizo:
        //-Arrange: Se agregan usuarios al contexto en memoria.
        //- Act: Se llama a la acción Index().
        //- Assert: Se verifica que el resultado sea un ViewResult y que el modelo contenga la colección de usuarios con la cuenta esperada.
        [Fact]
        public async Task Index_ReturnsUserList()
        {
            // Arrange
            var context = GetInMemoryContext();
            context.Users.Add(new User { Id = 1, Name = "User1", Email = "user1@example.com", Password = "pass1", Rol = "Role1" });
            context.Users.Add(new User { Id = 2, Name = "User2", Email = "user2@example.com", Password = "pass2", Rol = "Role2" });
            context.SaveChanges();
            var controller = new UsersController(context);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<User>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        //**Test para la Acción GET de Details cuando el ID es nulo**
        //- Propósito:
        //Verificar que al llamar a Details con un ID nulo se retorne NotFound.
        //- Cómo se hizo:
        //-Arrange: Se crea el controller.
        //- Act: Se llama a Details(null).
        //- Assert: Se comprueba que el resultado es un NotFoundResult.
        [Fact]
        public async Task Details_NullId_ReturnsNotFound()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new UsersController(context);

            // Act
            var result = await controller.Details(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        //**Test para la Acción GET de Details cuando el usuario no existe**
        //- Propósito:
        //Verificar que al solicitar los detalles de un usuario inexistente se retorne NotFound.
        //- Cómo se hizo:
        //-Arrange: Se crea el controller sin usuarios en el contexto.
        //- Act: Se llama a Details con un ID que no existe.
        //- Assert: Se verifica que se retorne NotFound.
        [Fact]
        public async Task Details_NonExistentUser_ReturnsNotFound()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new UsersController(context);

            // Act
            var result = await controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        //**Test para la Acción GET de Details cuando el usuario existe**
        //- Propósito:
        //Verificar que al solicitar los detalles de un usuario existente se retorne la vista con el modelo correcto.
        //- Cómo se hizo:
        //-Arrange: Se agrega un usuario al contexto.
        //- Act: Se llama a Details con el ID del usuario.
        //- Assert: Se verifica que el resultado sea un ViewResult y que el modelo tenga los datos esperados.
        [Fact]
        public async Task Details_ExistingUser_ReturnsViewWithUser()
        {
            // Arrange
            var context = GetInMemoryContext();
            var user = new User { Id = 1, Name = "Test User", Email = "test@example.com", Password = "pass", Rol = "User" };
            context.Users.Add(user);
            context.SaveChanges();

            var controller = new UsersController(context);

            // Act
            var result = await controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<User>(viewResult.Model);
            Assert.Equal("Test User", model.Name);
        }

        //**Test para la Acción GET de Create**
        //- Propósito:
        //Verificar que la acción GET de Create retorne la vista para crear un usuario.
        //- Cómo se hizo:
        //-Arrange: Se crea el controller.
        //- Act: Se llama a la acción Create().
        //- Assert: Se verifica que el resultado sea un ViewResult.
        [Fact]
        public void Create_Get_ReturnsViewResult()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new UsersController(context);

            // Act
            var result = controller.Create();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        //**Test para la Acción POST de Create con datos válidos**
        //- Propósito:
        //Verificar que al enviar datos válidos se cree un usuario, se convierta la contraseña a hash y se redirija a Index.
        //- Cómo se hizo:
        //-Arrange: Se crea un usuario (con contraseña en texto plano) y el contexto en memoria.
        //- Act: Se llama a la acción Create(user) y se guarda.
        //- Assert: Se verifica que se retorne un RedirectToActionResult y que el usuario en la base de datos tenga la contraseña hasheada.
        [Fact]
        public async Task Create_Post_WithValidData_RedirectsToIndex()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new UsersController(context);
            var user = new User { Id = 1, Name = "New User", Email = "new@example.com", Password = "plainPassword", Rol = "User" };

            // Act
            var result = await controller.Create(user);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            var createdUser = context.Users.FirstOrDefault(u => u.Id == 1);
            Assert.NotNull(createdUser);
            // Verificar que la contraseña ha sido hasheada, es decir que ya no es igual a "plainPassword"
            Assert.NotEqual("plainPassword", createdUser.Password);
            Assert.True(BCrypt.Net.BCrypt.Verify("plainPassword", createdUser.Password));
        }

        //**Test para la Acción POST de Create con datos inválidos**
        //- Propósito:
        //Verificar que al enviar datos inválidos (ModelState inválido) se retorne la vista con el mismo modelo.
        //- Cómo se hizo:
        //-Arrange: Se añade un error al ModelState.
        //- Act: Se llama a la acción Create(user).
        //- Assert: Se verifica que se retorne un ViewResult y se conserve el modelo enviado.
        [Fact]
        public async Task Create_Post_WithInvalidData_ReturnsViewWithModel()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new UsersController(context);
            controller.ModelState.AddModelError("Name", "El nombre es obligatorio");
            var user = new User { Id = 1, Name = "", Email = "invalid@example.com", Password = "pass", Rol = "User" };

            // Act
            var result = await controller.Create(user);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(user, viewResult.Model);
        }

        //**Test para la Acción GET de Edit cuando el ID es nulo**
        //- Propósito:
        //Verificar que si se llama a Edit con ID nulo se retorne NotFound.
        //- Cómo se hizo:
        //-Arrange: Se crea el controller.
        //- Act: Se llama a Edit(null).
        //- Assert: Se verifica que el resultado sea NotFound.
        [Fact]
        public async Task Edit_Get_NullId_ReturnsNotFound()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new UsersController(context);

            // Act
            var result = await controller.Edit(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        //**Test para la Acción GET de Edit cuando el usuario no existe**
        //- Propósito:
        //Verificar que si se solicita editar un usuario inexistente se retorne NotFound.
        //- Cómo se hizo:
        //-Arrange: Se crea un contexto sin el usuario solicitado.
        //- Act: Se llama a Edit(id) con un ID que no exista.
        //- Assert: Se verifica que se retorne NotFound.
        [Fact]
        public async Task Edit_Get_NonExistentUser_ReturnsNotFound()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new UsersController(context);

            // Act
            var result = await controller.Edit(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        //**Test para la Acción GET de Edit cuando el usuario existe**
        //- Propósito:
        //Verificar que al solicitar la edición de un usuario existente se retorne la vista con el modelo correcto.
        //- Cómo se hizo:
        //-Arrange: Se agrega un usuario al contexto.
        //- Act: Se llama a Edit(id) con un ID existente.
        //- Assert: Se verifica que se retorne un ViewResult y que el modelo corresponde al usuario solicitado.
        [Fact]
        public async Task Edit_Get_ExistingUser_ReturnsViewWithUser()
        {
            // Arrange
            var context = GetInMemoryContext();
            var user = new User { Id = 1, Name = "Edit User", Email = "edit@example.com", Password = "pass", Rol = "User" };
            context.Users.Add(user);
            context.SaveChanges();

            var controller = new UsersController(context);

            // Act
            var result = await controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<User>(viewResult.Model);
            Assert.Equal("Edit User", model.Name);
        }

        //**Test para la Acción POST de Edit cuando el ID no coincide con el usuario enviado**
        //- Propósito:
        //Verificar que si el ID pasado en la URL no coincide con el ID del modelo se retorne NotFound.
        //- Cómo se hizo:
        //-Arrange: Se prepara un usuario con ID 1.
        //- Act: Se llama a Edit con id 2 y el modelo de usuario con id 1.
        //- Assert: Se verifica que se retorne NotFound.
        [Fact]
        public async Task Edit_Post_IdMismatch_ReturnsNotFound()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new UsersController(context);
            var user = new User { Id = 1, Name = "User", Email = "user@example.com", Password = "pass", Rol = "User" };

            // Act
            var result = await controller.Edit(2, user);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        //**Test para la Acción POST de Edit con datos inválidos**
        //- Propósito:
        //Verificar que si el ModelState es inválido, se retorne la vista con el mismo modelo, sin proceder a la actualización.
        //- Cómo se hizo:
        //-Arrange: Se añade un error al ModelState.
        //- Act: Se llama a Edit(id, user) con datos inválidos.
        //- Assert: Se verifica que se retorne un ViewResult con el mismo modelo.
        [Fact]
        public async Task Edit_Post_WithInvalidData_ReturnsViewWithModel()
        {
            // Arrange
            var context = GetInMemoryContext();
            var user = new User { Id = 1, Name = "User", Email = "user@example.com", Password = "pass", Rol = "User" };
            context.Users.Add(user);
            context.SaveChanges();

            var controller = new UsersController(context);
            controller.ModelState.AddModelError("Name", "El nombre es obligatorio");

            // Act
            var result = await controller.Edit(1, user);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(user, viewResult.Model);
        }

        //**Test para la Acción POST de Edit cuando se envía cadena vacía en Password (debe conservar la contraseña original)**
        //- Propósito:
        //Verificar que si en la edición se envía una cadena vacía o espacios en blanco para la contraseña,
        //el sistema retenga la contraseña original sin modificarla.
        //- Cómo se hizo:
        //-Arrange: Se agrega un usuario al contexto con una contraseña ya hasheada.
        //- Se prepara un modelo de usuario para editar con Password vacío.
        //- Act: Se llama a Edit(id, user) y se guarda.
        //- Assert: Se verifica que el usuario actualizado conservó la contraseña original.
        [Fact]
        public async Task Edit_Post_WithEmptyPassword_RetainsOriginalPassword()
        {
            // Arrange
            var context = GetInMemoryContext();
            // Creamos un usuario con contraseña ya hasheada
            var originalPassword = BCrypt.Net.BCrypt.HashPassword("originalPass");
            var user = new User { Id = 1, Name = "User", Email = "user@example.com", Password = originalPassword, Rol = "User" };
            context.Users.Add(user);
            context.SaveChanges();

            var controller = new UsersController(context);
            // Crear el modelo de edición con la misma información, pero Password vacío
            var editedUser = new User { Id = 1, Name = "User Edited", Email = "user@example.com", Password = "", Rol = "User" };

            // Act
            var result = await controller.Edit(1, editedUser);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Verificar que la contraseña no cambió
            var updatedUser = context.Users.Find(1);
            Assert.Equal(originalPassword, updatedUser.Password);
        }

        //**Test para la Acción POST de Edit cuando se envía una nueva contraseña**
        //- Propósito:
        //Verificar que al enviar una nueva contraseña, ésta se convierta a hash y se actualice en la base de datos.
        //- Cómo se hizo:
        //-Arrange: Se agrega un usuario con contraseña original.
        //- Se prepara un modelo de usuario para editar con una nueva contraseña.
        //- Act: Se llama a Edit(id, user) y se guarda.
        //- Assert: Se verifica que la contraseña en la base de datos haya cambiado y que sea un hash válido correspondiente a la nueva contraseña.
        [Fact]
        public async Task Edit_Post_WithNewPassword_UpdatesAndHashesPassword()
        {
            // Arrange
            var context = GetInMemoryContext();
            var originalPassword = BCrypt.Net.BCrypt.HashPassword("originalPass");
            var user = new User { Id = 1, Name = "User", Email = "user@example.com", Password = originalPassword, Rol = "User" };
            context.Users.Add(user);
            context.SaveChanges();

            var controller = new UsersController(context);
            // Modelo de edición con nueva contraseña
            var editedUser = new User { Id = 1, Name = "User Edited", Email = "user@example.com", Password = "newPass", Rol = "User" };

            // Act
            var result = await controller.Edit(1, editedUser);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var updatedUser = context.Users.Find(1);
            // La nueva contraseña ya no debe ser igual a la antigua ni en claro
            Assert.NotEqual(originalPassword, updatedUser.Password);
            Assert.True(BCrypt.Net.BCrypt.Verify("newPass", updatedUser.Password));
        }

        //**Test para la Acción GET de Delete cuando el ID es nulo**
        //- Propósito:
        //Verificar que al llamar a Delete con un ID nulo se retorne NotFound.
        //- Cómo se hizo:
        //-Arrange: Se crea el controller.
        //- Act: Se llama a Delete(null).
        //- Assert: Se verifica que el resultado sea NotFound.
        [Fact]
        public async Task Delete_Get_NullId_ReturnsNotFound()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new UsersController(context);

            // Act
            var result = await controller.Delete(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        //**Test para la Acción GET de Delete cuando el usuario no existe**
        //- Propósito:
        //Verificar que si se solicita la eliminación de un usuario inexistente se retorne NotFound.
        //- Cómo se hizo:
        //-Arrange: Se crea el controller sin agregar el usuario solicitado.
        //- Act: Se llama a Delete(id) con un ID que no existe.
        //- Assert: Se verifica que se retorne NotFound.
        [Fact]
        public async Task Delete_Get_NonExistentUser_ReturnsNotFound()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new UsersController(context);

            // Act
            var result = await controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        //**Test para la Acción GET de Delete cuando el usuario existe**
        //- Propósito:
        //Verificar que al solicitar la eliminación de un usuario existente se retorne la vista con el modelo correcto.
        //- Cómo se hizo:
        //-Arrange: Se agrega un usuario al contexto.
        //- Act: Se llama a Delete(id) con un ID existente.
        //- Assert: Se verifica que el resultado sea un ViewResult con el modelo del usuario.
        [Fact]
        public async Task Delete_Get_ExistingUser_ReturnsViewWithUser()
        {
            // Arrange
            var context = GetInMemoryContext();
            var user = new User { Id = 1, Name = "User Delete", Email = "delete@example.com", Password = "pass", Rol = "User" };
            context.Users.Add(user);
            context.SaveChanges();

            var controller = new UsersController(context);

            // Act
            var result = await controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<User>(viewResult.Model);
            Assert.Equal("User Delete", model.Name);
        }

        //**Test para la Acción POST de Delete (DeleteConfirmed) cuando el usuario existe**
        //- Propósito:
        //Verificar que al confirmar la eliminación de un usuario existente, se elimine el usuario y se redirija a Index.
        //- Cómo se hizo:
        //-Arrange: Se agrega un usuario al contexto.
        //- Act: Se llama a DeleteConfirmed(id).
        //- Assert: Se verifica que se retorne un RedirectToActionResult y que el usuario ya no exista en el contexto.
        [Fact]
        public async Task Delete_Post_ExistingUser_RedirectsToIndex()
        {
            // Arrange
            var context = GetInMemoryContext();
            var user = new User { Id = 1, Name = "User to Delete", Email = "delete@example.com", Password = "pass", Rol = "User" };
            context.Users.Add(user);
            context.SaveChanges();

            var controller = new UsersController(context);

            // Act
            var result = await controller.DeleteConfirmed(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Null(context.Users.Find(1));
        }

        //**Test para la Acción POST de Delete (DeleteConfirmed) cuando el usuario no existe**
        //- Propósito:
        //Verificar que si se llama a DeleteConfirmed con un ID inexistente, se redirija a Index sin errores.
        //- Cómo se hizo:
        //-Arrange: Se crea el controller sin el usuario solicitado.
        //- Act: Se llama a DeleteConfirmed(id) con un ID no existente.
        //- Assert: Se verifica que se retorne un RedirectToActionResult a Index.
        [Fact]
        public async Task Delete_Post_NonExistentUser_RedirectsToIndex()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new UsersController(context);

            // Act
            var result = await controller.DeleteConfirmed(999);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
    }
}
