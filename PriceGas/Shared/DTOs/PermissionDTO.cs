using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceGas.Shared.DTOs
{
    public class PermissionDTO
    {
        public string RoleId { get; set; }
        public IList<RoleClaimsDTO> RoleClaims { get; set; }
    }

    public class RoleClaimsDTO
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; }
    }
}
