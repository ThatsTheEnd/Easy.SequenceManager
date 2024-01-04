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

        /// <summary>
        /// Loads the sub-sequence from its JSON file, and recursively loads any nested sub-sequences.
        /// </summary>
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

                // Recursively load nested sub-sequences
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
                nextElements.Add(nextElement);

                // Check if the next element is a Step and if it is parallel
                if (nextElement is Step step && step.IsParallel)
                {
                    // Add all subsequent parallel steps
                    CurrentStepIndex++;
                    while (CurrentStepIndex < Sequence.Elements.Count && Sequence.Elements[CurrentStepIndex] is Step nextStep && nextStep.IsParallel)
                    {
                        nextElements.Add(nextStep);
                        CurrentStepIndex++;
                    }
                }
                else
                {
                    // Move to the next element if it's not a parallel step
                    CurrentStepIndex++;
                }
            }

            // Check if the sub-sequence is completed and if so, pop it from the context
            if (CurrentStepIndex >= Sequence.Elements.Count)
            {
                context.PopSequence(); // Remove this sub-sequence as it's completed
            }

            return nextElements;
        }
    }
}