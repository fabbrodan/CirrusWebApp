using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CirrusWebApp.Data.Services;
using CirrusWebApp.Data.Models;

namespace CirrusWebApp.Pages
{
    [AllowAnonymous]
    public class LoginCallGoogleModel : PageModel
    {
        private readonly CosmosDbService _dbService;

        public LoginCallGoogleModel(CosmosDbService CosmosDbService)
        {
            _dbService = CosmosDbService;
        }
        public IActionResult OnGetAsync(string returnUrl = null)
        {
            string provider = "Google";
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = Url.Page("./LoginCallGoogle",
                pageHandler: "Callback",
                values: new { returnUrl }
                )
            };

            return new ChallengeResult(provider, authenticationProperties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(
            string returnUrl = null, string remoteError = null)
        {
            // Get the information about the user from the external login provider
            var GoogleUser = this.User.Identities.FirstOrDefault();
            if (GoogleUser.IsAuthenticated)
            {
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    RedirectUri = this.Request.Host.Value
                };
                User NativeUser = new User
                {
                    id = GoogleUser.Claims.Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").FirstOrDefault().Value,
                    Email = GoogleUser.Claims.Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").FirstOrDefault().Value,
                    Firstname = GoogleUser.Claims.Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname").FirstOrDefault().Value,
                    Lastname = GoogleUser.Claims.Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname").FirstOrDefault().Value
                };

                User DbUser = await _dbService.GetUser(NativeUser);

                if (DbUser is null)
                {
                    NativeUser.RegisteredDateTime = DateTime.UtcNow;
                    await _dbService.AddUser(NativeUser);
                }

                await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(GoogleUser),
                authProperties);
            }
            return LocalRedirect("/");
        }
    }
}
