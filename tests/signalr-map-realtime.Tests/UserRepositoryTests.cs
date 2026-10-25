#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Tests;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SignalRMapRealtime.Data;
using SignalRMapRealtime.Data.Repositories;
using SignalRMapRealtime.Domain.Models;
using Xunit;

/// <summary>
/// Tests for the UserRepository class.
/// This project has no EF query provider, so only the constructor and the argument guards that run before any data access are covered.
/// Query behaviour (lookups, filtering, deactivation) needs a provider such as EF InMemory, which the IntegrationTests project references.
/// </summary>
public class UserRepositoryTests
{
    /// <summary>
    /// Creates a context over an unused SQL Server configuration. The context is never opened, so no server is contacted.
    /// </summary>
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=unused;Database=unused")
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public void Constructor_ValidContext_CreatesBaseRepository()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var repository = new UserRepository(context);

        // Assert
        repository.Should().BeAssignableTo<BaseRepository<User>>();
    }

    [Fact]
    public void Constructor_NullContext_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new UserRepository(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetByEmailAsync_NullEmail_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new UserRepository(context);

        // Act
        Func<Task> act = () => repository.GetByEmailAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetByEmailAsync_BlankEmail_ThrowsArgumentException(string email)
    {
        // Arrange
        using var context = CreateContext();
        var repository = new UserRepository(context);

        // Act
        Func<Task> act = () => repository.GetByEmailAsync(email);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t ")]
    public async Task GetByEmployeeIdAsync_BlankEmployeeId_ThrowsArgumentException(string employeeId)
    {
        // Arrange
        using var context = CreateContext();
        var repository = new UserRepository(context);

        // Act
        Func<Task> act = () => repository.GetByEmployeeIdAsync(employeeId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetUsersByDepartmentAsync_BlankDepartment_ThrowsArgumentException(string department)
    {
        // Arrange
        using var context = CreateContext();
        var repository = new UserRepository(context);

        // Act
        Func<Task> act = () => repository.GetUsersByDepartmentAsync(department);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetUsersByJobTitleAsync_NullJobTitle_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new UserRepository(context);

        // Act
        Func<Task> act = () => repository.GetUsersByJobTitleAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("\t")]
    public async Task GetUsersByJobTitleAsync_BlankJobTitle_ThrowsArgumentException(string jobTitle)
    {
        // Arrange
        using var context = CreateContext();
        var repository = new UserRepository(context);

        // Act
        Func<Task> act = () => repository.GetUsersByJobTitleAsync(jobTitle);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
