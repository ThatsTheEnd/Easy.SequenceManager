using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Easy.SequenceManager
{
    /// <summary>
    /// Loads a sequence from a JSON file.
    /// </summary>
    /// <param name="filePath">The path to the JSON file.</param>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
    /// <exception cref="JsonReaderException">Thrown when the file is not in a valid JSON format.</exception>
    /// <exception cref="InvalidOperationException">Thrown when required JSON properties are missing.</exception>
    public class SequenceManager
    {
        public string FileVersion { get; set; }
        public string CheckSum { get; set; }
        public string BaseDirectory { get; private set; }
        public Sequence Sequence { get; set; }

        /// <summary>
        /// Loads a sequence from a JSON file and initializes the sequence elements.
        /// </summary>
        /// <param name="filePath">The path to the JSON file.</param>
        /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
        /// <exception cref="JsonException">Thrown when there is an error processing the JSON file.</exception>
        /// <exception cref="Exception">General exceptions for other errors.</exception>
        public void LoadJsonSequence(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file '{filePath}' does not exist.");
            }

            try
            {
                BaseDirectory = Path.GetDirectoryName(filePath);
                string jsonText = File.ReadAllText(filePath);
                JObject jObject = JObject.Parse(jsonText);

                JToken sequenceManagerToken = jObject["SequenceManager"] ?? throw new JsonException("Missing 'SequenceManager' section in JSON.");
                FileVersion = (string)sequenceManagerToken["FileVersion"];
                CheckSum = (string)sequenceManagerToken["CheckSum"];

                Sequence = new Sequence();
                JArray elementsArray = (JArray)sequenceManagerToken["Sequence"]["Elements"];
                foreach (var element in elementsArray)
                {
                    string type = (string)element["Type"];
                    switch (type)
                    {
                        case "Step":
                            Sequence.Elements.Add(element.ToObject<Step>());
                            break;
                        case "SubSequence":
                            SubSequence subSequence = element.ToObject<SubSequence>();
                            if (!string.IsNullOrWhiteSpace(subSequence.SubSequenceFilePath))
                            {
                                string fullPath = Path.Combine(BaseDirectory, subSequence.SubSequenceFilePath);
                                if (!File.Exists(fullPath))
                                    throw new FileNotFoundException($"Sub-sequence file '{fullPath}' does not exist.");

                                SequenceManager subSequenceManager = new SequenceManager();
                                subSequenceManager.LoadJsonSequence(fullPath);
                                subSequence.Sequence = subSequenceManager.Sequence;
                            }
                            Sequence.Elements.Add(subSequence);
                            break;
                            // Add cases for other types of sequence elements here. E.g. when adding a for-loop or a step to declare a variable.
                    }
                }
            }
            catch (JsonReaderException ex)
            {
                throw new JsonException("Error reading JSON file.", ex);
            }
        }
    }

}

