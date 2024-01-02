using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

namespace SequencerLibrary
{

    /// <summary>
    /// Manages the sequence of operations as defined in an XML file.
    /// Responsible for loading and parsing the XML into a Sequence object.
    /// </summary>
    [XmlRoot("SequenceManager")]
    public class SequenceManager
    {
        [XmlAttribute("CheckSum")]
        public string Checksum { get; set; }

        [XmlAttribute("FileVersion")]
        public string FileVersion { get; set; }

        [XmlElement("Sequence")]
        public Sequence Sequence { get; set; }

        /// <summary>
        /// Loads and deserializes the XML file into a Sequence object.
        /// </summary>
        /// <param name="filePath">The path to the XML file.</param>
        public void LoadXML(string filePath)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SequenceManager));
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    var loadedData = (SequenceManager)serializer.Deserialize(fs);
                    this.Checksum = loadedData.Checksum;
                    this.FileVersion = loadedData.FileVersion;
                    this.Sequence = loadedData.Sequence;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred during XML deserialization: " + ex.Message);
                Exception innerException = ex.InnerException;
                while (innerException != null)
                {
                    Console.WriteLine("Inner exception: " + innerException.Message);
                    innerException = innerException.InnerException;
                }
                throw; // Rethrow the exception to allow the test to fail and show the error message.
            }
        }


    }





    // Additional classes and implementations as needed...
}
