namespace SequencerLibrary
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class Sequence
    {
        [XmlElement("Step")]
        public List<StepBase> Steps { get; }
        private int CurrentStepIndex { get; set; }

        public Sequence()
        {
            Steps = new List<StepBase>();
            CurrentStepIndex = 0;
        }

        /// <summary>
        /// Adds a step to the sequence.
        /// </summary>
        public void AddStep(StepBase step)
        {
            Steps.Add(step);
        }

        /// <summary>
        /// Retrieves the next step to be executed.
        /// </summary>
        public StepBase GetNextStep()
        {
            if (CurrentStepIndex < Steps.Count)
            {
                return Steps[CurrentStepIndex++];
            }
            else
            {
                return null; // No more steps left in the sequence
            }
        }

        /// <summary>
        /// Marks the current step as completed and moves to the next step.
        /// </summary>
        public void StepDone()
        {
            // Increment the CurrentStepIndex, or handle it as per the logic
            // This could be automatically done in GetNextStep method as well
            CurrentStepIndex++;
        }

        /// <summary>
        /// Returns the remaining steps in the sequence.
        /// </summary>
        public IEnumerable<StepBase> GetRemainingSteps()
        {
            for (int i = CurrentStepIndex; i < Steps.Count; i++)
            {
                yield return Steps[i];
            }
        }
    }
}
