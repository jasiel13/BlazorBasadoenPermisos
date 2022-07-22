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
    public class CarruselController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper mapper;
        public CarruselController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
            _userManager = userManager;          
        }

        [HttpPost]
        public async Task<ActionResult<int>> Post(Carrusel carrusel)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);           
            context.Add(carrusel);
            await context.SaveChangesAsync(user.Id);
            return carrusel.CarruselId;
        }

        [HttpGet]
        public async Task<ActionResult<List<Carrusel>>> Get([FromQuery] PaginacionDTO paginacion)
        {
            var queryable = context.Carrusel
                //.Include(x => x.Imagenes)               
                .AsQueryable();
            await HttpContext.InsertarParametrosPaginacionEnRespuesta(queryable, paginacion.CantidadRegistros);
            return await queryable.Paginar(paginacion).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Carrusel>> Get(int id)
        {
            var carrusel = await context.Carrusel.Where(x=>x.CarruselId == id)
                .Include(x => x.Imagenes).FirstOrDefaultAsync();
            if (carrusel == null) { return NotFound(); }
            return carrusel;
        }

        [Route("Mostrar/{lugar}")]
        [HttpGet]
        public async Task<ActionResult<List<Carrusel>>> GetMostar(LugardeVisualizacion lugar)
        {
            //validar que sean enteros de la enumeracion
            //Type enumType = lugar.GetType();
            //bool isEnumValid = Enum.IsDefined(enumType, lugar);
            //if (!isEnumValid)
            //{
            //    throw new Exception("...");
            //}

            var carrusel = await context.Carrusel.Where(x => x.LugardeVisualizacion == lugar && x.Mostrar == true)
                .Include(x => x.Imagenes).ToListAsync();
            if (carrusel == null) { return NotFound(); }
            return carrusel;
        }

        [HttpPut]
        public async Task<ActionResult> Put(Carrusel carrusel)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            
            var carruselDB = await context.Carrusel
                .Include(x => x.Imagenes)
                .FirstOrDefaultAsync(x => x.CarruselId == carrusel.CarruselId);
            if (carruselDB == null) { return NotFound(); }           

            carruselDB = mapper.Map(carrusel, carruselDB);

            //recorremos las imagenes 
            foreach (var item in carrusel.Imagenes)
            {
                //si viene una imagen el usuario quiso editarla sino viene el usuario no quiso editar la imagen solo lo demas, y aplicamos el ignore de automapper
                if (!string.IsNullOrWhiteSpace(item.Imagen))
                {                    
                    carruselDB.Imagenes = carrusel.Imagenes;
                }
            }
            await context.SaveChangesAsync(user.Id);
            return NoContent();
        }
    }
}
