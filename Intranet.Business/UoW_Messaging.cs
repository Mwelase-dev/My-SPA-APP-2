using Intranet.Data.EF;
using Intranet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intranet.Models.Enums;

namespace Intranet.Business
{
    public class UoWLeaveApplication
    {
        public static IEnumerable<StaffLeaveModel> GetSimultaniousLeaveApps(StaffModel staffMember, DateTime leaveStart, DateTime leaveEnd)
        {
            var staff = new DataContextEF()
                .Staff
                .Include("StaffLeaveData")
                .Where(m => m.DivisionId.Equals(staffMember.DivisionId) && m.RecordStatus.Equals("Active") && (!m.StaffId.Equals(staffMember.StaffId)));

            if (!staff.Any())
                return null;

            var leaveApps = new List<StaffLeaveModel>();

            staff.ToList().ForEach((m) => m.StaffLeaveData.Where(
                x =>
                ((x.LeaveDateStart >= leaveStart.Date && x.LeaveDateStart.Date <= leaveStart) ||
                 (x.LeaveDateStart.Date >= leaveEnd.Date && x.LeaveDateEnd.Date <= leaveEnd.Date)) &&
                (x.RecordStatus.Equals("Active")) &&
                ((x.LeaveStatus.Equals((int) LeaveStatus.Approved)) || x.LeaveStatus.Equals((int) LeaveStatus.Pending)))
                                           .ToList().ForEach(leaveApps.Add));

            return leaveApps;
        }

        // private static 
    }
}
