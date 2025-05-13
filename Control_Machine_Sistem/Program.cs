using Control_Machine_Sistem.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//Dependency injection
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("conexionDb"))
    );

// Agregar servicios de autenticación y autorización
builder.Services.AddAuthentication(options =>
{
options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie(options =>
    {
        // To mitigate the risk of session hijacking and XSS (Cross-Site Scripting) attacks.
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;

        options.LoginPath = "/UsersLogin/Login"; // Login path
        options.AccessDeniedPath = "/UsersLogin/AccessDenied"; // Denied path 
        //Expire time cookie
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        //Reload cookie time
        options.SlidingExpiration = true;
    });


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsTec", policy => policy.RequireRole("Tec"));
    options.AddPolicy("IsAdm", policy => policy.RequireRole("Adm"));   
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");    
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

//Middleware security headers
app.Use(async (context, next) => 
{
    // Previene la interpretación incorrecta del tipo de contenido
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    // Previene que la página se muestre en un iframe
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    // Activa el filtro XSS del navegador
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    // Política de seguridad para la carga de contenido (ajústala según tus necesidades)

    //context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self' https://cdnjs.cloudflare.com https://kit.fontawesome.com; " +
        "img-src 'self' data:; " +
        "script-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com https://kit.fontawesome.com; " +
        "style-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com");    
    // Política para el Referer
    context.Response.Headers.Append("Referrer-Policy", "no-referrer");

    await next();
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=UsersLogin}/{action=Login}/{id?}");

app.Run();
