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
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrador,Usuario")]
    public class TemaController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAlmacenadorArchivos almacenadorDeArchivos;
        private readonly IMapper mapper;
        public TemaController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IAlmacenadorArchivos almacenadorDeArchivos, IMapper mapper)
        {
            this.context = context;
            _userManager = userManager;
            this.almacenadorDeArchivos = almacenadorDeArchivos;
            this.mapper = mapper;
        }        
     
        [HttpPost]
        [DisableRequestSizeLimit]//permite subir archivos superiores a 30000 30MB
        //[RequestSizeLimit(52428800)]//limite 50MB
        [RequestSizeLimit(524288000)]//limite 500MB
        public async Task<ActionResult<int>> Post(Tema tema)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            tema.Activo = true;
            //tema.CursoId = tema.Curso.CursoId;
            //tema.Curso = null;          

            context.Add(tema);
            await context.SaveChangesAsync(user.Id);
            return tema.TemaId;
        }        
      
        [Route("Listado/{id}")]
        [HttpGet]
        public async Task<ActionResult<PaginadorGenerico<Tema>>> Get(int id, string buscar, Boolean filtro, int pagina, int registros_por_pagina = 10)
        {           
            PaginadorGenerico<Tema> _PaginadorConceptos;

            var temas = await context.Temas.Where(x => x.CursoId == id && x.Activo == true).ToListAsync();

            foreach (var item in temas)
            {
                //si el usuario no subio imagen poner una por defecto
                if (string.IsNullOrEmpty(item.Imagen))
                {
                    // Aquí colocas la URL de la imagen por defecto
                    item.Imagen = "Img" + "/" + "Imagenotfound.jpg";
                }
            }

            ///////////////////////////
            // SISTEMA DE PAGINACIÓN //
            ///////////////////////////
            ///
            int _TotalRegistros = 0;
            int _TotalPaginas = 0;

            // Número total de registros de la coleccion 
            _TotalRegistros = temas.Count();
            // Obtenemos la 'página de registros' de la coleccion 
            temas = temas.Skip((pagina - 1) * registros_por_pagina)
                                             .Take(registros_por_pagina)
                                             .ToList();
            // Número total de páginas de la coleccion
            _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / registros_por_pagina);

            //Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
            _PaginadorConceptos = new PaginadorGenerico<Tema>()
            {
                RegistrosPorPagina = registros_por_pagina,
                TotalRegistros = _TotalRegistros,
                TotalPaginas = _TotalPaginas,
                PaginaActual = pagina,

                BusquedaActual = buscar,
                Resultado = temas
            };
            return _PaginadorConceptos;
        }      
              
        [HttpGet("{id}")]
        public async Task<ActionResult<Tema>> Get(int id)
        {
            var tema = await context.Temas.Where(x => x.TemaId == id).FirstOrDefaultAsync();
            if (tema == null) { return NotFound(); }

            //si el usuario no subio imagen poner una por defecto
            if (string.IsNullOrEmpty(tema.Imagen))
            {
                // Aquí colocas la URL de la imagen por defecto
                tema.Imagen = "Img" + "/" + "Imagenotfound.jpg";
            }

            return tema;
        }

        [HttpPut]
        public async Task<ActionResult> Put(Tema tema)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var oldtema = await context.Temas.FindAsync(tema.TemaId);

            if(tema.Curso != null)
            {
                tema.CursoId = tema.Curso.CursoId;
                tema.Curso = null;
            }          

            if (string.IsNullOrWhiteSpace(tema.Imagen))
            {
               tema.Imagen = oldtema.Imagen;
            }

            context.Entry(oldtema).CurrentValues.SetValues(tema);

            await context.SaveChangesAsync(user.Id);
            return NoContent();
        }

        [Route("Desactivar")]
        [HttpPut]
        public async Task<ActionResult> PutDesactivar(Tema tema)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            //obtener el registro original usando el método FindAsync 
            var oldtema = await context.Temas.FindAsync(tema.TemaId);

            //las propiedades sin cambios se ignoran y solo los valores de cambios se incluyen en la consulta de actualización
            context.Entry(oldtema).CurrentValues.SetValues(tema);

            await context.SaveChangesAsync(user.Id);
            return NoContent();
        }

        ///////////////////////////////////
        // APIS PARA FILTROS DE BÚSQUEDA //
        //////////////////////////////////  

        //filtrar para ver los temas eliminados
        [Route("FiltroActivos")]
        [HttpGet]
        public async Task<ActionResult<PaginadorGenerico<Tema>>> Get(string buscar, Boolean filtro, int pagina, int registros_por_pagina = 10)
        {
            List<Tema> tema;
            PaginadorGenerico<Tema> _PaginadorConceptos;

            ////////////////////////
            // FILTRO DE BÚSQUEDA //
            ////////////////////////         

            // Recuperamos los registros completos                 

            if (filtro == true)
            {
                tema = context.Temas.Where(x => x.Activo == false).ToList();
            }
            else
            {
                tema = context.Temas.Where(x => x.Activo == true).ToList();
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
            _TotalRegistros = tema.Count();
            // Obtenemos la 'página de registros' de la coleccion 
            tema = tema.Skip((pagina - 1) * registros_por_pagina)
                                             .Take(registros_por_pagina)
                                             .ToList();
            // Número total de páginas de la coleccion
            _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / registros_por_pagina);

            //Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
            _PaginadorConceptos = new PaginadorGenerico<Tema>()
            {
                RegistrosPorPagina = registros_por_pagina,
                TotalRegistros = _TotalRegistros,
                TotalPaginas = _TotalPaginas,
                PaginaActual = pagina,

                BusquedaActual = buscar,
                Resultado = tema
            };
            return _PaginadorConceptos;
        }
    }
}
