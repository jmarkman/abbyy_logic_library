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
            try
            {
                if (userInputYear.Length > 1)
                {
                    int fullYear = CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(Int32.Parse(userInputYear));

                    return fullYear.ToString();
                }
                else
                {
                    return ""; // Return a blank string to the text box on the verification form
                }
            }
            catch (ArgumentOutOfRangeException arnge)
            {
                arnge.ToString(); // Not graceful, but I don't expect this to happen often enough
            }
            return null;
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
                 *     ?<= is a lookbehind, of the parent category of lookarounds http://www.regular-expressions.info/lookaround.html
                 *     "When matching a group of numbers between 0 and 9, look behind the matching character and make sure it falls within the group"
                 *     [0-9]{7,} says to match characters that are the digits 0 to 9, but make sure to match at least 7 of those characters"
                 *     ?= is a lookahead, of the parent category of lookarounds. It repeats the same idea of a lookbehind but in reverse:
                 *     "When matching a group of numbers, look in front of the matching character and make sure it falls within the group"
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
        /// <param name="isUsingIMS">Boolean value: true uses the company internal format</param>
        /// <returns>The numeric representation of the construction type as a string</returns>
        public static string ConvertConstrTypeToInteger(string userInputConstrType, bool isUsingIMS)
        {
            // This is how we classify each construction type
            Dictionary<string, int> imsType = new Dictionary<string, int>
            {
                // Modified Fire Resisitive, ISO Number: 5, IMS ID: 6
                {"mfr", 6 }, {"modified fire resistive", 6},

                // Frame, ISO Number: 1, IMS ID: 5
                {"brick frame", 5}, {"frame", 5}, {"brick veneer", 5}, {"frame block", 5},
                {"heavy timber", 5}, {"masonry frame", 5}, {"masonry wood", 5}, {"metal building", 5},
                {"sheet metal", 5}, {"wood", 5}, 

                // Joisted Masonry, ISO Number: 2, IMS ID: 4
                {"brick", 4}, {"brick steel", 4}, {"cd", 4}, {"cement", 4}, {"masonry", 4}, {"masonry timbre", 4},
                {"stone", 4}, {"stucco", 4}, {"joist masonry", 4}, {"tilt-up", 4}, {"jm", 4}, {"joisted masonry", 4},
                {"joisted mason", 4}, {"j/masonry", 4}, {"joist mason", 4},

                // Non-Combustible, ISO Number: 3, IMS ID: 3
                {"cb", 3}, {"concrete block", 3}, {"icm", 3}, {"iron clad metal", 3}, {"steel concrete", 3},
                {"steel cmu", 3}, {"non-comb.", 3}, {"non-comb", 3}, {"pole", 3}, {"non-combustible", 3},
                {"non-combustib", 3}, {"metal/aluminum", 3}, {"metal / aluminum", 3}, {"metal on slab", 3},
                {"steel & concr", 3},

                // Masonry Non-Combustible, ISO Number: 4, IMS ID: 2
                {"cement block", 2}, {"cbs", 2}, {"mnc", 2}, {"ctu", 2}, {"concrete tilt-up", 2}, {"pre-cast com", 2},
                {"reinforced concrete", 2}, {"masonry nc", 2}, {"masonry non-c", 2}, {"masonry non-combustible", 2},

                // Fire Resistive, ISO Number: 6, IMS ID: 1
                {"aaa", 1}, {"fire resistive", 1}, {"cinder block", 1}, {"steel", 1}, {"steel frame", 1},
                {"superior", 1}, {"w/r", 1}, {"fire resist", 1}, {"fire resistiv", 1}, {"fr", 1}, {"fire resitive", 1}
            };

            // This is how the international standard does it
            Dictionary<string, int> isoType = new Dictionary<string, int>
            {
                // Modified Fire Resisitive, ISO Number: 5, IMS ID: 6
                {"mfr", 5 }, {"modified fire resistive", 5},

                // Frame, ISO Number: 1, IMS ID: 5
                {"brick frame", 1}, {"frame", 1}, {"brick veneer", 1}, {"frame block", 1},
                {"heavy timber", 1}, {"masonry frame", 1}, {"masonry wood", 1}, {"metal building", 1},
                {"sheet metal", 1}, {"wood", 1}, {"metal/aluminum", 1},

                // Joisted Masonry, ISO Number: 2, IMS ID: 4
                {"brick", 2}, {"brick steel", 2}, {"cd", 2}, {"cement", 2}, {"masonry", 2}, {"masonry timbre", 2},
                {"stone", 2}, {"stucco", 2}, {"joist masonry", 2}, {"tilt-up", 2}, {"jm", 2}, {"joisted masonry", 2},
                {"joisted mason", 2}, {"j/masonry", 2}, {"joist mason", 2},

                // Non-Combustible, ISO Number: 3, IMS ID: 3
                {"cb", 3}, {"concrete block", 3}, {"icm", 3}, {"iron clad metal", 3}, {"steel concrete", 3},
                {"steel cmu", 3}, {"non-comb.", 3}, {"non-comb", 3}, {"pole", 3}, {"non-combustible", 3},
                {"non-combustib", 3}, {"metal/aluminum", 3}, {"metal / aluminum", 3}, {"metal on slab", 3},
                {"steel & concr", 3},

                // Masonry Non-Combustible, ISO Number: 4, IMS ID: 2
                {"cement block", 4}, {"cbs", 4}, {"mnc", 4}, {"ctu", 4}, {"concrete tilt-up", 4}, {"pre-cast com", 4},
                {"reinforced concrete", 4}, {"masonry nc", 4}, {"masonry non-c", 4}, {"masonry non-combustible", 4},

                // Fire Resistive, ISO Number: 6, IMS ID: 1
                {"aaa", 6}, {"fire resistive", 6}, {"cinder block", 6}, {"steel", 6}, {"steel frame", 6},
                {"superior", 6}, {"w/r", 6}, {"fire resist", 6}, {"fire resistiv", 6}, {"fr", 6}, {"fire resitive", 6}
            };

            if (isUsingIMS)
            {
                foreach (KeyValuePair<string, int> imsPair in imsType)
                {
                    if (userInputConstrType.ToLower().Equals(imsPair.Key))
                        return imsPair.Value.ToString();
                }
            }
            else
            {
                foreach (KeyValuePair<string, int> isoPair in isoType)
                {
                    if (userInputConstrType.ToLower().Equals(isoPair.Key))
                        return isoPair.Value.ToString();
                }
            }
            return null;
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
        /// Returns a parsed and stripped numerical value from the "amount" column of the Acord 140 Premises Info table
        /// </summary>
        /// <param name="colValue">The monetary value as a string</param>
        /// <returns>The parsed and cleaned amount as a string</returns>
        public static string GetValueFromAmtCol(string colValue)
        {
            // TODO: Reimplement method to handle input values like "34M" being 34,000,000 and "10k" being 10,000
            Regex rgx = new Regex(@"(\$*,*\s*)");
            string cleanColValue = rgx.Replace(colValue, "");

            bool isNumber = System.Int32.TryParse(cleanColValue, out int amount);
            if (!isNumber)
            {
                amount = 0;
            }
            return amount.ToString();                
        }

         /// <summary>
        /// If "3A" was input as a building number, it caused database input exceptions. I want to delete this ASAP.
        /// </summary>
        /// <param name="userInputNumber">The result from OCR as a string</param>
        /// <returns>Null if not valid, a string if valid</returns>
        public static string CheckIfNumeric(string userInputNumber)
        {
            bool isNumber;
            int number;

            isNumber = Int32.TryParse(userInputNumber, out number);

            if (isNumber)
                return number.ToString();
            else
                return null;
        }
        
    }
}