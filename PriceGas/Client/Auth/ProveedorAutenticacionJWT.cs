﻿using PriceGas.Client.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using PriceGas.Shared.Entidades;

namespace PriceGas.Client.Auth
{
    public class ProveedorAutenticacionJWT : AuthenticationStateProvider, ILoginService
    {   //inyectamos jsruntime
        public ProveedorAutenticacionJWT(IJSRuntime js, HttpClient httpClient)
        {
            //lo inicializamos como un campo
            this.js = js;
            this.httpClient = httpClient;
        }

        //le pasamos una llave a localstorage
        public static readonly string TOKENKEY = "TOKENKEY";
        private readonly IJSRuntime js;
        private readonly HttpClient httpClient;       

        //creamos el usuario anonimo
        private AuthenticationState Anonimo => new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        //este metodo me va a permitr saber si estoy autenticado mediante un token que va a recibir
        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            //verificamos si ya hay un token en localstorage
            var token = await js.GetFromLocalStorage(TOKENKEY);

            //si el usuario no tiene un token es un usuario anonimo
            if (string.IsNullOrEmpty(token))
            {
                return Anonimo;
            }

            return ConstruirAuthenticationState(token);
        }       

        //si tiene un token lo vamos a utilizar para crear el estado de autenticacion, creamos un metodo que recibe como parametro el token
        private AuthenticationState ConstruirAuthenticationState(string token)
        {            
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            //se puso aqui para poder ver los claims que trae el webtoken desde el controlador cuentas
            var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");

            /*el claimprincipal tiene todos los claim y a su vez el authenticationstate tiene el claimprincipal
            y asi podemos injectar el authenticationstate del lado del cliente para usar la autorizacion basada en permisos*/
            return new AuthenticationState(new ClaimsPrincipal(identity));

            //asi estaba antes
            //return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
        }

        //este metodo lo creo el equipo de microsoft para trabajar con los claims aun no esta incluido en blazor 
        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles);

            if (roles != null)
            {
                if (roles.ToString().Trim().StartsWith("["))
                {
                    var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString());

                    foreach (var parsedRole in parsedRoles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                    }
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
                }

                keyValuePairs.Remove(ClaimTypes.Role);
            }

            claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())));
            return claims;
        }
        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }

        //aqui implementamos la interfaz de loginservice
        public async Task Login(string token)
        {
            await js.SetInLocalStorage(TOKENKEY, token);//guardamos el token en localstorage
            var authState = ConstruirAuthenticationState(token);//construimos el estado de autenticacion
            //con esto notificamos a blazor que el estado de autenticacion del usuario a cambiado
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }
        public async Task Logout()
        {
            await js.RemoveItem(TOKENKEY);//eliminamos el token de localstorage
            httpClient.DefaultRequestHeaders.Authorization = null;//quitamos de la cabecera http el token
            NotifyAuthenticationStateChanged(Task.FromResult(Anonimo));//notificamos a blazor que hubo un cambio en el estado
        }       
    }
}
