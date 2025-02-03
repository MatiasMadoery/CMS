using Control_Machine_Sistem.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

//Dependency injection
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("conexionDb"))
    );

// Agregar servicios de autenticación y autorización
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // To mitigate the risk of session hijacking and XSS (Cross-Site Scripting) attacks.
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        options.LoginPath = "/UsersLogin/Login"; // Login path
        options.AccessDeniedPath = "/UsersLogin/AccessDenied"; // Denied path 
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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=UsersLogin}/{action=Login}/{id?}");

app.Run();
