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
                if (firstSequence.Elements != null && firstSequence.Elements.Any())
                {
                    var firstElement = firstSequence.Elements[0];
                    Assert.Equal("Fill GBs With FB Hydrogel", firstElement.Name);
                    Assert.True(firstElement.IsSynchronous);
                    Assert.False(firstElement.IsParallel);
                    Assert.Equal("Any documentation needed", firstElement.Documentation);

                    if (firstElement is Step firstStep)  // Check if the element is a Step and cast it
                    {
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
                        // Handle the case if the first element is not a Step
                        throw new System.Exception("First element is not a Step");
                    }
                }
                else
                {
                    // Handle the case where the Elements list is empty or null
                    throw new System.Exception("Elements is empty or null");
                }
            }
            else
            {
                // Handle the case where the Sequence is empty or null
                throw new System.Exception("Sequence is empty or null");
            }

        }

        [Fact]
        public void AssertSecondStepValues()
        {
            // Arrange
            var sequenceManager = new SequenceManager();
            string filePath = Path.Combine("Testfiles", "minimum-high-level-sequence.json");

            // Act
            sequenceManager.LoadJSON(filePath);
            var firstElement = sequenceManager.Sequence.GetNextElementsToExecute(); // Get the first step
            var secondElement = sequenceManager.Sequence.GetNextElementsToExecute()[0]; // Get the second step

            // Assert
            Assert.NotNull(secondElement);
            Assert.Equal("Hydrogel Compression", secondElement.Name);
            Assert.False(secondElement.IsSynchronous);
            Assert.Equal("Any documentation needed", secondElement.Documentation);
            if (secondElement is Step secondStep)  // Check if the element is a Step and cast it
            {
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
            else
            {
                // Handle the case if the first element is not a Step
                throw new System.Exception("First element is not a Step");
            }
        }

        [Fact]
        public void AssertParallelStepsAreGrouped()
        {
            // Arrange
            var sequenceManager = new SequenceManager();
            string filePath = Path.Combine("Testfiles", "high-level-sequence.json");

            // Act
            sequenceManager.LoadJSON(filePath);

            // Get the first set of elements (step 1)
            var firstGroup = sequenceManager.Sequence.GetNextElementsToExecute();
            // Get the second set of elements (steps 2 and 3)
            var secondGroup = sequenceManager.Sequence.GetNextElementsToExecute();
            // Get the third set of elements (step 4)
            var thirdGroup = sequenceManager.Sequence.GetNextElementsToExecute();
            // Get the fourth set of elements (steps 5 and 6)
            var fourthGroup = sequenceManager.Sequence.GetNextElementsToExecute();

            // Assert
            Assert.Single(firstGroup); // Only step 1
            Assert.Equal(2, secondGroup.Count); // Steps 2 and 3
            Assert.Single(thirdGroup); // Only step 4
            Assert.Equal(2, fourthGroup.Count); // Steps 5 and 6

            // Check names of steps in the second group (steps 2 and 3)
            Assert.Contains(secondGroup, step => step.Name == "Hydrogel Compression");
            Assert.Contains(secondGroup, step => step.Name == "Incubate");

            // Check names of steps in the fourth group (steps 5 and 6)
            Assert.Contains(fourthGroup, step => step.Name == "Incubate");
            Assert.Contains(fourthGroup, step => step.Name == "In-process Control 1");
        }

        //[Fact]
        //public void AssertMainAndSubSequenceElements()
        //{
        //    // Arrange
        //    var sequenceManager = new SequenceManager();
        //    string mainSequenceFilePath = Path.Combine("Testfiles", "sequence_with_subsequence.json");

        //    // Act
        //    sequenceManager.LoadJSON(mainSequenceFilePath);

        //    // Assert Main Sequence
        //    Assert.Equal("1.0.0", sequenceManager.FileVersion); // Example version
        //    Assert.Equal("mainsequencechecksum", sequenceManager.CheckSum); // Example checksum

        //    var mainSequence = sequenceManager.Sequence;
        //    Assert.NotNull(mainSequence);

        //    // Assert Sub-Sequence
        //    var subSequenceStep = mainSequence.Elements.OfType<SubSequence>().FirstOrDefault();
        //    Assert.NotNull(subSequenceStep);

        //    // Load and assert the sub-sequence
        //    string subSequenceFilePath = Path.Combine("Testfiles", "Subsequences", "My-Subsequence.json");
        //    var subSequenceManager = new SequenceManager();
        //    subSequenceManager.LoadJSON(subSequenceFilePath);

        //    var subSequence = subSequenceManager.Sequence;
        //    Assert.NotNull(subSequence);

        //    // Example assertions for the sub-sequence step
        //    var subSequenceElement = subSequence.Elements.FirstOrDefault();
        //    Assert.NotNull(subSequenceElement);
        //    Assert.Equal("Data Analysis", subSequenceElement.Name);
        //    Assert.False(subSequenceElement.IsSynchronous);
        //    Assert.Equal("Calibrating sensors for accurate readings", subSequenceElement.Documentation);

        //    // Assert Normal Step in Main Sequence
        //    var normalStep = mainSequence.Elements.OfType<Step>().FirstOrDefault();
        //    Assert.NotNull(normalStep);
        //    Assert.Equal("Final Step", normalStep.Name); // Example name for normal step
        //                                                 // Add more assertions for the normal step
        //}
    }
}