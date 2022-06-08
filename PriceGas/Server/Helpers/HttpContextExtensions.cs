using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriceGas.Server.Helpers
{
    public static class HttpContextExtensions
    {
        //este metodo es reusable para todos los componentes
        //el maximo de registros existentes lo vamos a colocar en la cabecera de la respuesta http
        public async static Task InsertarParametrosPaginacionEnRespuesta<T>(this HttpContext context,
            IQueryable<T> queryable, int cantidadRegistrosAMostrar)
        {
            if (context == null) { throw new ArgumentNullException(nameof(context)); }
            double conteo = await queryable.CountAsync();//con esto contamos todos los registros
            //aqui hacemos una division entre la cantidad total de registros de la bd y la cantidad de registros a mostrar
            double totalPaginas = Math.Ceiling(conteo / cantidadRegistrosAMostrar);
            //colocamos el total en la cabecera de la respuesta http
            context.Response.Headers.Add("conteo", conteo.ToString());
            context.Response.Headers.Add("totalPaginas", totalPaginas.ToString());
        }
    }
}
