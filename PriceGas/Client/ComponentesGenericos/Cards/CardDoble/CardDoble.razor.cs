using PriceGas.Client.ComponentesGenericos.Base;
using PriceGas.Client.ComponentesGenericos.Utilities;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriceGas.Client.ComponentesGenericos.Cards.CardDoble
{
    public partial class CardDoble : SimComponentBase
    {
        protected string Classname =>
        new CssBuilder("row no-gutters")
          .AddClass(Class)
        .Build();
        [Parameter] public RenderFragment ChildContent { get; set; }
    }
}
