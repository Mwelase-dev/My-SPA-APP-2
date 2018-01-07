using System;
using Intranet.PhoneUsage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.CDRData
{
    [TestClass]
    public class CDRUnitTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            foreach (var cdrProvider in CDRAccessLayer.CDRProviders(new DateTime(), new DateTime()))
            {
                Console.WriteLine(cdrProvider);
            }
        }
    }
}
