using Xunit;
using Easy.SequenceManager;
using System.IO;
using System.Linq;

namespace Easy.SequenceManager
{
    public class SequenceManagerTests
    {
        [Fact]
        public void LoadJSON_AndAssertValues()
        {
            // Arrange
            var sequenceManager = new SequenceManager();
            string filePath = Path.Combine("Testfiles", "minimum-high-level-sequence.json");

            // Act
            sequenceManager.LoadJSON(filePath);

            // Assert
            Assert.Equal("0.1.0", sequenceManager.FileVersion);
            Assert.Equal("cc9d1910bb6e9e878a322eb4add25093", sequenceManager.CheckSum);

            if (sequenceManager.Sequence != null)
            {
                var firstSequence = sequenceManager.Sequence;
                if (firstSequence.Steps != null && firstSequence.Steps.Any())
                {
                    var firstStep = firstSequence.Steps[0];
                    Assert.Equal("Fill GBs With FB Hydrogel", firstStep.Name);
                    Assert.True(firstStep.IsSychronous);
                    Assert.Equal("Any documentation needed", firstStep.Documentation);
                    Assert.Equal(30, firstStep.Timeout);
                    Assert.Equal("", firstStep.TargetModule);

                    var firstParameter = firstStep.Parameters[0];
                    Assert.Equal("pumpSpeed", firstParameter.Name);
                    Assert.Equal("Integer", firstParameter.Type);
                    Assert.Equal("500", firstParameter.Value);
                    Assert.True(firstParameter.Required);
                    Assert.Equal("300", firstParameter.DefaultValue);
                    Assert.Equal("Pump speed in RPM", firstParameter.HooverOverInfo);
                }
                else
                {
                    // Handle the case where the list is empty or null
                    throw new System.Exception("Steps is empty or null");
                }
            }
            else
            {
                // Handle the case where the list is empty or null
                throw new System.Exception("Sequence is empty or null");
            }

            // Assert other properties of secondParameter...
        }

        [Fact]
        public void AssertSecondStepValues()
        {
            // Arrange
            var sequenceManager = new SequenceManager();
            string filePath = Path.Combine("Testfiles", "minimum-high-level-sequence.json");

            // Act
            sequenceManager.LoadJSON(filePath);
            var firstStep = sequenceManager.Sequence.GetNextStepToExecute(); // Get the first step
            var secondStep = sequenceManager.Sequence.GetNextStepToExecute(); // Get the second step

            // Assert
            Assert.NotNull(secondStep);
            Assert.Equal("Hydrogel Compression", secondStep.Name);
            Assert.True(secondStep.IsSychronous);
            Assert.Equal("Any documentation needed", secondStep.Documentation);
            Assert.Equal(45, secondStep.Timeout);
            Assert.Equal("", secondStep.TargetModule);

            // Asserting the parameters of the second step
            Assert.NotNull(secondStep.Parameters);
            Assert.Equal(4, secondStep.Parameters.Count);
            Assert.Equal("axisTargetPosition1", secondStep.Parameters[0].Name);
            Assert.Equal("Float", secondStep.Parameters[0].Type);
            Assert.Equal("10.5", secondStep.Parameters[0].Value);
            Assert.True(secondStep.Parameters[0].Required);
            Assert.Equal("0.0", secondStep.Parameters[0].DefaultValue);
            Assert.Equal("Target position for the axis movement", secondStep.Parameters[0].HooverOverInfo);

        }
    }
}