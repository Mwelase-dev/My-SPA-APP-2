using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Intranet.Messages;
using Utilities;

namespace Data.FaceID
{
    public class FaceID
    {
        #region Private
        private int Hour { get; set; }
        private FaceIDConfig config = (FaceIDConfig)System.Configuration.ConfigurationManager.GetSection("clockingDevices");
        private delegate int CallBack(ulong nTotal, ulong nDone);

        [DllImport("FKAttend.dll")]
        public static extern int FK_SaveEnrollData(int nHandleIndex);

        [DllImport("HwDevComm.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern int HwDev_Execute(string pDevInfoBuf, int nDevInfoLen, string pSendBuf, int nSendLen, ref IntPtr pRecvBuf, ref uint pRecvLen, CallBack pFuncTotalDone);

        [DllImport("FKAttend.dll")]
        public static extern int FK_PutEnrollDataWithString(int nHandleIndex, int nEnrollNumber, int nBackupNumber,
            int nMachinePrivilege, string pnEnrollData);
        [DllImport("FKAttend.dll")]
        public static extern int FK_GetUserName(int nHandleIndex, int nEnrollNumber, ref string pstrUserName);
        [DllImport("FKAttend.dll")]
        public static extern int FK_SetUserName(int nHandleIndex, int nEnrollNumber, string pstrUserName);
        [DllImport("FKAttend.dll")]
        public static extern int FK_GetSuperLogData(int nHandleIndex, ref int pnSEnrollNumber, ref int pnGEnrollNumber, ref int nManipulation, ref int pnBackupNumber, ref DateTime pnDateTime);
        [DllImport("FKAttend.dll")]
        public static extern int FK_EnableDevice(int nHandleIndex, byte nEnableFlag);
        [DllImport("FKAttend.dll")]
        public static extern int FK_ConnectNet(int nMachineNo, string strIpAddress, int nNetPort, int nTimeOut, int nProtocolType, int nNetPassword);
        [DllImport("FKAttend.dll")]
        public static extern int FK_LoadGeneralLogData(int nHandleIndex, int nReadMark);
        [DllImport("FKAttend.dll")]
        public static extern int FK_USBLoadGeneralLogDataFromFile(int nHandleIndex, string astrFilePath);
        [DllImport("FKAttend.dll")]
        public static extern int FK_GetGeneralLogData(int nHandleIndex, ref int pnEnrollNumber, ref int pnVerifyMode, ref int pnInOutMode, ref DateTime pnDateTime);
        [DllImport("FKAttend.dll")]
        public static extern int FK_DeleteEnrollData(int nHandleIndex, int nEnrollNumber, int nBackupNumber);


        private void ProccessStringData(string inputData, ref List<FaceRecord> dataList)
        {
            if (!String.IsNullOrEmpty(inputData))
            {
                foreach (string line in Regex.Split(inputData, "\r\n"))
                {
                    /* time="2011-11-02 13:54:57" id="123456" name="craig" workcode="" status="1" card_src="from_check" */
                    var Record = new FaceRecord();
                    foreach (string item in Regex.Split(line, "\" "))
                    {
                        if (item.StartsWith("time="))
                        {
                            Trace.WriteLine(item.Substring(6, item.Length - 6));
                            if (item.Substring(6, item.Length - 6).Contains("ULL"))//2015-10-08 18:53:30"ULL"
                                continue;
                            Record.RecordDateTime = DateTime.Parse(item.Substring(6, item.Length - 6));
                        }
                        if (item.StartsWith("id="))
                        {
                            Record.RecordID = item.Substring(4, item.Length - 4);
                        }
                        if (item.StartsWith("name="))
                        {
                            Record.RecordName = item.Substring(6, item.Length - 6);
                        }

                        //We have what we need, move on.
                        if (Record.RecordPopulated)
                        {
                            dataList.Add(Record);
                            break;
                        }
                    }
                }
            }
        }

        #region Helpers
        public int ConvertHexAuthorityNumber(string AuthorityNum)
        {
            string[] strArray = new string[10];
            int index = 0;
            foreach (char ch in AuthorityNum)
            {
                strArray[index] = ch.ToString();
                index++;
            }
            return int.Parse(strArray[2], NumberStyles.AllowHexSpecifier);
        }

        public int ConvertHexCardNumber(string HexCardNumber)
        {
            string[] strArray = new string[10];
            int index = 0;
            string str = string.Empty;
            foreach (char ch in HexCardNumber)
            {
                strArray[index] = ch.ToString();
                index++;
            }
            str = strArray[8];
            var test = str + strArray[9] + strArray[6] + strArray[7] + strArray[4] + strArray[5] + strArray[2] + strArray[3]; //"0Xac784f00"
            return int.Parse((((str + strArray[9]) + strArray[6] + strArray[7]) + strArray[4] + strArray[5]) + strArray[2] + strArray[3], NumberStyles.AllowHexSpecifier);
        }

        public string ConvertCardNumberToHex(int CardNumber)
        {
            string[] strArray = new string[12];
            string str = CardNumber.ToString("X");
            int index = 0;
            string str2 = string.Empty;
            foreach (char ch in str)
            {

                strArray[index] = ch.ToString();
                index++;
            }
            if ((strArray[6] == null) && (strArray[7] == null))
            {
                str2 = strArray[4];
                var tes = (((str2 + strArray[5] + strArray[2]) + strArray[3] + strArray[0]) + strArray[1] + "00").ToLower();  //"0Xac784f00" // "AC784F00"
                return (((str2 + strArray[5] + strArray[2]) + strArray[3] + strArray[0]) + strArray[1] + "00").ToLower();
            }
            str2 = strArray[6];
            //"0Xac784f00"
            var test = ((((str2 + strArray[7]) + strArray[4] + strArray[5]) + strArray[2] + strArray[3]) + strArray[0] + strArray[1]).ToLower();
            return ((((str2 + strArray[7]) + strArray[4] + strArray[5]) + strArray[2] + strArray[3]) + strArray[0] + strArray[1]).ToLower();
        }

        public string ConvertAuthorityNumToHex(int AuthorityNum)
        {
            return AuthorityNum.ToString("X");
        }
        #endregion
        #endregion

        #region Public Methods
        #region Retreive Data
        /// <summary>
        /// Gets all of the logged data. Defaults dates are 01/01/1900 to 31/12/2099.
        /// Requires section in the config file.
        /// section name="clockingDevices" type="Data.FaceID.FaceIDConfig"
        /// {clockingDevices}
        ///   {add name="Device1" ipAddress="172.16.0.100"}
        /// {clockingDevices}
        /// </summary>
        /// <returns>IList of FaceRecord</returns>
        public IList<FaceRecord> DataRetrieve()
        {
            //Forced Change for Jay
            var internalList = new List<FaceRecord>();

            //Read from Devices based on IP
            if (config != null)
            {
                if (config.ClockingDevices.Count > 0)
                {
                    var dateStart = new DateTime(1900, 01, 01, 00, 00, 00);
                    var dateEnd = DateTime.Now;
                    if (config.daysBack > 0)
                    {
                        // Convert value into minus
                        dateStart = dateEnd.AddDays(config.daysBack * -1);
                    }

                    // Loop through devices
                    foreach (ClockDevice device in config.ClockingDevices)
                    {
                        foreach (FaceRecord record in DataRetrieve(device.Number, device.ipAddress, dateStart, dateEnd))
                        {
                            internalList.Add(record);
                        }
                    }
                }
            }
            else
            {
                throw new Exception("Error. Cannot read clocking devices from the configuration file. Please ensure you have the 'clockingDevices' section added to your configuration file.");
            }

            //var j = internalList.Where(m => m.RecordID.Equals("111")).ToList();
            //var mw = internalList.Where(m => m.RecordID.Equals("112")).ToList();
            //var ol = internalList.Where(m => m.RecordID.Equals("103")).OrderByDescending(m=>m.RecordDateTime).ToList();

            return internalList;
        }

        /// <summary>
        /// Reads the clocking data from a file as apposed to the IP addresses.
        /// </summary>
        /// <param name="fileName">The file to read the data from. One at a time</param>
        /// <returns></returns>
        public IList<FaceRecord> DataRetrieve(string fileName)
        {
            //Forced Change for Jay
            if (String.IsNullOrEmpty(fileName))
            {
                return DataRetrieve();
            }
            else
            {
                var internalList = new List<FaceRecord>();
                var text = System.IO.File.ReadAllText(fileName);
                ProccessStringData(text, ref internalList);
                return internalList;
            }
        }

        /// <summary>
        /// Reads data from a list of files.
        /// </summary>
        /// <param name="fileList">The list of filenames to read from</param>
        /// <returns></returns>
        public IList<FaceRecord> DataRetrieve(IList<string> fileList)
        {
            //Forced Change for Jay
            if (fileList.Count <= 0)
            {
                return DataRetrieve();
            }
            else
            {
                var internalList = new List<FaceRecord>();
                foreach (var name in fileList)
                {
                    var text = System.IO.File.ReadAllText(name);
                    ProccessStringData(text, ref internalList);
                }
                return internalList;
            }
        }

        /// <summary>
        /// Gets all of the logged data. Defaults dates are 01/01/1900 to 31/12/2099.
        /// </summary>
        /// <param name="machineNumber">The number of the machine. Normally 1,2 or 3</param>
        /// <param name="ipAddress">the IP address of the machine "172.16.0.110"</param>
        /// <returns>IList of FaceRecord</returns>
        public IList<FaceRecord> DataRetrieve(string machineNumber, string ipAddress)
        {
            return DataRetrieve(machineNumber, ipAddress, new DateTime(1900, 01, 01, 00, 00, 00), new DateTime(2099, 12, 31, 23, 59, 59));
        }

        /// <summary>
        /// Gets the logged data. Can specify the start and end dates.
        /// </summary>
        /// <param name="machineNumber">The number of the machine. Normally 1,2 or 3</param>
        /// <param name="ipAddress">the IP address of the machine "172.16.0.110"</param>
        /// <param name="dateTimeStart">Start Date of data to collect from</param>
        /// <param name="dateTimeEnd">End date of data to stop collecting from</param>
        /// <returns>IList of FaceRecord</returns>
        public IList<FaceRecord> DataRetrieve(string machineNumber, string ipAddress, DateTime dateTimeStart, DateTime dateTimeEnd)
        {
            var logRow = new List<string>();
            var log = (AppDomain.CurrentDomain.BaseDirectory + "\\Utilities\\ProcessClockDataLogs.txt");

            logRow.Add(String.Format("{0} - Retrieving data from {1}...", DateTime.Now, ipAddress));
            Trace.WriteLine(String.Format("Retrieving data from {0}...", ipAddress));
            var internalList = new List<FaceRecord>();
            IntPtr zero = IntPtr.Zero;
            uint pRecvLen = 0;
            string pDevInfoBuf = String.Format("DeviceInfo( dev_id = \"{0}\" dev_type = \"HW_HDCP\" comm_type = \"ip\" ip_address = \"{1}\")", machineNumber, ipAddress);
            string pSendBuf = String.Format("GetRecord(start_time=\"{0}\"end_time=\"{1}\")", dateTimeStart.ToString("yyyy-MM-dd HH:mm:ss"), dateTimeEnd.ToString("yyyy-MM-dd HH:mm:ss"));

            if (
                HwDev_Execute(pDevInfoBuf, pDevInfoBuf.Length, pSendBuf, pSendBuf.Length, ref zero, ref pRecvLen, null) ==
                0)
            {
                string pollData = Marshal.PtrToStringAnsi(zero);
                if (pollData.Contains("success"))
                {
                    logRow.Add(String.Format("{0} - Processing data from {1}...", DateTime.Now, ipAddress));
                    Trace.WriteLine(String.Format("Processing data from {0}...", ipAddress));
                    ProccessStringData(pollData, ref internalList);
                }
                else
                {
                    logRow.Add(String.Format("{0} - Error retrieving data from device {1}!", DateTime.Now, ipAddress));
                    Trace.WriteLine(String.Format("Error retrieving data from device {0}!", ipAddress));
                    throw new Exception(String.Format("Error retrieving data from device {0}!", ipAddress));
                }
            }
            else if (FK_ConnectNet(1, ipAddress, 5005, 20000, 0, 0) == 1)
            {
                ReadDataFromDevice(ref internalList);
                FK_EnableDevice(1, 1);
            }

            else
            {
                logRow.Add(String.Format("{0} - Communication failure to device {1}!", DateTime.Now, ipAddress));
                Trace.WriteLine(String.Format("Communication failure to device {0}!", ipAddress));
               
                using (var file = new StreamWriter(log))
                {
                    foreach (string line in logRow)
                    {
                        file.WriteLine(line);
                    }
                }

                var mailer = new Emailer();
                mailer.subject = "Clock device offline";
                mailer.body = string.Format("Dear {0} ", "Support") + String.Format("Communication failure to device {0}!", ipAddress);
                mailer.TOList.Add(ConfigurationManager.AppSettings["SupportEmail"]);
                mailer.SendEmail();

                //throw new Exception(String.Format("Communication failure to device {0}!", ipAddress));
            }
            logRow.Add(String.Format("{0} - Retrieved {1} record(s) from device {2}.", DateTime.Now, internalList.Count.ToString(), ipAddress));
            Trace.WriteLine(String.Format("Retrieved {0} record(s) from device {1}.", internalList.Count.ToString(), ipAddress));

            using (var file = File.AppendText(log))
            {
                foreach (string line in logRow)
                {
                    file.WriteLine(line);
                }
            }
            return internalList;
        }


        private static void ReadDataFromDevice(ref List<FaceRecord> internalList)
        {
            int vSEnrollNumber = 0;
            int vVerifyMode = 0;
            int vInOutMode = 0;
            DateTime vdwDate = DateTime.MinValue;
            int vnCount = 1;
            int vnResultCode = 0;
            string vstrFileName;
            string vstrFileData;
            int vnReadMark;
            var clockData = new string[] { };
            var clockInfo = new List<string>();
            vnResultCode = FK_EnableDevice(1, 0);
            vnResultCode = FK_LoadGeneralLogData(1, 0);
            var test = "";
            var enabler = "";
            string[] formats = {"M/d/yyyy h:mm:ss tt", "M/d/yyyy h:mm tt", 
                   "MM/dd/yyyy hh:mm:ss", "M/d/yyyy h:mm:ss", 
                   "M/d/yyyy hh:mm tt", "M/d/yyyy hh tt", 
                   "M/d/yyyy h:mm", "M/d/yyyy h:mm", 
                   "MM/dd/yyyy hh:mm", "M/dd/yyyy hh:mm","yyyy-mm-dd hh:mm:ss tt"};
            DateTime dateValue;
            do
            {
                #region
                if (clockInfo.Count > 2)
                {
                    var testcase1 = "";
                    var testcase2 = "";

                    foreach (string item in Regex.Split(clockInfo[clockInfo.Count - 1], "\""))
                    {
                        if (DateTime.TryParse(item, out dateValue))
                        {
                            testcase1 = item;
                        }
                    }
                    foreach (string item in Regex.Split(clockInfo[clockInfo.Count - 2], "\""))
                    {
                        if (DateTime.TryParse(item, out dateValue))
                        {
                            testcase2 = item;
                        }
                    }

                    if (testcase1 != "" && testcase2 != "" && testcase1 == testcase2)
                    {
                        enabler = "enabled";
                        continue;
                    }
                }
                #endregion
                vnResultCode = FK_GetGeneralLogData(1, ref vSEnrollNumber, ref vVerifyMode, ref vInOutMode, ref vdwDate);
                vstrFileData = FuncMakeGeneralLogFileData(vnCount, vSEnrollNumber, vVerifyMode, vInOutMode, vdwDate);//"1\t900\t35\t1\t2015/10/14 09:45:33\r\n"
                test += vstrFileData;

                vnCount = vnCount + 1;
                clockInfo.Add(vstrFileData);
            }
            while (enabler == "");

            vnResultCode = FK_EnableDevice(1, 1);
            //var dataList = new List<FaceRecord>();
            ProccessStringOfData(test, ref internalList);


        }

        private static string FuncMakeGeneralLogFileData(long anCount, long aSEnrollNumber, long aVerifyMode, long aInOutMode, DateTime adwDate)
        {
            string vstrDTimeMwe;
            vstrDTimeMwe = "time=" + "\"" + adwDate + "\"" + " id=" + "\"" + anCount + "\"" + " name=" + "\"" + aSEnrollNumber + "\"" + " workcode=\"\" card_src=\"\"";
            return (vstrDTimeMwe + Convert.ToChar(13) + Convert.ToChar(10));
        }

        private static void ProccessStringOfData(string inputData, ref List<FaceRecord> dataList)
        {
            if (!String.IsNullOrEmpty(inputData))
            {
                foreach (string line in Regex.Split(inputData, "\r\n"))
                {
                    /* time="2011-11-02 13:54:57" id="123456" name="craig" workcode="" status="1" card_src="from_check" */
                    var Record = new FaceRecord();
                    foreach (string item in Regex.Split(line, "\" "))
                    {
                        if (item.StartsWith("time="))
                        {
                            Record.RecordDateTime = DateTime.Parse(item.Substring(6, item.Length - 6));
                        }
                        if (item.StartsWith("id="))
                        {
                            Record.RecordName = item.Substring(4, item.Length - 4);
                        }
                        if (item.StartsWith("name="))
                        {
                            Record.RecordID = item.Substring(6, item.Length - 6);
                        }

                        //We have what we need, move on.
                        if (Record.RecordPopulated)
                        {
                            dataList.Add(Record);
                            break;
                        }
                    }
                }
            }
        }



        #endregion

        #region Clear Data
        /// <summary>
        /// Clears ALL data from the devices listed in the configuration.
        /// </summary>
        public bool DataClear()
        {
            bool result = true;
            foreach (ClockDevice device in config.ClockingDevices)
            {
                result = DataClear(device.Number, device.ipAddress);
                if (result.Equals(false))
                {
                    throw new Exception("Error occurred while clearing data for device " + device.ipAddress);
                }
            }
            return result;
        }

        /// <summary>
        /// Clears ALL data from the device
        /// </summary>
        /// <param name="machineNumber">The number of the machine. Normally 1,2 or 3</param>
        /// <param name="ipAddress">the IP address of the machine "172.16.0.110"</param>
        /// <returns>Boolean - true | false</returns>
        public bool DataClear(string machineNumber, string ipAddress)
        {
            Trace.WriteLine(String.Format("Clearing device {0}", ipAddress));

            IntPtr zero = IntPtr.Zero;
            uint pRecvLen = 0;
            string pDevInfoBuf = String.Format("DeviceInfo( dev_id = \"{0}\" dev_type = \"HW_HDCP\" comm_type = \"ip\" ip_address = \"{1}\")", machineNumber, ipAddress);
            string pSendBuf = "DeleteAllRecord()";

            if (HwDev_Execute(pDevInfoBuf, pDevInfoBuf.Length, pSendBuf, pSendBuf.Length, ref zero, ref pRecvLen, null) == 0)
            {

                string results = Marshal.PtrToStringAnsi(zero);
                if (results.IndexOf("success") > 0)
                {
                    Trace.WriteLine(String.Format("Device cleared - {0}", ipAddress));
                    return true;
                }
                else
                {
                    Trace.Write(String.Format("Failed to clear device {0}.", ipAddress));
                    throw new Exception(String.Format("Failed to clear device {0}.", ipAddress));
                }
            }
            else
            {
                Trace.WriteLine(String.Format("Communication failure to device {0}!", ipAddress));
                throw new Exception(String.Format("Communication failure to device {0}!", ipAddress));
            }
        }
        #endregion

        #region Sync time
        /// <summary>
        /// Set's the device time to this PC's time from the configuration file
        /// </summary>
        public bool SyncTime()
        {


            bool result = true;
            foreach (ClockDevice device in config.ClockingDevices)
            {
                result = SyncTime(device.Number, device.ipAddress);
                if (result.Equals(false))
                {
                    throw new Exception("Error occurred while syncing the time data for device " + device.ipAddress);
                }
            }
            return result;
        }

        /// <summary>
        /// Set's the device time to this PC's time
        /// </summary>
        /// <param name="machineNumber">The number of the machine. Normally 1,2 or 3</param>
        /// <param name="ipAddress">the IP address of the machine "172.16.0.110"</param>
        /// <returns>Boolean - true | false</returns>
        public bool SyncTime(string machineNumber, string ipAddress)
        {
            Trace.WriteLine(String.Format("Synchronizing Device {0} Time...", ipAddress));

            IntPtr zero = IntPtr.Zero;
            uint pRecvLen = 0;
            string pDevInfoBuf = String.Format("DeviceInfo( dev_id = \"{0}\" dev_type = \"HW_HDCP\" comm_type = \"ip\" ip_address = \"{1}\")", machineNumber, ipAddress);
            string pSendBuf = String.Format("SetDeviceInfo(time = \"{0}\" week = \"3\")", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            if (HwDev_Execute(pDevInfoBuf, pDevInfoBuf.Length, pSendBuf, pSendBuf.Length, ref zero, ref pRecvLen, null) == 0)
            {

                if ((Marshal.PtrToStringAnsi(zero)).IndexOf("success") > 0)
                {
                    Trace.WriteLine("Device sync'd - {0}", ipAddress);
                    return true;
                }
                //else
                //{
                //    Trace.WriteLine(String.Format("Error clearing device {0}", ipAddress));
                //    throw new Exception(String.Format("Error clearing device {0}", ipAddress));
                //}

            }
            //else if (FK_ConnectNet(1, ipAddress, 5005, 20000, 0, 0) == 1)
            //{
            //    FK_EnableDevice(1, 0);
            //    FK_EnableDevice(1, 1);
            //    return true;
            //}
            //else
            //{
            //    Trace.WriteLine(String.Format("Communication failure to device {0}!", ipAddress));
            //    throw new Exception(String.Format("Communication failure to device {0}!", ipAddress));
            //}
            return true;
        }
        #endregion

        #region Delete User
        /// <summary>
        /// Deletes a user of the device
        /// </summary>
        public bool DeleteUser(string userID)
        {
            bool result = true;
            foreach (ClockDevice device in config.ClockingDevices)
            {
                result = DeleteUser(device.Number, device.ipAddress, userID);
                if (result.Equals(false))
                {
                    throw new Exception("Error occurred while deleting data from device " + device.ipAddress);
                }
            }
            return result;
        }

        /// <summary>
        /// Deletes a user of the device
        /// </summary>
        /// <param name="machineNumber">The number of the machine. Normally 1,2 or 3</param>
        /// <param name="ipAddress">the IP address of the machine "172.16.0.110"</param>
        /// <param name="user">User data to upload</param>
        /// <returns>Boolean - true | false</returns>
        public bool DeleteUser(string machineNumber, string ipAddress, string userID)
        {
            IntPtr zero = IntPtr.Zero;
            uint pRecvLen = 0;
            string pDevInfoBuf = String.Format("DeviceInfo( dev_id = \"{0}\" dev_type = \"HW_HDCP\" comm_type = \"ip\" ip_address = \"{1}\")", machineNumber, ipAddress);
            string pSendBuf = String.Format("DeleteEmployee(id=\"{0}\")", userID);

            if (HwDev_Execute(pDevInfoBuf, pDevInfoBuf.Length, pSendBuf, pSendBuf.Length, ref zero, ref pRecvLen, null) == 0)
            {
                Trace.WriteLine(String.Format("Template deleted from device {0}", ipAddress));
                return true;
            }
            else if (FK_DeleteEnrollData(1, Convert.ToInt32(userID), 0) == 1)
            {
                return true;
            }
            else
            {
                Trace.WriteLine(String.Format("Communication failure to device {0}!", ipAddress));
                return false;//throw new Exception(String.Format("Communication failure to device {0}!", ipAddress));
            }
        }
        #endregion

        #region Download Templates
        /// <summary>
        /// Downloads the data for a specified clock entry
        /// </summary>
        /// <param name="machineNumber">The number of the machine. Normally 1,2 or 3</param>
        /// <param name="ipAddress">the IP address of the machine "172.16.0.110"</param>
        /// <param name="clockId">the specific clock ID</param>
        /// <returns>FaceUser</returns>
        public FaceUser DownloadTemplates(string machineNumber, string ipAddress, string clockId)
        {
            Trace.Write("Downloading Template For Badge: " + clockId + " --->");
            FaceUser Record = new FaceUser();

            IntPtr zero = IntPtr.Zero;
            uint pRecvLen = 0;
            string pDevInfoBuf = String.Format("DeviceInfo( dev_id = \"{0}\" dev_type = \"HW_HDCP\" comm_type = \"ip\" ip_address = \"{1}\")", machineNumber, ipAddress);
            string pSendBuf = String.Format("GetEmployee(id=\"{0}\")", clockId);

            if (HwDev_Execute(pDevInfoBuf, pDevInfoBuf.Length, pSendBuf, pSendBuf.Length, ref zero, ref pRecvLen, null) == 0)
            {
                Trace.WriteLine("processing template data...");

                string templateData = Marshal.PtrToStringAnsi(zero);
                templateData = templateData.Substring(7, templateData.Length - 8);
                templateData = templateData
                    .Replace("\r\n", "")
                    .Replace(" ", "|")
                    .Replace("face_data=", "|face_data=")
                    .Replace("||", "|");

                // Split on "|"
                var dataArray = templateData.Split('|');
                if (dataArray[0].Contains("success"))
                {
                    string TmpStr = "";
                    int QuoteIdx = 0;
                    int StrCount = 0;
                    for (int i = 1; i < dataArray.Count(); i++)
                    {
                        TmpStr = dataArray[i];
                        QuoteIdx = TmpStr.IndexOf('\"') + 1;
                        StrCount = TmpStr.LastIndexOf("\"") - QuoteIdx;
                        var Data = TmpStr.Substring(0, TmpStr.IndexOf('='));
                        var Value = TmpStr.Substring(QuoteIdx, StrCount);
                        switch (Data)
                        {
                            case "id": Record.FaceID = int.Parse(Value); break;
                            case "name": Record.FaceName = Value; break;
                            case "authority": Record.FaceAuthority = Value; break;
                            case "card_num": Record.FaceCardNumber = Value; break;
                            case "calid": Record.FaceCalID = Value; break;
                            case "opendoor_type": Record.FaceDoorType = Value; break;
                            case "check_type": Record.FaceCheckType = Value; break;
                            case "face_data":
                                {
                                    Record.FaceData.Add(new FaceData { Data = Value });
                                    break;
                                }
                            default: break;
                        }
                    }
                }
                else
                {
                    Trace.WriteLine(String.Format("Error retrieving template for {0}", clockId));
                    throw new Exception(String.Format("Error retrieving template for {0}", clockId));
                }
            }
            else
            {
                Trace.WriteLine(String.Format("Communication failure to device {0}!", ipAddress));
                throw new Exception(String.Format("Communication failure to device {0}!", ipAddress));
            }
            return Record;
        }
        #endregion

        #region UploadTemplates

        /// <summary>
        /// Uploads a user's data to the device
        /// </summary>
        /// <param name="machineNumber">The number of the machine. Normally 1,2 or 3</param>
        /// <param name="ipAddress">the IP address of the machine "172.16.0.110"</param>
        /// <param name="user">User data to upload</param>
        /// <param name="staffClockId"></param>
        /// <param name="staffClockCardNumber"></param>
        /// <returns>Boolean - true | false</returns>
        public bool UploadTemplates(string machineNumber, string ipAddress, FaceUser user, string staffClockId, string staffClockCardNumber)
        {
            int vEnrollNumber = Convert.ToInt32(staffClockId);
            const int vBackupNumber = 11;
            const int vPrivilege = 0;
            var cardNumber = staffClockCardNumber;


            Trace.WriteLine("Uploading Template For Badge: " + user.FaceName + " --->");

            IntPtr zero = IntPtr.Zero;
            uint pRecvLen = 0;
            string pDevInfoBuf = String.Format("DeviceInfo( dev_id = \"{0}\" dev_type = \"HW_HDCP\" comm_type = \"ip\" ip_address = \"{1}\")", machineNumber, ipAddress);

            //------------------------------      
            string pSendBuf = String.Format("SetEmployee(id=\"{0}\" name=\"{1}\" authority=\"{2}\" card_num=\"{3}\" calid=\"{4}\" opendoor_type=\"{5}\" check_type=\"{6}\" ",
                /*0*/ user.FaceID,
                /*1*/ user.FaceName,
                /*2*/ user.FaceAuthority,
                /*2*/ user.FaceCardNumber,
                /*4*/ user.FaceCalID,
                /*5*/ user.FaceDoorType,
                /*6*/ user.FaceCheckType);

            // Add Face Data
            foreach (var data in user.FaceData)
            {
                pSendBuf += String.Format("\r\nface_data=\"{0}\"", data.Data);
            }
            pSendBuf = pSendBuf + ")";
            if (HwDev_Execute(pDevInfoBuf, pDevInfoBuf.Length, pSendBuf, pSendBuf.Length, ref zero, ref pRecvLen, null) == 0)
            {
                Trace.WriteLine("Template data uploaded.");
                return true;
            }
            else if (FK_PutEnrollDataWithString(1, vEnrollNumber, vBackupNumber, vPrivilege, cardNumber) == 1)
            {

            }
            else
            {
                Trace.WriteLine(String.Format("Communication failure to device {0}!", ipAddress));
                return false;//throw new Exception(String.Format("Communication failure to device {0}!", ipAddress));
            }
            return false;
        }
        #endregion

        #region Register user

        public bool RegisterUserOnClockingDevice(string ipAddress, int staffClockId, string cardNumber, string deviceNumber = "1")
        {
            if (FK_ConnectNet(1, ipAddress, 5005, 20000, 0, 0) == 1)
            {
                int vEnrollNumber = staffClockId;
                const int vBackupNumber = 11;
                const int vPrivilege = 0;
                int vnResultCode = FK_PutEnrollDataWithString(1, vEnrollNumber, vBackupNumber, vPrivilege, cardNumber);
                if (vnResultCode == 1)
                {
                    vnResultCode = FK_SaveEnrollData(1);
                    if (vnResultCode == 1)
                    {
                        FK_EnableDevice(1, 1);
                        return true;
                    }
                }
                else
                {

                }
            }
            else
            {
                return false;
            }

            return false;
        }


        #endregion
        #endregion



        /*
          private void ProccessStringData(string inputData, ref List<FaceRecord> dataList)
        {
            if (!String.IsNullOrEmpty(inputData))
            {
                foreach (string line in Regex.Split(inputData, "\r\n"))
                {
                    // time="2011-11-02 13:54:57" id="123456" name="craig" workcode="" status="1" card_src="from_check" 
                    var Record = new FaceRecord();
                    foreach (string item in Regex.Split(line, "\" "))
                    {
                        if (item.StartsWith("time="))
                        {
                            Record.RecordDateTime = DateTime.Parse(item.Substring(6, item.Length - 6));
                        }
                        if (item.StartsWith("id="))
                        {
                            Record.RecordID = item.Substring(4, item.Length - 4);
                        }
                        if (item.StartsWith("name="))
                        {
                            Record.RecordName = item.Substring(6, item.Length - 6);
                        }

                        //We have what we need, move on.
                        if (Record.RecordPopulated)
                        {
                            dataList.Add(Record);
                            break;
                        }
                    }
                }
            }
        }
         */
    }
}