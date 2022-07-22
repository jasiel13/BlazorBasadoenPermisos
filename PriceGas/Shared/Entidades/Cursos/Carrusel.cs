using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceGas.Shared.Entidades.Cursos
{
    //relacion uno a muchos
    public class Carrusel
    {
        public int CarruselId { get; set; }
        public string Descripcion { get; set; }
        public LugardeVisualizacion LugardeVisualizacion { get; set; }
        public bool Mostrar { get; set; } 
        public List<ImagenesCarrusel> Imagenes { get; set; } //propiedad de navegacion
    }
    public class ImagenesCarrusel
    {
        public int ImagenesCarruselId { get; set; }
        public string Imagen { get; set; }     
        public int CarruselId { get; set; }  //propiedad de navegacion   
    }
    public enum LugardeVisualizacion
    {
        [Description("Pantalla Principal")]
        PantallaPrincipal,       
    }
}
