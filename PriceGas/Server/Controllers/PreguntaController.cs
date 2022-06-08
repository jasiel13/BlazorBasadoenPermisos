using PriceGas.Server.Datos;
using PriceGas.Server.Helpers;
using PriceGas.Shared.DTOs;
using PriceGas.Shared.Entidades;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using PriceGas.Shared.Entidades.Cursos;

namespace PriceGas.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrador,Usuario")]
    public class PreguntaController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> _userManager;
        public PreguntaController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            _userManager = userManager;
        }

        [Route("Listado/{id}")]
        [HttpGet]
        public async Task<ActionResult<PaginadorGenerico<Pregunta>>> Get(int id, string buscar, Boolean filtro, int pagina, int registros_por_pagina = 10)
        {
            PaginadorGenerico<Pregunta> _PaginadorConceptos;

            var pregunta = await context.Preguntas
                .Where(x => x.Quiz.QuizId == id && x.Activo == true)
                .OrderBy(x=>x.FechaRegistro)
                .ToListAsync();           

            ///////////////////////////
            // SISTEMA DE PAGINACIÓN //
            ///////////////////////////
            ///
            int _TotalRegistros = 0;
            int _TotalPaginas = 0;

            // Número total de registros de la coleccion 
            _TotalRegistros = pregunta.Count();
            // Obtenemos la 'página de registros' de la coleccion 
            pregunta = pregunta.Skip((pagina - 1) * registros_por_pagina)
                                             .Take(registros_por_pagina)
                                             .ToList();
            // Número total de páginas de la coleccion
            _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / registros_por_pagina);

            //Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
            _PaginadorConceptos = new PaginadorGenerico<Pregunta>()
            {
                RegistrosPorPagina = registros_por_pagina,
                TotalRegistros = _TotalRegistros,
                TotalPaginas = _TotalPaginas,
                PaginaActual = pagina,

                BusquedaActual = buscar,
                Resultado = pregunta
            };
            return _PaginadorConceptos;
        }

        [HttpPost]
        public async Task<ActionResult<int>> Post(List<Respuesta> listarespuestas)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            int Id = 0;//se tuvo que poner una variable id global para poder retonar algo ya que el return de respuesta esta adentro del if y no tiene alcance fuera de el            
           
            foreach (var respuesta in listarespuestas)
            {
                respuesta.Activo = true;
                respuesta.FechaRegistro = DateTime.Now;               

                context.Add(respuesta);
                await context.SaveChangesAsync(user.Id);
                Id = respuesta.RespuestaId;
            }
            return Id;
        }

        ///////////////////////////////////
        // APIS PARA FILTROS DE BÚSQUEDA //
        //////////////////////////////////  

        [HttpGet]
        public async Task<ActionResult<List<Respuesta>>> Get([FromQuery] PaginacionDTO paginacion)
        {
            var queryable = context.Respuestas.Where(x => x.Activo == true)
                .Include(x=>x.Pregunta)
                .AsQueryable();
            await HttpContext.InsertarParametrosPaginacionEnRespuesta(queryable, paginacion.CantidadRegistros);
            return await queryable.Paginar(paginacion).ToListAsync();
        }

        //buscar respuestas 
        [HttpGet("buscar/{textoBusqueda}")]
        public async Task<ActionResult<List<Respuesta>>> GetRes(string textoBusqueda)
        {
            if (textoBusqueda.Length > 3)
            {
                if (string.IsNullOrWhiteSpace(textoBusqueda)) { return new List<Respuesta>(); }
                textoBusqueda = textoBusqueda.ToLower();
                //se limpio la lista ya que me daria resultado de todos los campos con el nombre buscado el problema es que todos tienen el mismo nombre
                var listaprincipal = await context.Respuestas.Where(x => x.NombreUsuario.ToLower().Contains(textoBusqueda)).Take(25).ToListAsync();
                var listaARetornar = listaprincipal.GroupBy(x => x.NombreUsuario).Select(x => x.First()).ToList();
                return listaARetornar;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(textoBusqueda)) { return new List<Respuesta>(); }
                textoBusqueda = textoBusqueda.ToLower();
                var listaprincipal = await context.Respuestas.Where(x => x.NombreUsuario.ToLower().Contains(textoBusqueda)).Take(5).ToListAsync();
                var listaARetornar = listaprincipal.GroupBy(x => x.NombreUsuario).Select(x => x.First()).ToList();
                return listaARetornar;
            }
        }

        //filtrar por respuesta seleccionada       
        [HttpGet("FiltroRespuesta/{textonombre}")]
        public async Task<ActionResult<List<Respuesta>>> Filtro(string textonombre)
        {
            var respuestas = context.Respuestas
                .Where(x => x.NombreUsuario == textonombre && x.Activo == true)
                .Include(x => x.Pregunta)
                .ToList();
            
            return respuestas;
        }

        //filtrar por quiz seleccionado 
        [Route("FiltroQuiz/{id}")]
        [HttpGet]
        public async Task<ActionResult<List<Respuesta>>> FiltroQuiz(int id)
        {
            var preguntas = context.Preguntas.Where(x => x.Quiz.QuizId == id && x.Activo == true).ToList();

            List<Respuesta> listaderespuestas = new List<Respuesta>();

            foreach(var item in preguntas)
            {
                var respuesta = context.Respuestas.Where(x => x.PreguntaId == item.PreguntaId).FirstOrDefault();
                if(respuesta != null)
                {
                   listaderespuestas.Add(respuesta);
                }              
            }

            return listaderespuestas;
        }
    }
}
