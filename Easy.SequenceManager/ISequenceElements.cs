using System.Collections.Generic;

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

    public abstract class SequenceElement : ISequenceElement
    {
        public string Name { get; set; }
        public string Documentation { get; set; }

        // Abstract method to get next elements
        public abstract List<SequenceElement> GetNextElements(ExecutionContext context);
    }
}