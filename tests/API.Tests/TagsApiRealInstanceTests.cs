using API.Data;
using API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;
using System.Collections.Generic;
using Xunit;

namespace API.Tests;

public class TagsApiRealInstanceTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private AppDbContext _context;

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");
                builder.ConfigureAppConfiguration(config =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["InMemoryDbName"] = "TestDb_" + Guid.NewGuid().ToString("N")
                    });
                });
            });

        _client = _factory.CreateClient();

        // Get the context and seed data
        using (var scope = _factory.Services.CreateScope())
        {
            _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await _context.Database.EnsureCreatedAsync();
            
            SeedTestData();
        }
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    private void SeedTestData()
    {
        _context.Tags.AddRange(
            new Tag { Name = "c#", Count = 1000, Percentage = 20.0m },
            new Tag { Name = "javascript", Count = 800, Percentage = 16.0m },
            new Tag { Name = "python", Count = 600, Percentage = 12.0m },
            new Tag { Name = "java", Count = 550, Percentage = 11.0m },
            new Tag { Name = "go", Count = 450, Percentage = 9.0m },
            new Tag { Name = "rust", Count = 300, Percentage = 6.0m },
            new Tag { Name = "typescript", Count = 700, Percentage = 14.0m }
        );
        _context.SaveChanges();
    }

    [Fact]
    public async Task Given_ApiRunning_When_GetTagsEndpoint_Then_ReturnsOkWithData()
    {
        // Act
        var response = await _client.GetAsync("/api/tags");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.True(root.TryGetProperty("data", out var data));
        Assert.True(root.TryGetProperty("totalRecords", out var totalRecords));
        Assert.Equal(7, totalRecords.GetInt32());
        Assert.Equal(7, data.GetArrayLength());
    }

    [Fact]
    public async Task Given_ApiRunning_When_GetTagsWithPaging_Then_ReturnsCorrectPage()
    {
        // Act
        var response = await _client.GetAsync("/api/tags?first=2&rows=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.True(root.TryGetProperty("data", out var data));
        Assert.True(root.TryGetProperty("totalRecords", out var totalRecords));
        
        Assert.Equal(7, totalRecords.GetInt32());
        Assert.Equal(2, data.GetArrayLength()); // Should return 2 items (rows=2)
    }

    [Fact]
    public async Task Given_ApiRunning_When_GetTagsSortedByCountDescending_Then_ReturnsSortedData()
    {
        // Act
        var response = await _client.GetAsync("/api/tags?sortField=count&sortOrder=-1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.True(root.TryGetProperty("data", out var data));
        var items = data.EnumerateArray().ToList();

        // First item should be c# with count 1000 (highest)
        Assert.True(items[0].TryGetProperty("count", out var firstCount));
        Assert.Equal(1000, firstCount.GetInt32());
    }

    [Fact]
    public async Task Given_ApiRunning_When_GetTagsSortedByNameAscending_Then_ReturnsAlphabeticalOrder()
    {
        // Act
        var response = await _client.GetAsync("/api/tags?sortField=name&sortOrder=1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.True(root.TryGetProperty("data", out var data));
        var items = data.EnumerateArray().ToList();

        // First item should be c# (alphabetically first)
        Assert.True(items[0].TryGetProperty("name", out var firstName));
        Assert.Equal("c#", firstName.GetString());

        // Last item should be typescript (alphabetically last)
        Assert.True(items[items.Count - 1].TryGetProperty("name", out var lastName));
        Assert.Equal("typescript", lastName.GetString());
    }

    [Fact]
    public async Task Given_ApiRunning_When_GetTagsWithInvalidSortField_Then_ReturnsUnsortedData()
    {
        // Act
        var response = await _client.GetAsync("/api/tags?sortField=invalid&sortOrder=1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.True(root.TryGetProperty("data", out var data));
        Assert.True(root.TryGetProperty("totalRecords", out var totalRecords));
        Assert.Equal(7, totalRecords.GetInt32());
        Assert.Equal(7, data.GetArrayLength());
    }

    [Fact]
    public async Task Given_ApiRunning_When_GetTagsWithoutParameters_Then_ReturnsDefaultPagedResult()
    {
        // Act
        var response = await _client.GetAsync("/api/tags");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.True(root.TryGetProperty("data", out var data));
        Assert.True(root.TryGetProperty("totalRecords", out var totalRecords));
        
        // Should return all 7 tags (default rows = 10, which is more than 7)
        Assert.Equal(7, totalRecords.GetInt32());
        Assert.Equal(7, data.GetArrayLength());
    }

    [Fact]
    public async Task Given_ApiRunning_When_GetTagsWithZeroRows_Then_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/tags?rows=0");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
