using HackerNewsReader.Core.Models;
using System;

namespace HackerNewsReader.Tests;

public class StoryModelTests
{
    [Fact]
    public void Story_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var story = new Story
        {
            Id = 1,
            Title = "Test Story",
            Url = "https://test.com",
            Score = 100,
            By = "testuser",
            Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Descendants = 10,
            Type = "story"
        };

        // Assert
        Assert.Equal(1, story.Id);
        Assert.Equal("Test Story", story.Title);
        Assert.Equal("https://test.com", story.Url);
        Assert.Equal(100, story.Score);
        Assert.Equal("testuser", story.By);
        Assert.True(story.Time > 0);
        Assert.Equal(10, story.Descendants);
        Assert.Equal("story", story.Type);
    }

    [Fact]
    public void Story_WithEmptyTitle_ShouldThrowException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new Story { Title = string.Empty });
    }

    [Fact]
    public void Story_WithInvalidUrl_ShouldThrowException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new Story { Url = "invalid-url" });
    }
}