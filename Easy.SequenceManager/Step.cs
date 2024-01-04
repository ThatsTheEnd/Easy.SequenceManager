using Newtonsoft.Json;
using System.Collections.Generic;

namespace Easy.SequenceManager
{
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
}