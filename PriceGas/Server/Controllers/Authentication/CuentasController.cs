using PriceGas.Server.Datos;
using PriceGas.Shared.DTOs;
using PriceGas.Shared.Entidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PriceGas.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CuentasController : ControllerBase
    {
        //creamos nuevos usarios
        private readonly UserManager<ApplicationUser> _userManager;
        //con esto el usuario se va a poder logear
        private readonly SignInManager<ApplicationUser> _signInManager;
        //conesto podemos buscar la llave jwt
        private readonly IConfiguration _configuration;

        private readonly ApplicationDbContext context;

        public CuentasController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            this.context = context;
        }

        //creamos un controlador 
        [HttpPost("Crear")]
        //creamos un usuario al cual le pasamos un token y el modelo que contiene la informacion del usuario
        public async Task<ActionResult<UserToken>> CreateUser([FromBody] UserInfo model)
        {
            //creamos una instancia de identityuser y usamos el metodo createasync 
            var user = new ApplicationUser
            {
                UserName = model.Usuario,
                //Email = model.Usuario,                
                Activo = true
            };         

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {               
                //si el resultado es exitoso creamos el token
                return BuildToken(user, new List<string>());
            }
            else
            {
                //si el resultado no es valido mostramos un mensaje de que el usuario o contraseña no es valido
                return BadRequest("La contraseña no es válida o el usuario ya existe");
            }
        }

        //creamos un controlador para el login
        [HttpPost("Login")]
        //le pasamos un modelo de userinfoLogin
        public async Task<ActionResult<UserToken>> Login([FromBody] UserInfoLogin userInfoLogin)
        {
            //buscamos el usuario en la tabla aspnetuser por su username mandado en el formulario de login
            var usuarioexite = await _userManager.FindByNameAsync(userInfoLogin.Usuario);

            //utlizamos el singmanager para hacer un sing con password, le pasamos el nombredelusuario y si es persistente
            var result = await _signInManager.PasswordSignInAsync(userInfoLogin.Usuario,
                userInfoLogin.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                //una vez encontrado si el usuario esta activo iniciar sesion, sino esta activo mandar mensaje de error
                if (usuarioexite.Activo == true)
                {
                    //con esto obtenemos un listado de roles
                    var usuario = await _userManager.FindByNameAsync(userInfoLogin.Usuario);
                    var roles = await _userManager.GetRolesAsync(usuario);
                    //si es existoso construimos el token y le pasamos el listado de roles de arriba
                    return BuildToken(usuario, roles);
                }
                else
                {
                    //sino es esta activo mandamos un mensaje de error
                    return BadRequest("El usuario fue dado de baja");
                }
            }
            else
            {
                //sino es existoso mandamos un mensaje de login incorrecto
                return BadRequest("El usuario o contraseña es inválido");
            }
        }

        //creamos el metodo buildtoken que recibe como parametro un modelo userinfo el cual contiene usuario y contraseña
        private UserToken BuildToken(ApplicationUser user, IList<string> roles)
        {
            //creamos un claim es una infomracion en la cual podemos confiar ya que la creamos desde la webapi
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(ClaimTypes.Name, user.UserName),//nombre con el que se registro del usuario              
                new Claim("miValor", "Lo que yo quiera"),//informacion que tu quieras  
                new Claim(ClaimTypes.NameIdentifier, user.Id),//recuperamos el id del usuario logueado
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())//identificador para identificar un webtoken en particular
             };

            foreach (var rol in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, rol));
            }

            //creamos una instancia con la llave simetrica de seguridad
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //creamos una variable para controlar la fecha de expiracion del token en esta caso sera de 3 meses
            var expiration = DateTime.UtcNow.AddMonths(3);

            //creamos la estructura del token
            JwtSecurityToken token = new JwtSecurityToken(
               issuer: null,
               audience: null,
               claims: claims,
               expires: expiration,
               signingCredentials: creds);

            //retornamos el usuariotoken y el tiempo de expiracion
            return new UserToken()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }

        ///////////////////////////////////
        // RESTABLECER CONTRASEÑA ////////
        ////////////////////////////////// 

        [HttpPost("OlvidoPassword")]
        public async Task<ActionResult> OlvidoPassword([FromBody] RecoveryPassword recoveryPassword)
        {
            //buscamos un usuario por medio de su UserName
            var usuario = await _userManager.FindByNameAsync(recoveryPassword.Usuario);

            //si el usuario no es nulo 
            if (usuario != null)
            {
                //creamos un token para resetear su password
                var tokenreset = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                //pasamos al metodo de verificacion
                await VerifyResetPassAsync(usuario.UserName,tokenreset,recoveryPassword.Password);
            }
            return Ok();
        }       
        public async Task<ActionResult> VerifyResetPassAsync(string nombre, string token, string pass)
        {
            if (nombre == null || token == null)
                return Content("Faltan datos para restablecer contraseña");

            //buscamos el usuario por su UserName
            var user = await _userManager.FindByNameAsync(nombre);

            if (user == null)
                return Content("Usuario no encontrado");

            //verificamos el token           
            bool ConfirmarToken = await _userManager.VerifyUserTokenAsync
            (user, this._userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", token);

            //si es un token valido restableces la contraseña
            if (ConfirmarToken)
            {               
                //codigo para restablecer contraseña
                var usuario = await _userManager.FindByNameAsync(nombre);//buscamos el usuario por su UserName
                if (usuario != null)
                {
                    var resultado = await _userManager.ResetPasswordAsync(usuario,token,pass);
                    if (resultado.Succeeded)
                    {
                        return Ok("Se reestablecio correctamente");
                    }
                    else
                    {
                        string mensajeError = "La contraseña no se ha podido cambiar";
                        return BadRequest(mensajeError);
                    }
                }
                return NotFound("No se encontro el usuario");
            }
            else
            {
                return Content("Token de verificación no válido");
            }
        }       
    }
}
