using System;
using System.Linq;
using Xunit;
using SignalRMapRealtime.DTOs;

namespace SignalRMapRealtime.Tests
{
    public class UserDtoExtensionsTests
    {
        [Fact]
        public void FormatUserSummary_HappyPath_NoPhone_ReturnsNameAndEmail()
        {
            // Arrange
            var user = new UserDto
            {
                FullName = "Alice Smith",
                Email = "alice@example.com",
                PhoneNumber = null
            };

            // Act
            var result = user.FormatUserSummary();

            // Assert
            Assert.Equal("Alice Smith (alice@example.com)", result);
        }

        [Fact]
        public void FormatUserSummary_HappyPath_WithPhone_ReturnsNameEmailAndPhone()
        {
            // Arrange
            var user = new UserDto
            {
                FullName = "Bob Jones",
                Email = "bob@example.com",
                PhoneNumber = "555-1234"
            };

            // Act
            var result = user.FormatUserSummary();

            // Assert
            Assert.Equal("Bob Jones (bob@example.com), 555-1234", result);
        }

        [Fact]
        public void FormatUserSummary_NullUser_ThrowsArgumentNullException()
        {
            // Arrange
            UserDto user = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => user.FormatUserSummary());
        }

        [Fact]
        public void ToDisplayModel_HappyPath_ReturnsAllProperties()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var user = new UserDto
            {
                Id = 1,
                FullName = "Charlie Brown",
                Email = "charlie@example.com",
                PhoneNumber = "555-9876",
                IsActive = true,
                LastLoginAt = now.AddHours(-2),
                CreatedAt = now.AddDays(-10),
                EmployeeId = "EMP001"
            };

            // Act
            var result = user.ToDisplayModel();

            // Assert
            dynamic dyn = result;
            Assert.Equal(user.Id, dyn.id);
            Assert.Equal(user.FullName, dyn.fullName);
            Assert.Equal(user.Email, dyn.email);
            Assert.Equal(user.PhoneNumber, dyn.phoneNumber);
            Assert.Equal("Active", dyn.status);
            Assert.Equal(now.AddHours(-2).ToString("MM/dd/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture), dyn.lastLogin);
            Assert.Equal(now.AddDays(-10).ToString("MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture), dyn.createdAt);
        }

        [Fact]
        public void ToDisplayModel_NullUser_ThrowsArgumentNullException()
        {
            // Arrange
            UserDto user = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => user.ToDisplayModel());
        }

        [Fact]
        public void ToDisplayModel_NullCreatedAt_ThrowsArgumentNullException()
        {
            // Arrange
            var user = new UserDto
            {
                Id = 2,
                FullName = "Dana White",
                Email = "dana@example.com",
                CreatedAt = default // null for DateTime? DateTime is non-nullable, so use default value
            };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => user.ToDisplayModel());
        }

        [Fact]
        public void ValidateRequiredFields_AllFieldsPresent_ReturnsEmpty()
        {
            // Arrange
            var user = new UserDto
            {
                FullName = "Eve Adams",
                Email = "eve@example.com"
            };

            // Act
            var errors = user.ValidateRequiredFields();

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void ValidateRequiredFields_MissingFullName_ReturnsFullNameError()
        {
            // Arrange
            var user = new UserDto
            {
                FullName = null,
                Email = "frank@example.com"
            };

            // Act
            var errors = user.ValidateRequiredFields();

            // Assert
            Assert.Single(errors);
            Assert.Contains("Full name is required", errors);
        }

        [Fact]
        public void ValidateRequiredFields_MissingEmail_ReturnsEmailError()
        {
            // Arrange
            var user = new UserDto
            {
                FullName = "Grace Hopper",
                Email = null
            };

            // Act
            var errors = user.ValidateRequiredFields();

            // Assert
            Assert.Single(errors);
            Assert.Contains("Email is required", errors);
        }

        [Fact]
        public void ValidateRequiredFields_BothMissing_ReturnsBothErrors()
        {
            // Arrange
            var user = new UserDto
            {
                FullName = null,
                Email = null
            };

            // Act
            var errors = user.ValidateRequiredFields();

            // Assert
            Assert.Equal(2, errors.Count);
            Assert.Contains("Full name is required", errors);
            Assert.Contains("Email is required", errors);
        }

        [Fact]
        public void ValidateRequiredFields_NullUser_ThrowsArgumentNullException()
        {
            // Arrange
            UserDto user = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => user.ValidateRequiredFields());
        }

        [Fact]
        public void GetContactInformation_AllFieldsPresent_ReturnsAll()
        {
            // Arrange
            var user = new UserDto
            {
                Email = "henry@example.com",
                PhoneNumber = "555-0000",
                EmployeeId = "EMP123"
            };

            // Act
            var contacts = user.GetContactInformation();

            // Assert
            Assert.Equal(3, contacts.Count);
            Assert.Contains("henry@example.com", contacts);
            Assert.Contains("555-0000", contacts);
            Assert.Contains("EMP123", contacts);
        }

        [Fact]
        public void GetContactInformation_OnlyEmail_ReturnsSingle()
        {
            // Arrange
            var user = new UserDto
            {
                Email = "irene@example.com",
                PhoneNumber = null,
                EmployeeId = null
            };

            // Act
            var contacts = user.GetContactInformation();

            // Assert
            Assert.Single(contacts);
            Assert.Contains("irene@example.com", contacts);
        }

        [Fact]
        public void GetContactInformation_Empty_ReturnsEmpty()
        {
            // Arrange
            var user = new UserDto
            {
                Email = null,
                PhoneNumber = null,
                EmployeeId = null
            };

            // Act
            var contacts = user.GetContactInformation();

            // Assert
            Assert.Empty(contacts);
        }

        [Fact]
        public void GetContactInformation_NullUser_ThrowsArgumentNullException()
        {
            // Arrange
            UserDto user = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => user.GetContactInformation());
        }
    }
}
