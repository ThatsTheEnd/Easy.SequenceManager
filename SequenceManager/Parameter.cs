using System.Xml.Serialization;

namespace SequencerLibrary
{
    /// <summary>
    /// Represents a parameter within a step in the sequence.
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        /// 
        [XmlAttribute("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the data type of the parameter.
        /// </summary>
        /// 
        [XmlAttribute("Type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        /// 
        [XmlAttribute("Value")]
        public string Value { get; set; }

        /// <summary>
        /// Indicates whether this parameter is required.
        /// </summary>
        /// 
        [XmlAttribute("Required")]
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets the default value of the parameter.
        /// </summary>
        /// 
        [XmlAttribute("DefaultValue")]
        public string DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the hover-over information for the parameter.
        /// </summary>
        /// 
        [XmlAttribute("HooverOverInfo")]
        public string HooverOverInfo { get; set; }
    }
}
