using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceGas.Shared.Enums
{
    //enum para elegir que operacion del crud se realizo
    public enum AuditType
    {
        None = 0,
        Create = 1,
        Update = 2,
        Delete = 3
    }
}
