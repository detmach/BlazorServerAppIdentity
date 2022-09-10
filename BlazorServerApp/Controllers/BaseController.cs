using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Models;
using Repos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlazorServerApp.Controllers
{
    public class BaseController : ControllerBase
    {
        public readonly IConfiguration Configuration;
        public readonly IUnitOfWork Repo;
        public BaseController(IUnitOfWork UnitOfWork, IConfiguration configuration)
        {
            Repo = UnitOfWork;
            Configuration = configuration;

        }
        [NonAction]
        internal async Task<Response<TokenView>> GetToken(ApplicationUser user)
        {
            var sonuc = new Response<TokenView>();
            var roles = await Repo.UserManager.GetRolesAsync(user);
            var utcNow = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                        new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Dns, Request.HttpContext.Connection.RemoteIpAddress.ToString()),
                        new Claim(ClaimTypes.Email,user.Email ),
            };
            for (int i = 0; i < roles.Count(); i++)
            {
                claims.Add(new Claim(ClaimTypes.Role, roles[i]));
            };
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Application:Secret"]));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(
                signingCredentials: signingCredentials,
                claims: claims,
                notBefore: utcNow,
                expires: utcNow.AddDays(1),
                audience: Configuration["Audience"],
                issuer: Configuration["Issuer"]
                );

            var tokens = new TokenView()
            {
                Expires = utcNow.AddDays(1),
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                Roles = roles,
            };
            sonuc.Sonuc = tokens;
            return sonuc;
        }

        [NonAction]
        internal IActionResult RedirectToLocal(string returnUrl)
        {

            returnUrl = returnUrl.Contains("account-signin") ? "" : returnUrl;
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("/");
            }
        }
    }
}
