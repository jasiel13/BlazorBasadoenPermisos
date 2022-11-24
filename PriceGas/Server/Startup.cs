using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PriceGas.Client.Helpers;
using PriceGas.Server.Datos;
using PriceGas.Server.Helpers;
using PriceGas.Server.Permission;
using PriceGas.Shared.Constants;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace PriceGas.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //servicios para administrar politicas o permisos de roles
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            //poner la conexion a la bd  
            services.AddDbContext<ApplicationDbContext>
                (options => options.UseSqlServer(Configuration.GetConnectionString("SqlConnectionDesarrollo")));

            //configuramos identity para control de usuarios
            services.AddIdentity<ApplicationUser, IdentityRole>(
                 options =>
                 {
                     /*De forma predeterminada, requiere que las contraseñas contengan un carácter en mayúsculas,
                     un carácter en minúsculas, un dígito y un carácter no Identity alfanumérico.
                     Las contraseñas deben tener al menos seis caracteres.*/

                     //alterar la configuracion predeterminada
                     options.Password.RequireDigit = true;
                     options.Password.RequireLowercase = true;
                     options.Password.RequireNonAlphanumeric = true;
                     options.Password.RequireUppercase = true;
                     options.Password.RequiredLength = 8;
                     options.Password.RequiredUniqueChars = 1;
                     options.User.AllowedUserNameCharacters = null; //permitir nombres de usuario en identity con caracteres ya sea espacion o acentos etc... 
                     //options.User.RequireUniqueEmail = true;
                     //options.SignIn.RequireConfirmedEmail = true;
                 }
                )
               .AddEntityFrameworkStores<ApplicationDbContext>()
               .AddRoleManager<RoleManager<IdentityRole>>()//nuevo
               .AddSignInManager<SignInManager<ApplicationUser>>()//nuevo
               .AddDefaultTokenProviders();

            //configuramos los jsonwebtokens conesto vamos a poder enviar hacia el proyecto de blazor las credenciales del usuario
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                  .AddJwtBearer(options =>
               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = false,
                   ValidateAudience = false,
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true,
                   IssuerSigningKey = new SymmetricSecurityKey(
                   Encoding.UTF8.GetBytes(Configuration["jwt:key"])),
                   ClockSkew = System.TimeSpan.Zero
               });

            //uso de Automapper          
            services.AddAutoMapper(typeof(Startup));

            //servicio para guardar imagen de manera local 
            services.AddScoped<IAlmacenadorArchivos, AlmacenadorArchivosLocal>();

            //agremaos el servicios de addhttpcontextaccesor
            services.AddHttpContextAccessor();

            //si se encuentra con un bucle de referencia al deserealizar debe ignorar esa situacion
            services.AddMvc().AddNewtonsoftJson(options =>
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddControllersWithViews();
            services.AddRazorPages();

            //nuevo
            services.AddAuthorization(options =>
            {
                // Here I stored necessary permissions/roles in a constant
                foreach (var prop in typeof(Permissions).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
                {
                    options.AddPolicy(prop.GetValue(null).ToString(), policy => policy.RequireClaim("Permission", prop.GetValue(null).ToString()));
                }
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            //app.UsePathBase("/capacitate");

            //creamos un middlewaer de autenticacion y autorizacion
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
