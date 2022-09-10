using BlazorServerApp.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Models;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Authentication;
using BlazorServerApp.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using IdentityUI.Extensions;

namespace BlazorServerApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.RepoStoryRegister();

            builder.Services.AddRazorPages();
            builder.Services.AddBlazoredLocalStorage();


            builder.Services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; }); ;
            builder.Services.AddBlazoredSessionStorage();

            builder.Services.Baglanti(builder.Configuration);
            builder.Services.CookiesAndBearer(builder.Configuration);
            builder.Services.HangfireAyarlar();
            builder.Services.AddControllers(options => options.EnableEndpointRouting = false);
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.Swagger();
           
          
            builder.Services.AddAuthenticationCore();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                OnPrepareResponse = context => context.Context.Response.Headers.Add("Cache-Control", "public, max-age=2592000")
            });
            app.UseHttpsRedirection();
            app.UseBlazorIdentityUI();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseStatusCodePages(context =>
            {
                var response = context.HttpContext.Response;
                if (response.StatusCode == (int)HttpStatusCode.Unauthorized ||
                    response.StatusCode == (int)HttpStatusCode.Forbidden)
                    response.Redirect("/Login");
                return Task.CompletedTask;
            });
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
            });
            app.MapFallbackToPage("/_Host");
            app.Datalar();

            app.Run();
        }
    }
}