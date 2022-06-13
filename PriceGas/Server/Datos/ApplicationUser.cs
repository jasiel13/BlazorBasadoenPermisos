using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriceGas.Server.Datos
{
    public class ApplicationUser : IdentityUser
    {     
        public TipodeUsuario TipodeUsuarios { get; set; }
        public bool Activo { get; set; }
        public string ContraseñaTextoPlano { get; set; }//poder ver la contraseña sin hash

        //aqui podemos agregar mas campos para el usuario identity
        public enum TipodeUsuario
        {
            Ninguno,
            Administrador, //Administrador del sistema tiene todos los privilegios y permisos
            Usuario,//Usuario del sistema, tiene menos privilegios que el administrador            
        }
    }
}
