using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace WKFCBusinessRules
{
    public class WKFCLogic
    {

        #region AdjustYearBuilt
        /// <summary>
        /// Converts a two-digit year to a 4-digit year utilizing CultureInfo.Calendar
        /// </summary>
        /// <param name="userInputYear">The year provided via ABBYY as a string</param>
        /// <returns>The year in four digits as a string</returns>
        public static string AdjustYearBuilt(string userInputYear)
        {
            // Check if the incoming data is actually a number
            int year = CheckIfNumeric(userInputYear);
            // Previously had this as > instead of >= which excluded '00 as a year
            if (year >= 0)
            {
                // Sometimes ABBYY will only read one digit from the form, just skip it if this happens
                if (userInputYear.Length > 1)
                {
                    int fullYear = CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(year);

                    if (fullYear.ToString().Length > 2)
                    {
                        return fullYear.ToString();
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
            return string.Empty;
        }
        #endregion

        #region RemoveCountySuffix
        /// <summary>
        /// Removes the "county" suffix from a given county for database reasons. 
        /// </summary>
        /// <param name="userInputCounty">The county provided via ABBYY as a string</param>
        /// <returns>The county minus its suffix as a string</returns>
        public static string RemoveCountySuffix(string userInputCounty)
        {
            if (userInputCounty.ToLower().Contains("county"))
            {
                int space = userInputCounty.IndexOf(' ');

                // This needed to be modified because there are locations like "New York County"
                // and it would return "New" as the un-countified string. 
                if (space != -1 && userInputCounty.LastIndexOf(' ') == space)
                    return userInputCounty.Substring(0, space);
                else
                    // This handles things like "New York County"
                    return userInputCounty.Substring(0, userInputCounty.LastIndexOf(' '));
            }
            return null;
        }
        #endregion

        #region AdjustProtectionClass
        /// <summary>
        /// Adjusts the protection class to trim any leading zeros from the protection class code.
        /// </summary>
        /// <param name="userInputProtCl">The class code as a string</param>
        /// <returns>The adjusted class code as a string</returns>
        public static string AdjustProtectionClass(string userInputProtCl)
        {
            // If the parameter is two characters long and it's not equal to 10
            if (userInputProtCl.Length == 2 && !userInputProtCl.Equals("10"))
            {
                return userInputProtCl.Substring(1, 1);
            }
            if (userInputProtCl.Equals("10"))
            {
                return "10";
            }
            return null;
        }
        #endregion

        #region GetControlNumber
        /// <summary>
        /// Extract the company database ID number for the insured account from the email subject line.
        /// </summary>
        /// <param name="subjectLine">The email subject line as a string</param>
        /// <returns>The ID number as a string</returns>
        public static string GetControlNumber(string subjectLine)
        {
            if (subjectLine.Length == 0)
                return string.Empty;
            else
            {
                /*
                    * Regex definition:
                    *     ?<= is a lookbehind, of the parent category of lookarounds 
                    *     http://www.regular-expressions.info/lookaround.html
                    *     
                    *     "When matching a group of numbers between 0 and 9, look behind the matching
                    *     character and make sure it falls within the group"
                    *     
                    *     [0-9]{7,} says to match characters that are the digits 0 to 9, 
                    *     but make sure to match at least 7 of those characters
                    *     
                    *     ?= is a lookahead, of the parent category of lookarounds. It repeats the 
                    *     same idea of a lookbehind but in reverse: "When matching a group of numbers,
                    *     look in front of the matching character and make sure it falls within the group"
                    * 
                    * Thus, a control number between brackets, curly braces, or parentheses can be found in a given string
                    */
                Match match = Regex.Match(subjectLine, @"(?<=\[|\(|\{)[0-9]{7,}(?=\]|\}|\))");
                if (match.Success)
                    return match.Value;
                else
                    return string.Empty;
            }
        }
        #endregion

        #region ConvertConstrTypeToInteger
        /// <summary>
        /// This method will convert the construction type to a number based 
        /// on a boolean value
        /// </summary>
        /// <param name="userInputConstrType">The construction type as a string</param>
        /// <returns>The numeric representation of the construction type as a string</returns>
        public static int ConvertConstrTypeToInteger(string userInputConstrType)
        {
            // Don't need to have both an IMS version and a ISO version anymore, just use ISO
            Dictionary<string, int> isoType = new Dictionary<string, int>
            {
                // Modified Fire Resisitive, ISO Number: 5, IMS ID: 6
                {"mfr", 5 }, {"modified fire resistive", 5},

                // Frame, ISO Number: 1, IMS ID: 5
                {"brick frame", 1}, {"frame", 1}, {"brick veneer", 1}, {"frame block", 1},
                {"heavy timber", 1}, {"masonry frame", 1}, {"masonry wood", 1}, {"metal building", 1},
                {"sheet metal", 1}, {"wood", 1}, {"frajne", 1},

                // Joisted Masonry, ISO Number: 2, IMS ID: 4
                {"brick", 2}, {"brick steel", 2}, {"cd", 2}, {"cement", 2}, {"masonry", 2},
                {"masonry timbre", 2}, {"stone", 2}, {"stucco", 2}, {"joist masonry", 2}, {"tilt-up", 2},
                {"jm", 2}, {"joisted masonry", 2}, {"joisted mason", 2}, {"j/masonry", 2}, {"joist mason", 2},

                // Non-Combustible, ISO Number: 3, IMS ID: 3
                {"cb", 3}, {"concrete block", 3}, {"icm", 3}, {"iron clad metal", 3}, {"steel concrete", 3},
                {"steel cmu", 3}, {"non-comb.", 3}, {"non-comb", 3}, {"pole", 3}, {"non-combustible", 3},
                {"non-combustib", 3}, {"metal/aluminum", 3}, {"metal on slab", 3}, {"steel & concr", 3},

                // Masonry Non-Combustible, ISO Number: 4, IMS ID: 2
                {"cement block", 4}, {"cbs", 4}, {"mnc", 4}, {"ctu", 4}, {"concrete tilt-up", 4},
                {"pre-cast com", 4}, {"reinforced concrete", 4}, {"masonry nc", 4}, {"masonry non-c", 4},
                {"masonry non-combustible", 4},

                // Fire Resistive, ISO Number: 6, IMS ID: 1
                {"aaa", 6}, {"fire resistive", 6}, {"cinder block", 6}, {"steel", 6}, {"steel frame", 6},
                {"superior", 6}, {"w/r", 6}, {"fire resist", 6}, {"fire resistiv", 6}, {"fr", 6},
                {"fire resitive", 6}
            };
            foreach (KeyValuePair<string, int> isoPair in isoType)
            {
                if (userInputConstrType.ToLower().Equals(isoPair.Key))
                    return isoPair.Value;
            }
            return 0;
        }
        #endregion

        #region GetBuildingNumber
        /// <summary>
        /// Extracts the building number from the street address
        /// </summary>
        /// <param name="userInputStreet">The street as a string</param>
        /// <param name="getFirst">True to only get the first building number, false for the whole range</param>
        /// <returns>The building number as a string</returns>
        public static string GetBuildingNumber(string userInputStreet, bool getFirst)
        {
            // A building range will always be separated from the rest of the street by a space
            int space = userInputStreet.IndexOf(" ");
            // Find the index of the separating dash
            string bldgNums;

            bldgNums = userInputStreet.Substring(0, space);

            if (getFirst) // Get the first building number
            {
                Regex rgx = new Regex(@"^(\w*)|^(\w*\s)");
                var match = rgx.Match(bldgNums);
                if (match.Success)
                {
                    return match.Value;
                }
                else
                {
                    return string.Empty;
                }
            }
            else // Get the entire building number range
            {
                return bldgNums;
            }
        }
        #endregion

        #region ParseAddress
        /// <summary>
        /// Returns a complete address split into parts based on the address passed to the method
        /// </summary>
        /// <param name="userInputAddress">The property address as a string</param>
        /// <returns>A custom object (ABBYYLocation) that contains all of the address parts as strings</returns>
        public static ABBYYLocation ParseAddress(string userInputAddress)
        {
            ABBYYLocation location;

            string requestUri = "https://maps.googleapis.com/maps/api/geocode/json?sensor=false&address=" + userInputAddress;

            using (WebClient webc = new WebClient())
            {
                // Go forth, my json
                var json = webc.DownloadString(requestUri);
                JObject geocodeResults = JObject.Parse(json);
                if ((string)geocodeResults["status"] != "OK")
                {
                    location = new ABBYYLocation
                    {
                        SingleBldg = string.Empty,
                        Street1 = string.Empty,
                        Street2 = string.Empty,
                        County = string.Empty,
                        City = string.Empty,
                        State = string.Empty,
                        Zip = string.Empty
                    };
                }
                else
                {
                    location = new ABBYYLocation
                    {
                        SingleBldg = GetAddressPiece(geocodeResults, "street_number"),
                        Street1 = GetAddressPiece(geocodeResults, "route"),
                        Street2 = GetAddressPiece(geocodeResults, "subpremise"),
                        County = GetAddressPiece(geocodeResults, "administrative_area_level_2"),
                        City = GetAddressPiece(geocodeResults, "locality"),
                        State = GetAddressPiece(geocodeResults, "administrative_area_level_1"),
                        Zip = GetAddressPiece(geocodeResults, "postal_code")
                    };
                }
                return location;
            }
        }
        #endregion

        #region GetValueFromAmtCol
        /// <summary>
        /// Returns a parsed and stripped numerical value from the "amount" column
        /// </summary>
        /// <param name="colValue">The monetary value as a string</param>
        /// <returns>The parsed and cleaned amount as a string</returns>
        public static string GetValueFromAmtCol(string colValue)
        {
            // TODO: Reimplement method to handle input values like "34M" being 34,000,000 and "10k" being 10,000
            Regex rgx = new Regex(@"(\$*,*\s*)");
            string cleanColValue = rgx.Replace(colValue, "");

            int finalColValue = CheckIfNumeric(cleanColValue);

            if (finalColValue == -1)
            {
                return string.Empty;
            }
            else
            {
                return finalColValue.ToString();
            }
        }
        #endregion

        #region RoundStories
        /// <summary>
        /// If the broker writes a decimal for the number of stories, round that value up
        /// </summary>
        /// <param name="userInputStories">The number of stories as a string</param>
        /// <returns>The rounded value as an integer</returns>
        public static int RoundStories(string userInputStories)
        {
            bool isNumber = Double.TryParse(userInputStories, out double parsedStories);
            if (isNumber)
            {
                parsedStories = Math.Ceiling(parsedStories);
                return Convert.ToInt32(parsedStories);
            }
            return 0;
        }
        #endregion

        #region SplitAddressByNewline
        /// <summary>
        /// Splits an incoming address by the used newline character (\n, \r\n, env.newline, etc.).
        /// </summary>
        /// <param name="addressInput">The address as a string</param>
        /// <returns>String array of each line of the string separated by a return of some kind</returns>
        public static AcordAddress SplitAddressByNewline(string addressInput)
        {
            // Array to hold the address parts before it's appended to the object
            string[] finalAddress;
            // Just reuse this, I think it's ok? Haven't consulted the council of elders
            Match match;
            AcordAddress acordAddress = new AcordAddress();

            // Split the incoming address string by new lines
            string[] splitInput = addressInput.Split(new string[] { "\n", "\r\n", Environment.NewLine }, StringSplitOptions.None);

            Regex dbaRegex = new Regex(@"dba|c/o");
            match = dbaRegex.Match(splitInput[1].ToLower());

            if (match.Success)
            {
                acordAddress.Insured = $"{splitInput[0]} {splitInput[1]}";
            }
            else
            {
                acordAddress.Insured = splitInput[0];
            }

            // Get the last position in the the array holding the split address, since that's
            // guaranteed to be the City/State/Zip portion of the address
            int last = splitInput.Length - 1;

            Regex suiteRegex = new Regex(@"suite|ste|#\w\w\w|#\s\w\w\w");
            // Match the line above the last because that will be our "weird" line:
            // The street will be above this line if the suite has its own dedicated line
            match = suiteRegex.Match(splitInput[last - 1].ToLower());

            if (match.Success)
            {
                acordAddress.Address = finalAddress = new string[] { splitInput[last - 2], splitInput[last - 1], splitInput[last] };
            }
            else
            {
                acordAddress.Address = finalAddress = new string[] { splitInput[last - 1], splitInput[last] };
            }

            return acordAddress;
        }
        #endregion

        #region BuildSubmissionID
        /// <summary>
        /// Creates a submission ID based on the name of the Flexicapture batch
        /// </summary>
        /// <param name="batchName">The name of the batch as a string</param>
        /// <returns>The submission ID as a string</returns>
        public static string BuildSubmissionID(string batchName)
        {
            Match match = Regex.Match(batchName, @"ID\d*");

            if (match.Success)
                return match.Value;
            else
                return string.Empty;
        }
        #endregion

        #region CheckIfNumeric
        /// <summary>
        /// TryParse wrapper
        /// </summary>
        /// <param name="userInputNumber">The result from OCR as a string</param>
        /// <returns>-1 if not valid, the number if valid</returns>
        private static int CheckIfNumeric(string userInputNumber)
        {
            bool isNumber = Int32.TryParse(userInputNumber, out int number);

            if (isNumber)
                return number;
            else
                return -1;
        }
        #endregion

        #region GetAddressPiece
        /// <summary>
        /// Gets the part of the address we need from the returned JSON
        /// </summary>
        /// <param name="jsonObj">The google geocode json as a Json.Net JObject</param>
        /// <param name="addrComponentType">The type of address part as a string</param>
        /// <returns>The returned value as a string</returns>
        private static string GetAddressPiece(JObject jsonObj, string addrComponentType)
        {
            /*
                * LINQ documentation:
                *  1. From the json object returned, select the address components, located
                *  in the first index of the results
                *  
                *  2. Where the first item of the "types" array in the address component is
                *  equal to the argument supplied in addrComponentType
                *  
                *  3. Select the first item within the "long_name" array
                *    
                *  4. If the field is empty or doesn't exist, default to returning an empty string
                *  
                *  5. Select the first item available
                *  
                *  6. Convert it to a string from a JToken object
                */
            string result = jsonObj["results"][0]["address_components"]
                                .Where(x => (string)x["types"][0] == $"{addrComponentType}")
                                .Select(x => x["long_name"])
                                .DefaultIfEmpty(string.Empty)
                                .First()
                                .ToString();

            return result;
        }
        #endregion
    }
}