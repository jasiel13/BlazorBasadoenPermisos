using PriceGas.Client.ComponentesGenericos.Base;
using PriceGas.Client.ComponentesGenericos.Utilities;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriceGas.Client.ComponentesGenericos.Imagen
{
    partial class Imagen : SimComponentBase
    {
        protected string Classname =>
        new CssBuilder("imagen")
          .AddClass(Class)
        .Build();
        [Parameter] public string Src { get; set; }
        [Parameter] public string Alt { get; set; }        
    }
}
