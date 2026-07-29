using System;
using Xunit;
using SignalRMapRealtime.Utilities;

namespace SignalRMapRealtime.Tests
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("  ", false)]
        [InlineData("test", true)]
        public void HasValue_ReturnsExpected(string? value, bool expected)
        {
            Assert.Equal(expected, value.HasValue());
        }

        [Theory]
        [InlineData(null, 5, "")]
        [InlineData("Hello World", 5, "He...")]
        [InlineData("Hello World", 11, "Hello World")]
        [InlineData("Hello World", 5, "Hello", false)]
        public void Truncate_ReturnsExpected(string? value, int maxLength, string expected, bool addEllipsis = true)
        {
            Assert.Equal(expected, value.Truncate(maxLength, addEllipsis));
        }

        [Fact]
        public void ToTitleCase_ValidString_ReturnsTitleCase()
        {
            Assert.Equal("Hello World", "hello world".ToTitleCase());
        }

        [Fact]
        public void ToKebabCase_ValidString_ReturnsKebabCase()
        {
            Assert.Equal("hello-world", "HelloWorld".ToKebabCase());
        }

        [Fact]
        public void ToSnakeCase_ValidString_ReturnsSnakeCase()
        {
            Assert.Equal("hello_world", "HelloWorld".ToSnakeCase());
        }

        [Theory]
        [InlineData("Hello", 0, 3, "Hel")]
        [InlineData("Hello", 10, 3, "")]
        [InlineData(null, 0, 3, "")]
        public void SubstringSafe_ReturnsExpected(string? value, int start, int length, string expected)
        {
            Assert.Equal(expected, value.SubstringSafe(start, length));
        }

        [Fact]
        public void RemoveCharacters_RemovesSpecifiedCharacters()
        {
            Assert.Equal("Hll Wrld", "Hello World".RemoveCharacters('e', 'o'));
        }

        [Theory]
        [InlineData("banana", "ana", 1)]
        [InlineData("banana", "a", 3)]
        [InlineData(null, "a", 0)]
        public void CountOccurrences_ReturnsExpectedCount(string? value, string substring, int expected)
        {
            Assert.Equal(expected, value.CountOccurrences(substring));
        }

        [Fact]
        public void Reverse_ReturnsReversedString()
        {
            Assert.Equal("olleh", "hello".Reverse());
        }

        [Theory]
        [InlineData("abc", 3, "abcabcabc")]
        [InlineData("abc", 0, "")]
        [InlineData(null, 3, "")]
        public void Repeat_ReturnsExpectedString(string? value, int count, string expected)
        {
            Assert.Equal(expected, value.Repeat(count));
        }
    }
}
