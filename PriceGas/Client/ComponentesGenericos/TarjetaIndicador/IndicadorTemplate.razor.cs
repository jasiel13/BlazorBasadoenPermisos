using PriceGas.Client.ComponentesGenericos.Base;
using PriceGas.Client.ComponentesGenericos.Utilities;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriceGas.Client.ComponentesGenericos.TarjetaIndicador
{
    public partial class IndicadorTemplate : SimComponentBase
    {
        protected string Classname =>
             new CssBuilder("card-deck")
             .AddClass(Class)
        .Build();
        [Parameter] public RenderFragment ChildContent { get; set; }
    }
}
