using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication.Models.Authentication;

namespace WebApplication.Controllers
{
    [Authorize]
    public class AuthenticationController : Controller
    {
        [AllowAnonymous]
        [HttpGet("/login")]
        public IActionResult Login()
        {
            ViewData["title"] = "Login";

            return View();
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost("/login")]
        public async Task<IActionResult> Login(LoginCredentialsModel credentials)
        {
            // TODO: Remove this! It's temporary while we have no database impl.
            const int id = 1;
            const string username = "admin";
            const string password = "admin";

            if (
                string.Equals(credentials.Username, username, StringComparison.CurrentCultureIgnoreCase) &&
                string.Equals(credentials.Password, password, StringComparison.CurrentCultureIgnoreCase)
            )
            {
                const string issuer = "minitwit";

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, id.ToString(), ClaimValueTypes.Integer, issuer),
                    new Claim(ClaimTypes.Name, username, ClaimValueTypes.String)
                };
                
                var properties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IssuedUtc = DateTimeOffset.UtcNow
                };

                const string scheme = CookieAuthenticationDefaults.AuthenticationScheme;
                var identity = new ClaimsIdentity(claims, scheme);

                await HttpContext.SignInAsync(scheme, new ClaimsPrincipal(identity), properties);
                
                return RedirectToAction("UserTimeline", "Timeline", new { username });
            }

            ViewData["title"] = "Login";
            ViewData["messages"] = new List<string>
            {
                "Invalid login credentials"
            };

            return View();
        }
        
        [HttpGet("/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Timeline", "Timeline");
        }
        
        [AllowAnonymous]
        [HttpGet("/register")]
        public IActionResult Register()
        {
            ViewData["title"] = "Register";
            
            throw new NotImplementedException();
        }
    }
}
