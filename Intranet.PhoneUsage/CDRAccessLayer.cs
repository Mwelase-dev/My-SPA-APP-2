using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
//using Intranet.Models;
//using Intranet.Models;
//using Intranet.Data.EF;
//using Intranet.Data.EF;
using Intranet.Data.EF;
using Intranet.Models;
using MySql.Data.MySqlClient;
//using Intranet.Data.EF;

namespace Intranet.PhoneUsage
{
    public static class CDRAccessLayer
    {
        #region Private Methods
        private static StaffPhoneRecord FillDataRecord(IDataRecord aDataRecord)
        {
            return new StaffPhoneRecord
                {
                    calldate = aDataRecord.GetDateTime(aDataRecord.GetOrdinal("calldate")),
                    clid = aDataRecord.GetString(aDataRecord.GetOrdinal("clid")),
                    src = aDataRecord.GetString(aDataRecord.GetOrdinal("src")),
                    dst = aDataRecord.GetString(aDataRecord.GetOrdinal("dst")),
                    dcontext = aDataRecord.GetString(aDataRecord.GetOrdinal("dcontext")),
                    channel = aDataRecord.GetString(aDataRecord.GetOrdinal("channel")),
                    dstchannel = aDataRecord.GetString(aDataRecord.GetOrdinal("dstchannel")),
                    lastapp = aDataRecord.GetString(aDataRecord.GetOrdinal("lastapp")),
                    lastdata = aDataRecord.GetString(aDataRecord.GetOrdinal("lastdata")),
                    duration = aDataRecord.GetString(aDataRecord.GetOrdinal("duration")),
                    billsec = aDataRecord.GetInt32(aDataRecord.GetOrdinal("billsec")),
                    disposition = aDataRecord.GetString(aDataRecord.GetOrdinal("disposition")),
                    amaflags = aDataRecord.GetString(aDataRecord.GetOrdinal("amaflags")),
                    accountcode = aDataRecord.GetString(aDataRecord.GetOrdinal("accountcode")),
                    uniqueid = aDataRecord.GetString(aDataRecord.GetOrdinal("uniqueid")),
                    userfield = aDataRecord.GetString(aDataRecord.GetOrdinal("userfield")),
                    carrier = aDataRecord.GetString(aDataRecord.GetOrdinal("trunkname"))
                };
        }

        private static List<StaffPhoneRecord> GetCDRData(DateTime DateFrom, DateTime DateTo)
        {
            var aCDRList = new List<StaffPhoneRecord>();
            const string aConnectionStr = "server=172.16.0.102;Uid=cdr_user;Pwd=P@55word;Database=asteriskcdrdb;";
            var aCommandText = new StringBuilder("select *, substring(dstchannel, 1, length(dstchannel)-9) as trunkname from cdr where (dstchannel regexp 'SIP/[[:alpha:]]')");

            // From Date
            if ((DateFrom != DateTime.MinValue) && (DateTo != DateTime.MinValue))
            {
                aCommandText.Append(String.Format(" and (calldate between '{0}' and '{1}')", DateFrom.ToString("yyyy/MM/dd 00:00:00"), DateTo.ToString("yyyy/MM/dd 23:59:59")));
            }

            using (var conn = new MySqlConnection(aConnectionStr))
            {
                var aCommand = new MySqlCommand(aCommandText.ToString(), conn);
                conn.Open();

                using (var aReader = aCommand.ExecuteReader())
                {
                    if (aReader.HasRows)
                    {
                        while (aReader.Read())
                        {
                            aCDRList.Add(FillDataRecord(aReader));
                        }
                    }
                    aReader.Close();
                }
            }
            return aCDRList;
        }
        #endregion

        #region Public Methods
        public static List<StaffPhoneRecord> CDRData(DateTime DateFrom, DateTime DateTo, String StaffTelExt)
        {
            var aList = new List<StaffPhoneRecord>();
            if (StaffTelExt != null)
            {
                aList = GetCDRData(DateFrom, DateTo);
                if (!String.IsNullOrEmpty(StaffTelExt))
                {
                    aList = aList.FindAll(data => data.Extension == StaffTelExt);
                }
            }
            return aList;
        }
        public static List<BranchModel> CDRData(DateTime dateFrom, DateTime dateTo, Guid? CompanyID, Guid? DivisionID, Guid? StaffID, String Carrier)
        {
            using (var store = new DataContextEF())
            {
                var dataList = GetCDRData(dateFrom, dateTo);
                var branches = store.Branches.Include("BranchDivisions").Include("BranchDivisions.DivisionStaff").Where(b => b.RecordStatus == "Active").ToList();
                if ((!String.IsNullOrEmpty(Carrier)) && (!Carrier.Equals("--All--")))
                {
                    dataList = dataList.Where(x => x.Carrier.Contains(Carrier)).ToList();
                }

                #region Filter Branches
                if ((CompanyID != null) && (CompanyID != Guid.Empty))
                {
                    branches = branches.FindAll(data => data.BranchId == CompanyID);
                }
                #endregion
                foreach (var branch in branches)
                {
                    #region Filter Divisions
                    branch.BranchDivisions = branch.BranchDivisions.Where(b => b.RecordStatus.Equals("Active")).ToList();
                    if ((DivisionID != null) && (DivisionID != Guid.Empty))
                    {
                        branch.BranchDivisions = branch.BranchDivisions.ToList().FindAll(m => m.DivisionId.Equals(DivisionID));
                    }

                    #endregion
                    foreach (var div in branch.BranchDivisions.ToList())
                    {
                        #region Filter Staff
                        div.DivisionStaff = div.DivisionStaff.ToList();
                        if ((StaffID != null) && (StaffID != Guid.Empty))
                        {
                            div.DivisionStaff = div.DivisionStaff.Where(data => data.StaffId == StaffID).ToList();
                        }
                        #endregion

                        foreach (var staff in div.DivisionStaff.ToList())
                        {
                            staff.StaffCallRecords = dataList.FindAll(m => m.Extension.Equals(staff.StaffTellExt));
                            staff.StaffCallRecords = staff.StaffCallRecords.ToList().FindAll(m => m.calldate.Date > staff.StaffJoinDate.Date);
                        }
                        //div.DivisionStaff = div.DivisionStaff.Where(s => s.TotalCallCost > 0).ToList();
                    }
                    //branch.BranchDivisions = branch.BranchDivisions.Where(p => p.TotalCallCost > 0).ToList();
                }
                return branches.ToList();
            }
        }
        public static List<StaffModel> CDRLookupNumber(string numberDialed)
        {
            using (var store = new DataContextEF())
            {
                if (!String.IsNullOrEmpty(numberDialed))
                {
                    //todo ref Quentin - names?
                    var aList = new List<StaffPhoneRecord>();
                    var sList = store.Staff;

                    aList = GetCDRData(DateTime.Now.AddDays(-7), DateTime.Now);

                    aList = aList.FindAll(data => data.dst == numberDialed);
                    aList = aList.OrderByDescending(x => x.DisplayCallDate).ToList();

                    foreach (var staff in sList)
                    {
                        staff.StaffCallRecords = aList.FindAll(data => data.Extension == staff.StaffTellExt);
                    }
                    return sList.Where(x => x.StaffCallRecords.Count > 0).ToList();

                    #region LINQ join

                    //var result = (from staff in sList
                    //              join callrec in aList on staff.StaffTelExt equals callrec.Extension into staffcallrec
                    //              select staff, staff.StaffCallRecords = staffcallrec).ToList();                             


                    //return (from s in sList
                    //        from c in aList
                    //             join a in aList on s.StaffTelExt equals a.Extension
                    //             into callList
                    //             select new
                    //             {
                    //                 s,
                    //                 StaffCallRecords = callList.ToList<StaffPhoneRecord>()
                    //             });

                    //return (from client in ClientList    ( = List<Client>)
                    //        join address in AddressList  ( = List<Address>)
                    //        on client.ClientID equals address.ClientID
                    //           --  into callList
                    //           --  select new
                    //           --  {
                    //           --      s,
                    //           --      StaffCallRecords = callList.ToList<StaffPhoneRecord>()
                    //           --  });
                    //           should return ( List<Client>) with Addresses populated
                    //
                    //return result;

                    #endregion
                }
            }
            return null;
        }
        public static List<string> CDRProviders(DateTime DateFrom, DateTime DateTo)
        {
            var aList = GetCDRData(DateFrom, DateTo).Select(x => x.Carrier);


            return aList.Distinct().ToList();
        }
        #endregion
    }
}