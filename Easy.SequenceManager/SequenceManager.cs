using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace Easy.SequenceManager
{
    public class SequenceManager
    {
        public string FileVersion { get; set; }
        public string CheckSum { get; set; }
        public Sequence Sequence { get; set; }

        /// <summary>
        /// Loads a sequence from a JSON file.
        /// </summary>
        /// <param name="filePath">The path to the JSON file.</param>
        /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
        /// <exception cref="JsonReaderException">Thrown when the file is not in a valid JSON format.</exception>
        /// <exception cref="InvalidOperationException">Thrown when required JSON properties are missing.</exception>
        public void LoadJSON(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file {filePath} does not exist.");
            }

            try
            {
                string jsonText = File.ReadAllText(filePath);
                JObject jObject = JObject.Parse(jsonText);

                JToken sequenceManagerToken = jObject["SequenceManager"] ?? throw new InvalidOperationException("Missing 'SequenceManager' section in JSON.");
                FileVersion = (string)sequenceManagerToken["FileVersion"] ?? throw new InvalidOperationException("Missing 'FileVersion' property.");
                CheckSum = (string)sequenceManagerToken["CheckSum"] ?? throw new InvalidOperationException("Missing 'CheckSum' property.");
                Sequence = sequenceManagerToken["Sequence"].ToObject<Sequence>() ?? throw new InvalidOperationException("Missing 'Sequence' property.");
            }
            catch (JsonReaderException ex)
            {
                throw new JsonReaderException("Invalid JSON format.", ex);
            }
        }
    }

    public class Sequence
    {
        private int CurrentExecutingStepIndex = 0;
        public List<Step> Steps { get; set; }

        public Sequence()
        {
            Steps = new List<Step>();
        }

        /// <summary>
        /// Returns the next step to execute or will return nothing if no more steps are available
        /// Increments the CurrentExecutingStepIndex
        /// </summary>
        /// <returns>Step or Null</returns>
        public Step GetNextStepToExecute()
        {             
            if (CurrentExecutingStepIndex < Steps.Count)
            {
                Step step = Steps[CurrentExecutingStepIndex];
                CurrentExecutingStepIndex++;
                // return the current step to execute
                return step;
            }
            else
            {
                return null;
            }
        }
    }


    public class Step
    {
        public string Name { get; set; }
        public bool IsSychronous { get; set; }
        public string Documentation { get; set; }
        public int Timeout { get; set; }
        public string TargetModule { get; set; }
        [JsonProperty("Parameters")]
        public List<Parameter> Parameters { get; set; }

        public Step()
        {
            Parameters = new List<Parameter>();
        }
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
