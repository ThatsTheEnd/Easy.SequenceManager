using Xunit;
using SequencerLibrary;
using System.IO;
using System.Reflection;
using System.Linq; // For using LINQ to count elements

namespace SequencerLibrary.Test
{

    public class SequenceManagerTests
{
        [Fact]
        public void LoadXML_WithSingleStep_ShouldParseCorrectly()
        {
            // Arrange
            var sequenceManager = new SequenceManager();
            string xmlContent = @"
            <SequenceManager FileVersion='0.1.0' CheckSum='abcded'>
                <Sequence>
                    <Step Name=""Hydrogel Compression"" Sychronous=""true"" Documentation=""Any documentation needed"" Timeout=""45"" TargetModule="""">
                        <Parameters>
                            <Parameter Name=""axisTargetPosition1"" Type=""Float"" Value=""10.5"" Required=""true"" DefaultValue=""0.0"" HooverOverInfo=""Target position for the axis movement""/>
                            <Parameter Name=""axisSpeed1"" Type=""Float"" Value=""10.5"" Required=""true"" DefaultValue=""0.0"" HooverOverInfo=""Target position for the axis movement""/>
                            <Parameter Name=""axisTargetPosition2"" Type=""Float"" Value=""10.5"" Required=""true"" DefaultValue=""0.0"" HooverOverInfo=""Target position for the axis movement""/>
                            <Parameter Name=""axisSpeed2"" Type=""Float"" Value=""10.5"" Required=""true"" DefaultValue=""0.0"" HooverOverInfo=""Target position for the axis movement""/>
                        </Parameters>
                    </Step>
                </Sequence>
            </SequenceManager>";

            // Create a temporary file
            string tempFilePath = Path.GetTempFileName();
            File.WriteAllText(tempFilePath, xmlContent);

            // Act
            sequenceManager.LoadXML(tempFilePath);
            var steps = sequenceManager.Sequence.GetRemainingSteps();

            // Additional assertions and null checks
            Assert.NotNull(sequenceManager);
            Assert.NotNull(sequenceManager.Sequence);

            // Assert
            Assert.NotNull(steps);
            Assert.Single(steps);
            Assert.Equal("abcded", sequenceManager.Checksum);
            Assert.Equal("0.1.0", sequenceManager.FileVersion);

            // Clean up
            File.Delete(tempFilePath);
        }

        [Fact]

    public void LoadXML_ShouldCreateSequenceWithCorrectNumberOfSteps()
    {
        // Arrange
        var sequenceManager = new SequenceManager();
        string filePath = Path.Combine("Testfiles", "high-level-sequence.xml");

        // Act
        sequenceManager.LoadXML(filePath);
        var steps = sequenceManager.Sequence.GetRemainingSteps();

        // Assert
        Assert.NotNull(steps);
        int expectedStepCount = 6; // Update this based on the actual number of steps in your XML
        Assert.Equal(expectedStepCount, steps.Count());
    }
}
}
