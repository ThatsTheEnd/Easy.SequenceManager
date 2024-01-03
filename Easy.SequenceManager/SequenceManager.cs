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
                            // Add cases for other types of sequence elements here.
                    }
                }
            }
            catch (JsonReaderException ex)
            {
                throw new JsonException("Error reading JSON file.", ex);
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
        /// Returns the next set of elements to execute.
        /// </summary>
        /// <returns>List of SequenceElement or empty list if no more elements are available.</returns>
        public List<SequenceElement> GetNextElementsToExecute()
        {
            List<SequenceElement> elementsToExecute = new List<SequenceElement>();
            if (CurrentExecutingStepIndex < Elements.Count)
            {
                SequenceElement currentElement = Elements[CurrentExecutingStepIndex];
                elementsToExecute.Add(currentElement);
                CurrentExecutingStepIndex++;

                // If the first element is not parallel, return it alone
                if (!currentElement.IsParallel)
                {
                    return elementsToExecute;
                }

                // Otherwise, continue adding elements only if they are marked as parallel
                while (CurrentExecutingStepIndex < Elements.Count && Elements[CurrentExecutingStepIndex].IsParallel)
                {
                    elementsToExecute.Add(Elements[CurrentExecutingStepIndex]);
                    CurrentExecutingStepIndex++;
                }
            }
            return elementsToExecute;
        }

    }



    public abstract class SequenceElement
    {
        public string Name { get; set; }
        // The property IsParallel on two neighbouring steps is used to indicate that the steps are kicked of in parallel.
        // The property IsSynchronous is used to indicate that the step is synchronous, i.e. the orchestrator will wait for the step to finish before continuing.
        public bool IsSynchronous { get; set; }
        public bool IsParallel { get; set; }  // New property for parallel execution
        public string Documentation { get; set; }

        // Abstract method to get next elements
        public abstract List<SequenceElement> GetNextElements();

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

        public override List<SequenceElement> GetNextElements()
        {
            return new List<SequenceElement> { this };
        }
    }

    public class SubSequence : SequenceElement
    {
        public string SubSequenceFilePath { get; set; }
        public Sequence Sequence { get; set; }
        private int CurrentStepIndex = 0; // Internal index for sub-sequence
        public override List<SequenceElement> GetNextElements()
        {
            if (Sequence != null && CurrentStepIndex < Sequence.Elements.Count)
            {
                var elements = Sequence.Elements.Skip(CurrentStepIndex).TakeWhile(e => e.IsSynchronous).ToList();
                CurrentStepIndex += elements.Count;
                return elements;
            }
            return new List<SequenceElement>();
        }

    }

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

