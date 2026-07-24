using System;
using Xunit;
using SignalRMapRealtime.DTOs;

namespace SignalRMapRealtime.Tests
{
    public class UserDtoValidationTests
    {
        private static UserDto CreateValidUser() => new()
        {
            Id = 1,
            FullName = "Test User",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        [Fact] public void Validate_HappyPath_ValidUser_ReturnsEmpty()
        {
            var user = CreateValidUser();
            Assert.Empty(user.Validate());
        }

        [Fact] public void Validate_NullUser_Throws()
        {
            UserDto user = null!;
            Assert.Throws<ArgumentNullException>(() => user.Validate());
        }

        [Fact] public void Validate_InvalidId_ReturnsError()
        {
            var user = CreateValidUser();
            user.Id = -1;
            var errors = user.Validate();
            Assert.Single(errors);
            Assert.Contains("Id must be a positive integer.", errors);
        }

        [Fact] public void Validate_EmptyFullName_ReturnsError()
        {
            var user = CreateValidUser();
            user.FullName = "";
            var errors = user.Validate();
            Assert.Single(errors);
            Assert.Contains("FullName cannot be null or whitespace.", errors);
        }

        [Fact] public void Validate_TooLongFullName_ReturnsError()
        {
            var user = CreateValidUser();
            user.FullName = new string('A', 201);
            var errors = user.Validate();
            Assert.Single(errors);
            Assert.Contains("FullName cannot exceed 200 characters.", errors);
        }

        [Fact] public void Validate_NullEmail_ReturnsError()
        {
            var user = CreateValidUser();
            user.Email = null!;
            var errors = user.Validate();
            Assert.Single(errors);
            Assert.Contains("Email cannot be null or whitespace.", errors);
        }

        [Fact] public void Validate_TooLongEmail_ReturnsError()
        {
            var user = CreateValidUser();
            user.Email = new string('a', 255) + "@example.com";
            var errors = user.Validate();
            Assert.Single(errors);
            Assert.Contains("Email cannot exceed 254 characters.", errors);
        }

        [Theory] [InlineData("invalid")] [InlineData("@test.com")]
        public void Validate_InvalidEmail_ReturnsError(string email)
        {
            var user = CreateValidUser();
            user.Email = email;
            var errors = user.Validate();
            Assert.Single(errors);
            Assert.Contains("Email must be a valid email address.", errors);
        }

        [Fact] public void Validate_TooLongPhone_ReturnsError()
        {
            var user = CreateValidUser();
            user.PhoneNumber = new string('1', 21);
            var errors = user.Validate();
            Assert.Single(errors);
            Assert.Contains("PhoneNumber cannot exceed 20 characters.", errors);
        }

        [Fact] public void Validate_TooLongEmployeeId_ReturnsError()
        {
            var user = CreateValidUser();
            user.EmployeeId = new string('E', 51);
            var errors = user.Validate();
            Assert.Single(errors);
            Assert.Contains("EmployeeId cannot exceed 50 characters.", errors);
        }

        [Fact] public void Validate_TooLongJobTitle_ReturnsError()
        {
            var user = CreateValidUser();
            user.JobTitle = new string('J', 101);
            var errors = user.Validate();
            Assert.Single(errors);
            Assert.Contains("JobTitle cannot exceed 100 characters.", errors);
        }

        [Fact] public void Validate_TooLongDepartment_ReturnsError()
        {
            var user = CreateValidUser();
            user.Department = new string('D', 101);
            var errors = user.Validate();
            Assert.Single(errors);
            Assert.Contains("Department cannot exceed 100 characters.", errors);
        }

        [Fact] public void Validate_FutureLastLogin_ReturnsError()
        {
            var user = CreateValidUser();
            user.LastLoginAt = DateTime.UtcNow.AddDays(1);
            var errors = user.Validate();
            Assert.Single(errors);
            Assert.Contains("LastLoginAt cannot be in the future.", errors);
        }

        [Fact] public void Validate_LastLoginBeforeCreated_ReturnsError()
        {
            var now = DateTime.UtcNow;
            var user = CreateValidUser();
            user.LastLoginAt = now.AddDays(-15);
            user.CreatedAt = now.AddDays(-10);
            var errors = user.Validate();
            Assert.Single(errors);
            Assert.Contains("LastLoginAt cannot be before CreatedAt.", errors);
        }

        [Fact] public void Validate_DefaultCreatedAt_ReturnsError()
        {
            var user = CreateValidUser();
            user.CreatedAt = default;
            var errors = user.Validate();
            Assert.Single(errors);
            Assert.Contains("CreatedAt must be set to a valid DateTime.", errors);
        }

        [Fact] public void Validate_FutureCreatedAt_ReturnsError()
        {
            var user = CreateValidUser();
            user.CreatedAt = DateTime.UtcNow.AddHours(1);
            var errors = user.Validate();
            Assert.Single(errors);
            Assert.Contains("CreatedAt cannot be in the future.", errors);
        }

        [Fact] public void Validate_MultipleErrors_ReturnsAll()
        {
            var user = CreateValidUser();
            user.Id = -1;
            user.FullName = "";
            user.Email = "bad";
            user.PhoneNumber = "123";
            user.EmployeeId = new string('E', 51);
            user.JobTitle = new string('J', 101);
            user.Department = new string('D', 101);
            user.CreatedAt = DateTime.UtcNow.AddHours(1);
            Assert.Equal(7, user.Validate().Count);
        }

        [Fact] public void IsValid_ValidUser_ReturnsTrue()
        {
            Assert.True(CreateValidUser().IsValid());
        }

        [Fact] public void IsValid_NullUser_ReturnsFalse()
        {
            UserDto user = null!;
            Assert.False(user.IsValid());
        }

        [Fact] public void IsValid_InvalidUser_ReturnsFalse()
        {
            var user = CreateValidUser();
            user.Id = -1;
            Assert.False(user.IsValid());
        }

        [Fact] public void EnsureValid_ValidUser_NoException()
        {
            var ex = Record.Exception(() => CreateValidUser().EnsureValid());
            Assert.Null(ex);
        }

        [Fact] public void EnsureValid_NullUser_Throws()
        {
            UserDto user = null!;
            Assert.Throws<ArgumentNullException>(() => user.EnsureValid());
        }

        [Fact] public void EnsureValid_InvalidUser_Throws()
        {
            var user = CreateValidUser();
            user.Id = -1;
            var ex = Assert.Throws<ArgumentException>(() => user.EnsureValid());
            Assert.Contains("UserDto validation failed:", ex.Message);
        }

        [Fact] public void Validate_ReturnsReadOnlyList()
        {
            Assert.IsAssignableFrom<IReadOnlyList<string>>(CreateValidUser().Validate());
        }
    }
}