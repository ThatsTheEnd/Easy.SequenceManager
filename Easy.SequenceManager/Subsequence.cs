using System.Collections.Generic;
using System.IO;

namespace Easy.SequenceManager
{
    public class SubSequence : SequenceElement
    {
        public string SubSequenceFilePath { get; set; }
        public Sequence Sequence { get; set; }
        private string baseDirectory;
        public int CurrentStepIndex = 0;

        public SubSequence()
        {
            // Initialization logic
        }

        public void Initialize(string baseDirectory)
        {
            this.baseDirectory = baseDirectory;
            if (!string.IsNullOrWhiteSpace(SubSequenceFilePath))
            {
                string fullPath = Path.Combine(baseDirectory, SubSequenceFilePath);
                if (!File.Exists(fullPath))
                    throw new FileNotFoundException($"Sub-sequence file '{fullPath}' does not exist.");

                SequenceManager subSequenceManager = new SequenceManager();
                subSequenceManager.LoadJsonSequence(fullPath);
                Sequence = subSequenceManager.Sequence;

                foreach (var element in Sequence.Elements)
                {
                    if (element is SubSequence nestedSubSequence)
                    {
                        nestedSubSequence.baseDirectory = baseDirectory;
                        nestedSubSequence.Initialize(baseDirectory);
                    }
                }
            }
        }

        public override List<SequenceElement> GetNextElements(ExecutionContext context)
        {
            List<SequenceElement> nextElements = new List<SequenceElement>();

            if (Sequence != null && CurrentStepIndex < Sequence.Elements.Count)
            {
                var nextElement = Sequence.Elements[CurrentStepIndex];
                if (nextElement is SubSequence nestedSubSequence)
                {
                    context.PushSequence(nestedSubSequence); // Push the nested sub-sequence onto the stack
                    nextElements.AddRange(nestedSubSequence.GetNextElements(context));
                    CurrentStepIndex++;
                }
                else
                {
                    nextElements.Add(nextElement);
                    CurrentStepIndex++;
                }

                // Add subsequent parallel steps if the current step is parallel
                if (nextElement is Step step && step.IsParallel)
                {
                    while (CurrentStepIndex < Sequence.Elements.Count && Sequence.Elements[CurrentStepIndex] is Step nextStep && nextStep.IsParallel)
                    {
                        nextElements.Add(nextStep);
                        CurrentStepIndex++;
                    }
                }
            }

            if (CurrentStepIndex >= Sequence.Elements.Count)
            {
                context.PopSequence(); // Pop this sub-sequence as it's completed
            }

            return nextElements;
        }
    }
}
