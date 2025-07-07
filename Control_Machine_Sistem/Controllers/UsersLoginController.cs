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

        // Action para mostrar la vista de login
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            // Si el modelo no es válido, enviamos un mensaje de alerta
            if (!ModelState.IsValid)
            {
                TempData["AlertMessage"] = "Por favor, revise los datos ingresados.";
                return View(loginDto);
            }

            try
            {
                // Buscar el usuario en la base de datos por el nombre
                var user = _context.Users.FirstOrDefault(u => u.Name == loginDto.Name);
                if (user != null && BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
                {
                    // Si la verificación del hash es exitosa, se crean los claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Name ?? string.Empty),
                        new Claim(ClaimTypes.Role, user.Rol ?? string.Empty)
                    };

                    // Se crea la identidad del usuario
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // Se firma la cookie de autenticación
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    // Redirigir al usuario a la página principal (ej: "Home/Index")
                    return RedirectToAction("Index", "Home");
                }

                // Si el usuario no existe o la contraseña es incorrecta, enviar alerta
                TempData["AlertMessage"] = "Usuario o contraseña incorrectos.";
                return View(loginDto);
            }
            catch (Exception ex)
            {
                // Podés registrar la excepción si lo deseas con tu logger
                TempData["AlertMessage"] = "Ocurrió un error al procesar la solicitud. Intente nuevamente más tarde.";
                return View(loginDto);
            }
        }

        // Log Out Action
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // Access Denied View
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}


        //Post para ingresar con usuario y contraseña
        //[HttpPost]
        //public async Task<IActionResult> Login(LoginDto loginDto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(loginDto);
        //    }

        //    // Buscar usuario por nombre (o por email, según convenga)
        //    var user = _context.Users.FirstOrDefault(u => u.Name == loginDto.Name);
        //    if (user != null)
        //    {
        //        bool isPasswordValid = false;
        //        // Verificar si la contraseña está en formato hash (asumiendo que un hash BCrypt comienza con "$2")
        //        if (user.Password!.StartsWith("$2"))
        //        {
        //            // Validación usando BCrypt
        //            try
        //            {
        //                isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password);
        //            }
        //            catch (Exception ex)
        //            {
        //                // En caso de error (por ejemplo, salt mal formado) puedes registrar o manejar el error
        //                ModelState.AddModelError("", "Error en la validación de la contraseña.");
        //                return View(loginDto);
        //            }
        //        }
        //        else
        //        {
        //            // La contraseña en la base de datos está en texto plano.
        //            // Comparamos directamente:
        //            if (user.Password == loginDto.Password)
        //            {
        //                isPasswordValid = true;
        //                // Opcional: actualiza el usuario, hasheando la contraseña ingresada,
        //                // para migrar el valor a un formato seguro.
        //                user.Password = BCrypt.Net.BCrypt.HashPassword(loginDto.Password);
        //                _context.Users.Update(user);
        //                await _context.SaveChangesAsync();
        //            }
        //        }

        //        if (isPasswordValid)
        //        {
        //            // Crear claims y firmar la cookie
        //            var claims = new List<Claim>
        //    {
        //        new Claim(ClaimTypes.Name, user.Name!),
        //        new Claim(ClaimTypes.Role, user.Rol!)
        //    };

        //            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        //            await HttpContext.SignInAsync(
        //                CookieAuthenticationDefaults.AuthenticationScheme,
        //                new ClaimsPrincipal(claimsIdentity));

        //            return RedirectToAction("Index", "Home");
        //        }
        //    }

        //    ModelState.AddModelError("", "Usuario o contraseña incorrectos");
        //    ViewData["Error"] = "Usuario o Contraseña incorrectos!";
        //    return View(loginDto);
        //}


