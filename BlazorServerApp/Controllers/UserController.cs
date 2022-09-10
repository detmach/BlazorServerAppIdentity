using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repos;

namespace BlazorServerApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : BaseController
    {
        private readonly SignInManager<ApplicationUser> SignInManager;
        private readonly ILogger<UserController> Logger;

        public UserController(IUnitOfWork UnitOfWork, IConfiguration configuration, ILogger<UserController> logger, SignInManager<ApplicationUser> signInManager) : base(UnitOfWork, configuration)
        {
            this.SignInManager = signInManager;
            this.Logger = logger; ;
        }

        [HttpPost("")]
        [AllowAnonymous]
        [ProducesResponseType(200, Type = typeof(Response<TokenView>))]
       // [MapToApiVersion("1.0")]

        public async Task<IActionResult> Auth(LoginModel request)
        {
            if (ModelState.IsValid)
            {

                var user = await Repo.UserManager.FindByNameAsync(request.Email);

                if (user != null)
                {
                    if (user.PasswordHash == null)
                        return Ok(new Response<TokenView>("Kullanıcınız Yönetici Tarafından Deaktif Edilmiştir"));

                    var result = await SignInManager.CheckPasswordSignInAsync(user, request.password, true);
                    if (result.Succeeded)
                    {
                        return Ok(await GetToken(user));
                    }
                }
                else
                {
                    return Ok(new Response<TokenView>("Böyle Bir Hesap Bulunamadı."));
                }
            }
            return Ok(new Response<TokenView>("Kullanıcı Adınız veya Şifreniz Hatalıdır."));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("google-login")]
        public async Task LoginGoogle()
        {
            await HttpContext.ChallengeAsync("Google", new AuthenticationProperties() { RedirectUri = "/signin-google" });
        }

        [HttpGet("token")]
        public ActionResult Auth()
        {
            var properties = new AuthenticationProperties()
            {
                // actual redirect endpoint for your app
                RedirectUri = $"/api/authentication/MyExternalLoginBack",
                Items =
                {
                    { "LoginProvider", "Google" },
                },
                AllowRefresh = true,
            };

            return Challenge(properties, "Google");
        }

    }
}
