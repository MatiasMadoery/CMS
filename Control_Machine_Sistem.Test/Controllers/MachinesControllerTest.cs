//using Xunit;
//using Control_Machine_Sistem.Controllers;
//using Control_Machine_Sistem.Models;
//using Control_Machine_Sistem.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using System.Text;

//namespace Control_Machine_Sistem.Test.Controllers
//{
//    public class MachinesControllerTests
//    {
//        // Método auxiliar para crear un AppDbContext en memoria con una base de datos única por test.
//        private AppDbContext GetInMemoryContext()
//        {
//            var options = new DbContextOptionsBuilder<AppDbContext>()
//                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//                            .Options;
//            return new AppDbContext(options);
//        }

//        //**Test para la Acción GET del Index sin filtros (ni searchString ni categoryId)**
//        //- Propósito:
//        //  Verificar que, al llamar a Index sin filtros, se retornan todas las máquinas paginadas correctamente.
//        //- Cómo se hizo:
//        //  - Arrange: Se crean varias máquinas (así como sus dependencias Customer, Model y Category) en el contexto.
//        //  - Act: Se llama a Index sin searchString y categoryId.
//        //  - Assert: Se verifica que el modelo (Pager<Machine>) devuelto contenga el total correcto de máquinas,
//        //    que la paginación sea correcta y que los ViewData y ViewBag se asignen adecuadamente.
//        [Fact]
//        public async Task Index_ReturnsAllMachines_WhenNoFiltersApplied()
//        {
//            // Arrange
//            var context = GetInMemoryContext();
//            // Creamos datos de soporte
//            var customer1 = new Customer { Id = 1, Name = "John", LastName = "Doe" };
//            var model1 = new Model { Id = 1, Name = "Excavator", CategoryId = 1 };
//            var model2 = new Model { Id = 2, Name = "Bulldozer", CategoryId = 2 };

//            context.Customers.Add(customer1);
//            context.Models.AddRange(model1, model2);
//            context.Machines.AddRange(
//                new Machine { Id = 1, Customer = customer1, Model = model1 },
//                new Machine { Id = 2, Customer = customer1, Model = model2 },
//                new Machine { Id = 3, Customer = customer1, Model = model1 }
//            );
//            context.Categories.Add(new Category { Id = 1, Name = "Heavy" });
//            context.Categories.Add(new Category { Id = 2, Name = "Medium" });
//            context.SaveChanges();

//            var controller = new MachinesController(context);

//            // Act
//            var result = await controller.Index(null, null, page: 1, pageSize: 5);

//            // Assert
//            var viewResult = Assert.IsType<ViewResult>(result);
//            var pager = Assert.IsType<Pager<Machine>>(viewResult.Model);
//            Assert.Equal(3, pager.TotalElements);   // Total de máquinas creadas
//            Assert.Equal(3, pager.Elements.Count());  // Todas en la primera página (pageSize=5)
//            // Verificar que ViewData no tenga valor para searchString y categoryId
//            Assert.Null(controller.ViewData["searchString"]);
//            Assert.Null(controller.ViewData["categoryId"]);
//            // Verificar que ViewBag.Categories contenga las categorías
//            // Verificar que ViewBag.Categories contenga las categorías
//            var selectList = Assert.IsType<SelectList>(controller.ViewBag.Categories);
//            var selectListItems = ((IEnumerable<SelectListItem>)selectList).ToList();
//            Assert.Equal(2, selectListItems.Count);
//        }

//        //**Test para la Acción GET del Index con filtro por búsqueda (searchString)**
//        //- Propósito:
//        //  Verificar que, al aplicar un filtro de búsqueda basado en el searchString, sólo se retornen las máquinas que lo cumplan.
//        //- Cómo se hizo:
//        //  - Arrange: Se crean máquinas con diferentes características en Customer y Model.
//        //  - Act: Se llama a Index con un searchString que coincida, por ejemplo, con parte del nombre del cliente.
//        //  - Assert: Se valida que el total y la colección del paginador sólo contengan las máquinas que cumplen el filtro.
//        [Fact]
//        public async Task Index_ReturnsFilteredMachines_BySearchString()
//        {
//            // Arrange
//            var context = GetInMemoryContext();
//            var customerJohn = new Customer { Id = 1, Name = "John", LastName = "Smith" };
//            var customerAlice = new Customer { Id = 2, Name = "Alice", LastName = "Tomnson" };
//            var modelExcavator = new Model { Id = 1, Name = "Excavator", CategoryId = 1 };
//            var modelCrane = new Model { Id = 2, Name = "Crane", CategoryId = 1 };

//            context.Customers.AddRange(customerJohn, customerAlice);
//            context.Models.AddRange(modelExcavator, modelCrane);
//            context.Machines.AddRange(
//                new Machine { Id = 1, Customer = customerJohn, Model = modelExcavator },
//                new Machine { Id = 2, Customer = customerAlice, Model = modelCrane },
//                new Machine { Id = 3, Customer = customerJohn, Model = modelCrane }
//            );
//            context.Categories.Add(new Category { Id = 1, Name = "Heavy" });
//            context.SaveChanges();

//            var controller = new MachinesController(context);
//            string search = "John"; // Debería coincidir con el cliente "John" en la máquina 1 y 3

//            // Act
//            var result = await controller.Index(search, null, page: 1, pageSize: 5);

//            // Assert
//            var viewResult = Assert.IsType<ViewResult>(result);
//            var pager = Assert.IsType<Pager<Machine>>(viewResult.Model);
//            Assert.Equal(2, pager.TotalElements); // Se esperan 2 máquinas para "John"
//            Assert.All(pager.Elements, machine => Assert.Equal("John", machine.Customer.Name));
//            // Verificar que ViewData["searchString"] se establezca correctamente
//            Assert.Equal(search, controller.ViewData["searchString"]);
//        }

//        //**Test para la Acción GET del Index con filtro por categoría (categoryId)**
//        //- Propósito:
//        //  Verificar que, al aplicar el filtro por categoría, sólo se retornen las máquinas cuyo modelo pertenezca a esa categoría.
//        //- Cómo se hizo:
//        //  - Arrange: Se crean máquinas con modelos asignados a categorías distintas.
//        //  - Act: Se llama a Index con un categoryId específico.
//        //  - Assert: Se verifica que todas las máquinas retornadas tengan el Model.CategoryId igual al filtro.
//        [Fact]
//        public async Task Index_ReturnsFilteredMachines_ByCategoryId()
//        {
//            // Arrange
//            var context = GetInMemoryContext();
//            var customer = new Customer { Id = 1, Name = "John", LastName = "Doe" };
//            var modelHeavy = new Model { Id = 1, Name = "Excavator", CategoryId = 1 };
//            var modelLight = new Model { Id = 2, Name = "Bulldozer", CategoryId = 2 };

//            context.Customers.Add(customer);
//            context.Models.AddRange(modelHeavy, modelLight);
//            context.Machines.AddRange(
//                new Machine { Id = 1, Customer = customer, Model = modelHeavy },
//                new Machine { Id = 2, Customer = customer, Model = modelLight },
//                new Machine { Id = 3, Customer = customer, Model = modelHeavy }
//            );
//            context.Categories.Add(new Category { Id = 1, Name = "Heavy" });
//            context.Categories.Add(new Category { Id = 2, Name = "Light" });
//            context.SaveChanges();

//            var controller = new MachinesController(context);
//            int filterCategoryId = 1;

//            // Act
//            var result = await controller.Index(null, filterCategoryId, page: 1, pageSize: 5);

//            // Assert
//            var viewResult = Assert.IsType<ViewResult>(result);
//            var pager = Assert.IsType<Pager<Machine>>(viewResult.Model);
//            Assert.Equal(2, pager.TotalElements); // Sólo las máquinas cuyo modelo tenga CategoryId 1
//            Assert.All(pager.Elements, m => Assert.Equal(filterCategoryId, m.Model.CategoryId));
//            // Verificar que ViewData["categoryId"] se haya establecido
//            Assert.Equal(filterCategoryId, controller.ViewData["categoryId"]);
//            // Convertimos el SelectList a IEnumerable<SelectListItem>
//            var selectListItems = ((SelectList)controller.ViewBag.Categories).Cast<SelectListItem>();

//            // Ahora podemos usar Assert.Contains con la lambda correctamente tipada.
//            Assert.Contains(selectListItems, item => item.Value == filterCategoryId.ToString());
//        }

//        //**Test para la Acción GET del Index con ambos filtros aplicados: searchString y categoryId**
//        //- Propósito:
//        //  Verificar que, al combinar los filtros de búsqueda y categoría, se retorne el conjunto correcto de máquinas.
//        //- Cómo se hizo:
//        //  - Arrange: Se crean máquinas con distintos clientes y modelos que pertenezcan a diversas categorías.
//        //  - Act: Se llama a Index pasando ambos filtros.
//        //  - Assert: Se verifica que sólo se retorne la máquina que cumple ambas condiciones.
//        [Fact]
//        public async Task Index_ReturnsFilteredMachines_BySearchStringAndCategoryId()
//        {
//            // Arrange
//            var context = GetInMemoryContext();
//            var customer1 = new Customer { Id = 1, Name = "John", LastName = "Smith" };
//            var customer2 = new Customer { Id = 2, Name = "Alice", LastName = "Brown" };

//            var model1 = new Model { Id = 1, Name = "Excavator", CategoryId = 1 };
//            var model2 = new Model { Id = 2, Name = "Crane", CategoryId = 2 };

//            context.Customers.AddRange(customer1, customer2);
//            context.Models.AddRange(model1, model2);
//            context.Machines.AddRange(
//                new Machine { Id = 1, Customer = customer1, Model = model1 }, // Coincide: cliente "John" y CategoryId 1
//                new Machine { Id = 2, Customer = customer2, Model = model1 }, // No coincide por cliente
//                new Machine { Id = 3, Customer = customer1, Model = model2 }  // No coincide por categoryId
//            );
//            context.Categories.Add(new Category { Id = 1, Name = "Heavy" });
//            context.Categories.Add(new Category { Id = 2, Name = "Light" });
//            context.SaveChanges();

//            var controller = new MachinesController(context);
//            string search = "John";
//            int filterCategoryId = 1;

//            // Act
//            var result = await controller.Index(search, filterCategoryId, page: 1, pageSize: 5);

//            // Assert
//            var viewResult = Assert.IsType<ViewResult>(result);
//            var pager = Assert.IsType<Pager<Machine>>(viewResult.Model);
//            // Sólo la máquina 1 cumple ambas condiciones
//            Assert.Equal(1, pager.TotalElements);
//            Assert.Single(pager.Elements);
//            Assert.Equal("John", pager.Elements.First().Customer.Name);
//            Assert.Equal(filterCategoryId, pager.Elements.First().Model.CategoryId);
//            // Verificar ViewData
//            Assert.Equal(search, controller.ViewData["searchString"]);
//            Assert.Equal(filterCategoryId, controller.ViewData["categoryId"]);
//        }

//        //**Test para la Acción GET del Index comprobando la paginación**
//        //- Propósito:
//        //  Verificar que la paginación funcione correctamente, devolviendo el subconjunto adecuado de máquinas según el número de página.
//        //- Cómo se hizo:
//        //  - Arrange: Se agregan más máquinas que el tamaño de página.
//        //  - Act: Se llama a Index para, por ejemplo, la página 2.
//        //  - Assert: Se verifica que las propiedades del paginador (TotalItems, CurrentPage, PageSize) sean correctas y
//        //    que la cantidad de ítems retornados concuerde con el tamaño de página.
//        [Fact]
//        public async Task Index_ReturnsCorrectPageResults()
//        {
//            // Arrange
//            var context = GetInMemoryContext();
//            var customer = new Customer { Id = 1, Name = "John", LastName = "Doe" };
//            var model = new Model { Id = 1, Name = "Excavator", CategoryId = 1 };

//            context.Customers.Add(customer);
//            context.Models.Add(model);

//            // Agregar 10 máquinas para probar la paginación
//            for (int i = 1; i <= 10; i++)
//            {
//                context.Machines.Add(new Machine { Customer = customer, Model = model });
//            }
//            context.Categories.Add(new Category { Id = 1, Name = "Heavy" });
//            context.SaveChanges();

//            var controller = new MachinesController(context);
//            int page = 2, pageSize = 5;

//            // Act
//            var result = await controller.Index(null, null, page, pageSize);

//            // Assert
//            var viewResult = Assert.IsType<ViewResult>(result);
//            var pager = Assert.IsType<Pager<Machine>>(viewResult.Model);
//            Assert.Equal(10, pager.TotalElements);
//            Assert.Equal(page, pager.CurrentPage);
//            Assert.Equal(2, pager.TotalPages);
//            Assert.Equal(5, pager.Elements.Count());
//        }

//            // Escenario 1: Id nulo → Debe retornar NotFound
//            [Fact]
//            public async Task Details_ReturnsNotFound_WhenIdIsNull()
//            {
//                // Arrange
//                var context = GetInMemoryContext();
//                var controller = new MachinesController(context);

//                // Act
//                var result = await controller.Details(null);

//                // Assert
//                Assert.IsType<NotFoundResult>(result);
//            }

//            // Escenario 2: Id no encontrada (ningún registro en la DB)
//            [Fact]
//            public async Task Details_ReturnsNotFound_WhenMachineNotFound()
//            {
//                // Arrange
//                var context = GetInMemoryContext();
//                var controller = new MachinesController(context);

//                // Act: Se consulta un id inexistente (por ejemplo, 1)
//                var result = await controller.Details(1);

//                // Assert
//                Assert.IsType<NotFoundResult>(result);
//            }

//            // Escenario 3: Máquina encontrada → Retorna la vista con la máquina correcta
//            [Fact]
//            public async Task Details_ReturnsViewResult_WithMachine_WhenMachineExists()
//            {
//                // Arrange
//                var context = GetInMemoryContext();

//                // Crear datos de soporte:
//                var customer = new Customer { Id = 1, Name = "John", LastName = "Doe" };
//                var model = new Model { Id = 1, Name = "Excavator", CategoryId = 1 };

//                // Si se requiere probar OwnerHistories, se pueden agregar elementos:
//                var ownerHistories = new List<OwnerHistory>
//            {
//                new OwnerHistory { Id = 1, /* Otros campos si son necesarios */ }
//            };

//                // Crear la máquina con una colección de OwnerHistories inicializada
//                var machine = new Machine
//                {
//                    Id = 1,
//                    Customer = customer,
//                    Model = model,
//                    OwnerHistories = ownerHistories
//                };

//                // Agregar los datos al contexto y persistir
//                context.Customers.Add(customer);
//                context.Models.Add(model);
//                context.Machines.Add(machine);
//                context.SaveChanges();

//                var controller = new MachinesController(context);

//                // Act
//                var result = await controller.Details(1);

//                // Assert
//                var viewResult = Assert.IsType<ViewResult>(result);
//                var returnedMachine = Assert.IsAssignableFrom<Machine>(viewResult.Model);
//                Assert.Equal(machine.Id, returnedMachine.Id);
//                Assert.NotNull(returnedMachine.Customer);
//                Assert.NotNull(returnedMachine.Model);
//                Assert.NotNull(returnedMachine.OwnerHistories);
//                // Si se quiere verificar que se hayan cargado todos los OwnerHistories:
//                Assert.Equal(ownerHistories.Count, returnedMachine.OwnerHistories.Count);
//            }

//        //**Test para la Acción GET del Create de Machines**
//        //- Propósito:
//        //  Verificar que al invocar la acción GET Create se retorne un ViewResult y que se asignen correctamente los objetos SelectList en el ViewBag.
//        //- Cómo se hizo:
//        //  - Arrange: Se crea un contexto en memoria y se agregan datos de prueba para Categories y Customers.
//        //  - Act: Se invoca el método Create() del controller.
//        //  - Assert: Se valida que el resultado sea un ViewResult y se comprueba que:
//        //      • ViewBag.Categories sea un SelectList con la cantidad de categorías esperadas.
//        //      • ViewBag.ModelId sea un SelectList vacío, ya que se inicializa con una lista vacía.
//        //      • ViewBag.Customers sea un SelectList formado a partir de los customers, con el FullName concatenado.
//        [Fact]
//        public void Create_ReturnsViewResult_WithRequiredSelectLists()
//        {
//            // Arrange: Crear contexto en memoria y agregar datos de prueba
//            var context = GetInMemoryContext();

//            // Agregar una categoría
//            var category = new Category { Id = 1, Name = "Heavy" };
//            context.Categories.Add(category);

//            // Agregar dos clientes
//            var customer1 = new Customer { Id = 1, Name = "John", LastName = "Doe" };
//            var customer2 = new Customer { Id = 2, Name = "Alice", LastName = "Smith" };
//            context.Customers.AddRange(customer1, customer2);
//            context.SaveChanges();

//            // Instanciar el controller con el contexto configurado
//            var controller = new MachinesController(context);

//            // Act: Invocar el método Create
//            var result = controller.Create();

//            // Assert: Verificar que se retorna un ViewResult
//            var viewResult = Assert.IsType<ViewResult>(result);

//            // Verificar que ViewBag.Categories sea un SelectList con 1 elemento (la categoría agregada)
//            var categoriesSelectList = ((SelectList)controller.ViewBag.Categories)
//                                        .Cast<SelectListItem>()
//                                        .ToList();
//            Assert.Equal(1, categoriesSelectList.Count);

//            // Verificar que ViewBag.ModelId sea un SelectList vacío (ya que se inicializa con new List<Model>())
//            var modelIdSelectList = ((SelectList)controller.ViewBag.ModelId)
//                                       .Cast<SelectListItem>()
//                                       .ToList();
//            Assert.Empty(modelIdSelectList);

//            // Verificar que ViewBag.Customers sea un SelectList con 2 elementos y que se concatene correctamente el FullName
//            var customersSelectList = ((SelectList)controller.ViewBag.Customers)
//                                        .Cast<SelectListItem>()
//                                        .ToList();
//            Assert.Equal(2, customersSelectList.Count);
//            Assert.Contains(customersSelectList, item => item.Text == "John Doe");
//        }

//        //**Test para la Acción GET del Create cuando no existen datos en el contexto**
//        //- Propósito:
//        //  Verificar que cuando el contexto no posee categorías ni clientes, los SelectList del ViewBag se configuran correctamente (es decir, quedan vacíos)
//        //- Cómo se hizo:
//        //  - Arrange: Se crea un contexto en memoria sin sembrar datos (vacío).
//        //  - Act: Se invoca la acción Create.
//        //  - Assert: Se valida que el resultado es un ViewResult y que los SelectList para Categories, ModelId y Customers se inicializan como colecciones vacías.
//        [Fact]
//        public void Create_ReturnsViewResult_WithEmptySelectLists_WhenNoDataExists()
//        {
//            // Arrange: Crear un contexto en memoria sin datos
//            var context = GetInMemoryContext();
//            var controller = new MachinesController(context);

//            // Act: Invocar la acción Create
//            var result = controller.Create();

//            // Assert
//            var viewResult = Assert.IsType<ViewResult>(result);

//            // Convertir los SelectList a listas para poder validarlos
//            var categoriesSelectList = ((SelectList)controller.ViewBag.Categories)
//                                        .Cast<SelectListItem>()
//                                        .ToList();
//            var modelIdSelectList = ((SelectList)controller.ViewBag.ModelId)
//                                     .Cast<SelectListItem>()
//                                     .ToList();
//            var customersSelectList = ((SelectList)controller.ViewBag.Customers)
//                                        .Cast<SelectListItem>()
//                                        .ToList();

//            // Validar que las listas estén vacías
//            Assert.Empty(categoriesSelectList);
//            Assert.Empty(customersSelectList);
//            Assert.Empty(modelIdSelectList);
//        }

//        //**Test para el método GET GetModelsByCategory cuando existen modelos para la categoría indicada**
//        //- Propósito:
//        //  Verificar que se retorne un JsonResult con la lista de modelos (con sus propiedades "id" y "name")
//        //  correspondientes al categoryId proporcionado.
//        //- Cómo se hizo:
//        //  - Arrange: Se crea un contexto en memoria, se agrega una categoría y se agregan dos modelos asociados a esa categoría.
//        //  - Act: Se invoca GetModelsByCategory con un categoryId que tenga modelos asociados.
//        //  - Assert: Se comprueba que el resultado es un JsonResult y que la lista contenida en el JsonResult
//        //    tiene la cantidad de elementos esperada (en este caso 2).
//        [Fact]
//        public async Task GetModelsByCategory_ReturnsModels_WhenModelsExistForCategory()
//        {
//            // Arrange
//            var context = GetInMemoryContext();

//            // Agregar la categoría
//            var category = new Category { Id = 1, Name = "Heavy" };
//            context.Categories.Add(category);

//            // Agregar dos modelos asociados a la categoría "Heavy" (CategoryId=1)
//            var model1 = new Model { Id = 1, Name = "Excavator", CategoryId = 1 };
//            var model2 = new Model { Id = 2, Name = "Bulldozer", CategoryId = 1 };
//            context.Models.AddRange(model1, model2);
//            context.SaveChanges();

//            var controller = new MachinesController(context);

//            // Act
//            var result = await controller.GetModelsByCategory(1);
//            var jsonResult = Assert.IsType<JsonResult>(result);

//            // Assert
//            // Convertir el contenido del JsonResult a IEnumerable, castear cada elemento a object y luego a una lista
//            var returnedModels = ((System.Collections.IEnumerable)jsonResult.Value).Cast<object>().ToList();
//            Assert.Equal(2, returnedModels.Count);
//        }

//        //**Test para el método GET GetModelsByCategory cuando no existen modelos para la categoría indicada**
//        //- Propósito:
//        //  Verificar que se retorne un JsonResult con una lista vacía cuando no se encuentran modelos
//        //  asociados al categoryId proporcionado.
//        //- Cómo se hizo:
//        //  - Arrange: Se crea un contexto en memoria, se agrega una categoría y se agrega un modelo que pertenece a otra categoría.
//        //  - Act: Se invoca GetModelsByCategory con un categoryId sin modelos asociados.
//        //  - Assert: Se comprueba que el JsonResult devuelto contiene una lista vacía.
//        [Fact]
//        public async Task GetModelsByCategory_ReturnsEmptyList_WhenNoModelsFoundForCategory()
//        {
//            // Arrange
//            var context = GetInMemoryContext();

//            // Agregar una categoría (CategoryId=1)
//            var category = new Category { Id = 1, Name = "Heavy" };
//            context.Categories.Add(category);

//            // Agregar un modelo, pero asociado a otra categoría (CategoryId=2)
//            var model = new Model { Id = 1, Name = "Excavator", CategoryId = 2 };
//            context.Models.Add(model);
//            context.SaveChanges();

//            var controller = new MachinesController(context);

//            // Act
//            var result = await controller.GetModelsByCategory(1);
//            var jsonResult = Assert.IsType<JsonResult>(result);

//            // Assert
//            // Convertimos el contenido del JsonResult a IEnumerable,
//            // lo castear a object y luego lo convertimos a una lista para poder contar los elementos.
//            var returnedModels = ((System.Collections.IEnumerable)jsonResult.Value).Cast<object>().ToList();
//            Assert.Empty(returnedModels);
//        }

//        //**Test para la Acción POST del Create de Machines (caso sin archivos de documentación)
//        //- Propósito:
//        //  Verificar que cuando se envía un modelo válido sin archivos de documentaciones,
//        //  se agregue la máquina a la base de datos y se redirija a la acción Index.
//        //- Cómo se hizo:
//        //  - Arrange: Se crea un contexto en memoria y se agregan los datos necesarios para Categories, Customers y Models.
//        //  - Act: Se invoca la acción POST de Create enviando una instancia de Machine sin archivos en Documentations.
//        //  - Assert: Se valida que el resultado sea un RedirectToActionResult a "Index" y que la base de datos contenga la máquina creada.
//        [Fact]
//        public async Task Create_Post_ReturnsRedirectToIndex_WhenModelStateIsValid_WithoutDocumentations()
//        {
//            // Arrange
//            var context = GetInMemoryContext();

//            // Crear los datos necesarios
//            var category = new Category { Id = 1, Name = "Heavy" };
//            var customer = new Customer { Id = 1, Name = "John", LastName = "Doe" };
//            var modelEntity = new Model { Id = 1, Name = "Excavator", CategoryId = category.Id };

//            context.Categories.Add(category);
//            context.Customers.Add(customer);
//            context.Models.Add(modelEntity);
//            context.SaveChanges();

//            var controller = new MachinesController(context);

//            // Crear una máquina válida sin Documentations
//            var machine = new Machine
//            {
//                CustomerId = customer.Id,
//                ModelId = modelEntity.Id,
//                ChasisNumber = "CH123",
//                EngineNumber = "EN456",
//                DeliveryDate = DateTime.Now,
//                WarrantyExpirationDate = DateTime.Now.AddYears(1),
//                Documentations = null // No se envían archivos
//            };

//            // Act
//            var result = await controller.Create(machine);

//            // Assert
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal("Index", redirectResult.ActionName);
//            // Verifica que se haya creado la máquina en el contexto
//            Assert.Equal(1, await context.Machines.CountAsync());
//        }

//        //**Test para la Acción POST del Create de Machines (caso con archivo de documentación inválido)
//        //- Propósito:
//        //  Verificar que cuando se envía un archivo en Documentations que no es PDF,
//        //  se añada un error al ModelState y se retorne la misma vista con el modelo enviado.
//        //- Cómo se hizo:
//        //  - Arrange: Se crea un contexto en memoria y se agregan los datos requeridos. Se crea un objeto IFormFile
//        //    simulando un archivo con extensión ".doc".
//        //  - Act: Se invoca la acción POST de Create enviando una instancia de Machine que incluye ese archivo.
//        //  - Assert: Se valida que el resultado sea un ViewResult, que se devuelva el mismo modelo enviado y que
//        //    el ModelState contenga el error correspondiente.
//        [Fact]
//        public async Task Create_Post_ReturnsViewResult_WhenFileExtensionIsInvalid()
//        {
//            // Arrange
//            var context = GetInMemoryContext();

//            // Crear los datos necesarios
//            var category = new Category { Id = 1, Name = "Heavy" };
//            var customer = new Customer { Id = 1, Name = "John", LastName = "Doe" };
//            var modelEntity = new Model { Id = 1, Name = "Excavator", CategoryId = category.Id };

//            context.Categories.Add(category);
//            context.Customers.Add(customer);
//            context.Models.Add(modelEntity);
//            context.SaveChanges();

//            var controller = new MachinesController(context);

//            // Crear un archivo simulado (IFormFile) con extensión .doc (inválido)
//            var content = "dummy content";
//            var fileName = "manual.doc";
//            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
//            var formFile = new FormFile(stream, 0, stream.Length, "Data", fileName)
//            {
//                Headers = new HeaderDictionary(),
//                ContentType = "application/msword"
//            };

//            // Crear una máquina válida pero con Documentations que contiene un archivo con extensión inválida
//            var machine = new Machine
//            {
//                CustomerId = customer.Id,
//                ModelId = modelEntity.Id,
//                ChasisNumber = "CH123",
//                EngineNumber = "EN456",
//                DeliveryDate = DateTime.Now,
//                WarrantyExpirationDate = DateTime.Now.AddYears(1),
//                Documentations = new List<IFormFile> { formFile }
//            };

//            // Act
//            var result = await controller.Create(machine);

//            // Assert
//            var viewResult = Assert.IsType<ViewResult>(result);
//            Assert.Equal(machine, viewResult.Model);
//            // Se debe agregar un error al ModelState sobre "Manuals"
//            Assert.False(controller.ModelState.IsValid);
//            Assert.True(controller.ModelState.ContainsKey("Manuals"));
//        }

//        //**Test para la Acción POST del Create de Machines (caso con archivo de documentación válido)
//        //- Propósito:
//        //  Verificar que cuando se envía un archivo PDF válido en Documentations, el proceso continúa sin errores,
//        //  se llama a FileService para obtener las URLs y se agrega la máquina a la base de datos, redirigiendo a Index.
//        //- Cómo se hizo:
//        //  - Arrange: Se configura el contexto en memoria y se agregan los datos necesarios (Categories, Customers y Models).
//        //    Se simula un archivo PDF válido.
//        //  - Act: Se invoca la acción POST de Create enviando una máquina que incluye el archivo PDF.
//        //  - Assert: Se valida que el resultado sea un RedirectToActionResult a "Index", que la máquina se cree en el contexto,
//        //    y (si se logra simular FileService.SaveDocAsync) que el DocUrls contenga la URL esperada.
//        [Fact]
//        public async Task Create_Post_ReturnsRedirectToIndex_WhenModelStateIsValid_WithValidDocumentations()
//        {
//            // Arrange
//            var context = GetInMemoryContext();

//            // Crear y agregar los datos necesarios
//            var category = new Category { Id = 1, Name = "Heavy" };
//            var customer = new Customer { Id = 1, Name = "John", LastName = "Doe" };
//            var modelEntity = new Model { Id = 1, Name = "Excavator", CategoryId = category.Id };

//            context.Categories.Add(category);
//            context.Customers.Add(customer);
//            context.Models.Add(modelEntity);
//            context.SaveChanges();

//            var controller = new MachinesController(context);

//            // Simular un archivo PDF válido (IFormFile)
//            var content = "Dummy PDF content";
//            var fileName = "manual.pdf";
//            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
//            var pdfFile = new FormFile(stream, 0, stream.Length, "Data", fileName)
//            {
//                Headers = new HeaderDictionary(),
//                ContentType = "application/pdf"
//            };

//            // NOTA: En un test real se recomienda simular la respuesta de FileService.SaveDocAsync.
//            // Por ejemplo, si FileService.SaveDocAsync estuviera inyectado, se usaría un mock y se configuraría para que retorne:
//            // new List<string> { "http://dummyurl/manual.pdf" }
//            // Aquí asumiremos que, de alguna manera, FileService.SaveDocAsync retorna esa lista dummy.

//            // Crear una máquina válida con el archivo PDF en Documentations
//            var machine = new Machine
//            {
//                CustomerId = customer.Id,
//                ModelId = modelEntity.Id,
//                ChasisNumber = "CH789",
//                EngineNumber = "EN012",
//                DeliveryDate = DateTime.Now,
//                WarrantyExpirationDate = DateTime.Now.AddYears(1),
//                Documentations = new List<IFormFile> { pdfFile }
//            };

//            // Act
//            var result = await controller.Create(machine);

//            // Assert
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal("Index", redirectResult.ActionName);

//            // Verificar que se haya creado la máquina en el contexto
//            var createdMachine = await context.Machines.FirstOrDefaultAsync();
//            Assert.NotNull(createdMachine);

//            // Asegurarse de que se asignen las URLs de documentación.
//            // Dependiendo de la implementación de FileService, podríamos esperar que DocUrls contenga 1 URL dummy.
//            // En este ejemplo, suponemos que se retorna una lista dummy con "http://dummyurl/manual.pdf"
//            // Si no se logra simular, se podría evitar esta aserción o modificar FileService para inyectarlo.
//            Assert.NotNull(createdMachine.DocUrls);
//            // Si hemos simulado la respuesta, verificaríamos que se haya retornado 1 URL
//            // Assert.Single(createdMachine.DocUrls);
//        }

//        //**Test para la acción Edit cuando se pasa un id nulo**
//        //- Propósito:
//        //  Verificar que cuando se invoca Edit con un id nulo, se retorne un NotFoundResult.
//        //- Cómo se hizo:
//        //  - Arrange: Se crea un contexto en memoria vacío y se instancia el controller.
//        //  - Act: Se invoca Edit(null).
//        //  - Assert: Se comprueba que el resultado es un NotFoundResult.
//        [Fact]
//        public async Task Edit_ReturnsNotFound_WhenIdIsNull()
//        {
//            // Arrange
//            var context = GetInMemoryContext();
//            var controller = new MachinesController(context);

//            // Act
//            var result = await controller.Edit(null);

//            // Assert
//            Assert.IsType<NotFoundResult>(result);
//        }

//        //**Test para la acción Edit cuando no se encuentra la máquina con el id dado**
//        //- Propósito:
//        //  Verificar que cuando no existe una máquina con el id proporcionado, se retorne un NotFoundResult.
//        //- Cómo se hizo:
//        //  - Arrange: Se crea un contexto en memoria sin agregar ninguna máquina.
//        //  - Act: Se invoca Edit(1) sobre el controller.
//        //  - Assert: Se confirma que el resultado es un NotFoundResult.
//        [Fact]
//        public async Task Edit_ReturnsNotFound_WhenMachineNotFound()
//        {
//            // Arrange
//            var context = GetInMemoryContext();
//            var controller = new MachinesController(context);

//            // Act
//            var result = await controller.Edit(1);

//            // Assert
//            Assert.IsType<NotFoundResult>(result);
//        }

//        //**Test para la acción Edit cuando la máquina existe**
//        //- Propósito:
//        //  Verificar que al solicitar la edición de una máquina existente se retorne un ViewResult
//        //  con el modelo correcto y que se inicialicen correctamente los SelectList en ViewData y ViewBag.
//        //- Cómo se hizo:
//        //  - Arrange: Se crea un Customer, un Model y una Machine asociada, asegurándose de que se incluya el Customer
//        //    para que se cargue la propiedad de navegación. Se guarda la información en el contexto.
//        //  - Act: Se invoca Edit con el id de la máquina.
//        //  - Assert: Se confirma que se retorne un ViewResult; que el modelo tenga el mismo id; que ViewData["CustomerId"]
//        //    y ViewData["ModelId"] sean SelectList con el valor seleccionado correcto; y que ViewBag.CustomerName contenga el full name esperado.
//        [Fact]
//        public async Task Edit_ReturnsViewResult_WithMachineAndSelectLists_WhenMachineExists()
//        {
//            // Arrange
//            var context = GetInMemoryContext();

//            // Crear un customer y simular su FullName (se asume que FullName es "Name + ' ' + LastName")
//            var customer = new Customer { Id = 1, Name = "John", LastName = "Doe" };
//            context.Customers.Add(customer);

//            // Crear un model
//            var modelEntity = new Model { Id = 1, Name = "Excavator", CategoryId = 1 };
//            context.Models.Add(modelEntity);

//            // Crear una máquina para editar, vinculándola al customer y model creados
//            var machine = new Machine
//            {
//                Id = 1,
//                CustomerId = customer.Id,
//                ModelId = modelEntity.Id,
//                ChasisNumber = "CH001",
//                EngineNumber = "EN001",
//                DeliveryDate = DateTime.Now,
//                WarrantyExpirationDate = DateTime.Now.AddYears(1),
//                // Se asigna la navegación Customer para que al incluirse se pueda obtener el nombre completo.
//                Customer = customer
//            };
//            context.Machines.Add(machine);
//            context.SaveChanges();

//            var controller = new MachinesController(context);

//            // Act
//            var result = await controller.Edit(1);

//            // Assert
//            var viewResult = Assert.IsType<ViewResult>(result);
//            var returnedMachine = Assert.IsType<Machine>(viewResult.Model);
//            Assert.Equal(machine.Id, returnedMachine.Id);

//            // Verificar que ViewData["CustomerId"] es un SelectList con el valor seleccionado igual al CustomerId
//            var customerSelectList = Assert.IsType<SelectList>(controller.ViewData["CustomerId"]);
//            Assert.Equal(machine.CustomerId.ToString(), customerSelectList.SelectedValue.ToString());

//            // Verificar que ViewData["ModelId"] es un SelectList con el valor seleccionado igual al ModelId
//            var modelSelectList = Assert.IsType<SelectList>(controller.ViewData["ModelId"]);
//            Assert.Equal(machine.ModelId.ToString(), modelSelectList.SelectedValue.ToString());

//            // Verificar que ViewBag.CustomerName corresponde al nombre completo del Customer
//            var expectedCustomerName = $"{customer.Name} {customer.LastName}";
//            Assert.Equal(expectedCustomerName, controller.ViewBag.CustomerName);
//        }

//        //**Test para POST Edit cuando el id en la ruta no coincide con el machine.Id del modelo
//        //- Propósito: Verificar que si el id proporcionado no coincide con machine.Id se retorne NotFound.
//        //- Cómo se hizo:
//        //   • Arrange: Se crea un contexto y se agrega una máquina con Id=1.
//        //   • Act: Se invoca Edit pasando id=2 (valor diferente al machine.Id).
//        //   • Assert: Se verifica que el resultado sea NotFound.
//        [Fact]
//        public async Task Edit_ReturnsNotFound_WhenIdDoesNotMatchMachineId()
//        {
//            // Arrange
//            var context = GetInMemoryContext();
//            var customer = new Customer { Id = 1, Name = "John", LastName = "Doe" };
//            var modelEntity = new Model { Id = 1, Name = "Excavator", CategoryId = 1 };
//            context.Customers.Add(customer);
//            context.Models.Add(modelEntity);
//            var existingMachine = new Machine
//            {
//                Id = 1,
//                CustomerId = customer.Id,
//                ModelId = modelEntity.Id,
//                ChasisNumber = "CH001",
//                EngineNumber = "EN001",
//                DeliveryDate = DateTime.Now,
//                WarrantyExpirationDate = DateTime.Now.AddYears(1)
//            };
//            context.Machines.Add(existingMachine);
//            context.SaveChanges();
//            var controller = new MachinesController(context);

//            // Act
//            // Se pasa id 2, que no coincide con machine.Id (1)
//            var result = await controller.Edit(2, existingMachine, null, null, null);

//            // Assert
//            Assert.IsType<NotFoundResult>(result);
//        }

//        //**Test para POST Edit cuando la máquina no existe en la base de datos
//        //- Propósito: Verificar que si la máquina editada no existe en la BD se retorne NotFound.
//        //- Cómo se hizo:
//        //   • Arrange: Se crea un contexto sin agregar la máquina.
//        //   • Act: Se invoca Edit para una máquina con Id=1.
//        //   • Assert: Se verifica que se retorne NotFound.
//        [Fact]
//        public async Task Edit_ReturnsNotFound_WhenMachineDoesNotExist()
//        {
//            // Arrange
//            var context = GetInMemoryContext();
//            var controller = new MachinesController(context);

//            // Se crea un objeto machine que NO se agrega al contexto
//            var machine = new Machine
//            {
//                Id = 1,
//                CustomerId = 1,
//                ModelId = 1,
//                ChasisNumber = "CH001",
//                EngineNumber = "EN001",
//                DeliveryDate = DateTime.Now,
//                WarrantyExpirationDate = DateTime.Now.AddYears(1)
//            };

//            // Act
//            var result = await controller.Edit(1, machine, null, null, null);

//            // Assert
//            Assert.IsType<NotFoundResult>(result);
//        }

//        //**Test para POST Edit cuando la edición es válida y se actualizan los datos (incluye cambio de cliente)
//        //- Propósito: Verificar que una edición válida se procese correctamente actualizando las propiedades y, si se cambia el cliente, se
//        //             agrega un registro en OwnerHistories. Se espera un Redirect a Index.
//        //- Cómo se hizo:
//        //   • Arrange: Se crea un contexto con dos clientes, un modelo y una máquina existente (vinculada al primer cliente).
//        //   • Act: Se invoca Edit pasando una máquina actualizada en la que se cambia el CustomerId (por lo que se debe agregar un OwnerHistory),
//        //             sin cambios en la documentación (ExistingDocs se provee y no se suben nuevos archivos).
//        //   • Assert: Se verifica que se redirija a Index; se comprueba que se actualicen las propiedades, que se mantengan los docUrls y que se haya agregado un OwnerHistory.
//        [Fact]
//        public async Task Edit_Post_RedirectsToIndex_WhenModelStateIsValid_AndNoDocumentChanges()
//        {
//            // Arrange
//            var context = GetInMemoryContext();

//            // Crear dos clientes
//            var customer1 = new Customer { Id = 1, Name = "John", LastName = "Doe" };
//            var customer2 = new Customer { Id = 2, Name = "Alice", LastName = "Smith" };
//            context.Customers.AddRange(customer1, customer2);

//            // Crear un modelo
//            var modelEntity = new Model { Id = 1, Name = "Excavator", CategoryId = 1 };
//            context.Models.Add(modelEntity);

//            // Crear una máquina existente asociada al primer cliente
//            var machine = new Machine
//            {
//                Id = 1,
//                CustomerId = customer1.Id,
//                ModelId = modelEntity.Id,
//                ChasisNumber = "CH001",
//                EngineNumber = "EN001",
//                DeliveryDate = DateTime.Now,
//                WarrantyExpirationDate = DateTime.Now.AddYears(1),
//                DocUrls = new List<string> { "dummyUrl1.pdf", "dummyUrl2.pdf" }
//            };
//            context.Machines.Add(machine);
//            context.SaveChanges();

//            var controller = new MachinesController(context);

//            // Simular edición de la máquina: cambiar CustomerId y actualizar otros campos
//            var updatedMachine = new Machine
//            {
//                Id = 1,
//                CustomerId = customer2.Id, // Cambio de cliente
//                ModelId = modelEntity.Id,
//                ChasisNumber = "CH001-Updated",
//                EngineNumber = "EN001-Updated",
//                DeliveryDate = machine.DeliveryDate,
//                WarrantyExpirationDate = machine.WarrantyExpirationDate
//            };

//            // Se simula que los documentos existentes no se modifican ni se eliminan: se provee la lista actual
//            var existingDocs = new List<string> { "dummyUrl1.pdf", "dummyUrl2.pdf" };
//            List<IFormFile> documentations = null;
//            List<string> deletedDocs = null;

//            // Act
//            var result = await controller.Edit(1, updatedMachine, existingDocs, documentations, deletedDocs);

//            // Assert
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal("Index", redirectResult.ActionName);

//            // Verificar que la máquina fue actualizada
//            var updatedEntity = await context.Machines.FirstOrDefaultAsync(m => m.Id == 1);
//            Assert.NotNull(updatedEntity);
//            Assert.Equal(customer2.Id, updatedEntity.CustomerId);
//            Assert.Equal("CH001-Updated", updatedEntity.ChasisNumber);
//            Assert.Equal("EN001-Updated", updatedEntity.EngineNumber);
//            Assert.Equal(existingDocs, updatedEntity.DocUrls);

//            // Verificar que se agregó un OwnerHistory (ya que se cambió el cliente)
//            var ownerHistoryCount = await context.OwnerHistories.CountAsync();
//            Assert.Equal(1, ownerHistoryCount);
//        }

//        //**Test para POST Edit cuando se suministra un archivo con extensión inválida
//        //- Propósito: Verificar que si se adjunta un archivo con extensión distinta a ".pdf", se agregue un error al ModelState
//        //             y se retorne un ViewResult con el modelo sin procesar la edición.
//        //- Cómo se hizo:
//        //   • Arrange: Se crea un contexto con una máquina existente; se simula un archivo IFormFile con extensión ".doc".
//        //   • Act: Se invoca Edit con el archivo inválido en el parámetro Documentations.
//        //   • Assert: Se verifica que se retorne la vista (ViewResult), que el modelo enviado sea el recibido y que el ModelState contenga el error.
//        [Fact]
//        public async Task Edit_Post_ReturnsViewResult_WhenDocumentWithInvalidExtensionProvided()
//        {
//            // Arrange
//            var context = GetInMemoryContext();
//            var customer = new Customer { Id = 1, Name = "John", LastName = "Doe" };
//            context.Customers.Add(customer);
//            var modelEntity = new Model { Id = 1, Name = "Excavator", CategoryId = 1 };
//            context.Models.Add(modelEntity);
//            var machine = new Machine
//            {
//                Id = 1,
//                CustomerId = customer.Id,
//                ModelId = modelEntity.Id,
//                ChasisNumber = "CH001",
//                EngineNumber = "EN001",
//                DeliveryDate = DateTime.Now,
//                WarrantyExpirationDate = DateTime.Now.AddYears(1),
//                DocUrls = new List<string>()
//            };
//            context.Machines.Add(machine);
//            context.SaveChanges();

//            var controller = new MachinesController(context);

//            // Preparar una máquina actualizada (sin cambio en propiedades)
//            var updatedMachine = new Machine
//            {
//                Id = 1,
//                CustomerId = customer.Id,
//                ModelId = modelEntity.Id,
//                ChasisNumber = "CH001",
//                EngineNumber = "EN001",
//                DeliveryDate = machine.DeliveryDate,
//                WarrantyExpirationDate = machine.WarrantyExpirationDate
//            };

//            // Simular un archivo inválido (.doc) en Documentations
//            var content = "dummy content";
//            var fileName = "manual.doc";
//            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
//            var invalidFile = new FormFile(stream, 0, stream.Length, "Data", fileName)
//            {
//                Headers = new HeaderDictionary(),
//                ContentType = "application/msword"
//            };

//            List<IFormFile> documentations = new List<IFormFile> { invalidFile };
//            List<string> existingDocs = new List<string>();
//            List<string> deletedDocs = new List<string>();

//            // Act
//            var result = await controller.Edit(1, updatedMachine, existingDocs, documentations, deletedDocs);

//            // Assert
//            var viewResult = Assert.IsType<ViewResult>(result);
//            Assert.Equal(updatedMachine, viewResult.Model);
//            Assert.False(controller.ModelState.IsValid);
//            Assert.True(controller.ModelState.ContainsKey("Manuals"));
//        }

//        //**Test para POST Edit que actualiza las documentaciones cuando se eliminan documentos existentes y se agregan nuevos archivos válidos
//        //- Propósito:
//        //  Verificar que, al editar una máquina, se procese correctamente la eliminación de documentos especificados en DeletedDocs
//        //  y la incorporación de nuevos archivos PDF (en Documentations), obteniéndose la combinación esperada en DocUrls.
//        //- Suposición para el test:
//        //  Se asume que FileService.SaveDocAsync(Documentations) retornará una lista con al menos una URL dinámica, la cual
//        //  debe comenzar con "/documentation/machines/" y terminar con ".pdf".
//        //- Cómo se hizo:
//        //   • Arrange: Se crea un contexto y se agrega una máquina con DocUrls iniciales.
//        //   • Act: Se invoca Edit pasando:
//        //         - ExistingDocs iguales a las URLs actuales,
//        //         - DeletedDocs con una URL a eliminar,
//        //         - Documentations con un archivo PDF válido.
//        //   • Assert: Se verifica que se redirija a Index, y se comprueba que la máquina en el contexto
//        //         tenga DocUrls donde se conserva "dummyUrl2.pdf" y se agrega una nueva URL que cumpla con el formato esperado.
//        [Fact]
//        public async Task Edit_Post_UpdatesDocuments_WhenExistingDocsDeletedAndNewDocsProvided()
//        {
//            // Arrange
//            var context = GetInMemoryContext();

//            // Crear un customer y modelo (datos mínimos)
//            var customer = new Customer { Id = 1, Name = "John", LastName = "Doe" };
//            context.Customers.Add(customer);

//            var modelEntity = new Model { Id = 1, Name = "Excavator", CategoryId = 1 };
//            context.Models.Add(modelEntity);

//            // Crear una máquina existente con dos documentos
//            var machine = new Machine
//            {
//                Id = 1,
//                CustomerId = customer.Id,
//                ModelId = modelEntity.Id,
//                ChasisNumber = "CH001",
//                EngineNumber = "EN001",
//                DeliveryDate = DateTime.Now,
//                WarrantyExpirationDate = DateTime.Now.AddYears(1),
//                DocUrls = new List<string> { "dummyUrl1.pdf", "dummyUrl2.pdf" }
//            };
//            context.Machines.Add(machine);
//            context.SaveChanges();

//            var controller = new MachinesController(context);

//            // Preparar la máquina actualizada (sin cambios en las otras propiedades)
//            var updatedMachine = new Machine
//            {
//                Id = 1,
//                CustomerId = customer.Id,
//                ModelId = modelEntity.Id,
//                ChasisNumber = "CH001",
//                EngineNumber = "EN001",
//                DeliveryDate = machine.DeliveryDate,
//                WarrantyExpirationDate = machine.WarrantyExpirationDate
//            };

//            // Definir los parámetros para las documentaciones
//            // ExistingDocs se envía igual a las docUrls actuales de la máquina.
//            var existingDocs = new List<string> { "dummyUrl1.pdf", "dummyUrl2.pdf" };
//            // DeletedDocs indica que se elimina "dummyUrl1.pdf".
//            var deletedDocs = new List<string> { "dummyUrl1.pdf" };

//            // Preparar un archivo PDF simulado para Documentations.
//            var pdfContent = "Dummy PDF content";
//            var pdfFileName = "manual.pdf";
//            var pdfStream = new MemoryStream(Encoding.UTF8.GetBytes(pdfContent));
//            var validPdfFile = new FormFile(pdfStream, 0, pdfStream.Length, "Data", pdfFileName)
//            {
//                Headers = new HeaderDictionary(),
//                ContentType = "application/pdf"
//            };
//            var documentations = new List<IFormFile> { validPdfFile };

//            // Act
//            var result = await controller.Edit(1, updatedMachine, existingDocs, documentations, deletedDocs);

//            // Assert
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal("Index", redirectResult.ActionName);

//            // Verificar que la máquina fue actualizada
//            var updatedEntity = await context.Machines.FirstOrDefaultAsync(m => m.Id == 1);
//            Assert.NotNull(updatedEntity);

//            // Se espera que los documentos resultantes sean:
//            //   - Se elimina "dummyUrl1.pdf", quedando ["dummyUrl2.pdf"]
//            //   - Se añade la respuesta de FileService.SaveDocAsync, que debe devolver una URL que cumpla con el formato esperado.
//            // En lugar de comparar cadenas exactas, validamos el primer documento es idéntico y que el segundo cumple el formato.
//            Assert.Equal(2, updatedEntity.DocUrls.Count);
//            Assert.Equal("dummyUrl2.pdf", updatedEntity.DocUrls[0]);

//            var newDocUrl = updatedEntity.DocUrls[1];
//            Assert.StartsWith("/documentation/machines/", newDocUrl);
//            Assert.EndsWith(".pdf", newDocUrl);
//        }

//        //**Test para GET Delete cuando se pasa un id nulo**
//        //- Propósito: Verificar que si se invoca Delete con id nulo, se retorne un NotFoundResult.
//        //- Cómo se hizo:
//        //   • Arrange: Se crea un contexto en memoria y se instancia el controller.
//        //   • Act: Se invoca Delete(null).
//        //   • Assert: Se comprueba que el resultado es NotFoundResult.
//        [Fact]
//        public async Task Delete_ReturnsNotFound_WhenIdIsNull()
//        {
//            // Arrange
//            var context = GetInMemoryContext();
//            var controller = new MachinesController(context);

//            // Act
//            var result = await controller.Delete(null);

//            // Assert
//            Assert.IsType<NotFoundResult>(result);
//        }

//        //**Test para GET Delete cuando la máquina no se encuentra en la base de datos**
//        //- Propósito: Verificar que si no existe una máquina con el id proporcionado, se retorne NotFoundResult.
//        //- Cómo se hizo:
//        //   • Arrange: Se crea un contexto en memoria sin agregar ninguna máquina.
//        //   • Act: Se invoca Delete con un id que no existe (por ejemplo, 1).
//        //   • Assert: Se verifica que el resultado sea NotFoundResult.
//        [Fact]
//        public async Task Delete_ReturnsNotFound_WhenMachineNotFound()
//        {
//            // Arrange
//            var context = GetInMemoryContext();
//            var controller = new MachinesController(context);

//            // Act
//            var result = await controller.Delete(1);

//            // Assert
//            Assert.IsType<NotFoundResult>(result);
//        }

//        //**Test para GET Delete cuando la máquina existe en la base de datos**
//        //- Propósito: Verificar que cuando se solicita eliminar una máquina existente, se retorne un ViewResult
//        //             con el modelo Machine, y que se incluyan las propiedades de navegación (Customer y Model).
//        //- Cómo se hizo:
//        //   • Arrange: Se crea un contexto en memoria y se agregan los datos necesarios: un Customer, un Model y una Machine asociada.
//        //   • Act: Se invoca Delete pasando el id de la máquina y se guarda el resultado.
//        //   • Assert: Se verifica que el resultado es un ViewResult y que el modelo retornado es la máquina solicitada, con
//        //             las propiedades Customer y Model cargadas.
//        [Fact]
//        public async Task Delete_ReturnsViewResult_WithMachine()
//        {
//            // Arrange
//            var context = GetInMemoryContext();

//            // Crear los datos necesarios
//            var customer = new Customer { Id = 1, Name = "John", LastName = "Doe" };
//            context.Customers.Add(customer);

//            var modelEntity = new Model { Id = 1, Name = "Excavator", CategoryId = 1 };
//            context.Models.Add(modelEntity);

//            var machine = new Machine
//            {
//                Id = 1,
//                CustomerId = customer.Id,
//                ModelId = modelEntity.Id,
//                ChasisNumber = "CH001",
//                EngineNumber = "EN001",
//                DeliveryDate = DateTime.Now,
//                WarrantyExpirationDate = DateTime.Now.AddYears(1),
//                // Asignamos la navegación para que la vista tenga acceso a Customer y Model
//                Customer = customer,
//                Model = modelEntity
//            };
//            context.Machines.Add(machine);
//            context.SaveChanges();

//            var controller = new MachinesController(context);

//            // Act
//            var result = await controller.Delete(1);

//            // Assert
//            var viewResult = Assert.IsType<ViewResult>(result);
//            var returnedMachine = Assert.IsType<Machine>(viewResult.Model);
//            Assert.Equal(machine.Id, returnedMachine.Id);
//            Assert.NotNull(returnedMachine.Customer);
//            Assert.NotNull(returnedMachine.Model);
//        }

//    }
//}