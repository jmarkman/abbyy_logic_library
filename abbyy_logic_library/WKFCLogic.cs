using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Xml;
using System.IO;
using Newtonsoft.Json;

namespace WKFCBusinessRules
{
    public class WKFCLogic
    {
        public static string AdjustYearBuilt(string userInputYear)
        {
            try
            {
                int fullYear = CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(Int32.Parse(userInputYear));

                return fullYear.ToString();
            }
            catch (ArgumentOutOfRangeException arnge)
            {
                arnge.ToString();
            }
            return null;
        }

        public static string RemoveCountySuffix(string userInputCounty)
        {
            if (userInputCounty.ToLower().Contains("county"))
            {
                int space = userInputCounty.IndexOf(' ');
                if (space != -1)
                {
                    return userInputCounty.Substring(0, space);
                }
            }
            return null;
        }

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

        public static string GetControlNumber(string subjectLine)
        {
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

        public static string ConvertConstrTypeToInteger(string userInputConstrType)
        {
            Dictionary<string, int> isoType = new Dictionary<string, int>
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

            foreach (KeyValuePair<string, int> isoPair in isoType)
            {
                if (userInputConstrType.ToLower().Equals(isoPair.Key))
                {
                    return isoPair.Value.ToString();
                }
            }
            return null;
        }

        public static string GetFirstBuildingNumber(string userInputStreet)
        {
            int space = userInputStreet.IndexOf(" ");
            int dash = userInputStreet.IndexOf('-');
            bool isNumber;
            string bldgNums, testCase, extractedBldgNum;

            bldgNums = userInputStreet.Substring(0, space);

            try
            {
                int number;
                isNumber = Int32.TryParse(bldgNums, out number);

                if (isNumber)
                {
                    return bldgNums;
                }
                else if (dash > 0)
                {
                    return bldgNums.Substring(0, dash);
                }
            }
            catch (ArgumentOutOfRangeException rangeExc)
            {
                return "";
            }
            return null;
        }

        public static ABBYYLocation ParseAddress(string userInputAddress)
        {
            Dictionary<string, string> addressParts = new Dictionary<string, string>();
            ABBYYLocation location = new ABBYYLocation();

            string requestUri = "https://maps.googleapis.com/maps/api/geocode/json?sensor=false&address=" + userInputAddress;

            using (WebClient webc = new WebClient())
            {
                var json = webc.DownloadString(requestUri);
                var locationinfo = JsonConvert.DeserializeObject<LocationInfo>(json);

                foreach (var item in locationinfo.results[0].address_components)
                {
                    foreach (var type in item.types)
                    {
                        if (!addressParts.ContainsKey(item.types[0]))
                        {
                            addressParts.Add(item.types[0].ToString(), item.long_name.ToString());
                        }
                    }
                }
            }

            location.singleBldg = addressParts["street_number"];
            location.st1 = addressParts["route"];
            location.st2 = addressParts["subpremise"];
            location.city = addressParts["locality"];
            location.county = addressParts["administrative_area_level_2"];
            location.state = addressParts["administrative_area_level_1"];
            location.zip = addressParts["postal_code"];

            return location;
        }

        public static string sumTIV(string buildingValue, string personalProperty, string businessIncome)
        {
            if (buildingValue.Contains(",") || personalProperty.Contains(",") || businessIncome.Contains(","))
            {
                buildingValue.Replace(",", "");
                personalProperty.Replace(",", "");
                businessIncome.Replace(",", "");
            }

            double bldgValueNumeric, businessPersPropNumeric, businessIncomeNumeric;

            bool bvResult = Double.TryParse(buildingValue, out bldgValueNumeric);
            bool persPropResult = Double.TryParse(personalProperty, out businessPersPropNumeric);
            bool biResult = Double.TryParse(businessIncome, out businessIncomeNumeric);

            if (bvResult && persPropResult && biResult)
            {
                double tivNumeric = bldgValueNumeric + businessPersPropNumeric + businessIncomeNumeric;
                return tivNumeric.ToString();
            }
            else
            {
                return null;
            }
        }
    }
}