using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAppServiceBus.Models
{
    public class ServiceBusConfiguration
    {
        public string Namespace { get; set; }
        public string Queue { get; set; }
    }
}
