using Xunit;
using SequencerLibrary;

namespace SequencerLibrary.Test
{ 
public class ParameterTests
{
    [Fact]
    public void Initialization_ShouldSetProperties()
    {
        // Arrange
        var parameter = new Parameter
        {
            Name = "axisPosition",
            Type = "Float",
            Value = "10.5",
            Required = true,
            DefaultValue = "0.0",
            HooverOverInfo = "Axis position information"
        };

        // Assert
        Assert.Equal("axisPosition", parameter.Name);
        Assert.Equal("Float", parameter.Type);
        Assert.Equal("10.5", parameter.Value);
        Assert.True(parameter.Required);
        Assert.Equal("0.0", parameter.DefaultValue);
        Assert.Equal("Axis position information", parameter.HooverOverInfo);
    }

    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        // Arrange
        var parameter = new Parameter();

        // Assert
        Assert.Null(parameter.Name);
        Assert.Null(parameter.Type);
        Assert.Null(parameter.Value);
        Assert.False(parameter.Required);
        Assert.Null(parameter.DefaultValue);
        Assert.Null(parameter.HooverOverInfo);
    }

    // Additional test cases for Required, Type, and HooverOverInfo...
}
}
