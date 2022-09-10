using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Models;
using Repos;

namespace BlazorServerApp.Controllers.NormalController
{
    public class AccountController : BaseController
    {
        private readonly SignInManager<ApplicationUser> SignInManager;
        private readonly IWebHostEnvironment Environment;
        private readonly IDistributedCache Cache;
        private readonly ILogger<AccountController> Logger;
        public AccountController(IUnitOfWork UnitOfWork, SignInManager<ApplicationUser> signInManager, IWebHostEnvironment environment, IConfiguration configuration, IDistributedCache Cache, ILogger<AccountController> logger) : base(UnitOfWork, configuration)
        {
            SignInManager = signInManager;
            Environment = environment;
            this.Cache = Cache;
            this.Logger = logger;
        }
        #region Login
        [HttpPost("login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string returnUrl = null)
        {


            // This does not count login failures towards account lockout
            // To enable password failures to trigger account lockout,
            // set lockoutOnFailure: true
            if (ModelState.IsValid)
            {
                var result = await SignInManager.PasswordSignInAsync(model.email, model.password, true, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    Logger.LogInformation("User logged in.");

                    return RedirectToLocal(returnUrl);
                }
                //if (result.RequiresTwoFactor)
                //{
                //    return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.LoginView.BeniHatirla });
                //}
                if (result.IsLockedOut)
                {
                    Logger.LogWarning("User account locked out.");
                    return RedirectToLocal("Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "EPosta Adresi veya Şifre Hatalı");
                }
            }
            return RedirectToLocal(returnUrl);
        }
        #endregion

        #region Logout

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            return this.SignOut(new AuthenticationProperties
            {
                RedirectUri = "/"
            });
        }
    }

    #endregion

}
