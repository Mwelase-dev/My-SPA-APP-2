using System;
using System.Collections.Generic;
using System.Diagnostics;
using Data.FaceID;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClockingDataTests
{
    [TestClass]
    public class ClockingUnitTest
    {
        [TestMethod]
        public void Test_EF_DownloadTemplatesFromDevices()
        {
            IList<FaceUser> results = new List<FaceUser>();

            IList<string> ipAddresses = new List<string>();
            ipAddresses.Add("172.16.0.110");
            ipAddresses.Add("172.16.0.111");
            ipAddresses.Add("172.16.0.112");
            ipAddresses.Add("172.16.0.113");
            ipAddresses.Add("172.16.0.114");

            var test = new FaceID();
            for (int i = 1; i < 150; i++)
            {
                foreach (var ipAddress in ipAddresses)
                {
                    try
                    {
                        var obj = test.DownloadTemplates("1", ipAddress, i.ToString());
                        results.Add(obj);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }

            //Print results
            Debug.WriteLine("=================================");
            foreach (var faceUser in results)
            {
                Debug.WriteLine(faceUser.ToString());
            }
            Debug.WriteLine("=================================");
        }
    }
}
