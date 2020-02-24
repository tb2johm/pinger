using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Pinger.Services
{
    public class PingConfig
    {
        public int Interval { get; set; }
        public PingOptions PingOptions { get; set; }
        public int Timeout { get; set; }
        public string Address { get; set; }
    }
}
