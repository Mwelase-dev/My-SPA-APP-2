using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intranet.Models
{
    public class RolesModel
    {
        public Guid  RoleId { get; set; }
        public int Role { get; set; }
        public string Description { get; set; }

        public virtual ICollection<UserRolesModel> UserRoles { get; set; } 
 
    }

    public class UserRolesModel
    {
        public Guid RoleId { get; set; }
        public Guid StaffId { get; set; }

        //related entities
       // public virtual StaffModel StaffRoleModel { get; set; }
        public virtual RolesModel Role { get; set; }
    }
}
