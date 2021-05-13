using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ebus.Models
{
    public class Record
    {
        public string driverId { get; set; }
        public string busNumber { get; set; }
        public string customerId { get; set; }
        public string fare { get; set; }
        public string to { get; set; }
        public string from { get; set; }
        public string date { get; set; }

    }
}
