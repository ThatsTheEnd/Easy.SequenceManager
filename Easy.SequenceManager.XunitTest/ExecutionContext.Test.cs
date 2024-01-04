using Easy.SequenceManager;
using Xunit;
using System.Collections.Generic;
using Moq;

namespace Easy.SequenceManager.Test
{
    public class ExecutionContextTests
    {
        private ExecutionContext _executionContext;
        private ISequenceElement _sequenceElement;

        public ExecutionContextTests()
        {
            _executionContext = new ExecutionContext();
            _sequenceElement = new Mock<ISequenceElement>().Object;
        }

        [Fact]
        public void PushSequence_ShouldAddSequenceToStack()
        {
            _executionContext.PushSequence(_sequenceElement);
            Assert.Equal(_sequenceElement, _executionContext.GetCurrentSequence());
        }

        [Fact]
        public void PopSequence_ShouldRemoveSequenceFromStack()
        {
            _executionContext.PushSequence(_sequenceElement);
            var poppedSequence = _executionContext.PopSequence();
            Assert.Equal(_sequenceElement, poppedSequence);
        }

        [Fact]
        public void GetCurrentSequence_ShouldReturnCurrentSequenceWithoutRemovingIt()
        {
            _executionContext.PushSequence(_sequenceElement);
            var currentSequence = _executionContext.GetCurrentSequence();
            Assert.Equal(_sequenceElement, currentSequence);
            Assert.True(_executionContext.HasSequences);
        }

        [Fact]
        public void HasSequences_ShouldReturnFalseWhenNoSequencesInStack()
        {
            Assert.False(_executionContext.HasSequences);
        }
    }
}