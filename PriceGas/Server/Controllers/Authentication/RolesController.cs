using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PriceGas.Server.Datos;
using PriceGas.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriceGas.Server.Controllers.Authentication
{
    [Route("api/[controller]")]
    [ApiController]   
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ApplicationDbContext context;       
        public RolesController(RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            this.roleManager = roleManager;
            this.context = context;           
        }

        //con esta peticion traemos el listado de roles desde la bd
        [HttpGet]        
        public async Task<ActionResult<List<UserRolesDTO>>> Get()
        {
            //mapeamos hacia rolesdto donde el rolid es igual a id
            return await context.Roles.Select(x => new UserRolesDTO { RoleName = x.Name, RoleId = x.Id }).ToListAsync();
        }

        //añadir nuevos roles
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<string>> AddRole(UserRolesDTO rol)
        {
            if (rol != null)
            {
                await roleManager.CreateAsync(new IdentityRole(rol.RoleName.Trim()));
            }
            return rol.RoleId;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {            
            var rol = await context.Roles.Where(x => x.Id == id).FirstOrDefaultAsync();
            context.Remove(rol);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
