using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WKFCBusinessRules
{
    public class ABBYYLocation
    {
        // Variable names are conforming to our in-house naming consistency
        public string singleBldg { get; set; } = "";
        public string st1 { get; set; } = "";
        public string st2 { get; set; } = "";
        public string city { get; set; } = "";
        public string county { get; set; } = "";
        public string state { get; set; } = "";
        public string zip { get; set; } = "";
    }
}
