using Xunit;
using System.IO;
using System;
using System.Linq;

namespace SequencerLibrary.Test
{

    public class SequenceTests
    {
        [Fact]
        public void LoadXML_AndStepThroughSequence_ShouldReturnCorrectSteps()
        {
            // Arrange
            var sequenceManager = new SequenceManager();
            string filePath = Path.Combine("Testfiles", "high-level-sequence.xml");
            sequenceManager.LoadXML(filePath);

            // Act and Assert
            var step = sequenceManager.Sequence.GetNextStep();
            Assert.NotNull(step); // step should not be null if the sequence is starting

            while (step != null)
            {
                // print the contents of the step as json
                Console.WriteLine(step);

                Assert.NotNull(step); // Basic check to ensure a step is returned
                                      // Additional checks can be performed based on the properties of the step
                                      // For example, if you cast to StandardStep, you can check specific properties:
                                      // var standardStep = step as StandardStep;
                                      // Assert.NotNull(standardStep);
                                      // Assert.Equal(expectedValue, standardStep.SomeProperty);

                step = sequenceManager.Sequence.GetNextStep(); // Get the next step
            }

            // Assert that the sequence is complete
            Assert.Null(step); // step should be null if the sequence is complete
        }

        [Fact]
        public void LoadXML_AndAssertThirdStep_ShouldMatchExpectedProperties()
        {
            // Arrange
            var sequenceManager = new SequenceManager();
            string filePath = Path.Combine("Testfiles", "high-level-sequence.xml");
            sequenceManager.LoadXML(filePath);

            // Act
            StepBase firstStep = sequenceManager.Sequence.GetNextStep();
            StepBase secondStep = sequenceManager.Sequence.GetNextStep();
            StepBase thirdStep = sequenceManager.Sequence.GetNextStep();

            // Assert
            // Check that thirdStep is not null and is of type StandardStep
            Assert.NotNull(thirdStep);
            Assert.IsType<Step>(thirdStep);

            var standardStep = thirdStep as Step;

            // Now assert the specific properties of the third step
            Assert.Equal("Incubate", standardStep.Name);
            Assert.Equal(45, standardStep.Timeout);
            // Assert other properties specific to the third step
            // For example, for parameters
            Assert.Equal(1, standardStep.Parameters.Count);
            var parameter = standardStep.Parameters.FirstOrDefault();
            Assert.NotNull(parameter);
            Assert.Equal("timeTarget", parameter.Name);
            Assert.Equal("Float", parameter.Type);
            Assert.Equal("60000", parameter.Value);
            // Assert other properties of parameter

        }


    }
}
