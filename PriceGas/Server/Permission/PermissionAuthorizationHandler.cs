using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriceGas.Server.Permission
{
    //controlador de autorización que verifique si un usuario tiene el permiso necesario para acceder al recurso.
    internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        public PermissionAuthorizationHandler()
        {

        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User == null)
            {
                return;
            }

            //Obtiene todos los Reclamos del Usuario de Tipo 'Permiso' y verifica si alguien coincide con el permiso requerido
            var permissionss = context.User.Claims.Where(x => x.Type == "Permission" &&
                                                            x.Value == requirement.Permission &&
                                                            x.Issuer == "LOCAL AUTHORITY");

            //Si hay una coincidencia, el usuario puede acceder al recurso protegido. De lo contrario, al usuario se le presentará una página de acceso denegado.
            if (permissionss.Any())
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}
