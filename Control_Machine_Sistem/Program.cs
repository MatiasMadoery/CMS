using Control_Machine_Sistem.Models;
using Control_Machine_Sistem.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IImageStorageService, AzureImageStorageService>();

// Dependency injection
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("conexionDB"))
);

// Agregar servicios de autenticación y autorización
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})

.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;

    // ⚠ Cambiado: si no estás usando HTTPS, esto debe estar en None
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    // ⚠ Cambiado: SameSite.Strict puede romper sesiones en navegadores modernos, Lax es mejor para login
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;

    options.LoginPath = "/UsersLogin/Login";
    options.AccessDeniedPath = "/UsersLogin/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsViewer", policy => policy.RequireRole("Viewer"));
    options.AddPolicy("IsTec", policy => policy.RequireRole("Tec"));
    options.AddPolicy("IsAdm", policy => policy.RequireRole("Admin"));
});

// Agrega MVC
builder.Services.AddControllersWithViews();


//AZURE CONFIGURATION
// Configurar Data Protection con clave persistente
// 1. Recuperamos la cadena de conexión unificada
var storageConnectionString = builder.Configuration.GetConnectionString("AzureStorage");

// 2. Aseguramos que el contenedor para las llaves exista (Evita errores en Azurite y Azure)
var keysContainerClient = new Azure.Storage.Blobs.BlobContainerClient(storageConnectionString!, "dataprotection-keys");
keysContainerClient.CreateIfNotExists(); // Crea el contenedor de forma segura si no existe

// 3. Apuntamos al archivo específico xml donde se guardarán las llaves dentro del contenedor
var keysBlobClient = keysContainerClient.GetBlobClient("keys.xml");

// 4. Configurar Data Protection para usar ese Blob
builder.Services.AddDataProtection()
    .PersistKeysToAzureBlobStorage(keysBlobClient)
    .SetApplicationName("Terraplane");

builder.Services.AddAzureClients(clientBuilder =>
{
    // Al usar tu extensión AddBlobServiceClient, si le pasas un Connection String, lo resolverá bien automáticamente
    clientBuilder.AddBlobServiceClient(storageConnectionString!);
    clientBuilder.AddQueueServiceClient(storageConnectionString!);
    clientBuilder.AddTableServiceClient(storageConnectionString!);
});


//LOCAL CON AZURITE
//builder.Services.AddDataProtection()
//    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\DataProtectionKeys\Terraplane"))
//    .SetApplicationName("Terraplane");
//builder.Services.AddAzureClients(clientBuilder =>
//{
//    clientBuilder.AddBlobServiceClient(builder.Configuration["StorageConnection:blobServiceUri"]!).WithName("StorageConnection");
//    clientBuilder.AddQueueServiceClient(builder.Configuration["StorageConnection:queueServiceUri"]!).WithName("StorageConnection");
//    clientBuilder.AddTableServiceClient(builder.Configuration["StorageConnection:tableServiceUri"]!).WithName("StorageConnection");
//});

var app = builder.Build();

// Manejo de errores en producción
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Descomentá solo cuando tengas HTTPS
}

// ⚠ HTTPS redirection desactivado porque no está configurado el certificado
app.UseHttpsRedirection();

app.UseStaticFiles();
// Exponer App_Data/documentation como /documentation
var documentationPath = Path.Combine(app.Environment.ContentRootPath, "App_Data", "documentation");
var documentationProvider = new PhysicalFileProvider(documentationPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = documentationProvider,
    RequestPath = "/documentation",
    ContentTypeProvider = new FileExtensionContentTypeProvider
    {
        Mappings = { [".pdf"] = "application/pdf" }
    }
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Redirección si está logueado y accede al login
app.Use(async (context, next) =>
{
    if (context.User.Identity!.IsAuthenticated && context.Request.Path == "/UsersLogin/Login")
    {
        context.Response.Redirect("/Home/Index");
    }
    else
    {
        await next();
    }
});

// Rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=UsersLogin}/{action=Login}/{id?}");

app.Run();