using Microsoft.AspNetCore.Identity;
using PriceGas.Server.Datos;
using PriceGas.Shared.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PriceGas.Server.Seeds
{
    public static class DefaultUsers
    {
        public static async Task SeedBasicUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Crea un Usuario con Rol Básico
            var defaultUser = new ApplicationUser
            {
                UserName = "basicuser@gmail.com",
                Email = "basicuser@gmail.com",
                EmailConfirmed = true
            };
            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);//Obtiene el rol por nombre.
                if (user == null)
                {
                    //una vez que se crea/sembraa el usuario, lo agregamos al rol básico.
                    await userManager.CreateAsync(defaultUser, "123Pa$$word!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Basic.ToString());
                }
            }
        }
        public static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Default User
            var defaultUser = new ApplicationUser
            {
                UserName = "superadmin@gmail.com",
                Email = "superadmin@gmail.com",
                EmailConfirmed = true
            };
            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    //creamos otro usuario y lo agregamos a los roles básico, administrador y superadministrador. Básicamente, a este usuario se le otorgan todos los roles.
                    await userManager.CreateAsync(defaultUser, "123Pa$$word!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Basic.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Roles.Admin.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Roles.SuperAdmin.ToString());
                }

                //le agregamos todos los permisos de products
                await roleManager.SeedClaimsForSuperAdmin();
            }
        }

        //sembramos los permisos de products
        private async static Task SeedClaimsForSuperAdmin(this RoleManager<IdentityRole> roleManager)
        {
            var adminRole = await roleManager.FindByNameAsync("SuperAdmin");
            await roleManager.AddPermissionClaim(adminRole, "Products");//obtenga todas las reclamaciones existentes que ya existen para el rol.
        }

        /*
          Aquí, pasamos un parámetro de cadena Producto al método GeneratePermissionsForModule que devolverá una lista de permisos para el Módulo Producto (Crear, Leer, Ver, Eliminar, Modificar).
        */
        public static async Task AddPermissionClaim(this RoleManager<IdentityRole> roleManager, IdentityRole role, string module)
        {
            var allClaims = await roleManager.GetClaimsAsync(role);//obtenemos todos los claims del rol enviado

            //obtenemos todos los permisos del tipo enviado en este caso "products"
            var allPermissions = Permissions.GeneratePermissionsForModule(module);

            //recorremos todos los permisos
            foreach (var permission in allPermissions)
            {
                //sino existe en la lista de claims el permiso, se agrega uno nuevo 
                if (!allClaims.Any(a => a.Type == "Permission" && a.Value == permission))
                {
                    //agregamos un nuevo claim
                    await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
                }
            }
        }
    }
}
