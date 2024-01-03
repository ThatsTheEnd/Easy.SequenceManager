using Xunit;
using Easy.SequenceManager;
using System.IO;
using System.Linq;
using System.Collections.Generic;

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
            sequenceManager.LoadJsonSequence(filePath);

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

                    Assert.Equal("Any documentation needed", firstElement.Documentation);

                    if (firstElement is Step firstStep)  // Check if the element is a Step and cast it
                    {
                        Assert.True(firstStep.IsSynchronous);
                        Assert.False(firstStep.IsParallel);
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
            sequenceManager.LoadJsonSequence(filePath);
            var firstElement = sequenceManager.Sequence.GetNextElementsToExecute(); // Get the first step
            var secondElement = sequenceManager.Sequence.GetNextElementsToExecute()[0]; // Get the second step

            // Assert
            Assert.NotNull(secondElement);
            Assert.Equal("Hydrogel Compression", secondElement.Name);
            Assert.Equal("Any documentation needed", secondElement.Documentation);
            if (secondElement is Step secondStep)  // Check if the element is a Step and cast it
            {
                Assert.Equal(45, secondStep.Timeout);
                Assert.Equal("", secondStep.TargetModule);
                Assert.False(secondStep.IsSynchronous);
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
            sequenceManager.LoadJsonSequence(filePath);

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

        [Fact]
        public void AssertSubSequenceContents()
        {
            // Arrange
            var sequenceManager = new SequenceManager();
            string subSequenceFilePath = Path.Combine("Testfiles", "Subsequences", "My-Subsequence.json");

            // Act
            sequenceManager.LoadJsonSequence(subSequenceFilePath);

            // Assert Sub-Sequence is loaded
            Assert.NotNull(sequenceManager.Sequence);
            Assert.NotEmpty(sequenceManager.Sequence.Elements);

            // Verify the content of the sub-sequence
            var subSequenceStep = sequenceManager.Sequence.Elements.FirstOrDefault() as Step;
            Assert.NotNull(subSequenceStep);
            Assert.Equal("Data Analysis", subSequenceStep.Name);
            Assert.False(subSequenceStep.IsSynchronous);
            Assert.Equal("Calibrating sensors for accurate readings", subSequenceStep.Documentation);
            Assert.Equal(119, subSequenceStep.Timeout);
            Assert.Equal("ModuleD", subSequenceStep.TargetModule);

            // Verify Parameters of the sub-sequence step
            Assert.NotEmpty(subSequenceStep.Parameters);
            var parameter = subSequenceStep.Parameters.FirstOrDefault();
            Assert.NotNull(parameter);
            Assert.Equal("param4", parameter.Name);
            Assert.Equal("Float", parameter.Type);
            Assert.Equal("9", parameter.Value);
            Assert.True(parameter.Required);
            Assert.Equal("19", parameter.DefaultValue);
            Assert.Equal("Info about data analysis parameter", parameter.HooverOverInfo);
        }

        [Fact]
        public void AssertSubSequenceIsLoadedCorrectly()
        {
            // Arrange
            var sequenceManager = new SequenceManager();
            string mainSequenceFilePath = Path.Combine("Testfiles", "sequence_with_subsequence.json");

            // Act
            sequenceManager.LoadJsonSequence(mainSequenceFilePath);

            // Assert
            Assert.NotNull(sequenceManager.Sequence);
            Assert.NotEmpty(sequenceManager.Sequence.Elements);

            // Find the sub-sequence element
            var subSequenceElement = sequenceManager.Sequence.Elements
                                        .OfType<SubSequence>()
                                        .FirstOrDefault();
            Assert.NotNull(subSequenceElement);
            Assert.Equal("Sub-Sequence Step", subSequenceElement.Name);
            Assert.Equal("This step refers to a sub-sequence", subSequenceElement.Documentation);
            Assert.NotNull(subSequenceElement.Sequence);
            Assert.NotEmpty(subSequenceElement.Sequence.Elements);

            // Verify the content of the sub-sequence
            var subSequenceStep = subSequenceElement.Sequence.Elements.FirstOrDefault();
            Assert.NotNull(subSequenceStep);
            Assert.Equal("Data Analysis", subSequenceStep.Name);
            Assert.Equal("Calibrating sensors for accurate readings", subSequenceStep.Documentation);
            Assert.True(subSequenceStep is Step);

            // If it's a Step, verify its parameters
            var step = subSequenceStep as Step;
            Assert.NotNull(step.Parameters);
            var param = step.Parameters.FirstOrDefault();
            Assert.NotNull(param);
            Assert.Equal("param4", param.Name);
            Assert.Equal("Float", param.Type);
            Assert.Equal("9", param.Value);
        }

        [Fact]
        public void TestSequenceWithSubsequenceExecution()
        {
            // Arrange
            var sequenceManager = new SequenceManager();
            string filePath = Path.Combine("Testfiles", "sequence_with_subsequence.json");

            // Act
            sequenceManager.LoadJsonSequence(filePath);

            // Assert the first step of the subsequence
            var firstGroup = sequenceManager.Sequence.GetNextElementsToExecute();
            Assert.NotEmpty(firstGroup);
            if (firstGroup.Count!=2)
            {
                // convert the firstgroup object to string and include in exception message
                string firstGroupString = string.Join(",", firstGroup.Select(x => x.Name));
                throw new System.Exception($"Expected 2 step in first group, but got {firstGroup.Count} steps: {firstGroupString}");
                
            }
            Assert.Equal(2, firstGroup.Count); // Only one step is expected
            Assert.Equal("Data Analysis", firstGroup[0].Name);  // First step of the subsequence

            // Assert the second and third steps of the subsequence (executed in parallel)
            var secondGroup = sequenceManager.Sequence.GetNextElementsToExecute();
            Assert.Equal(1, secondGroup.Count); // Two steps are expected
            Assert.Contains(secondGroup, step => step.Name == "System Check");

            // Assert the final step of the main sequence
            var thirdGroup = sequenceManager.Sequence.GetNextElementsToExecute();
            Assert.Single(thirdGroup);
            Assert.Equal("Final Step", thirdGroup[0].Name);

            // Keep calling GetNextElementsToExecute until an empty list is returned
            int stepCount = firstGroup.Count + secondGroup.Count + thirdGroup.Count;
            List<SequenceElement> nextGroup;
            while ((nextGroup = sequenceManager.Sequence.GetNextElementsToExecute()).Count > 0)
            {
                stepCount += nextGroup.Count;
            }

            // Assert that the whole sequence has 4 steps (including subsequence steps)
            Assert.Equal(4, stepCount);
        }
    }
    }