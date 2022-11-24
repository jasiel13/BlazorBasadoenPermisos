using PriceGas.Server.Datos;
using PriceGas.Server.Helpers;
using PriceGas.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PriceGas.Server.Datos.ApplicationUser;

namespace PriceGas.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]   
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        //utilizamos un constructor para poder inyectar una instancia de applicationdbcontext y otras
        public UsuariosController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            this.context = context;//lo ponemos como un campo
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        //con esta peticion traemos un listado de usuarios desde la bd, excepto el usuario actualmente logueado
        [HttpGet]        
        public async Task<ActionResult<List<UsuarioDTO>>> Get([FromQuery] PaginacionDTO paginacion)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);

            //obtener una lista de todos los usuarios
            var listadeusuarios = await context.Users.ToListAsync();

            //obtener una lista de usuarios con el id del rol super admin de la tabla userrole
            var rolyusuario = await context.UserRoles.Where(x => x.RoleId == "57e9c804-f214-4416-b7a5-b6393cc1fc7e").ToListAsync();

            //obtener todos los usuarios que no sean superadmin que estan en la lista rolyusuario comparandola contra la listausuarios
            var listadeusuariosfinal = listadeusuarios.Where(p => !rolyusuario.Any(p2 => p2.UserId == p.Id));

            var bandera = false; 

            //recorremos la lista de usuaros superadmin
            foreach(var item in rolyusuario)
            {
                //si un usuario de la lista es igual al usuario logueado, cambiamos la bandera a true
                if(user.Id == item.UserId)
                {
                    bandera = true;
                }
            }

            IQueryable<ApplicationUser> queryable;

            //si la bandera es true quiere decir que el usuario logueado es superadmin y puede ver toda la lista de usuarios
            //si la bandera es false quiere decir que el usuario logueado es admin o basic y no puede ver a los usuarios superadmin
            if (bandera == true)
            {
                queryable = context.Users.Where(x => x.Activo == true).AsQueryable();
            }
            else
            {
                //filtrar la lista de usuarios final por usuarios activos y que no sea el usuario logueado
                queryable = listadeusuariosfinal.Where(x => x.Activo == true && x.Id != user.Id).AsQueryable();
            }

           
            //el usuario solo pueder ver a los usuarios admin y basic y no se puede ver a el mismo
            //el admin puede ver a los usuarios admin y basic y no se puede ver a el mismo
            //el superadmin puede ver todos los usuarios incluso a el mismo

            await HttpContext.InsertarParametrosPaginacionEnRespuesta(queryable, paginacion.CantidadRegistros,true);
            return queryable.Paginar(paginacion)
            .Select(x => new UsuarioDTO { Usuario = x.UserName, UserId = x.Id, Contraseña = x.ContraseñaTextoPlano }).ToList();//hacemos un mapeo hacia usuariodto          
        }

        ////creamos las peticiones para asignar y eliminar un rol en el httpost le pasamos los metodos creados en editarusuario.razor
        //[HttpPost("asignarRol")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrador")]
        //public async Task<ActionResult> AsignarRolUsuario(EditarRolDTO editarRolDTO)
        //{
        //    //lo buscamos por su id con findbyid
        //    var usuario = await userManager.FindByIdAsync(editarRolDTO.UserId);

        //    //buscamos el usuario en la tabla aspnetuserrol 
        //    var existe = await context.UserRoles.AnyAsync(x => x.UserId == usuario.Id);

        //    //sino exite el usuario en la tabla aspnetuserrol lo agregamos
        //    if (!existe)
        //    {
        //        await userManager.AddToRoleAsync(usuario, editarRolDTO.RoleId);//agregamos un rol a un usuario

        //        //esto es para ponerle el tipousuario al empleado en base al rol seleccionado
        //        await AsignarTipodeUsuario(editarRolDTO);
        //    }
        //    else
        //    {
        //        //si exite el usuario lo borramos directamente de la base de datos 
        //        await context.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM AspNetUserRoles WHERE UserId = {usuario.Id};");

        //        //y volvemos asignar el nuevo rol seleccionado
        //        await userManager.AddToRoleAsync(usuario, editarRolDTO.RoleId);

        //        //esto es para ponerle el tipousuario al empleado en base al rol seleccionado
        //        await AsignarTipodeUsuario(editarRolDTO);
        //    }          

        //    return NoContent();
        //}

        //[HttpPost("removerRol")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrador")]
        //public async Task<ActionResult> RemoverUsuarioRol(EditarRolDTO editarRolDTO)
        //{
        //    var usuario = await userManager.FindByIdAsync(editarRolDTO.UserId);
        //    await userManager.RemoveFromRoleAsync(usuario, editarRolDTO.RoleId);//eliminamos un rol de un usuario
        //    return NoContent();
        //}       

        [Route("Desactivar")]
        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult> PutDesactivar(UsuarioDTO usuarioDTO)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);

            //obtener el registro original usando el método FindAsync 
            var oldUsuario = await context.Users.FindAsync(usuarioDTO.UserId);

            //al momento de activar de nuevo el usuario el dto que se manda tiene estos campos null por eso antes de editar hay que llenarlos con datos
            if (usuarioDTO.Usuario == null)
            {
                usuarioDTO.Usuario = oldUsuario.UserName;
            }

            //las propiedades sin cambios se ignoran y solo los valores de cambios se incluyen en la consulta de actualización
            context.Entry(oldUsuario).CurrentValues.SetValues(usuarioDTO);

            await context.SaveChangesAsync(user.Id);
            return NoContent();
        }

        //public async Task AsignarTipodeUsuario(EditarRolDTO editarRolDTO)
        //{
        //    //lo buscamos por su id con findbyid
        //    var usuario = await userManager.FindByIdAsync(editarRolDTO.UserId);                      

        //    if(editarRolDTO.RoleId == "Administrador")
        //    {
        //        usuario.TipodeUsuarios = TipodeUsuario.Administrador;
        //    }
        //    else 
        //    {
        //        usuario.TipodeUsuarios = TipodeUsuario.Usuario;
        //    }           

        //    //obtener el registro original usando el método FindAsync 
        //    var oldusuario = await context.Users.FindAsync(usuario.Id);

        //    //las propiedades sin cambios se ignoran y solo los valores de cambios se incluyen en la consulta de actualización
        //    context.Entry(oldusuario).CurrentValues.SetValues(usuario);

        //    await context.SaveChangesAsync();
        //}

        //filtrar para ver los usuarios eliminados
        [Route("FiltroActivos")]
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<PaginadorGenerico<UsuarioDTO>>> Get(string buscar, Boolean filtro, int pagina, int registros_por_pagina = 10)
        {
            List<UsuarioDTO> usuario;
            PaginadorGenerico<UsuarioDTO> _PaginadorConceptos;

            ////////////////////////
            // FILTRO DE BÚSQUEDA //
            ////////////////////////         

            // Recuperamos los registros completos                 
            var user = await userManager.GetUserAsync(HttpContext.User);
            if (filtro == true)
            {
                usuario = context.Users.Where(x => x.Activo == false && x.Id != user.Id).Select(x => new UsuarioDTO { Usuario = x.UserName, UserId = x.Id }).ToList();//hacemos un mapeo hacia usuariodto  
            }
            else
            {
                usuario = context.Users.Where(x => x.Activo == true && x.Id != user.Id).Select(x => new UsuarioDTO { Usuario = x.UserName, UserId = x.Id }).ToList();//hacemos un mapeo hacia usuariodto  
                //_pro = await context.Proveedores.ToListAsync();
            }

            //Filtramos el resultado por el 'dropdown'
            //if (!string.IsNullOrEmpty(buscar))
            //{
            //    foreach (var item in buscar.Split(new char[] { ' ' },
            //             StringSplitOptions.RemoveEmptyEntries))
            //    {
            //        _pro = _pro.Where(x => x.Tipo.ToString().Contains(item) ||
            //                                      x.Descripcion.Contains(item) ||
            //                                      x.Valor.Contains(item))
            //                                      .ToList();
            //    }
            //}

            ///////////////////////////
            // SISTEMA DE PAGINACIÓN //
            ///////////////////////////
            ///
            int _TotalRegistros = 0;
            int _TotalPaginas = 0;

            // Número total de registros de la coleccion 
            _TotalRegistros = usuario.Count();
            // Obtenemos la 'página de registros' de la coleccion 
            usuario = usuario.Skip((pagina - 1) * registros_por_pagina)
                                             .Take(registros_por_pagina)
                                             .ToList();
            // Número total de páginas de la coleccion
            _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / registros_por_pagina);

            //Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
            _PaginadorConceptos = new PaginadorGenerico<UsuarioDTO>()
            {
                RegistrosPorPagina = registros_por_pagina,
                TotalRegistros = _TotalRegistros,
                TotalPaginas = _TotalPaginas,
                PaginaActual = pagina,

                BusquedaActual = buscar,
                Resultado = usuario
            };
            return _PaginadorConceptos;
        }
    }
}
