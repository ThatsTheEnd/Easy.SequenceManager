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

            while (CurrentExecutingStepIndex < Elements.Count)
            {
                SequenceElement currentElement = Elements[CurrentExecutingStepIndex];

                if (currentElement is SubSequence subSequence)
                {
                    var subSequenceElements = HandleNextSubSequence(subSequence);
                    if (subSequenceElements.Any())
                    {
                        return subSequenceElements;
                    }
                    else
                    {
                        // If sub-sequence is finished, increment index to move to the next element in the main sequence
                        CurrentExecutingStepIndex++;
                        continue;
                    }
                }

                // Process step elements
                if (currentElement is Step currentStep)
                {
                    elementsToExecute.Add(currentStep);
                    CurrentExecutingStepIndex++;

                    if (currentStep.IsParallel)
                    {
                        while (CurrentExecutingStepIndex < Elements.Count && Elements[CurrentExecutingStepIndex] is Step nextStep && nextStep.IsParallel)
                        {
                            elementsToExecute.Add(nextStep);
                            CurrentExecutingStepIndex++;
                        }
                    }

                    return elementsToExecute;
                }
                else
                {
                    // Increment index for non-step elements
                    CurrentExecutingStepIndex++;
                    elementsToExecute.Add(currentElement);
                    return elementsToExecute;
                }
            }

            return elementsToExecute;
        }



        /// <summary>
        /// Processes the next element when it is a Step. If the current step is parallel,
        /// it aggregates subsequent parallel steps. If the current step is not parallel,
        /// it returns only this step.
        /// </summary>
        /// <param name="currentStep">The current Step element to be processed.</param>
        /// <returns>A list of SequenceElement, either containing just the current step,
        /// or the current step along with subsequent parallel steps.</returns>
        private List<SequenceElement> HandleNextStep(Step currentStep)
        {
            List<SequenceElement> elementsToExecute = new List<SequenceElement> { currentStep };

            // Add subsequent parallel steps if the current step is parallel
            if (currentStep.IsParallel)
            {
                CurrentExecutingStepIndex++;
                while (CurrentExecutingStepIndex < Elements.Count)
                {
                    if (Elements[CurrentExecutingStepIndex] is Step nextStep && nextStep.IsParallel)
                    {
                        elementsToExecute.Add(nextStep);
                        CurrentExecutingStepIndex++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                // Increment index for non-parallel step
                CurrentExecutingStepIndex++;
            }
            return elementsToExecute;
        }

        /// <summary>
        /// Processes the next element when it is a SubSequence. It retrieves the next elements
        /// from the sub-sequence. If the sub-sequence has more elements to execute, it returns them.
        /// Once the sub-sequence is completed, it moves to the next element in the main sequence.
        /// </summary>
        /// <param name="subSequence">The SubSequence element to be processed.</param>
        /// <returns>A list of SequenceElement containing the next elements from the sub-sequence,
        /// or an empty list if the sub-sequence is completed.</returns>
        private List<SequenceElement> HandleNextSubSequence(SubSequence subSequence)
        {
            if (subSequence.Sequence != null)
            {
                while (subSequence.CurrentStepIndex < subSequence.Sequence.Elements.Count)
                {
                    var nextElement = subSequence.Sequence.Elements[subSequence.CurrentStepIndex];
                    List<SequenceElement> subSequenceElements = new List<SequenceElement> { nextElement };
                    subSequence.CurrentStepIndex++;

                    if (nextElement is Step step && step.IsParallel)
                    {
                        while (subSequence.CurrentStepIndex < subSequence.Sequence.Elements.Count)
                        {
                            if (subSequence.Sequence.Elements[subSequence.CurrentStepIndex] is Step nextStep && nextStep.IsParallel)
                            {
                                subSequenceElements.Add(nextStep);
                                subSequence.CurrentStepIndex++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (subSequenceElements.Any())
                    {
                        return subSequenceElements;
                    }
                }
            }

            // Indicate that the sub-sequence is completed
            return new List<SequenceElement>();
        }


        private List<SequenceElement> HandleSubSequenceStep(SubSequence subSequence, Step step)
        {
            List<SequenceElement> elementsToExecute = new List<SequenceElement> { step };

            if (step.IsParallel)
            {
                subSequence.CurrentStepIndex++;
                while (subSequence.CurrentStepIndex < subSequence.Sequence.Elements.Count)
                {
                    if (subSequence.Sequence.Elements[subSequence.CurrentStepIndex] is Step nextStep && nextStep.IsParallel)
                    {
                        elementsToExecute.Add(nextStep);
                        subSequence.CurrentStepIndex++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                subSequence.CurrentStepIndex++;
            }
            return elementsToExecute;
        }




    }



    public abstract class SequenceElement
    {
        public string Name { get; set; }
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
        // The property IsParallel on two neighbouring steps is used to indicate that the steps are kicked of in parallel.
        // The property IsSynchronous is used to indicate that the step is synchronous, i.e. the orchestrator will wait for the step to finish before continuing.
        public bool IsSynchronous { get; set; }
        public bool IsParallel { get; set; }

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
        public int CurrentStepIndex = 0;  // Internal index for sub-sequence

        public override List<SequenceElement> GetNextElements()
        {
            List<SequenceElement> nextElements = new List<SequenceElement>();
            if (Sequence != null)
            {
                while (CurrentStepIndex < Sequence.Elements.Count)
                {
                    nextElements.Add(Sequence.Elements[CurrentStepIndex]);
                    CurrentStepIndex++;
                }
                if (CurrentStepIndex < Sequence.Elements.Count && nextElements.Count == 0)
                {
                    nextElements.Add(Sequence.Elements[CurrentStepIndex]);
                    CurrentStepIndex++;
                }
            }
            return nextElements;
        }
    }


}

