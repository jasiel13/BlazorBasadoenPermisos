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
        public CarruselController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
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
        public async Task<ActionResult<List<Carrusel>>> Get()
        {
            var listadeimagenes = await context.Carrusel.ToListAsync(); 
            return listadeimagenes;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Carrusel>> Get(int id)
        {
            var carrusel = await context.Carrusel.FirstOrDefaultAsync();
            if (carrusel == null) { return NotFound(); }
            return carrusel;
        }

        [HttpPut]
        public async Task<ActionResult> Put(Carrusel carrusel)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var oldcarrusel = await context.Carrusel.FindAsync(carrusel.CarruselId);

            if (string.IsNullOrWhiteSpace(carrusel.Imagen))
            {
                carrusel.Imagen = oldcarrusel.Imagen;
            }

            context.Entry(oldcarrusel).CurrentValues.SetValues(carrusel);

            await context.SaveChangesAsync(user.Id);
            return NoContent();
        }
    }
}
