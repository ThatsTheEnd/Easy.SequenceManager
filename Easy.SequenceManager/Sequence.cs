using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
namespace Easy.SequenceManager
{
    /// <summary>
    /// This is an interface that is implemented by every type that can appear in the json file and be part of the sequence.
    /// The sequence itself also implements this interface so that sequence and sub-sequence can be treated the same.
    /// </summary>
    public interface ISequenceElement
    {
        List<SequenceElement> GetNextElements(ExecutionContext context);
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



    public abstract class SequenceElement : ISequenceElement
    {
        public string Name { get; set; }
        public string Documentation { get; set; }
        // Abstract method to get next elements
        public abstract List<SequenceElement> GetNextElements(ExecutionContext context);

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

        public override List<SequenceElement> GetNextElements(ExecutionContext context)
        {
            return new List<SequenceElement> { this };
        }
    }

    public class SubSequence : SequenceElement
    {
        public string SubSequenceFilePath { get; set; }
        public Sequence Sequence { get; set; }
        public int CurrentStepIndex = 0;  // Internal index for sub-sequence

        public override List<SequenceElement> GetNextElements(ExecutionContext context)
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
