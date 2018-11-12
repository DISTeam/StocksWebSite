using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IEXTrading.Models
{
    public class outputModel
    {
        [Key]
        public string companyName { get; set; }
        public string symbol { get; set; }
        public float? close { get; set; }
        public float? value { get; set; }
    }
}
