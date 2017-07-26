using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using Newtonsoft.Json;
using System.Linq;

namespace WKFCBusinessRules
{
    public class WKFCLogic
    {
        /// <summary>
        /// Converts a two-digit year to a 4-digit year utilizing CultureInfo.Calendar
        /// </summary>
        /// <param name="userInputYear">The year provided via ABBYY as a string</param>
        /// <returns>The year in four digits as a string</returns>
        public static string AdjustYearBuilt(string userInputYear)
        {
   
            int year = CheckIfNumeric(userInputYear); // Check if the incoming data is actually a number
            if (year >= 0) // Previously had this as > instead of >= which excluded '00 as a year
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
                        return "";
                    }
                }
                else
                {
                    return ""; // Return a empty string to the text box on the verification form
                }
            }
            return "";
        }

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

        /// <summary>
        /// Extract the company database ID number for the insured account from the email subject line.
        /// </summary>
        /// <param name="subjectLine">The email subject line as a string</param>
        /// <returns>The ID number as a string</returns>
        public static string GetControlNumber(string subjectLine)
        {
            if (subjectLine.Length == 0)
                return "";
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
                    return "";
            }
        }

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
                    return "";
                }
            }
            else // Get the entire building number range
            {
                return bldgNums;
            }
        }

        /// <summary>
        /// Returns a complete address split into parts based on the address passed to the method
        /// </summary>
        /// <param name="userInputAddress">The property address as a string</param>
        /// <returns>A custom object (ABBYYLocation) that contains all of the address parts as strings</returns>
        public static ABBYYLocation ParseAddress(string userInputAddress)
        {
            // Spawn a dictionary for the address component name and address to live in
            Dictionary<string, string> addressParts = new Dictionary<string, string>();
            ABBYYLocation location = new ABBYYLocation(); // Spawn a new ABBYYLocation for everything to live in

            string requestUri = "https://maps.googleapis.com/maps/api/geocode/json?sensor=false&address=" + userInputAddress;

            using (WebClient webc = new WebClient())
            {
                // Go forth, my json
                var json = webc.DownloadString(requestUri);
                var locationinfo = JsonConvert.DeserializeObject<LocationInfo>(json);

                if (locationinfo.status != "OK")
                    return location;

                // Now store the address component name as the key and the actual address component as the value
                foreach (var item in locationinfo.results[0].address_components)
                    foreach (var type in item.types)
                        if (!addressParts.ContainsKey(item.types[0]))
                            addressParts.Add(item.types[0].ToString(), item.long_name.ToString());
            }

            // Fill the object with the value contents of the dictionary
            // forgive me padre for i have sinned
            location.singleBldg = (addressParts.ContainsKey("street_number") ? addressParts["street_number"] : "");
            location.st1 = (addressParts.ContainsKey("route") ? addressParts["route"] : "");
            location.st2 = (addressParts.ContainsKey("subpremise") ? addressParts["subpremise"] : "");

            if (!addressParts.ContainsKey("locality"))
                location.city = (addressParts.ContainsKey("political") ? addressParts["political"] : "");
            else if (addressParts.ContainsKey("locality"))
                location.city = addressParts["locality"];

            location.county = (addressParts.ContainsKey("administrative_area_level_2") ? addressParts["administrative_area_level_2"] : "");
            location.state = (addressParts.ContainsKey("administrative_area_level_1") ? addressParts["administrative_area_level_1"] : "");
            location.zip = (addressParts.ContainsKey("postal_code") ? addressParts["postal_code"] : "");

            return location;
        }

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
                return "";
            }
            else
            {
                return finalColValue.ToString();
            }
                        
        }

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
    }
}