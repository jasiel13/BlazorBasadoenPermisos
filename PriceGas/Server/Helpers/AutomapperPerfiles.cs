using AutoMapper;
using PriceGas.Shared.Entidades;
using PriceGas.Shared.Entidades.Cursos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriceGas.Server.Helpers
{
    public class AutomapperPerfiles : Profile
    {
        //atravez de los perfiles es que automaper puede definir las reglas de mapeo,creamos un constructor      
        public AutomapperPerfiles()
        {                  
            CreateMap<Quiz, Quiz>().ForMember(x => x.Imagen, option => option.Ignore());

            CreateMap<Carrusel, Carrusel>().ForMember(x => x.Imagenes, option => option.Ignore());           
        }
    }
}
