﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
