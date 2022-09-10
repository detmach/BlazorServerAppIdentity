using BlazorServerApp.Data;
using Hangfire;
using Hangfire.Redis;
using IdentityUI.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Models;
using Newtonsoft.Json;
using Repos;
using Repos.Db;
using System.Reflection;

namespace BlazorServerApp.Extensions
{
    public static class StartupExtensions
    {
        #region Bağlantı
        public static void Baglanti(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("Repos")));

        }
        #endregion

        public static void RepoStoryRegister(this IServiceCollection services)
        {
            

            services.AddSingleton<IEmailService, EmailService>();
            services.AddHttpClient();
            services
                  .AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<WeatherForecastService>();
            services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5001/") });
            services.AddScoped<TokenView>();

           
        }


        #region Cookies ve Bearer Ayarları
        public static void CookiesAndBearer(this IServiceCollection services, IConfiguration Configuration)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential 
                // cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                // requires using Microsoft.AspNetCore.Http;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });
            services.AddScoped<ApplicationRole>();
            services.AddScoped<ApplicationUser>();
            services.AddIdentity<ApplicationUser, ApplicationRole>(options => options.Stores.MaxLengthForKeys = 128).AddRoles<ApplicationRole>()
                .AddUserManager<UserManagerRepo>().AddRoleManager<RoleManager<ApplicationRole>>()
                //.AddErrorDescriber<Turkcelestirme>()
               .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
            services.AddAuthorization(options =>
                options.AddPolicy("Gorevler", policy =>
                    policy.RequireRole(new[] { Roller.Root })
                ));
            services.Configure<IdentityOptions>(options =>
            {
                // Default Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(60);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ĞğÜüŞşİıÖöÇç";
                options.User.RequireUniqueEmail = true;
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
         .AddCookie(options =>
         {
             options.LoginPath = "/login";
             options.LogoutPath = "/account/logout";
         }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudience = "tr-api.com",
                    ValidateIssuer = true,
                    ValidIssuer = "tr-api.com",
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Configuration["Application:Secret"]))
                };
            })
                .AddGoogle(googleOptions =>
                {
                    googleOptions.ClientId = Configuration["Authentication:Google:ClientId"];
                    googleOptions.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                    googleOptions.ClaimActions.MapJsonKey("urn:google:profile", "link");
                    googleOptions.ClaimActions.MapJsonKey("urn:google:image", "picture");
                });

        }
        #endregion

        #region Hangfire ve Redis
        public static void HangfireAyarlar(this IServiceCollection services)
        {
            services.AddHangfire(_ =>
            {
                var option = new RedisStorageOptions()
                {
                    Db = 1,
                    Prefix = "SayoKitabevi_HangFire_",
                    FetchTimeout = TimeSpan.FromHours(5),
                    DeletedListSize = 10,
                    ExpiryCheckInterval = TimeSpan.FromHours(5),
                    InvisibilityTimeout = TimeSpan.FromMinutes(30.0),

                };
                _.UseRedisStorage("127.0.0.1", options: option)

                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();

                _.UseSerializerSettings(new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            });
            services.AddHangfireServer();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "127.0.0.1";
            });
        }

        #endregion
        #region Swagger
        public static void Swagger(this IServiceCollection services)
        {
            //services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

           
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }
        #endregion
        #region Sıkıştırmalar
        //        public static readonly IEnumerable<string> MimeTypes = new[]
        //{
        //    // General
        //    "text/plain",
        //    // Static files
        //    "text/css",
        //    "application/javascript",
        //    // MVC
        //    "text/html",
        //    "application/xml",
        //    "text/xml",
        //    "application/json",
        //    "text/json",
        //    "image/svg+xml",
        //    "image/png", "image/jpeg"
        //};
        //        public static void Sikistirmalar(this IServiceCollection services)
        //        {
        //            var excludedUrls = new List<IUrlMatcher>
        //            {
        //                new ExactUrlMatcher("/gorevler"),
        //                new RegexUrlMatcher(@"^\/gorevler\/.+$")
        //            };

        //            services.AddResponseCaching();
        //            services.AddResponseCompression(options =>
        //            {
        //                options.EnableForHttps = true;
        //                options.Providers.Add<BrotliCompressionProvider>();
        //                options.Providers.Add<GzipCompressionProvider>();
        //                options.MimeTypes =
        //                    ResponseCompressionDefaults.MimeTypes.Concat(MimeTypes);
        //            });
        //            services.AddWebMarkupMin(
        //            options =>
        //            {
        //                options.AllowMinificationInDevelopmentEnvironment = true;
        //                options.AllowCompressionInDevelopmentEnvironment = true;
        //            })
        //                .AddHtmlMinification(
        //            options =>
        //            {
        //                HtmlMinificationSettings settings = options.MinificationSettings;
        //                settings.RemoveHttpProtocolFromAttributes = true;
        //                settings.RemoveHttpsProtocolFromAttributes = true;
        //                settings.RemoveHtmlComments = true;
        //                settings.RemoveHtmlCommentsFromScriptsAndStyles = true;
        //                settings.RemoveJsTypeAttributes = true;
        //                settings.RemoveOptionalEndTags = true;
        //                options.ExcludedPages = excludedUrls;
        //            })
        //                .AddHttpCompression(options =>
        //                {
        //                    options.ExcludedPages = excludedUrls;
        //                    options.CompressorFactories = new List<ICompressorFactory>
        //    {
        //        new DeflateCompressorFactory(new DeflateCompressionSettings { Level = CompressionLevel.Fastest }),
        //        new GZipCompressorFactory(new GZipCompressionSettings { Level = CompressionLevel.Fastest })
        //    };
        //                });

        //        }
        #endregion

        public static async void Datalar(this IApplicationBuilder apps)
        {
            var app = apps.ApplicationServices.CreateScope();
            ApplicationDbContext db = app.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            UserManager<ApplicationUser> userManager = app.ServiceProvider.GetRequiredService<UserManagerRepo>();
            RoleManager<ApplicationRole> roleManager = app.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            //var ayarlar = app.ServiceProvider.GetRequiredService<ISiteAyarRepo>();
            var Repo = app.ServiceProvider.GetRequiredService<IUnitOfWork>();
            //ayarlar.BaslangicAyarlari();
            db.Database.Migrate();


            #region Roller

            IList<FieldInfo> roller = typeof(Roller).GetFields().ToList();
            var roles = roleManager.Roles.ToList();
            foreach (var r in roller)
            {
                if (!roles.Where(t => t.Name == r.Name).Any())
                    roleManager.CreateAsync(new ApplicationRole { Name = r.Name }).GetAwaiter().GetResult();
            }

            #endregion Roller

            #region Kullanicilar



            var detmach = new ApplicationUser()
            {
                UserName = "detmach@gmail.com",
                Adi = "Hüseyin",
                Soyadi = "ÖLMEZ",
                Email = "detmach@gmail.com",
                PhoneNumber = "5452162474",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                Avatar = "/picpng",
                Sifre = "99721952"
            };
            if (userManager.FindByEmailAsync(detmach.Email).GetAwaiter().GetResult() == null)
            {
                var result = userManager.CreateAsync(detmach, "99721952").GetAwaiter().GetResult();
                userManager.AddToRoleAsync(detmach, "Root").GetAwaiter().GetResult();
            }
           

            #endregion Kullanicilar

            //await db.SaveChangesAsync();

        }
    }
}
