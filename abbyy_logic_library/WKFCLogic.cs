using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace abbyy_logic_library
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
    }
}
