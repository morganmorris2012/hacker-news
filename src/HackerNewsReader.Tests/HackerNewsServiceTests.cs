using System.Net;
using HackerNewsReader.Core.Models;
using HackerNewsReader.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;

namespace HackerNewsReader.Tests;

public class HackerNewsServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly HackerNewsService _service;

    public HackerNewsServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _service = new HackerNewsService(_httpClient, _memoryCache);
    }

    [Fact]
    public async Task GetNewestStoriesAsync_ReturnsStories()
    {
        // Arrange
        var storyIds = new[] { 1, 2, 3 };
        var story = new Story { Id = 1, Title = "Test Story", Url = "http://test.com" };

        SetupMockResponse("https://hacker-news.firebaseio.com/v0/newstories.json",
            HttpStatusCode.OK,
            System.Text.Json.JsonSerializer.Serialize(storyIds));

        SetupMockResponse("https://hacker-news.firebaseio.com/v0/item/1.json",
            HttpStatusCode.OK,
            System.Text.Json.JsonSerializer.Serialize(story));

        // Act
        var result = await _service.GetNewestStoriesAsync(1, 1);

        // Assert
        Assert.Single(result);
        var firstStory = result.First();
        Assert.Equal(story.Id, firstStory.Id);
        Assert.Equal(story.Title, firstStory.Title);
        Assert.Equal(story.Url, firstStory.Url);
    }

    [Fact]
    public async Task SearchStoriesAsync_ReturnsMatchingStories()
    {
        // Arrange
        var storyIds = new[] { 1, 2 };
        var stories = new[]
        {
            new Story { Id = 1, Title = "Test Story 1", Url = "http://test1.com" },
            new Story { Id = 2, Title = "Different Story", Url = "http://test2.com" }
        };

        SetupMockResponse("https://hacker-news.firebaseio.com/v0/newstories.json",
            HttpStatusCode.OK,
            System.Text.Json.JsonSerializer.Serialize(storyIds));

        SetupMockResponse("https://hacker-news.firebaseio.com/v0/item/1.json",
            HttpStatusCode.OK,
            System.Text.Json.JsonSerializer.Serialize(stories[0]));

        SetupMockResponse("https://hacker-news.firebaseio.com/v0/item/2.json",
            HttpStatusCode.OK,
            System.Text.Json.JsonSerializer.Serialize(stories[1]));

        // Act
        var result = await _service.SearchStoriesAsync("Test", 1, 10);

        // Assert
        Assert.Single(result);
        var story = result.First();
        Assert.Equal("Test Story 1", story.Title);
    }

    private void SetupMockResponse(string url, HttpStatusCode statusCode, string content)
    {
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == url),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }
}