using Control_Machine_Sistem.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Control_Machine_Sistem.Controllers
{
    public class UsersLoginController : Controller
    {
        private readonly AppDbContext _context;

        public UsersLoginController(AppDbContext context)
        {
            _context = context;
        }

        // Action to show the login view
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Action to manage login POST
        [HttpPost]
        public async Task<IActionResult> Login(string name, string password)
        {
            var user = _context.Users!.FirstOrDefault(u => u.Name == name && u.Password == password);
            if (user != null)
            {
                // Create claims (user information)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Name!),
                    new Claim(ClaimTypes.Role, user.Rol!) // Assign role
                };

                // Create user identity
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // User authenticate
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                // Redirect depending on the role
                if (user.Rol == "Adm")
                {
                    return RedirectToAction("Index", "Home"); 
                }
                else
                {
                    return RedirectToAction("Index", "Home"); 
                }
            }
            else
            {
                // If login fails
                ModelState.AddModelError("", "Usuario o contraseña incorrectos");
                ViewData["Error"] = "Usuario o Contraseña incorrectos!";
            }
            return View();
        }

        // Log Out Action
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        //  Access Denied View
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

