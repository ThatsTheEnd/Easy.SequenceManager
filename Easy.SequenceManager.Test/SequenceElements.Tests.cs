using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Easy.SequenceManager.Test
{
    public class TestSequenceElements
    {
        [Fact]
        public void StepInitializationTest()
        {
            // Arrange
            var step = new Step
            {
                Name = "Test Step",
                Documentation = "Test Documentation",
                Timeout = 30,
                IsParallel = false,
                IsSynchronous = true
                // ... other properties
            };

            // Assert
            Assert.Equal("Test Step", step.Name);
            Assert.Equal("Test Documentation", step.Documentation);
            // ... other assertions
        }

        [Fact]
        public void GetNextElementsForStepTest()
        {
            // Arrange
            var step = new Step {
                Name = "Test Step",
                Documentation = "Test Documentation",
                Timeout = 30,
                IsParallel = false,
                IsSynchronous = true
            };
            var context = new ExecutionContext(); // Assuming ExecutionContext has a suitable constructor
            context.PushSequence(step);

            // Act
            var nextElements = step.GetNextElements(context);

            // Assert
            Assert.Single(nextElements);
            Assert.Equal(step, nextElements.First());
        }

    }
}
