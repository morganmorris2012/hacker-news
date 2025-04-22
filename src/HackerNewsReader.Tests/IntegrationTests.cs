using System.Net;
using System.Net.Http.Json;
using HackerNewsReader.Core.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HackerNewsReader.Tests;

public class IntegrationTests : IClassFixture<WebApplicationFactory<HackerNewsReader.Api.Startup>>
{
    private readonly WebApplicationFactory<HackerNewsReader.Api.Startup> _factory;
    private readonly HttpClient _client;

    public IntegrationTests(WebApplicationFactory<HackerNewsReader.Api.Startup> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetNewestStories_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/stories/newest");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetNewestStories_ReturnsStories()
    {
        // Act
        var response = await _client.GetAsync("/api/stories/newest");
        var stories = await response.Content.ReadFromJsonAsync<List<Story>>();

        // Assert
        Assert.NotNull(stories);
        Assert.NotEmpty(stories);
        Assert.All(stories, story =>
        {
            Assert.NotNull(story.Title);
            Assert.NotNull(story.By);
            Assert.True(story.Time > 0);
        });
    }

    [Fact]
    public async Task SearchStories_ReturnsMatchingStories()
    {
        // Act
        var response = await _client.GetAsync("/api/stories/search?query=test");
        var stories = await response.Content.ReadFromJsonAsync<List<Story>>();

        // Assert
        Assert.NotNull(stories);
        Assert.All(stories, story =>
        {
            Assert.Contains("test", story.Title, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public async Task GetStoryById_ReturnsCorrectStory()
    {
        // Arrange
        var storyId = 1;

        // Act
        var response = await _client.GetAsync($"/api/stories/{storyId}");
        var story = await response.Content.ReadFromJsonAsync<Story>();

        // Assert
        Assert.NotNull(story);
        Assert.Equal(storyId, story.Id);
    }

    [Fact]
    public async Task GetStoryById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = -1;

        // Act
        var response = await _client.GetAsync($"/api/stories/{invalidId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}