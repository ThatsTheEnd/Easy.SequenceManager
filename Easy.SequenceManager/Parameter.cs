using Newtonsoft.Json.Linq;

namespace Easy.SequenceManager
{
    /// <summary>
    /// This is a dataclass to hold information about a parameter. It is used in the Step class within a list.
    /// </summary>
    public class Parameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        /// <summary>
        /// The value of the parameter. This can be any JSON object.
        /// </summary>
        public JToken Value { get; set; }
        public bool Required { get; set; }
        public JToken DefaultValue { get; set; }
        public string HooverOverInfo { get; set; }
    }
}
