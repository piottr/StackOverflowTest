using API.Models;
using API.Services;
using API.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace API.Tests;

public class StackOverflowHttpServiceTests
{
    [Fact]
    public async Task Given_ValidApiResponse_When_GetTagsAsync_Then_ReturnsTags()
    {
        // Arrange
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockLogger = new Mock<ILogger<StackOverflowHttpService>>();
        var mockHandler = new Mock<HttpMessageHandler>();

        var expectedTags = new List<TagDto>
        {
            new TagDto { Name = "c#", Count = 1000 },
            new TagDto { Name = "javascript", Count = 800 }
        };

        var response = new StackExchangeResponse<TagDto> { Items = expectedTags };
        var jsonResponse = JsonSerializer.Serialize(response);

        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(jsonResponse)
        };

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        var httpClient = new HttpClient(mockHandler.Object);
        httpClient.BaseAddress = new Uri("https://api.stackexchange.com");
        mockHttpClientFactory.Setup(f => f.CreateClient("StackExchangeClient")).Returns(httpClient);

        var service = new StackOverflowHttpService(mockHttpClientFactory.Object, mockLogger.Object);

        // Act
        var result = await service.GetTagsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal("c#", result.First().Name);
    }
}