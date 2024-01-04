using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace Easy.SequenceManager
{
    /// <summary>
    /// Represents the execution context of a sequence, managing the current position within nested sequences.
    /// It functions similarly to a call stack, keeping track of the current and nested sequences.
    /// </summary>
    public class ExecutionContext
    {
        private Stack<ISequenceElement> sequenceStack = new Stack<ISequenceElement>();

        /// <summary>
        /// Pushes a sequence onto the stack. This is used when a sub-sequence is encountered,
        /// allowing the execution to transition to the sub-sequence.
        /// </summary>
        /// <param name="sequence">The sequence to push onto the stack.</param>
        public void PushSequence(ISequenceElement sequence)
        {
            sequenceStack.Push(sequence);
        }

        /// <summary>
        /// Pops the top sequence from the stack. This is used when a sub-sequence completes its execution,
        /// allowing the execution to return to the parent sequence.
        /// </summary>
        /// <returns>The sequence that was at the top of the stack.</returns>
        public ISequenceElement PopSequence()
        {
            return sequenceStack.Count > 0 ? sequenceStack.Pop() : null;
        }

        /// <summary>
        /// Retrieves the current sequence from the top of the stack without removing it.
        /// This represents the sequence that is currently being executed.
        /// </summary>
        /// <returns>The current sequence at the top of the stack.</returns>
        public ISequenceElement GetCurrentSequence()
        {
            return sequenceStack.Peek();
        }

        /// <summary>
        /// Checks if there are any sequences on the stack.
        /// </summary>
        /// <returns>True if the stack has one or more sequences; otherwise, false.</returns>
        public bool HasSequences => sequenceStack.Count > 0;
    }
}