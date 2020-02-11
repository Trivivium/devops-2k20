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
                    new Claim(ClaimTypes.Name, user.Username, ClaimValueTypes.String, issuer),
                    new Claim(ClaimTypes.Email, user.Email, ClaimValueTypes.Email)
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

            return View();
        }

        [AllowAnonymous]
        [HttpPost("/register")]
        public async Task<IActionResult> Register(RegisterModel credentials, CancellationToken ct)
        {
            var user = new User { Email = credentials.Email, PasswordHash = PasswordUtils.Hash(credentials.Password), Username = credentials.Username};

            var usernameExists = await _databaseContext.Users.AnyAsync(item => item.Username == credentials.Username, ct);

            var emailExists = await _databaseContext.Users.AnyAsync(item => item.Email == credentials.Email, ct);


            if (!usernameExists && !emailExists)
            {
                _databaseContext.Add<User>(user);
                _databaseContext.SaveChanges();
                ViewData["messages"] = new List<string>
            {
                "User registered successfully"
            };
            }
            else
            {
                ViewData["messages"] = new List<string>
            {
                "Username or email already exists"
            };
            }


            return RedirectToAction("Login");
        }
    }
}
