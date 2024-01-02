using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

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
        public void LoadJSON(string filePath)
        {
            BaseDirectory = Path.GetDirectoryName(filePath);
            string jsonText = File.ReadAllText(filePath);
            JObject jObject = JObject.Parse(jsonText);

            JToken sequenceManagerToken = jObject["SequenceManager"];
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
                            SequenceManager subSequenceManager = new SequenceManager();
                            subSequenceManager.LoadJSON(Path.Combine(BaseDirectory, subSequence.SubSequenceFilePath));
                            subSequence.Sequence = subSequenceManager.Sequence;
                        }
                        Sequence.Elements.Add(subSequence);
                        break;
                        // Add cases for other types of sequence elements here.
                }
            }
        }
    }




    public class Sequence
    {
        private int CurrentExecutingStepIndex = 0;
        public List<SequenceElement> Elements { get; set; }

        public Sequence()
        {
            Elements = new List<SequenceElement>();
        }

        /// <summary>
        /// Returns the next step to execute or will return nothing if no more steps are available
        /// Increments the CurrentExecutingStepIndex
        /// </summary>
        /// <returns>Step or Null</returns>
        public SequenceElement GetNextElementToExecute()
        {
            if (CurrentExecutingStepIndex < Elements.Count)
            {
                SequenceElement currentElement = Elements[CurrentExecutingStepIndex];
                CurrentExecutingStepIndex++;
                // return the current step to execute
                return currentElement;
            }
            else
            {
                return null;
            }
        }
    }

    public abstract class SequenceElement
    {
        public string Name { get; set; }
        public bool IsSynchronous { get; set; }
        public string Documentation { get; set; }

        // Common methods or properties for all sequence elements
    }


    public class Step : SequenceElement
    {
        public int Timeout { get; set; }
        public string TargetModule { get; set; }
        [JsonProperty("Parameters")]
        public List<Parameter> Parameters { get; set; }

        public Step()
        {
            Parameters = new List<Parameter>();
        }
    }

    public class SubSequence : SequenceElement
    {
        public string SubSequenceFilePath { get; set; }
        public Sequence Sequence { get; set; }

        // Load sub-sequence or reference another JSON
    }


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

