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
using AutoMapper;

namespace PriceGas.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrador,Usuario")]
    public class QuizController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper mapper;
        public QuizController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            this.context = context;
            _userManager = userManager;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<int>> Post(Quiz quiz)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            quiz.Activo = true;

            foreach (var item in quiz.LisadePreguntas)
            {
                item.FechaRegistro = DateTime.Now;
                item.Activo = true;

                if (string.IsNullOrEmpty(item.NombrePregunta))
                {
                    string mensajeError = "Ingresa una pregunta";
                    return BadRequest(mensajeError);
                }
            }

            context.Add(quiz);           
            await context.SaveChangesAsync(user.Id);
            return quiz.QuizId;
        }

        [HttpGet]
        public async Task<ActionResult<List<Quiz>>> Get([FromQuery] PaginacionDTO paginacion)
        {
            var queryable = context.Quizzes.Where(x => x.Activo == true).AsQueryable();

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
        public async Task<ActionResult<Quiz>> Get(int id)
        {
            var quiz = await context.Quizzes.Where(x => x.QuizId == id)
               .Include(x => x.LisadePreguntas).FirstOrDefaultAsync();
            if (quiz == null) { return NotFound(); }
            return quiz;
        }      

        [HttpPut]
        public async Task<ActionResult> Put(Quiz quiz)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var QuizDB = await context.Quizzes.FirstOrDefaultAsync(x => x.QuizId == quiz.QuizId);

            if (QuizDB == null) { return NotFound(); }         

            //evitamos el mapeo automatico y lo hacemos de manera manual para no estar actualizando la foto sino es necesario
            if (!string.IsNullOrWhiteSpace(quiz.Imagen))
            {
                QuizDB.Imagen = quiz.Imagen;
            }

            //actualizamos las preguntas
            foreach (var pregunta in quiz.LisadePreguntas)
            {
                if (pregunta.PreguntaId != 0)
                {
                    context.Entry(pregunta).State = EntityState.Modified;
                }
                else
                {
                    if (string.IsNullOrEmpty(pregunta.NombrePregunta))
                    {
                        string mensajeError = "Ingresa una pregunta";
                        return BadRequest(mensajeError);
                    }
                    pregunta.FechaRegistro = DateTime.Now;
                    pregunta.Activo = true;
                    context.Entry(pregunta).State = EntityState.Added;
                }

                ////activar todas las preguntas
                //pregunta.Activo = true;
                //var oldPregunta = await context.Preguntas.FindAsync(pregunta.PreguntaId);
                //context.Entry(oldPregunta).CurrentValues.SetValues(pregunta);
                //await context.SaveChangesAsync();

                //buscar las respuestas que pertenecen a las preguntas
                var respuestas = context.Respuestas.Where(x => x.PreguntaId == pregunta.PreguntaId).ToList();

                //recorre la lista de respuestas para asignarle el valor de false y editarlo
                foreach (var item2 in respuestas)
                {
                    item2.Activo = true;
                    var oldRespuesta = await context.Respuestas.FindAsync(item2.RespuestaId);
                    context.Entry(oldRespuesta).CurrentValues.SetValues(item2);
                    await context.SaveChangesAsync();
                }
            }

            //el mapeo se hace hasta el final por que despues no toma en cuenta el if (pregunta.PreguntaId != 0) y no entra al else donde se itene el EntityState.Added, por que el mapeo ya inserto una pregunta y el id ya es diferente de 0 si se pone antes del if 
            QuizDB = mapper.Map(quiz, QuizDB);
            await context.SaveChangesAsync(user.Id);
            return NoContent();
        }

        [Route("Desactivar")]
        [HttpPut]
        public async Task<ActionResult> PutDesactivar(Quiz quiz)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            //se tiene que ir a buscar el quiz e incluirle la data de la pregunta por que el que viene de la peticion post no la trae
            var quizparaeditarpregunta = await context.Quizzes.Where(x => x.QuizId == quiz.QuizId)
               .Include(x => x.LisadePreguntas).FirstOrDefaultAsync();          

            //si hay una pregunta entra al if para editar el campo activo
            if (quizparaeditarpregunta.LisadePreguntas.Count > 0)
            {
                //busca en la tabla preguntas las preguntas que pertenecen al quiz
                var preguntas = context.Preguntas
                    .Include(x => x.Quiz)
                    .Where(x => x.Quiz.QuizId == quiz.QuizId).ToList();

                //recorre la lista de preguntas para asignarle el valor de false y editarlo
                foreach (var item in preguntas)
                {
                    item.Activo = false;
                    var oldPregunta = await context.Preguntas.FindAsync(item.PreguntaId);
                    context.Entry(oldPregunta).CurrentValues.SetValues(item);
                    await context.SaveChangesAsync();

                    //buscar las respuestas que pertenecen a las preguntas
                    var respuestas = context.Respuestas.Where(x => x.PreguntaId == item.PreguntaId).ToList();

                    //recorre la lista de respuestas para asignarle el valor de false y editarlo
                    foreach (var item2 in respuestas)
                    {
                        item2.Activo = false;
                        var oldRespuesta = await context.Respuestas.FindAsync(item2.RespuestaId);
                        context.Entry(oldRespuesta).CurrentValues.SetValues(item2);
                        await context.SaveChangesAsync();
                    }
                }               
            }

            //obtener el registro original usando el método FindAsync 
            var oldQuiz = await context.Quizzes.FindAsync(quiz.QuizId);

            //las propiedades sin cambios se ignoran y solo los valores de cambios se incluyen en la consulta de actualización
            context.Entry(oldQuiz).CurrentValues.SetValues(quiz);

            await context.SaveChangesAsync(user.Id);
            return NoContent();
        }

        ///////////////////////////////////
        // APIS PARA FILTROS DE BÚSQUEDA //
        ////////////////////////////////// 

        //filtrar para ver los quiz eliminados
        [Route("FiltroActivos")]
        [HttpGet]
        public async Task<ActionResult<PaginadorGenerico<Quiz>>> Get(string buscar, Boolean filtro, int pagina, int registros_por_pagina = 10)
        {
            List<Quiz> quiz;
            PaginadorGenerico<Quiz> _PaginadorConceptos;

            ////////////////////////
            // FILTRO DE BÚSQUEDA //
            ////////////////////////         

            // Recuperamos los registros completos                 

            if (filtro == true)
            {
                quiz = context.Quizzes.Where(x => x.Activo == false).ToList();
            }
            else
            {
                quiz = context.Quizzes.Where(x => x.Activo == true).ToList();
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
            _TotalRegistros = quiz.Count();
            // Obtenemos la 'página de registros' de la coleccion 
            quiz = quiz.Skip((pagina - 1) * registros_por_pagina)
                                             .Take(registros_por_pagina)
                                             .ToList();
            // Número total de páginas de la coleccion
            _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / registros_por_pagina);

            //Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
            _PaginadorConceptos = new PaginadorGenerico<Quiz>()
            {
                RegistrosPorPagina = registros_por_pagina,
                TotalRegistros = _TotalRegistros,
                TotalPaginas = _TotalPaginas,
                PaginaActual = pagina,

                BusquedaActual = buscar,
                Resultado = quiz
            };
            return _PaginadorConceptos;
        }

        //buscar deptos para filtro
        [HttpGet("buscar/{textoBusqueda}")]
        public async Task<ActionResult<List<Quiz>>> GetQuiz(string textoBusqueda)
        {
            if (textoBusqueda.Length > 3)
            {
                if (string.IsNullOrWhiteSpace(textoBusqueda)) { return new List<Quiz>(); }
                textoBusqueda = textoBusqueda.ToLower();
                return await context.Quizzes.Where(x => x.Nombre.ToLower().Contains(textoBusqueda)).Take(25).ToListAsync();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(textoBusqueda)) { return new List<Quiz>(); }
                textoBusqueda = textoBusqueda.ToLower();
                return await context.Quizzes.Where(x => x.Nombre.ToLower().Contains(textoBusqueda)).Take(5).ToListAsync();
            }
        }
    }
}
