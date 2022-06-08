using PriceGas.Client.ComponentesGenericos.Base;
using PriceGas.Client.ComponentesGenericos.Utilities;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriceGas.Client.ComponentesGenericos.Cards
{
    public partial class Card : SimComponentBase
    {
        protected string Classname =>
          new CssBuilder("sim-card")
            .AddClass(Class)
          .Build();      
        [Parameter] public RenderFragment ChildContent { get; set; }
    }
}
