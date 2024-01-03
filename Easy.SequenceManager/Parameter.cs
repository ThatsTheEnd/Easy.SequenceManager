using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Easy.SequenceManager
{
    /// <summary>
    /// This is a dataclass to hold information about a parameter. It is used in the Step class within a list.
    /// </summary>
    public class Parameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public bool Required { get; set; }
        public string DefaultValue { get; set; }
        public string HooverOverInfo { get; set; }
    }
}
