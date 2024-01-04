using System.Collections.Generic;

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
            List<SequenceElement> elementsToExecute = new List<SequenceElement>();

            if (CurrentExecutingStepIndex < Elements.Count)
            {
                SequenceElement currentElement = Elements[CurrentExecutingStepIndex];

                if (currentElement is SubSequence subSequence)
                {
                    context.PushSequence(subSequence); // Push the sub-sequence onto the stack
                    elementsToExecute.AddRange(subSequence.GetNextElements(context));
                    CurrentExecutingStepIndex++;
                }
                else
                {
                    elementsToExecute.AddRange(currentElement.GetNextElements(context));
                    CurrentExecutingStepIndex++;

                    // Handle parallel steps
                    if (currentElement is Step step && step.IsParallel)
                    {
                        while (CurrentExecutingStepIndex < Elements.Count)
                        {
                            var nextElement = Elements[CurrentExecutingStepIndex];
                            if (nextElement is Step nextStep && nextStep.IsParallel)
                            {
                                elementsToExecute.AddRange(nextStep.GetNextElements(context));
                                CurrentExecutingStepIndex++;
                            }
                            else
                            {
                                break; // Stop if the next step is not parallel
                            }
                        }
                    }
                }
            }

            return elementsToExecute;
        }
    }
}