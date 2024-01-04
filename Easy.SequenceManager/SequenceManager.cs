using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Contexts;

namespace Easy.SequenceManager
{
    public class SequenceManager
    {
        // Properties to store sequence file details
        public string FileVersion { get; set; }
        private ExecutionContext context;
        public string CheckSum { get; set; }
        public string BaseDirectory { get; private set; }
        public Sequence Sequence { get; set; }

        public SequenceManager()
        {
            LogHelper.ConfigureLog4Net();
        }

        /// <summary>
        /// Loads a sequence from a JSON file and initializes the sequence elements.
        /// </summary>
        /// <param name="filePath">The path to the JSON file.</param>
        /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
        /// <exception cref="JsonException">Thrown when there is an error processing the JSON file.</exception>
        public void LoadJsonSequence(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The file '{filePath}' does not exist.");

            BaseDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;
            LoadSequence(filePath); // Load the sequence from the file
        }

        /// <summary>
        /// Private helper method to load a sequence from the given JSON file.
        /// </summary>
        /// <param name="filePath">Path to the JSON file containing the sequence.</param>
        private void LoadSequence(string filePath)
        {
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
                        subSequence.Initialize(BaseDirectory); // Initialize the sub-sequence, which recursively loads any nested sequences
                        Sequence.Elements.Add(subSequence);
                        break;
                }
            }
        }
        /// <summary>
        /// Initializes the sequence execution context. Call this method before starting the sequence execution.
        /// </summary>
        public void InitializeExecution()
        {
            context = new ExecutionContext();
            context.PushSequence(Sequence);
        }

        /// <summary>
        /// Retrieves the next set of steps that can be executed simultaneously.
        /// </summary>
        /// <returns>A list of SequenceElements representing the next steps for parallel execution.</returns>
        public List<SequenceElement> GetNextStepsToExecute()
        {
            if (context.HasSequences)
            {
                return context.GetCurrentSequence().GetNextElements(context);
            }

            return new List<SequenceElement>(); // Return an empty list when there are no more elements to execute
        }
    }
}