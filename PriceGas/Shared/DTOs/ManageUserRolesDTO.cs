using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceGas.Shared.DTOs
{
    public class ManageUserRolesDTO
    {
        public string UserId { get; set; }
        public IList<UserRolesDTO> UserRoles { get; set; }
    }   
}
