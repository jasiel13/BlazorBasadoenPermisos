using PriceGas.Client.ComponentesGenericos.Base;
using PriceGas.Client.ComponentesGenericos.Enum;
using PriceGas.Client.ComponentesGenericos.Utilities;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriceGas.Client.ComponentesGenericos.Tipografia
{
    public partial class Texto : SimComponentBase
    {
        protected string Classname =>
        new CssBuilder("sim-typography")        
        .AddClass(Class)
      .Build();

        /// <summary>
        /// Applies the theme typography styles.
        /// </summary>
        [Parameter] public Typo Typo { get; set; } = Typo.body1;      

        /// <summary>
        /// Child content of component.
        /// </summary>
        [Parameter] public RenderFragment ChildContent { get; set; }        
    }
}
