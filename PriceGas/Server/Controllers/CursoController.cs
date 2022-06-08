using AutoMapper;
using PriceGas.Server.Datos;
using PriceGas.Server.Helpers;
using PriceGas.Shared.Entidades;
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
using PriceGas.Shared.Entidades.Cursos;
using PriceGas.Shared.DTOs;

namespace PriceGas.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrador,Usuario")]
    public class CursoController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAlmacenadorArchivos almacenadorDeArchivos;
        private readonly IMapper mapper;
        public CursoController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IAlmacenadorArchivos almacenadorDeArchivos, IMapper mapper)
        {
            this.context = context;
            _userManager = userManager;
            this.almacenadorDeArchivos = almacenadorDeArchivos;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<int>> Post(Curso curso)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            curso.Activo = true;          

            context.Add(curso);
            await context.SaveChangesAsync(user.Id);
            return curso.CursoId;
        }

        [HttpGet]
        public async Task<ActionResult<List<Curso>>> Get([FromQuery] PaginacionDTO paginacion)
        {
            var queryable = context.Cursos.Where(x => x.Activo == true).AsQueryable();

            foreach (var item in queryable)
            {
                //si el usuario no subio imagen poner una por defecto
                if (string.IsNullOrEmpty(item.Imagen))
                {
                    // Aquí colocas la URL de la imagen por defecto
                    item.Imagen = "Img" + "/" + "Imagenotfound.jpg";
                }
            }

            await HttpContext.InsertarParametrosPaginacionEnRespuesta(queryable, paginacion.CantidadRegistros);
            return await queryable.Paginar(paginacion).ToListAsync();
        }
     
        [HttpGet("{id}")]
        public async Task<ActionResult<Curso>> Get(int id)
        {
            var curso = await context.Cursos.Where(x => x.CursoId == id)
               .Include(x => x.LisadeTemas).FirstOrDefaultAsync();
            if (curso == null) { return NotFound(); }
            return curso;
        }

        [HttpPut]
        public async Task<ActionResult> Put(Curso curso)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var oldcurso = await context.Cursos.FindAsync(curso.CursoId);            

            if (string.IsNullOrWhiteSpace(curso.Imagen))
            {
                curso.Imagen = oldcurso.Imagen;
            }

            context.Entry(oldcurso).CurrentValues.SetValues(curso);

            await context.SaveChangesAsync(user.Id);
            return NoContent();
        }

        [Route("Desactivar")]
        [HttpPut]
        public async Task<ActionResult> PutDesactivar(Curso curso)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            //obtener el registro original usando el método FindAsync 
            var oldcurso = await context.Cursos.FindAsync(curso.CursoId);

            //las propiedades sin cambios se ignoran y solo los valores de cambios se incluyen en la consulta de actualización
            context.Entry(oldcurso).CurrentValues.SetValues(curso);

            await context.SaveChangesAsync(user.Id);
            return NoContent();
        }

        ///////////////////////////////////
        // APIS PARA FILTROS DE BÚSQUEDA //
        //////////////////////////////////  

        //filtrar para ver los cursos eliminados
        [Route("FiltroActivos")]
        [HttpGet]
        public async Task<ActionResult<PaginadorGenerico<Curso>>> Get(string buscar, Boolean filtro, int pagina, int registros_por_pagina = 10)
        {
            List<Curso> curso;
            PaginadorGenerico<Curso> _PaginadorConceptos;

            ////////////////////////
            // FILTRO DE BÚSQUEDA //
            ////////////////////////         

            // Recuperamos los registros completos                 

            if (filtro == true)
            {
                curso = context.Cursos.Where(x => x.Activo == false).ToList();
            }
            else
            {
                curso = context.Cursos.Where(x => x.Activo == true).ToList();
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
            _TotalRegistros = curso.Count();
            // Obtenemos la 'página de registros' de la coleccion 
            curso = curso.Skip((pagina - 1) * registros_por_pagina)
                                             .Take(registros_por_pagina)
                                             .ToList();
            // Número total de páginas de la coleccion
            _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / registros_por_pagina);

            //Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
            _PaginadorConceptos = new PaginadorGenerico<Curso>()
            {
                RegistrosPorPagina = registros_por_pagina,
                TotalRegistros = _TotalRegistros,
                TotalPaginas = _TotalPaginas,
                PaginaActual = pagina,

                BusquedaActual = buscar,
                Resultado = curso
            };
            return _PaginadorConceptos;
        }

    }
}
