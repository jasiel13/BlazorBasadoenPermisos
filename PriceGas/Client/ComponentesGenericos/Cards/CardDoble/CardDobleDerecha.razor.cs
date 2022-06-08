﻿
using PriceGas.Client.ComponentesGenericos.Base;
using PriceGas.Client.ComponentesGenericos.Utilities;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriceGas.Client.ComponentesGenericos.Cards.CardDoble
{
    public partial class CardDobleDerecha : SimComponentBase
    {
        protected string Classname =>
        new CssBuilder("col-lg-4 pl-lg-2 mb-3")
          .AddClass(Class)
        .Build();
        [Parameter] public RenderFragment ChildContent { get; set; }
    }
}
