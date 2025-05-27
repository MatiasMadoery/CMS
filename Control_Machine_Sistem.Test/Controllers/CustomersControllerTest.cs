using Control_Machine_Sistem.Controllers;
using Control_Machine_Sistem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;


namespace Control_Machine_Sistem.Test.Controllers
{
    public class CustomersControllerTest
    {
        private DbContextOptions<AppDbContext> _options;

        public CustomersControllerTest()
        {
            // Cada test tendrá una base única con Guid
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithPaginatedCustomers()
        {
            //Base de datos en memoria y agrega 3 clientes
            using (var context = new AppDbContext(_options))
            {
                context.Customers.AddRange(
                    new Customer { Name = "Juan", LastName = "Pérez" },
                    new Customer { Name = "Ana", LastName = "García" },
                    new Customer { Name = "Luis", LastName = "Martínez" }
                );
                context.SaveChanges();
            }

            using (var context = new AppDbContext(_options)) { 
                var controller = new CustomersController(context);

                //Se llama index sin filtro de busqueda(null), primera pagina (1), mostrar 2 elementos por pag (2). Se espera el ViewResult con el Pager
                var result = await controller.Index(null, 1, 2) as ViewResult;
                //Verifica que la vista se haya devuelto correctamente.
                Assert.NotNull(result);
                //Verifica que el modelo pasado a la vista sea del tipo correcto.
                var model = Assert.IsAssignableFrom<Pager<Customer>>(result.Model);
                //Confirma que el Pager<Customer> contiene 2 elementos, lo cual es lo correcto para la primera página (con 3 elementos totales y tamaño de página 2).
                Assert.Equal(2, model.Elements.Count());
            }
        }


        [Fact]
        public async Task Index_WithSearchString_NoMatches()
        {
            using ( var context = new AppDbContext(_options))
            {
                context.Customers.AddRange(
                    new Customer { Name = "Roberto", LastName = "Pérez" },
                    new Customer { Name = "Martin", LastName = "García" },
                    new Customer { Name = "Carlos", LastName = "Carles" }
                );
                context.SaveChanges();
            }

            using ( var context = new AppDbContext(_options))
            {
                var controller = new CustomersController(context);
                string searchString = "an"; //No debbería coincidir con nadie

                var result = await controller.Index(searchString, page: 1, pageSize: 5) as ViewResult;

                Assert.NotNull(result);
                var model = Assert.IsAssignableFrom<Pager<Customer>>(result.Model);
                Assert.Equal(0, model.Elements.Count);
            }
   
        }

        [Fact]
        public async Task Index_WithSearchString_FiltersData()
        {
            // Arrange: Inicializamos la base en memoria con datos de prueba
            using (var context = new AppDbContext(_options))
            {
                context.Customers.AddRange(
                    new Customer { Name = "Juan", LastName = "Pérez" },
                    new Customer { Name = "Jana", LastName = "García" },
                    new Customer { Name = "Carlos", LastName = "Santana" }
                );
                context.SaveChanges();
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new CustomersController(context);
                string searchString = "an"; // Debería coincidir con "Ana" y "Carlos" (en el apellido o nombre)

                // Act: Se invoca el método con búsqueda y tamaño de página 5 para traer todos los resultados
                var result = await controller.Index(searchString, page: 1, pageSize: 5) as ViewResult;

                // Assert: Validamos la vista y que se hayan obtenido los clientes filtrados
                Assert.NotNull(result);
                var model = Assert.IsAssignableFrom<Pager<Customer>>(result.Model);

                // Se espera que se filtren "Jana", "Juan" y "Carlos" si se contiene "an")
                Assert.Equal(3, model.Elements.Count);
            }
        }

        [Fact]
        public async Task Index_ReturnsCorrectPage_ForPage2()
        {
            // Arrange: Agregamos 5 clientes en total
            using (var context = new AppDbContext(_options))
            {
                context.Customers.AddRange(
                    new Customer { Name = "Cliente1", LastName = "Apellido1" },
                    new Customer { Name = "Cliente2", LastName = "Apellido2" },
                    new Customer { Name = "Cliente3", LastName = "Apellido3" },
                    new Customer { Name = "Cliente4", LastName = "Apellido4" },
                    new Customer { Name = "Cliente5", LastName = "Apellido5" }
                );
                context.SaveChanges();
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new CustomersController(context);
                int pageSize = 2;
                int page = 2; // Solicitamos la segunda página

                // Act: Se obtiene la segunda página
                var result = await controller.Index(null, page, pageSize) as ViewResult;

                // Assert
                Assert.NotNull(result);
                var model = Assert.IsAssignableFrom<Pager<Customer>>(result.Model);

                // La segunda página debe tener 2 elementos (2 de la primera, 2 de la segunda y 1 en la última página)
                Assert.Equal(2, model.Elements.Count);
                Assert.Equal(5, model.TotalElements); // Total de elementos en el contexto
                Assert.Equal(3, model.TotalPages); // 5 elementos con tamaño 2: 3 páginas (2,2,1)
            }
        }

        [Fact]
        public async Task Index_WithNoCustomers_ReturnsEmptyPager()
        {
            // Arrange: Aseguramos que la base de datos esté vacía
            using (var context = new AppDbContext(_options))
            {
                // Se elimina cualquier dato existente si es que hay
                context.Customers.RemoveRange(context.Customers);
                context.SaveChanges();
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new CustomersController(context);

                // Act: Se llama al método Index sin clientes y con tamaño de página 5
                var result = await controller.Index(null, page: 1, pageSize: 5) as ViewResult;

                // Assert
                Assert.NotNull(result);
                var model = Assert.IsAssignableFrom<Pager<Customer>>(result.Model);
                Assert.Empty(model.Elements); // La lista debe estar vacía
                Assert.Equal(0, model.TotalElements);
                Assert.Equal(0, model.TotalPages);
            }
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            using var context = new AppDbContext(_options);
            var controller = new CustomersController(context);

            var newCustomer = new Customer
            {
                Name = "Pedro",
                LastName = "Gómez",
                Email = "pedro@example.com"
            };

            var result = await controller.Create(newCustomer) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);

            // Verifico que el cliente se haya agregado
            var customerInDb = await context.Customers.FirstOrDefaultAsync(c => c.Email == "pedro@example.com");
            Assert.NotNull(customerInDb);
            Assert.Equal("Pedro", customerInDb.Name);
        }

        [Fact]
        public void Create_Get_ReturnsView()
        {
            using var context = new AppDbContext(_options);
            var controller = new CustomersController(context);

            var result = controller.Create() as ViewResult;

            Assert.NotNull(result);
        }


        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsViewWithModel()
        {
            using var context = new AppDbContext(_options);
            var controller = new CustomersController(context);

            // Simula error de validación
            controller.ModelState.AddModelError("Name", "El nombre es obligatorio");

            var newCustomer = new Customer
            {
                LastName = "Gómez"
            };

            var result = await controller.Create(newCustomer) as ViewResult;

            Assert.NotNull(result);
            var model = Assert.IsType<Customer>(result.Model);
            Assert.Equal(newCustomer, model);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_UpdatesCustomer()
        {
            int customerId;
            using (var context = new AppDbContext(_options))
            {
                var customer = new Customer { Name = "OldName", LastName = "OldLastName" };
                context.Customers.Add(customer);
                await context.SaveChangesAsync();
                customerId = customer.Id;
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new CustomersController(context);
                var updatedCustomer = new Customer
                {
                    Id = customerId,
                    Name = "NewName",
                    LastName = "NewLastName"
                };

                var result = await controller.Edit(customerId, updatedCustomer) as RedirectToActionResult;

                Assert.NotNull(result);
                Assert.Equal("Index", result.ActionName);

                var customerInDb = await context.Customers.FindAsync(customerId);
                Assert.Equal("NewName", customerInDb.Name);
                Assert.Equal("NewLastName", customerInDb.LastName);
            }
        }

        [Fact]
        public async Task Edit_Post_InvalidId_ReturnsNotFound()
        {
            using var context = new AppDbContext(_options);
            var controller = new CustomersController(context);

            var updatedCustomer = new Customer { Id = 999, Name = "Name", LastName = "LastName" };

            var result = await controller.Edit(999, updatedCustomer);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ValidId_ReturnsView()
        {
            using var context = new AppDbContext(_options);
            var customer = new Customer { Id = 1, Name = "Test", LastName = "Test" };
            context.Customers.Add(customer);
            context.SaveChanges();

            var controller = new CustomersController(context);

            var result = await controller.Edit(1) as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<Customer>(result.Model);
        }

        [Fact]
        public async Task Edit_Get_InvalidId_ReturnsNotFound()
        {
            using var context = new AppDbContext(_options);
            var controller = new CustomersController(context);

            var result = await controller.Edit(999);

            Assert.IsType<NotFoundResult>(result);
        }


        [Fact]
        public async Task DeleteConfirmed_RemovesCustomerWithoutMachinesAndRedirects()
        {
            // Arrange
            using (var context = new AppDbContext(_options))
            {
                var customer = new Customer
                {
                    Name = "Test",
                    LastName = "User",
                    Machines = new List<Machine>() // Sin máquinas asociadas
                };
                context.Customers.Add(customer);
                await context.SaveChangesAsync();
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new CustomersController(context);

                // Simular TempData
                controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

                var customer = await context.Customers.FirstAsync();

                // Act
                //Espera poder eliminar el usuario ya que no tiene maquinas asociadas
                var result = await controller.DeleteConfirmed(customer.Id) as RedirectToActionResult;

                // Assert
                Assert.NotNull(result);
                Assert.Equal("Index", result.ActionName);
                
                var deletedCustomer = await context.Customers.FindAsync(customer.Id);
                Assert.Null(deletedCustomer);
            }
        }

        [Fact]
        public async Task DeleteConfirmed_WithMachinesAssociated_DoesNotDeleteCustomerAndRedirects()
        {
            // Arrange
            using (var context = new AppDbContext(_options))
            {
                // Crear la categoría requerida por el modelo
                var category = new Category { Id = 1, Name = "Excavadoras" };
                context.Categories.Add(category);

                // Crear el modelo con referencia a la categoría
                var model = new Model
                {
                    Id = 1,
                    Name = "Modelo X",
                    CategoryId = 1,
                    Category = category
                };
                context.Models.Add(model);

                // Crear el cliente
                var customer = new Customer { Id = 1, Name = "Juan", LastName = "Pérez" };

                // Crear la máquina asociada
                var machine = new Machine
                {
                    Id = 1,
                    CustomerId = 1,
                    Customer = customer,
                    ModelId = 1,
                    Model = model
                };

                customer.Machines = new List<Machine> { machine };

                // Agregar a la base
                context.Customers.Add(customer);
                context.Machines.Add(machine);

                context.SaveChanges();
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new CustomersController(context);

                // Simular TempData
                controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

                var customer = await context.Customers.FirstAsync();

                // Act
                var result = await controller.DeleteConfirmed(customer.Id) as RedirectToActionResult;

                // Assert
                Assert.NotNull(result);
                Assert.Equal("Index", result.ActionName);

                var stillExists = await context.Customers.FindAsync(customer.Id);
                Assert.NotNull(stillExists);

                Assert.True(controller.TempData.ContainsKey("ErrorMessage"));
                Assert.Equal("No se puede eliminar el cliente porque tiene máquinas asociadas.", controller.TempData["ErrorMessage"]);
            }
        }
        [Fact]
        public async Task Delete_Get_ValidId_ReturnsView()
        {
            using var context = new AppDbContext(_options);
            var customer = new Customer { Id = 1, Name = "Test", LastName = "Test" };
            context.Customers.Add(customer);
            context.SaveChanges();

            var controller = new CustomersController(context);

            var result = await controller.Delete(1) as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<Customer>(result.Model);
        }

        [Fact]
        public async Task Delete_Get_InvalidId_ReturnsNotFound()
        {
            using var context = new AppDbContext(_options);
            var controller = new CustomersController(context);

            var result = await controller.Delete(999);

            Assert.IsType<NotFoundResult>(result);
        }


        [Fact]
        public async Task Details_ValidId_ReturnsViewWithCustomer()
        {
            int customerId;
            using (var context = new AppDbContext(_options))
            {
                var customer = new Customer { Name = "DetailName", LastName = "DetailLast" };
                context.Customers.Add(customer);
                await context.SaveChangesAsync();
                customerId = customer.Id;
            }

            using (var context = new AppDbContext(_options))
            {
                var controller = new CustomersController(context);
                var result = await controller.Details(customerId) as ViewResult;

                Assert.NotNull(result);
                var model = Assert.IsAssignableFrom<Customer>(result.Model);
                Assert.Equal(customerId, model.Id);
            }
        }

        [Fact]
        public async Task Details_InvalidId_ReturnsNotFound()
        {
            using var context = new AppDbContext(_options);
            var controller = new CustomersController(context);

            var result = await controller.Details(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}

//✅ Acciones del controlador testeadas

// Index (GET)  
// - Sin filtro  
// - Con filtro sin coincidencias  
// - Con coincidencias  
// - Múltiples páginas  
// - Sin datos  

// Create (GET)  
// - Devuelve bien el ViewResult  

// Create (POST)  
// - Modelo válido (redirige a Index)  
// - Modelo inválido (devuelve View con modelo y errores)  

// Edit (GET)  
// - ID válido (devuelve View con modelo)  
// - ID inválido (devuelve NotFound)  

// Edit (POST)  
// - ID válido y actualización correcta (redirige a Index)  
// - ID inválido (devuelve NotFound)  

// Delete (GET)  
// - ID válido (devuelve View con modelo)  
// - ID inválido (devuelve NotFound)  

// DeleteConfirmed (POST)  
// - Cliente sin máquinas (eliminación exitosa y redirige a Index)  
// - Cliente con máquinas (no elimina, redirige a Index y muestra mensaje de error)  

// Details (GET)  
// - ID válido (devuelve View con modelo)  
// - ID inválido (devuelve NotFound)  
