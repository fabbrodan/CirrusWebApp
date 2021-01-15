using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using CirrusWebApp.Data.Models;
using CirrusWebApp.Data.Services;
using Microsoft.AspNetCore.Components;

namespace CirrusWebApp.Pages
{
    public class LoginCallNativeModel : PageModel
    {

        private readonly CosmosDbService _dbService;
        private readonly PasswordHashService _passwordService;

        public LoginCallNativeModel(CosmosDbService DbService, PasswordHashService PasswordHashService)
        {
            _dbService = DbService;
            _passwordService = PasswordHashService;
        }
        public async Task<IActionResult> OnGetAsync(string Email, string Password, bool PersistentCookie)
        {
            var SignInUser = new User { id = Email, Password = Password };
            User DbUser = await _dbService.GetUser(SignInUser.id);

            if (DbUser != null)
            {
                if (_passwordService.VerifyPassword(SignInUser.Password, DbUser.PasswordSalt, DbUser.Password))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, DbUser.Email),
                        new Claim(ClaimTypes.GivenName, DbUser.Firstname),
                        new Claim(ClaimTypes.Surname, DbUser.Lastname)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    AuthenticationProperties authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        ExpiresUtc = PersistentCookie ? null : DateTime.UtcNow.AddHours(1.0d),
                        IsPersistent = PersistentCookie,
                        IssuedUtc = PersistentCookie ? null : DateTime.UtcNow,
                        RedirectUri = this.Request.Host.Value
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);
                }
                else
                {
                    Console.WriteLine("Incorrect password");
                }
            }
            else
            {
                Console.WriteLine("No such user");
            }

            return LocalRedirect("/");
        }
    }
}
