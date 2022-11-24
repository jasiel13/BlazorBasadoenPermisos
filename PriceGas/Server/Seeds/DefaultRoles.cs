using Microsoft.AspNetCore.Identity;
using PriceGas.Server.Datos;
using PriceGas.Shared.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriceGas.Server.Seeds
{
    public static class DefaultRoles
    {
        //creará roles predeterminados en nuestra aplicación a través de la clase de identidad RoleManager Helper
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.SuperAdmin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Basic.ToString()));
        }
    }
}
