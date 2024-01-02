namespace SequencerLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;


    [XmlInclude(typeof(Step))]
    [XmlInclude(typeof(Subsequence))]
    [XmlInclude(typeof(ForLoop))]
    public class StepBase
    {
        [XmlAttribute("Timeout")]
        public int Timeout { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Sychronous")]
        public bool Sychronous { get; set; }

        [XmlAttribute("Documentation")]
        public string Documentation { get; set; }

        [XmlAttribute("TargetModule")]
        public string TargetModule { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

    }



    /// <summary>
    /// Represents a standard step in the sequence with parameters.
    /// </summary>
    public class Step : StepBase
    {
        /// <summary>
        /// Gets or sets the list of parameters for the StandardStep.
        /// </summary>
        /// 
        [XmlElement("Parameter")]
        public List<Parameter> Parameters { get; set; }



        public Step()
        {
            Parameters = new List<Parameter>();
        }

        /// <summary>
        /// Executes the action defined in the StandardStep.
        /// </summary>
        public void Execute()
        {
            // Implementation of the specific execution logic for StandardStep
            // This could involve using the Parameters in some way
            Console.WriteLine("Executing StandardStep");

            // Example usage of Parameters (pseudo-implementation)
            foreach (var parameter in Parameters)
            {
                Console.WriteLine($"Parameter Name: {parameter.Name}, Value: {parameter.Value}");
            }
        }
    }

    /// <summary>
    /// Represents a subsequence within the main sequence.
    /// </summary>
    public class Subsequence : StepBase
    {
        // Implementation of Subsequence
        public void Execute()
        {
            //just print something for now
            Console.WriteLine("Executing step");
            // Execute the step
        }
    }

    /// <summary>
    /// Represents a loop structure in the sequence.
    /// </summary>
    public class ForLoop : StepBase
    {
        public void Execute()
        {
            //just print something for now
            Console.WriteLine("Executing step");
            // Execute the step
        }
    }

    // ... Other classes like Parameter, etc.
}
