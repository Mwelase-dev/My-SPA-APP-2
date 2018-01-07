using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Intranet.UI.Helpers
{
    //NB - These should match what is on the client code(javascript)
    public enum CustomRoles
    {
        [Description("Human Relations")]
        HumanRelations = 1,
        [Description("Management")]
        Management = 2,
        [Description("IntranetAdmins")]
        IntranetAdmins = 3,
        [Description("Assistants")]
        Assistants = 4,
        [Description("Intranet Reports")]
        IntranetReports = 5,
    }
}