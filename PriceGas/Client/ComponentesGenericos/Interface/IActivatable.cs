using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriceGas.Client.ComponentesGenericos.Interface
{
    public interface IActivatable
    {
        void Activate(object activator, MouseEventArgs args);
    }
}
