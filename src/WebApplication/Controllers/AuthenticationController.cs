using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using WebApplication.Entities;
using WebApplication.Helpers;
using WebApplication.Models.Authentication;

namespace WebApplication.Controllers
{
    [Authorize]
    public class AuthenticationController : Controller
    {
        private readonly DatabaseContext _databaseContext;

        public AuthenticationController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
        }

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
        public async Task<IActionResult> Login(LoginCredentialsModel credentials, CancellationToken ct)
        {
            var user = await _databaseContext.Users.FirstOrDefaultAsync(item => item.Username == credentials.Username, ct);

            if (user != null && PasswordUtils.Compare(user.PasswordHash, credentials.Password))
            {
                const string issuer = "minitwit";

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.ID.ToString(), ClaimValueTypes.Integer, issuer),
                    new Claim(ClaimTypes.Name, user.Username, ClaimValueTypes.String, issuer)
                };
            
                var properties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IssuedUtc = DateTimeOffset.UtcNow
                };

                const string scheme = CookieAuthenticationDefaults.AuthenticationScheme;
                var identity = new ClaimsIdentity(claims, scheme);

                await HttpContext.SignInAsync(scheme, new ClaimsPrincipal(identity), properties);
            
                return RedirectToAction("UserTimeline", "Timeline", new { user.Username });
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
