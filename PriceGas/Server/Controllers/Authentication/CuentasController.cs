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
using PriceGas.Shared.Constants;
using PriceGas.Server.Helpers;

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
        //añadimos esto para poder usarlo en el metodo GetPermisoDto
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly ApplicationDbContext context;    

        public CuentasController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            this.context = context;
            _roleManager = roleManager;
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
                ContraseñaTextoPlano = model.Password,
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

            //instanciamos un nuevo permissiondto
            PermissionDTO permiso = new PermissionDTO();

            foreach (var rol in roles)
            {               
                //añadimos un claim por cada rol
                claims.Add(new Claim(ClaimTypes.Role, rol));

                //obtenemos un permissiondto por cada rol, el punto result se puso por que el metodo tiene un task
                permiso = GetPermisoDTO(rol).Result;              
            }


            /*NOTA:
             * -los permisos no se agregaban a los claim por eso no funcionaba, aqui es donde construimos los claim
             * -los claim van en el json web token que enviamos al ProveedorAutenticacionJWT
             * -en el ProveedorAutenticacionJWT construimos el calimPrincipal el cual va añadido al AuthenticationStateProvider
             * -el AuthenticationStateProvider es el que nos permitira poner la autorizacion basada en permisos del lado del cliente
             */


            //inicializamos una variable en 0 par aobtener el index de la iteracion
            int i = 0;

            if(permiso.RoleClaims != null && permiso.RoleClaims.Count > 0)
            {
                //recorremos la lista de permisos para ir creando un claim por cada permiso
                foreach (var item in permiso.RoleClaims)
                {
                    //solo vamos agregar los permisos que estan en true
                    if (item.Selected == true)
                    {
                        //creamos un nuevo claim de typo permiso, concatenamos al Type ("Permission") un numero ("Permission1")
                        //esto por que cada permiso debe ser un claim independiente y si todos son Type ("Permission") los mete en un array
                        //y si estan en un array en el autenticationstate no los puede leer y no funcionan los permisos del lado del cliente (error en la pagina ProveedorAutenticacionJWT)
                        var nuevoclaimpermiso = new Claim(item.Type + i, item.Value);
                        //añadimos el claim a la lista de claims
                        claims.Add(nuevoclaimpermiso);
                    }
                    i++;
                }
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
                await VerifyResetPassAsync(usuario.UserName,tokenreset,recoveryPassword);
            }
            return Ok();
        }       
        public async Task<ActionResult> VerifyResetPassAsync(string nombre, string token, RecoveryPassword recoveryPassword)
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
                    var resultado = await _userManager.ResetPasswordAsync(usuario,token, recoveryPassword.Password);
                    if (resultado.Succeeded)
                    {
                        //actualizar el campo contraseñatextoplano en la tabla aspnetuser cuando se restablezca la contraseña
                        var oldUsuario = await context.Users.FindAsync(usuario.Id);

                        //se tuvo que crear un nuevo objeto de ApplicationUser ya que el update solo se puede hacer entre mismos objetos
                        //si le pasamos recoverypassword con el password nuevo tecleado, da error ya que recoverypassword es un DTO
                        //el nuevo objeto de applicationuser lo igualamos con todo lo que trae el oldusuario, solo cambiamos el password
                        //que es el unico campo que cambio y que nos interesa cambiar
                        var nuevousuariosoloparaactualizar = new ApplicationUser();
                        nuevousuariosoloparaactualizar = oldUsuario;
                        nuevousuariosoloparaactualizar.ContraseñaTextoPlano = recoveryPassword.Password;
                        context.Entry(oldUsuario).CurrentValues.SetValues(nuevousuariosoloparaactualizar);
                        await context.SaveChangesAsync();

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

        //obtener la lista de permisos por rol
        public async Task<PermissionDTO> GetPermisoDTO(string nombrerol)
        {
            //instanciamos un nuevo permissiondto
            var model = new PermissionDTO();

            //instanciamos una nueva lista de roleclaimdto
            var allPermissions = new List<RoleClaimsDTO>();
            
            //buscamos el rol por su nombre
            var role = await _roleManager.FindByNameAsync(nombrerol);
         
            //obtenemos todos los permisos del que tiene un rol en especifico de tipo products
            allPermissions.GetPermissions(typeof(Permissions.Products), role.Id);

            //le pasamos el id del rol al permissiondto (no es necesario se puede omitir)
            model.RoleId = role.Id;

            //obtenemos los claims por rol especificado
            var claims = await _roleManager.GetClaimsAsync(role);
            var allClaimValues = allPermissions.Select(a => a.Value).ToList();
            var roleClaimValues = claims.Select(a => a.Value).ToList();
            var authorizedClaims = allClaimValues.Intersect(roleClaimValues).ToList();

            //recorremos los permisos y si el permiso fue seleccionado se pone en true su propiedad selected
            foreach (var permission in allPermissions)
            {
                if (authorizedClaims.Any(a => a == permission.Value))
                {
                    permission.Selected = true;
                }
            }

            //pasamos los permisos al permissiondto
            model.RoleClaims = allPermissions;

            //retornamos el dto
            return model;
        }
    }
}
