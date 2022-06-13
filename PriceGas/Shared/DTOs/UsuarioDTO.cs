using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceGas.Shared.DTOs
{
    public class UsuarioDTO
    {
        public string UserId { get; set; }
        public string Usuario { get; set; }    
        public bool Activo { get; set; }//se puso para cambiar el estado y no borrar de la bd el usuario
        public string Contraseña { get; set; }//se puso para poder actualizar la contraseña en texto plano de la tabla aspnetuser
    }
}
