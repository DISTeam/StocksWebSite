using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace IEXTrading.Models
{
    public class Stats
    {
        public string symbol { get; set; }
        public string companyName { get; set; }
        public float? close { get; set; }
        public float? week52High { get; set; }
        public float? week52Low { get; set; }
    }
}
