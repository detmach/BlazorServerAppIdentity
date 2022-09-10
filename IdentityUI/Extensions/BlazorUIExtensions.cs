using IdentityUI.Common;
using IdentityUI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace IdentityUI.Extensions
{
    public static class BlazorUIExtensions
    {
        public static IApplicationBuilder UseBlazorIdentityUI(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<AuthentificationService>();
            return builder;
        }
    }
}
