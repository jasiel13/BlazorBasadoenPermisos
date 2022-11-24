using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PriceGas.Server.Datos;
using PriceGas.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PriceGas.Server.Datos.ApplicationUser;

namespace PriceGas.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class UserRolesController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext context;
        public UserRolesController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            this.context = context;
        }

        //retorna una lista de roles por el usuario seleccionado
        [HttpGet("{userid}")]
        public async Task<ActionResult<ManageUserRolesDTO>> Get(string userId)
        {
            //lista de rolesdto
            var viewModel = new List<UserRolesDTO>();

            //buscamos el usuario
            var user = await _userManager.FindByIdAsync(userId);

            //obtenemos una lista de todos los roles
            foreach (var role in _roleManager.Roles.ToList())
            {
                //por cada rol creamos un userroldto y le asignamos el nombre
                var userRolesViewModel = new UserRolesDTO
                {
                    RoleName = role.Name
                };

                //si exite un usuario que tenga ese rol quiere decir que esta seleccionado, sino es que no esta selecionado
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.Selected = true;
                }
                else
                {
                    userRolesViewModel.Selected = false;
                }

                //guardamos cada userroldto en la lista
                viewModel.Add(userRolesViewModel);
            }

            //al final retornamos un manageuserroldo con el id del usuario seleccionado y la nueva lista de roles por usuario 
            //pero ya con el campo selected, todo esto se hace por que no exite una tabla que tenga el campo selected
            //la tabla rol solo puede darte la data del id y el nombre 
            var model = new ManageUserRolesDTO()
            {
                UserId = userId,
                UserRoles = viewModel
            };

            return model;
        }

        //esta api asigna y desasigna roles son mis endpoint asignarrol removerrol
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<string>> Update(ManageUserRolesDTO model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            result = await _userManager.AddToRolesAsync(user, model.UserRoles.Where(x => x.Selected).Select(y => y.RoleName));
            await AsignarTipodeUsuario(model);//jasiel
            var currentUser = await _userManager.GetUserAsync(User);
            await _signInManager.RefreshSignInAsync(currentUser);
            await Seeds.DefaultUsers.SeedSuperAdminAsync(_userManager, _roleManager);
            return model.UserId;
        }
        public async Task AsignarTipodeUsuario(ManageUserRolesDTO model)
        {
            //lo buscamos por su id con findbyid
            var usuario = await _userManager.FindByIdAsync(model.UserId);

            foreach (var item in model.UserRoles)
            {
                if (item.RoleName == "SuperAdmin" && item.Selected == true)
                {
                    usuario.TipodeUsuarios = TipodeUsuario.SuperAdmin;
                }
                else if (item.RoleName == "Admin" && item.Selected == true)
                {
                    usuario.TipodeUsuarios = TipodeUsuario.Admin;
                }
                else if (item.RoleName == "Basic" && item.Selected == true)
                {
                    usuario.TipodeUsuarios = TipodeUsuario.Basic;
                }
            }

            //obtener el registro original usando el método FindAsync 
            var oldusuario = await context.Users.FindAsync(usuario.Id);

            //las propiedades sin cambios se ignoran y solo los valores de cambios se incluyen en la consulta de actualización
            context.Entry(oldusuario).CurrentValues.SetValues(usuario);

            await context.SaveChangesAsync();
        }
    }
}
