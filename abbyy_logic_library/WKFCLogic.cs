using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using Newtonsoft.Json;

namespace WKFCBusinessRules
{
    public class WKFCLogic
    {
        /// <summary>
        /// Converts a two-digit year to a 4-digit year utilizing CultureInfo.Calendar's ToFourDigitYear() method
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
        /// Removes the "county" suffix from a given county according to how the WKFC data entry rules work. 
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
            // We told the office to only use square brackets but that's got a snowball's chance in hell of happening
            char[] leftEnclosures = { '(', '{', '[' };
            char[] rightEnclosures = { ')', '}', ']' };
            string controlNumber = "";

            int openEnclosure = subjectLine.IndexOfAny(leftEnclosures);
            int closeEnclosure = subjectLine.IndexOfAny(rightEnclosures);

            try
            {
                controlNumber = subjectLine.Substring(openEnclosure + 1, (closeEnclosure - openEnclosure) - 1);
                return controlNumber;
            }
            catch (ArgumentOutOfRangeException rangeExc)
            {
                rangeExc.ToString();
            }
            return null;
        }

        /// <summary>
        /// This method will convert the construction type provided to a number based on a supporting boolean value.
        /// </summary>
        /// <param name="userInputConstrType">The construction type as a string</param>
        /// <param name="isUsingIMS">Boolean value: passing true uses the company internal format</param>
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
                {"sheet metal", 5}, {"wood", 5}, {"metal/aluminum", 5},

                // Joisted Masonry, ISO Number: 2, IMS ID: 4
                {"brick", 4}, {"brick steel", 4}, {"cd", 4}, {"cement", 4}, {"masonry", 4}, {"masonry timbre", 4},
                {"stone", 4}, {"stucco", 4}, {"joist masonry", 4}, {"tilt-up", 4}, {"jm", 4}, {"joisted masonry", 4},
                {"joisted mason", 4}, {"j/masonry", 4}, {"joist mason", 4},

                // Non-Combustible, ISO Number: 3, IMS ID: 3
                {"cb", 3}, {"concrete block", 3}, {"icm", 3}, {"iron clad metal", 3}, {"steel concrete", 3},
                {"steel cmu", 3}, {"non-comb.", 3}, {"non-comb", 3}, {"pole", 3}, {"non-combustible", 3},
                {"non-combustib", 3},

                // Masonry Non-Combustible, ISO Number: 4, IMS ID: 2
                {"cement block", 2}, {"cbs", 2}, {"mnc", 2}, {"ctu", 2}, {"concrete tilt-up", 2}, {"pre-cast com", 2},
                {"reinforced concrete", 2}, {"masonry nc", 2}, {"masonry non-c", 2}, {"masonry non-combustible", 2},

                // Fire Resistive, ISO Number: 6, IMS ID: 1
                {"aaa", 1}, {"fire resistive", 1}, {"cinder block", 1}, {"steel", 1}, {"steel frame", 1},
                {"superior", 1}, {"w/r", 1}, {"fire resist", 1}, {"fire resistiv", 1}, {"fr", 1}
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
                {"non-combustib", 3},

                // Masonry Non-Combustible, ISO Number: 4, IMS ID: 2
                {"cement block", 4}, {"cbs", 4}, {"mnc", 4}, {"ctu", 4}, {"concrete tilt-up", 4}, {"pre-cast com", 4},
                {"reinforced concrete", 4}, {"masonry nc", 4}, {"masonry non-c", 4}, {"masonry non-combustible", 4},

                // Fire Resistive, ISO Number: 6, IMS ID: 1
                {"aaa", 6}, {"fire resistive", 6}, {"cinder block", 6}, {"steel", 6}, {"steel frame", 6},
                {"superior", 6}, {"w/r", 6}, {"fire resist", 6}, {"fire resistiv", 6}, {"fr", 6}
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
            int dash = userInputStreet.IndexOf('-');
            bool isNumber;
            string bldgNums, testCase;

            bldgNums = userInputStreet.Substring(0, space);

            if (getFirst) // Get the first building number
            {
                try
                {
                    int number;
                    isNumber = Int32.TryParse(bldgNums, out number);

                    if (isNumber)
                        return bldgNums;
                    else if (dash > 0)
                        return bldgNums.Substring(0, dash);
                }
                catch (ArgumentOutOfRangeException rangeExc)
                {
                    return "";
                }
            }
            else // Get the entire building number range
            {
                try
                {
                    int number;
                    testCase = bldgNums.Substring(0, dash);
                    isNumber = Int32.TryParse(testCase, out number);

                    if (isNumber)
                        return bldgNums;
                    else if (dash > 0)
                        return "";

                }
                catch (ArgumentOutOfRangeException)
                {
                    if (dash == -1)
                        return bldgNums;
                    else
                        return "";
                } 
            }
            return null;
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

                // Now store the address component name as the key and the actual address component as the value
                foreach (var item in locationinfo.results[0].address_components)
                    foreach (var type in item.types)
                        if (!addressParts.ContainsKey(item.types[0]))
                            addressParts.Add(item.types[0].ToString(), item.long_name.ToString());
            }
            // Fill the object with the value contents of the dictionary
            // forgive me padre for i have sinned
            location.singleBldg = 
                (addressParts.ContainsKey("street_number") ? addressParts["street_number"] : "");
            location.st1 = 
                (addressParts.ContainsKey("route") ? addressParts["route"] : "");
            location.st2 = 
                (addressParts.ContainsKey("subpremise") ? addressParts["subpremise"] : "");
            location.city = 
                (addressParts.ContainsKey("locality") ? addressParts["locality"] : "");
            location.county = 
                (addressParts.ContainsKey("administrative_area_level_2") ? addressParts["administrative_area_level_2"] : "");
            location.state = 
                (addressParts.ContainsKey("administrative_area_level_1") ? addressParts["administrative_area_level_1"] : "");
            location.zip = 
                (addressParts.ContainsKey("postal_code") ? addressParts["postal_code"] : "");

            return location;
        }

        /// <summary>
        /// Takes the numerical values that make up the Total Insured Value as strings and sums them together
        /// </summary>
        /// <param name="buildingValue">Building Value as a string</param>
        /// <param name="personalProperty">Business Personal Property as a string</param>
        /// <param name="businessIncome">Business Income as a string</param>
        /// <param name="miscRealProperty">Misc Real Property as a string</param>
        /// <returns>The sum (TIV) as a string</returns>
        public static string sumTIV(string buildingValue, string personalProperty, string businessIncome, string miscRealProperty)
        {
            // Chain Replace() methods to remove dollar signs, commas, and spaces: may have to expand for periods in place of commas
            // cool but gross
            buildingValue = buildingValue.Replace("$", "").Replace(",", "").Replace(" ", "");
            personalProperty = personalProperty.Replace("$", "").Replace(",", "").Replace(" ", "");
            businessIncome = businessIncome.Replace("$", "").Replace(",", "").Replace(" ", "");
            miscRealProperty = miscRealProperty.Replace("$", "").Replace(",", "").Replace(" ", "");

            double bldgValueNumeric, businessPersPropNumeric, businessIncomeNumeric, miscPropNumeric;

            // TryParse the input values
            bool bvResult = Double.TryParse(buildingValue, out bldgValueNumeric);
            bool persPropResult = Double.TryParse(personalProperty, out businessPersPropNumeric);
            bool biResult = Double.TryParse(businessIncome, out businessIncomeNumeric);
            bool miscPropResult = Double.TryParse(miscRealProperty, out miscPropNumeric);

            if (bvResult && persPropResult && biResult && miscPropResult)
            {
                double tivNumeric = bldgValueNumeric + businessPersPropNumeric + businessIncomeNumeric + miscPropNumeric; 
                return tivNumeric.ToString();
            }
            else
                return "";
        }
    }
}