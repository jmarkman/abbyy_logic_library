using Microsoft.VisualStudio.TestTools.UnitTesting;
using WKFCBusinessRules;

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
            string newYork = "New York County";

            string result1 = WKFCLogic.RemoveCountySuffix(queens);
            string result2 = WKFCLogic.RemoveCountySuffix(bronx);
            string result3 = WKFCLogic.RemoveCountySuffix(brooklyn);
            string result4 = WKFCLogic.RemoveCountySuffix(newYork);

            Assert.AreEqual("Queens", result1);
            Assert.AreEqual("Bronx", result2);
            Assert.AreEqual("Kings", result3);
            Assert.AreEqual("New York", result4);
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

        [TestMethod]
        public void DoesConstructionTypeConversionWork()
        {
            string testConstType1 = "joist mason";
            string testConstType2 = "sheet metal";
            string testConstType3 = "mnc";

            string result1 = WKFCLogic.ConvertConstrTypeToInteger(testConstType1, true);
            string result2 = WKFCLogic.ConvertConstrTypeToInteger(testConstType2, true);
            string result3 = WKFCLogic.ConvertConstrTypeToInteger(testConstType3, true);

            string result4 = WKFCLogic.ConvertConstrTypeToInteger(testConstType1, false);
            string result5 = WKFCLogic.ConvertConstrTypeToInteger(testConstType2, false);
            string result6 = WKFCLogic.ConvertConstrTypeToInteger(testConstType3, false);

            Assert.AreEqual("4", result1);
            Assert.AreEqual("5", result2);
            Assert.AreEqual("2", result3);

            Assert.AreEqual("2", result4);
            Assert.AreEqual("1", result5);
            Assert.AreEqual("4", result6);
        }

        [TestMethod]
        public void DoesBuildingNumberExtractionWork()
        {
            string bldgStreet1 = "137-141-143  ST JAMES ST";
            string bldgStreet2 = "27 WATERFORD POINTE CIR,";
            string bldgStreet3 = "7419-7461 W. Colonial Drive";

            string result1 = WKFCLogic.GetBuildingNumber(bldgStreet1, true);
            string result2 = WKFCLogic.GetBuildingNumber(bldgStreet2, true);
            string result3 = WKFCLogic.GetBuildingNumber(bldgStreet3, true);

            Assert.AreEqual("137", result1);
            Assert.AreEqual("27", result2);
            Assert.AreEqual("7419", result3);
        }

        [TestMethod]
        public void DoesGeocodingReturnProperResults()
        {
            string addressInput = "203 S St Marys Ste 170 San Antonio TX 78205";

            var results = WKFCLogic.ParseAddress(addressInput);
            
            Assert.IsInstanceOfType(results, typeof(ABBYYLocation));
            Assert.AreEqual("203", results.singleBldg);
            Assert.AreEqual("South Saint Mary's Street", results.st1);
            Assert.AreEqual("170", results.st2);
            Assert.AreEqual("San Antonio", results.city);
            Assert.AreEqual("Bexar County", results.county);
            Assert.AreEqual("Texas", results.state);
            Assert.AreEqual("78205", results.zip);

        }

        [TestMethod]
        public void CanWePerformBasicAddition()
        {
            string bv = "$112,3555";
            string bpp = "$234 65";
            string bi = "33,334";
            string mp = "0";

            var results = WKFCLogic.sumTIV(bv, bpp, bi, mp);

            Assert.AreEqual("1180354", results);
        }

        [TestMethod]
        public void DoesBldgNumRangeExtractionWorkAsIntended()
        {
            string addr1 = "137-141-143  ST JAMES ST";
            string addr2 = "27 WATERFORD POINTE CIR,";
            string addr3 = "7419-7461 W. Colonial Drive";

            string result1 = WKFCLogic.GetBuildingNumber(addr1, false);
            string result2 = WKFCLogic.GetBuildingNumber(addr2, false);
            string result3 = WKFCLogic.GetBuildingNumber(addr3, false);

            Assert.AreEqual("137-141-143", result1);
            Assert.AreEqual("27", result2);
            Assert.AreEqual("7419-7461", result3);
        }
    }
}
