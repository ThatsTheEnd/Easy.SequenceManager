using Xunit;
using Easy.SequenceManager;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Easy.SequenceManager.Test
{
    public class SequenceManagerWithContextTests
    {
        [Fact]
        public void LoadJSON_AndAssertJsonProperties()
        {
            // Arrange
            var sequenceManager = new SequenceManager();
            string filePath = Path.Combine("Testfiles", "minimum-high-level-sequence.json");

            // Act
            sequenceManager.LoadJsonSequence(filePath);

            // Assert
            Assert.Equal("0.1.0", sequenceManager.FileVersion);
            Assert.Equal("cc9d1910bb6e9e878a322eb4add25093", sequenceManager.CheckSum);
            Assert.NotEmpty(sequenceManager.Sequence.Elements);
        }

        [Fact]
        public void TestMinimumSequenceExecution()
        {
            // Arrange
            var sequenceManager = new SequenceManager();
            string filePath = Path.Combine("Testfiles", "minimum-high-level-sequence.json");

            // Act
            sequenceManager.LoadJsonSequence(filePath);
            sequenceManager.InitializeExecution();

            // Assert for Step 1: "Fill Box With Stuff"
            var firstStepGroup = sequenceManager.GetNextStepsToExecute();
            Assert.Single(firstStepGroup);
            var firstStep = firstStepGroup.First() as Step;
            Assert.NotNull(firstStep);
            Assert.Equal("Fill Box With Stuff", firstStep.Name);
            Assert.True(firstStep.IsSynchronous);
            Assert.False(firstStep.IsParallel);

            // Assert for Step 2: "Compression"
            var secondStepGroup = sequenceManager.GetNextStepsToExecute();
            Assert.Single(secondStepGroup);
            var secondStep = secondStepGroup.First() as Step;
            Assert.NotNull(secondStep);
            Assert.Equal("Compression", secondStep.Name);
            Assert.False(secondStep.IsSynchronous);
            Assert.False(secondStep.IsParallel);
        }

        [Fact]
        public void TestHighLevelSequenceExecution()
        {
            // Arrange
            var sequenceManager = new SequenceManager();
            string filePath = Path.Combine("Testfiles", "high-level-sequence.json");

            // Act
            sequenceManager.LoadJsonSequence(filePath);
            sequenceManager.InitializeExecution();

            // Assert Step 1: "Fill Box With Stuff"
            var firstStepGroup = sequenceManager.GetNextStepsToExecute();
            Assert.Single(firstStepGroup);
            Assert.Equal("Fill Box With Stuff", firstStepGroup.First().Name);

            // Assert Steps 2 and 3: "Compression" and "Wait" together
            var secondStepGroup = sequenceManager.GetNextStepsToExecute();
            Assert.Equal(2, secondStepGroup.Count);
            Assert.Contains(secondStepGroup, step => step.Name == "Compression");
            Assert.Contains(secondStepGroup, step => step.Name == "Wait");

            // Assert Step 4: "Exchange Something"
            var thirdStepGroup = sequenceManager.GetNextStepsToExecute();
            Assert.Single(thirdStepGroup);
            Assert.Equal("Exchange Something", thirdStepGroup.First().Name);

            // Assert Steps 5 and 6: "Incubate" and "In-process Control 1" together
            var fourthStepGroup = sequenceManager.GetNextStepsToExecute();
            Assert.Equal(2, fourthStepGroup.Count);
            Assert.Contains(fourthStepGroup, step => step.Name == "Incubate");
            Assert.Contains(fourthStepGroup, step => step.Name == "In-process Control 1");

            // Assert no more steps
            var fifthStepGroup = sequenceManager.GetNextStepsToExecute();
            Assert.Empty(fifthStepGroup);
        }

        [Fact]
        public void TestSequenceWithSubSequence()
        {
            // Arrange
            var sequenceManager = new SequenceManager();
            string filePath = Path.Combine("Testfiles", "sequence_with_subsequence.json");

            // Act
            sequenceManager.LoadJsonSequence(filePath);
            sequenceManager.InitializeExecution();

            // Assert the steps in the sub-sequence
            // Steps "Data Analysis" and "Sensor Calibration" should be together
            var firstStepGroup = sequenceManager.GetNextStepsToExecute();
            Assert.Equal(2, firstStepGroup.Count);
            Assert.Contains(firstStepGroup, step => step.Name == "Data Analysis");
            Assert.Contains(firstStepGroup, step => step.Name == "Sensor Calibration");

            // Step "System Check" should be alone
            var secondStepGroup = sequenceManager.GetNextStepsToExecute();
            Assert.Single(secondStepGroup);
            Assert.Equal("System Check", secondStepGroup.First().Name);

            // Assert the final step in the main sequence
            // Step "Final Step" should be alone
            var thirdStepGroup = sequenceManager.GetNextStepsToExecute();
            Assert.Single(thirdStepGroup);
            Assert.Equal("Final Step", thirdStepGroup.First().Name);

            // Assert no more steps
            var fourthStepGroup = sequenceManager.GetNextStepsToExecute();
            Assert.Empty(fourthStepGroup);
        }

        [Fact]
        public void TestSequenceWithSubSubSequence()
        {
            // Arrange
            var sequenceManager = new SequenceManager();
            string filePath = Path.Combine("Testfiles", "main.json");

            // Act
            sequenceManager.LoadJsonSequence(filePath);
            sequenceManager.InitializeExecution();

            // Assert "Main Step 1"
            var firstStepGroup = sequenceManager.GetNextStepsToExecute();
            Assert.Single(firstStepGroup);
            Assert.Equal("Main Step 1", firstStepGroup.First().Name);

            // Assert "Sub Step 1"
            var secondStepGroup = sequenceManager.GetNextStepsToExecute();
            Assert.Single(secondStepGroup);
            Assert.Equal("Sub Step 1", secondStepGroup.First().Name);

            // Assert "Sub-Sub Step 1", "Sub-Sub Step 2", "Sub-Sub Step 3" together
            var thirdStepGroup = sequenceManager.GetNextStepsToExecute();
            Assert.Equal(3, thirdStepGroup.Count);
            Assert.Contains(thirdStepGroup, step => step.Name == "Sub-Sub Step 1");
            Assert.Contains(thirdStepGroup, step => step.Name == "Sub-Sub Step 2");
            Assert.Contains(thirdStepGroup, step => step.Name == "Sub-Sub Step 3");

            // Assert "Sub Step 3"
            var fourthStepGroup = sequenceManager.GetNextStepsToExecute();
            Assert.Single(fourthStepGroup);
            Assert.Equal("Sub Step 3", fourthStepGroup.First().Name);

            // Assert "Main Step 3"
            var fifthStepGroup = sequenceManager.GetNextStepsToExecute();
            Assert.Single(fifthStepGroup);
            Assert.Equal("Main Step 3", fifthStepGroup.First().Name);

            // Assert no more steps
            var sixthStepGroup = sequenceManager.GetNextStepsToExecute();
            Assert.Empty(sixthStepGroup);
        }

        [Fact]
        public void TestSingleStepDetailsAsJson()
        {
            // Arrange
            var sequenceManager = new SequenceManager();
            string filePath = Path.Combine("Testfiles", "single-step-details.json");

            // Act
            sequenceManager.LoadJsonSequence(filePath);
            sequenceManager.InitializeExecution();
            string json = sequenceManager.GetNextStepsToExecuteAsJson();

            // Assert
            // Check if the JSON contains expected step details
            var jArray = JArray.Parse(json);
            foreach (var step in jArray)
            {
                Assert.NotNull(step);
                Assert.True(step.HasValues);
            }   

            // Assert first step "Move Axis"
            var firstStep = jArray[0];
            Assert.Equal("Move Axis", firstStep["Name"].Value<string>());
            Assert.Equal("Axis", firstStep["TargetModule"].Value<string>());

            json = sequenceManager.GetNextStepsToExecuteAsJson();
            jArray = JArray.Parse(json);
            // Assert second step "Wait"
            var secondStep = jArray[0];
            Assert.Equal("Wait", secondStep["Name"].Value<string>());
            Assert.Equal("Axis", secondStep["TargetModule"].Value<string>());

            // Additional assertions can be added based on specific requirements
        }
    }
}
