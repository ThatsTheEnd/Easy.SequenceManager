using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Easy.SequenceManager
{

    public class Sequence : SequenceElement
    {
        private int CurrentExecutingStepIndex = 0;
        public List<SequenceElement> Elements { get; set; }

        public Sequence()
        {
            Elements = new List<SequenceElement>();
        }

        public override List<SequenceElement> GetNextElements(ExecutionContext context)
        {
            return GetNextElementsToExecute();
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
     }
}
