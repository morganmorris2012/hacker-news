using HackerNewsReader.Core.Models;
using HackerNewsReader.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace HackerNewsReader.Tests;

/// <summary>
/// Tests for the HackerNewsService class, focusing on caching behavior and API interactions.
/// These tests verify that:
/// 1. Stories are properly cached and retrieved from cache
/// 2. Cache expiration works as expected
/// 3. API calls are made only when necessary
/// 4. Error cases are handled appropriately
/// </summary>
public class CacheServiceTests
{
    private readonly IMemoryCache _cache;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly HackerNewsService _service;
    private const string BaseUrl = "https://hacker-news.firebaseio.com/v0";

    public CacheServiceTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        // Setup default mock behavior for all HTTP requests to return 404
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("")
            });

        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _service = new HackerNewsService(_httpClient, _cache);
    }

    [Fact(DisplayName = "GetStoryById returns cached story when available")]
    public async Task GetStoryById_WithCachedStory_ReturnsFromCache()
    {
        // Arrange
        var storyId = 1;
        var story = new Story
        {
            Id = storyId,
            Title = "Test Story",
            By = "testuser",
            Score = 100,
            Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Descendants = 5
        };
        var cacheKey = $"story_{storyId}";
        _cache.Set(cacheKey, story);

        // Act
        var result = await _service.GetStoryByIdAsync(storyId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(storyId, result.Id);
        Assert.Equal("Test Story", result.Title);
        Assert.Equal("testuser", result.By);
        Assert.Equal(100, result.Score);
        Assert.Equal(5, result.Descendants);

        // Verify no HTTP call was made
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact(DisplayName = "GetNewestStories returns cached stories when available")]
    public async Task GetNewestStories_WithCachedIds_ReturnsFromCache()
    {
        // Arrange
        var storyIds = new[] { 1, 2, 3 };
        var stories = new[]
        {
            new Story { Id = 1, Title = "Test Story 1", By = "user1", Score = 100 },
            new Story { Id = 2, Title = "Test Story 2", By = "user2", Score = 200 },
            new Story { Id = 3, Title = "Test Story 3", By = "user3", Score = 300 }
        };

        _cache.Set("newstories", storyIds);
        foreach (var story in stories)
        {
            _cache.Set($"story_{story.Id}", story);
        }

        // Act
        var result = await _service.GetNewestStoriesAsync(1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        Assert.Contains(result, s => s.Title == "Test Story 1" && s.By == "user1" && s.Score == 100);
        Assert.Contains(result, s => s.Title == "Test Story 2" && s.By == "user2" && s.Score == 200);
        Assert.Contains(result, s => s.Title == "Test Story 3" && s.By == "user3" && s.Score == 300);

        // Verify no HTTP call was made
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact(DisplayName = "GetStoryById fetches from API when cache expires")]
    public async Task GetStoryById_WithExpiredCache_ReturnsFromApi()
    {
        // Arrange
        var storyId = 1;
        var story = new Story
        {
            Id = storyId,
            Title = "Old Story",
            By = "olduser",
            Score = 50
        };
        var cacheKey = $"story_{storyId}";

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMilliseconds(1));

        _cache.Set(cacheKey, story, cacheOptions);

        // Wait for cache to expire
        await Task.Delay(2);

        // Setup mock response
        var mockStory = new Story
        {
            Id = storyId,
            Title = "Updated Story",
            By = "newuser",
            Score = 100
        };
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(mockStory))
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains($"/item/{storyId}.json")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _service.GetStoryByIdAsync(storyId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Story", result.Title);
        Assert.Equal("newuser", result.By);
        Assert.Equal(100, result.Score);

        // Verify HTTP call was made exactly once
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains($"/item/{storyId}.json")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact(DisplayName = "GetNewestStories fetches from API when cache is empty")]
    public async Task GetNewestStories_WithoutCache_ReturnsFromApi()
    {
        // Arrange
        var storyIds = new[] { 1, 2, 3 };
        var mockStoriesResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(storyIds))
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("/newstories.json")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(mockStoriesResponse);

        var stories = new[]
        {
            new Story { Id = 1, Title = "Story 1", By = "user1", Score = 100 },
            new Story { Id = 2, Title = "Story 2", By = "user2", Score = 200 },
            new Story { Id = 3, Title = "Story 3", By = "user3", Score = 300 }
        };

        foreach (var story in stories)
        {
            var mockResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(story))
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains($"/item/{story.Id}.json")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(mockResponse);
        }

        // Act
        var result = await _service.GetNewestStoriesAsync(1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        Assert.Contains(result, s => s.Title == "Story 1" && s.By == "user1" && s.Score == 100);
        Assert.Contains(result, s => s.Title == "Story 2" && s.By == "user2" && s.Score == 200);
        Assert.Contains(result, s => s.Title == "Story 3" && s.By == "user3" && s.Score == 300);

        // Verify HTTP calls were made
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("/newstories.json")),
            ItExpr.IsAny<CancellationToken>()
        );

        foreach (var id in storyIds)
        {
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains($"/item/{id}.json")),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }

    [Fact(DisplayName = "GetStoryById returns null when story doesn't exist")]
    public async Task GetStoryById_WithNonExistentStory_ReturnsNull()
    {
        // Arrange
        var storyId = 999999;
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotFound,
            Content = new StringContent("")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains($"/item/{storyId}.json")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _service.GetStoryByIdAsync(storyId);

        // Assert
        Assert.Null(result);

        // Verify HTTP call was made exactly once
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains($"/item/{storyId}.json")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact(DisplayName = "SearchStories returns matching stories")]
    public async Task SearchStories_WithMatchingQuery_ReturnsFilteredStories()
    {
        // Arrange
        var stories = new[]
        {
            new Story { Id = 1, Title = "C# Programming", By = "user1", Score = 100 },
            new Story { Id = 2, Title = "Python Tutorial", By = "user2", Score = 200 },
            new Story { Id = 3, Title = "C# Best Practices", By = "user3", Score = 300 }
        };

        // Mock the GetNewestStoriesAsync response
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(stories))
            });

        // Act
        var result = await _service.SearchStoriesAsync("C#", 1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, s => s.Title == "C# Programming");
        Assert.Contains(result, s => s.Title == "C# Best Practices");
        Assert.DoesNotContain(result, s => s.Title == "Python Tutorial");
    }

    [Fact(DisplayName = "GetNewestStories handles pagination correctly")]
    public async Task GetNewestStories_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var storyIds = Enumerable.Range(1, 100).ToArray();
        var mockStoriesResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(storyIds))
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("/newstories.json")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(mockStoriesResponse);

        // Setup mock responses for individual stories
        for (int i = 1; i <= 100; i++)
        {
            var story = new Story { Id = i, Title = $"Story {i}", By = $"user{i}", Score = i * 10 };
            var mockResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(story))
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains($"/item/{i}.json")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(mockResponse);
        }

        // Act
        var page1 = await _service.GetNewestStoriesAsync(1, 20);
        var page2 = await _service.GetNewestStoriesAsync(2, 20);
        var page3 = await _service.GetNewestStoriesAsync(3, 20);

        // Assert
        Assert.Equal(20, page1.Count());
        Assert.Equal(20, page2.Count());
        Assert.Equal(20, page3.Count());

        // Verify first page contains stories 1-20
        Assert.All(page1, s => Assert.True(s.Id >= 1 && s.Id <= 20));
        // Verify second page contains stories 21-40
        Assert.All(page2, s => Assert.True(s.Id >= 21 && s.Id <= 40));
        // Verify third page contains stories 41-60
        Assert.All(page3, s => Assert.True(s.Id >= 41 && s.Id <= 60));
    }
}