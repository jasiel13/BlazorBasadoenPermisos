using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceGas.Shared.Entidades.Cursos
{
    public class Quiz
    {
        public int QuizId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Descripcion { get; set; }
        public string Imagen { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public bool Activo { get; set; }       
        public List<Pregunta> LisadePreguntas { get; set; } = new List<Pregunta>();
        public string NombreCortado
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Nombre))
                {
                    return null;
                }

                if (Nombre.Length > 60)
                {
                    return Descripcion.Substring(0, 60) + "...";
                }
                else
                {
                    return Nombre;
                }
            }
        }
        public string DescripcionCortada
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Descripcion))
                {
                    return null;
                }

                if (Descripcion.Length > 60)
                {
                    return Descripcion.Substring(0, 60) + "...";
                }
                else
                {
                    return Descripcion;
                }
            }
        }
    }
    public class Pregunta
    {
        public int PreguntaId { get; set; }
        public string NombrePregunta { get; set; }        
        public DateTime? FechaRegistro { get; set; }
        public bool Activo { get; set; }

        //propiedades de navegacion
        public virtual Quiz Quiz { get; set; }        
    }
    public class Respuesta
    {
        public int RespuestaId { get; set; }
        public string NombreRespuesta { get; set; }
        public string NombreUsuario { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public bool Activo { get; set; }
        public int? PreguntaId { get; set; }
        public virtual Pregunta Pregunta { get; set; }
    }
}
