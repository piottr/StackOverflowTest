using API.Controllers;
using API.Data;
using API.Models;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Linq;
using Xunit;

namespace API.Tests;

public class TagsControllerIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ITagSyncService> _mockTagSyncService;
    private readonly TagsController _controller;

    public TagsControllerIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockTagSyncService = new Mock<ITagSyncService>();
        _controller = new TagsController(_context, _mockTagSyncService.Object);

        SeedData();
    }

    private void SeedData()
    {
        _context.Tags.AddRange(
            new Tag { Name = "c#", Count = 1000, Percentage = 20.0m },
            new Tag { Name = "javascript", Count = 800, Percentage = 16.0m },
            new Tag { Name = "python", Count = 600, Percentage = 12.0m }
        );
        _context.SaveChanges();
    }

    [Fact]
    public async Task Given_TagsInDatabase_When_GetTags_Then_ReturnsPagedResult()
    {
        // Arrange
        var request = new PrimeNgRequestDto { First = 0, Rows = 10 };

        // Act
        var result = await _controller.GetTags(request);

        // Assert
        var pagedResult = Assert.IsType<PagedResult<TagPercentageDto>>(result);
        Assert.Equal(3, pagedResult.TotalRecords);
        Assert.Equal(3, pagedResult.Data.Count());
    }

    [Fact]
    public async Task Given_TagsInDatabase_When_GetTagsWithSortByNameDescending_Then_ReturnsSortedTags()
    {
        // Arrange
        var request = new PrimeNgRequestDto { First = 0, Rows = 10, SortField = "name", SortOrder = -1 };

        // Act
        var result = await _controller.GetTags(request);

        // Assert
        var pagedResult = Assert.IsType<PagedResult<TagPercentageDto>>(result);
        Assert.True(pagedResult.Data.ToList().First().Name == "python");
    }

    [Fact]
    public async Task Given_TagsInDatabase_When_ForceSync_Then_CallsTagSyncService()
    {
        // Arrange
        _mockTagSyncService.Setup(s => s.ForceSyncAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ForceSync();

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _mockTagSyncService.Verify(s => s.ForceSyncAsync(), Times.Once);
    }

    [Fact]
    public async Task Given_LargeDatabaseWithPaging_When_GetTags_Then_ReturnsCorrectPage()
    {
        // Arrange - Add more tags to test pagination
        var additionalTags = Enumerable.Range(1, 20)
            .Select(i => new Tag 
            { 
                Name = $"tag{i}", 
                Count = 100 * i, 
                Percentage = i * 2.0m 
            })
            .ToList();

        _context.Tags.AddRange(additionalTags);
        _context.SaveChanges();

        var request = new PrimeNgRequestDto { First = 10, Rows = 10 };

        // Act
        var result = await _controller.GetTags(request);

        // Assert
        var pagedResult = Assert.IsType<PagedResult<TagPercentageDto>>(result);
        Assert.Equal(23, pagedResult.TotalRecords);
        Assert.Equal(10, pagedResult.Data.Count());
    }

    [Fact]
    public async Task Given_EmptyDatabase_When_GetTags_Then_ReturnsEmptyPagedResult()
    {
        // Arrange - Create a fresh context with no data
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var emptyContext = new AppDbContext(options);
        var emptyController = new TagsController(emptyContext, _mockTagSyncService.Object);

        var request = new PrimeNgRequestDto { First = 0, Rows = 10 };

        // Act
        var result = await emptyController.GetTags(request);

        // Assert
        var pagedResult = Assert.IsType<PagedResult<TagPercentageDto>>(result);
        Assert.Equal(0, pagedResult.TotalRecords);
        Assert.Empty(pagedResult.Data);

        emptyContext.Dispose();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}