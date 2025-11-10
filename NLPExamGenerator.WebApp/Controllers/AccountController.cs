using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NLPExamGenerator.WebApp.Models; 

using NLPExamGenerator.Logica;
using NLPExamGenerator.Entidades;

namespace PNLExamGenerator.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IUsuarioLogica _usuarioLogica;



        public AccountController(ILogger<AccountController> logger, IUsuarioLogica usuarioLogica)
        {
            _logger = logger;
            _usuarioLogica = usuarioLogica;
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string pass)
        {
            bool exito = _usuarioLogica.Login(email, pass);

            if (exito)
            {
                var usuario = _usuarioLogica.GetUsuarioByEmail(email);
                
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Nombre ?? usuario.Email),
                    new Claim(ClaimTypes.Email, usuario.Email ?? string.Empty),
                    new Claim("NombreUsuario", usuario.Nombre ?? string.Empty),
                    new Claim("UserId", usuario.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );
                
                HttpContext.Session.SetString("NombreUsuario", usuario.Nombre ?? "");

                return Json(new { success = true, mensaje = "Bienvenido", nombre = usuario.Nombre });
            }
            else
            {
                return Json(new { success = false, mensaje = "Email o contraseña incorrecta" });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            HttpContext.Session.Clear();

            return Json(new { success = true });
        }


        [HttpPost]
        // [ValidateAntiForgeryToken]
        public IActionResult Register(string name, string email, string pass, string confirmPass)
        {
            try
            {
                if (pass != confirmPass)
                    return Json(new { success = false, mensaje = "Las contraseñas no coinciden" });

                _usuarioLogica.Register(new Usuario { Nombre = name, Email = email, Password = pass });
                HttpContext.Session.SetString("NombreUsuario", name);

                return Json(new { success = true, mensaje = $"Usuario {name} registrado", nombre = name });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Register");
                return Json(new { success = false, mensaje = "Ocurrió un error al registrarse" });
            }
        }
    }

}
