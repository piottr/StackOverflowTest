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

    public void Dispose()
    {
        _context.Dispose();
    }
}