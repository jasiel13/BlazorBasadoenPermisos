using PriceGas.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceGas.Shared.DTOs
{
    public class UserInfo
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]       
        public string Usuario { get; set; }

        [Required(ErrorMessage = "El campo contraseña es requerido")]
        [CustomPasswordValidator]
        public string Password { get; set; }     
    }
    public class UserInfoLogin
    {
        /*con estas propiedades el usuario se va a poder loguear pero se separaron de userinfo
        ya que userinfo contiene mas porpiedades que no son necesarias para loguearse*/
        [Required(ErrorMessage = "El campo {0} es requerido")]        
        public string Usuario { get; set; }

        [Required(ErrorMessage = "El campo contraseña es requerido")]
        public string Password { get; set; }
    }
    public class RecoveryPassword
    {
        /*con estas propiedades el usuario va poder recuperar su contraseña*/             
        public string Usuario { get; set; }

        [Required(ErrorMessage = "El campo contraseña es requerido")]
        [CustomPasswordValidator]
        public string Password { get; set; }      
        public string Token { get; set; }
    }    
}
