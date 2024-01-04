using Xunit;

namespace Easy.SequenceManager.Test
{
    public class ParameterTests
    {
        private Parameter _parameter;

        public ParameterTests()
        {
            _parameter = new Parameter();
        }

        [Fact]
        public void NameProperty_ShouldGetAndSet()
        {
            _parameter.Name = "TestName";
            Assert.Equal("TestName", _parameter.Name);
        }

        [Fact]
        public void TypeProperty_ShouldGetAndSet()
        {
            _parameter.Type = "TestType";
            Assert.Equal("TestType", _parameter.Type);
        }

        [Fact]
        public void ValueProperty_ShouldGetAndSet()
        {
            _parameter.Value = "TestValue";
            Assert.Equal("TestValue", _parameter.Value);
        }

        [Fact]
        public void RequiredProperty_ShouldGetAndSet()
        {
            _parameter.Required = true;
            Assert.True(_parameter.Required);
        }

        [Fact]
        public void DefaultValueProperty_ShouldGetAndSet()
        {
            _parameter.DefaultValue = "TestDefaultValue";
            Assert.Equal("TestDefaultValue", _parameter.DefaultValue);
        }

        [Fact]
        public void HooverOverInfoProperty_ShouldGetAndSet()
        {
            _parameter.HooverOverInfo = "TestHooverOverInfo";
            Assert.Equal("TestHooverOverInfo", _parameter.HooverOverInfo);
        }
    }
}