using Microsoft.AspNetCore.Identity;
using PriceGas.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PriceGas.Server.Helpers
{
    public static class ClaimsHelper
    {
        /*
         * El método GetPermissions Extension toma una lista de permisos disponibles,
         * el tipo de permiso (por ejemplo, ProductPermissions) que se agregará y el ID de función. 
         * Luego agrega todas las propiedades mencionadas en ProductPermissions usando Reflection.
         * Esta es una clase auxiliar simple que se puede optimizar aún más.
         */
        public static void GetPermissions(this List<RoleClaimsDTO> allPermissions, Type policy, string roleId)
        {
            FieldInfo[] fields = policy.GetFields(BindingFlags.Static | BindingFlags.Public);

            foreach (FieldInfo fi in fields)
            {
                allPermissions.Add(new RoleClaimsDTO { Value = fi.GetValue(null).ToString(), Type = "Permissions" });
            }
        }

        //este método de extensión es responsable de agregar las notificaciones seleccionadas de la interfaz de usuario al rol de usuario.
        public static async Task AddPermissionClaim(this RoleManager<IdentityRole> roleManager, IdentityRole role, string permission)
        {
            var allClaims = await roleManager.GetClaimsAsync(role);
            if (!allClaims.Any(a => a.Type == "Permission" && a.Value == permission))
            {
                await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
            }
        }
    }
}
