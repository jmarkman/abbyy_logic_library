using Microsoft.VisualStudio.TestTools.UnitTesting;
using abbyy_logic_library;
using System.Collections.Generic;

namespace abbyy_library_test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void DoesYearBuiltReturnNewMilleniumYears()
        {
            string testInputYear1 = "00";
            string testInputYear2 = "07";
            string testInputYear3 = "11";
            string testInputYear4 = "15";
            string testInputYear5 = "45";
            string testInputYear6 = "99";

            string year1 = WKFCLogic.AdjustYearBuilt(testInputYear1);
            string year2 = WKFCLogic.AdjustYearBuilt(testInputYear2);
            string year3 = WKFCLogic.AdjustYearBuilt(testInputYear3);
            string year4 = WKFCLogic.AdjustYearBuilt(testInputYear4);
            string year5 = WKFCLogic.AdjustYearBuilt(testInputYear5);
            string year6 = WKFCLogic.AdjustYearBuilt(testInputYear6);

            Assert.AreEqual("2000", year1);
            Assert.AreEqual("2007", year2);
            Assert.AreEqual("2011", year3);
            Assert.AreEqual("2015", year4);
            Assert.AreEqual("1945", year5);
            Assert.AreEqual("1999", year6);
        }

        [TestMethod]
        public void DoesCountySuffixRemoveCounty()
        {
            string queens = "Queens County";
            string bronx = "Bronx County";
            string brooklyn = "Kings County";

            string result1 = WKFCLogic.RemoveCountySuffix(queens);
            string result2 = WKFCLogic.RemoveCountySuffix(bronx);
            string result3 = WKFCLogic.RemoveCountySuffix(brooklyn);

            Assert.AreEqual("Queens", result1);
            Assert.AreEqual("Bronx", result2);
            Assert.AreEqual("Kings", result3);
        }

        [TestMethod]
        public void DoesProtClMethodAlterProtCl()
        {
            string protcl1 = "05";
            string protcl2 = "03";
            string protcl3 = "10";

            string result1 = WKFCLogic.AdjustProtectionClass(protcl1);
            string result2 = WKFCLogic.AdjustProtectionClass(protcl2);
            string result3 = WKFCLogic.AdjustProtectionClass(protcl3);

            Assert.AreEqual("5", result1);
            Assert.AreEqual("3", result2);
            Assert.AreEqual("10", result3);
        }

        [TestMethod]
        public void DoesControlNumExtractionWork()
        {
            string testSubject1 = "FW: [111234] Some Homeowners Association";
            string testSubject2 = "{1231246} A Condominium Somewhere";
            string testSubject3 = "(2436531) The Bodega where Omar Little from The Wire Got Shot";

            string result1 = WKFCLogic.GetControlNumber(testSubject1);
            string result2 = WKFCLogic.GetControlNumber(testSubject2);
            string result3 = WKFCLogic.GetControlNumber(testSubject3);

            Assert.AreEqual("111234", result1);
            Assert.AreEqual("1231246", result2);
            Assert.AreEqual("2436531", result3);
            Assert.AreNotEqual("123456", result1);
        }


    }
}
