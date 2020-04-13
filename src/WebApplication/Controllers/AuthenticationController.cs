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
using Microsoft.Extensions.Logging;

using WebApplication.Auth;
using Bcr = BCrypt.Net;

using WebApplication.Entities;
using WebApplication.Exceptions;
using WebApplication.Extensions;
using WebApplication.Models.Authentication;
using WebApplication.Services;

namespace WebApplication.Controllers
{
    [Authorize(Policy = AuthPolicies.Registered)]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ActionLogger]
    public class AuthenticationController : Controller
    {
        private readonly DatabaseContext _databaseContext;
        private readonly UserService _service;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(DatabaseContext databaseContext, UserService service, ILogger<AuthenticationController> logger)
        {
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
            _service = service;
            _logger = logger;
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

            if (user != null && Bcr.BCrypt.Verify(credentials.Password,user.Password))
            {
                const string issuer = "minitwit";

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.ID.ToString(), ClaimValueTypes.Integer, issuer),
                    new Claim(ClaimTypes.Name, user.Username, ClaimValueTypes.String, issuer),
                    new Claim(ClaimTypes.Email, user.Email, ClaimValueTypes.Email, issuer),
                    new Claim(ClaimTypes.Role, AuthRoles.Registered, ClaimValueTypes.String, issuer)
                };

                if (user.Username.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    claims.Add(new Claim(ClaimTypes.Role, AuthRoles.Administrator, ClaimValueTypes.String, issuer));
                }
            
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

            _logger.LogWarning($"Login failed using username: {credentials.Username}.");
            
            return View();
        }
        
        [HttpGet("/logout")]
        public async Task<IActionResult> Logout()
        {
            var username = User.GetUsername();
            
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation($"Successfully logged out user with username: {username}.");
            
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
            try
            {
                await _service.CreateUser(credentials, ct);
                
                _logger.LogInformation($"Successfully registered user with username: {credentials.Username}.");
            }
            catch (CreateUserException exception)
            {
                _logger.LogWarning(exception, "Failed to register user.");
                
                ViewData["messages"] = new List<string>
                {
                    exception.Message
                };
                
                return View();
            }
            
            ViewData["messages"] = new List<string>
            {
                "User registered successfully"
            };
                
            return View("Login");
        }
    }
}
