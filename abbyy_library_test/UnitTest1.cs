using Microsoft.VisualStudio.TestTools.UnitTesting;
using WKFCBusinessRules;
using static WKFCBusinessRules.WKFCLogic;

namespace abbyy_library_test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestAdjustYearBuilt()
        {
            string testInputYear1 = "00";
            string testInputYear2 = "07";
            string testInputYear3 = "11";
            string testInputYear4 = "15";
            string testInputYear5 = "45";
            string testInputYear6 = "99";
            string testInputYear7 = "05";

            string year1 = AdjustYearBuilt(testInputYear1);
            string year2 = AdjustYearBuilt(testInputYear2);
            string year3 = AdjustYearBuilt(testInputYear3);
            string year4 = AdjustYearBuilt(testInputYear4);
            string year5 = AdjustYearBuilt(testInputYear5);
            string year6 = AdjustYearBuilt(testInputYear6);
            string year7 = AdjustYearBuilt(testInputYear7);

            /*
             * MSDN defines the property TwoDigitYearMax as allowing "a 2-digit year
             * to be properly translated to a 4-digit year". Essentially, the property
             * establishes a cutoff point for year conversion within a 100-year range.
             * If we manually set the property instead of getting the current culture,
             * we can set it to 2029.
             * 
             * If we pass "30" to ToFourDigitYear(), it spits out 1930. If we pass "29"
             * to ToFourDigitYear(), we get 2029. The property basically decides whether
             * or not to expand the abbreviation to a year in the 1900s or a year in the
             * 2000s.
             * 
             * According to the current default culture, our cutoff point is 2029 like
             * the MSDN example.
             * 
             * Relevant docs:
             * https://msdn.microsoft.com/en-us/library/system.globalization.calendar.tofourdigityear(v=vs.110).aspx
             * https://msdn.microsoft.com/en-us/library/system.globalization.calendar.twodigityearmax(v=vs.110).aspx
             */
            Assert.AreEqual("2000", year1);
            Assert.AreEqual("2007", year2);
            Assert.AreEqual("2011", year3);
            Assert.AreEqual("2015", year4);
            Assert.AreEqual("1945", year5);
            Assert.AreEqual("1999", year6);
            Assert.AreNotEqual("1905", year7);
        }

        [TestMethod]
        public void TestRemoveCountySuffix()
        {
            /*
             * A requirement of the WKFC database system is that the suffix "county" 
             * be removed from any county submitted. Don't know why, just is.
             */ 
            string queens = "Queens County";
            string bronx = "Bronx County";
            string brooklyn = "Kings County";
            string newYork = "New York County";

            string result1 = RemoveCountySuffix(queens);
            string result2 = RemoveCountySuffix(bronx);
            string result3 = RemoveCountySuffix(brooklyn);
            string result4 = RemoveCountySuffix(newYork);

            Assert.AreEqual("Queens", result1);
            Assert.AreEqual("Bronx", result2);
            Assert.AreEqual("Kings", result3);
            Assert.AreEqual("New York", result4);
        }

        [TestMethod]
        public void TestAdjustProtectionClass()
        {
            /*
             * Protection class is a rating that says how well or poorly protected a given
             * location is from a fire. Our company has all protection class codes 1-9 
             * listed as singular digits, but sometimes they come in from insurance brokers as "01". 
             * 
             * https://github.com/jmarkman/abbyy_logic_library/wiki/AdjustProtectionClass
             */
            string protcl1 = "05";
            string protcl2 = "03";
            string protcl3 = "10";

            string result1 = AdjustProtectionClass(protcl1);
            string result2 = AdjustProtectionClass(protcl2);
            string result3 = AdjustProtectionClass(protcl3);

            Assert.AreEqual("5", result1);
            Assert.AreEqual("3", result2);
            Assert.AreEqual("10", result3);
        }

        [TestMethod]
        public void TestGetControlNumber()
        {
            string testSubject1 = "[1112345] Some Homeowners Association";
            string testSubject2 = "{1231246} A Condominium Somewhere";
            string testSubject3 = "(2436531) The Bodega where Omar Little from The Wire Got Shot";
            string testSubject4 = "submission 1: {2341234} laguna 140";
            string testSubject5 = "11541423 An Apartment Complex";
            string testSubject6 = "";
            string testSubject7 = "(1234678) Edward L Donaldson DDS";
            string testSubject8 = "(2345678) Graham Village Apartments, LTD";

            string result1 = GetControlNumber(testSubject1);
            string result2 = GetControlNumber(testSubject2);
            string result3 = GetControlNumber(testSubject3);
            string result4 = GetControlNumber(testSubject4);
            string result5 = GetControlNumber(testSubject5);
            string result6 = GetControlNumber(testSubject6);
            string result7 = GetControlNumber(testSubject7);
            string result8 = GetControlNumber(testSubject8);

            Assert.AreEqual("1112345", result1);
            Assert.AreEqual("1231246", result2);
            Assert.AreEqual("2436531", result3);
            Assert.AreEqual("2341234", result4);
            Assert.AreEqual("", result5);
            Assert.AreEqual("", result6);
            Assert.AreEqual("1234678", result7);
            Assert.AreEqual("2345678", result8);
            Assert.AreNotEqual("123456", result1);
        }

        [TestMethod]
        public void TestConvertConstrTypeToInteger()
        {
            string testConstType1 = "joist mason";
            string testConstType2 = "sheet metal";
            string testConstType3 = "mnc";

            int result1 = ConvertConstrTypeToInteger(testConstType1);
            int result2 = ConvertConstrTypeToInteger(testConstType2);
            int result3 = ConvertConstrTypeToInteger(testConstType3);

            Assert.AreEqual(2, result1);
            Assert.AreEqual(1, result2);
            Assert.AreEqual(4, result3);
        }

        [TestMethod]
        public void TestGetBuildingNumber()
        {
            string bldgStreet1 = "137-141-143  ST JAMES ST";
            string bldgStreet2 = "27 WATERFORD POINTE CIR,";
            string bldgStreet3 = "7419-7461 W. Colonial Drive";

            string result1 = GetBuildingNumber(bldgStreet1, true);
            string result2 = GetBuildingNumber(bldgStreet2, true);
            string result3 = GetBuildingNumber(bldgStreet3, true);

            Assert.AreEqual("137", result1);
            Assert.AreEqual("27", result2);
            Assert.AreEqual("7419", result3);
        }

        [TestMethod]
        public void TestGetBuildingNumberFull()
        {
            string bldgStreet1 = "137-141-143  ST JAMES ST";
            string bldgStreet2 = "27 WATERFORD POINTE CIR,";
            string bldgStreet3 = "7419-7461 W. Colonial Drive";

            string result1 = GetBuildingNumber(bldgStreet1, false);
            string result2 = GetBuildingNumber(bldgStreet2, false);
            string result3 = GetBuildingNumber(bldgStreet3, false);

            Assert.AreEqual("137-141-143", result1);
            Assert.AreEqual("27", result2);
            Assert.AreEqual("7419-7461", result3);
        }

        [TestMethod]
        public void TestParseAddress()
        {
            string addressInput = "203 S St Marys Ste 170 San Antonio TX 78205";

            var results = ParseAddress(addressInput);
            
            Assert.IsInstanceOfType(results, typeof(ABBYYLocation));
            Assert.AreEqual("203", results.SingleBldg);
            Assert.AreEqual("South Saint Mary's Street", results.Street1);
            Assert.AreEqual("170", results.Street2);
            Assert.AreEqual("San Antonio", results.City);
            Assert.AreEqual("Bexar County", results.County);
            Assert.AreEqual("Texas", results.State);
            Assert.AreEqual("78205", results.Zip);
        }

        [TestMethod]
        public void TestParseAddress1()
        {
            string address = "39 North Fullerton, Montclair, NJ 07042";
            var results = ParseAddress(address);
            Assert.AreEqual("39", results.SingleBldg);
            Assert.AreEqual("North Fullerton Avenue", results.Street1);
            Assert.AreEqual("Montclair", results.City);
            Assert.AreEqual("Essex County", results.County);
            Assert.AreEqual("New Jersey", results.State);
            Assert.AreEqual("07042", results.Zip);
        }

        [TestMethod]
        public void TestGetValueFromAmtCol()
        {
            /*
             * Since automatic comma addition is trivial when it comes to Excel and
             * other WKFC systems like Appia, trim out any extraneous symbols including
             * dollar signs and commas so there's as little to "misread" as possible
             */ 
            string testValue1 = "$1,300,250";
            string testValue2 = "45,000";
            string testValue3 = "10k";

            string result1 = GetValueFromAmtCol(testValue1);
            string result2 = GetValueFromAmtCol(testValue2);
            string result3 = GetValueFromAmtCol(testValue3);

            Assert.AreEqual("1300250", result1);
            Assert.AreEqual("45000", result2);
            Assert.AreNotEqual("10000", result3);
            Assert.AreEqual("", result3);
        }

        [TestMethod]
        public void TestRoundStories()
        {
            /*
             * For some reason, brokers sometimes send us Acords where they have
             * the number of stories written as "2.5". I have no idea how you can
             * have half a story in a building unless it's a FEATURE. Regardless,
             * our business rules state that when given these scenarios, we round
             * up to the nearest whole total number of stories no matter what number
             * the decimal is. If it's 5.2 stories, it rounds up to 6. If it's 3.7,
             * it rounds up to 4.
             */ 
            string testStories1 = "2.6";
            string testStories2 = "3.5";
            string testStories3 = "1.2";

            int results1 = RoundStories(testStories1);
            int results2 = RoundStories(testStories2);
            int results3 = RoundStories(testStories3);

            Assert.AreEqual(3, results1);
            Assert.AreEqual(4, results2);
            Assert.AreEqual(2, results3);
        }

        [TestMethod]
        public void DoesGeocodeReturnNullIfStatusIsNotOK()
        {
            /*
             * The returning object from the ParseAddress() method should actually
             * not return null. Instead, the object should consist of empty strings.
             */
            string addressInput = "ahgggggggggggggggg";

            var results = ParseAddress(addressInput);

            Assert.IsInstanceOfType(results, typeof(ABBYYLocation));
            Assert.IsNotNull(results);
            Assert.AreEqual("", results.SingleBldg);
        }

        [TestMethod]
        public void SplitAddressByNewlineTest_BestPossibleInput()
        {
            string testAddress1 = "Reserve Investors, LLC, DBA: Reserve at Stockbridge\n115 Rock Quarry Rd\nStockbridge GA 30281";

            var results = SplitAddressByNewline(testAddress1);

            Assert.AreEqual(results.Insured, "Reserve Investors, LLC, DBA: Reserve at Stockbridge");
            Assert.AreEqual(results.Address[0], "115 Rock Quarry Rd");
            Assert.AreEqual(results.Address[1], "Stockbridge GA 30281");
        }

        [TestMethod]
        public void BuildSubmissionIDTest()
        {
            string testBatchName = "Batch HF_ID1136";

            var subID = BuildSubmissionID(testBatchName);

            Assert.AreEqual("ID1136", subID);
        }
    }
}
