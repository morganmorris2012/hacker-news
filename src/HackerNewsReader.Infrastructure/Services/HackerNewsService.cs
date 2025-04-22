using System.Text.Json;
using HackerNewsReader.Core.Interfaces;
using HackerNewsReader.Core.Models;
using Microsoft.Extensions.Caching.Memory;

namespace HackerNewsReader.Infrastructure.Services;

public class HackerNewsService : IHackerNewsService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private const string BaseUrl = "https://hacker-news.firebaseio.com/v0";
    private const string NewStoriesCacheKey = "newstories";
    private const int CacheExpirationMinutes = 5;
    private const int CacheSlidingExpirationMinutes = 2;
    private const int MaxCacheSize = 1000; // Maximum number of items in cache

    public HackerNewsService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<IEnumerable<Story>> GetNewestStoriesAsync(int page = 1, int pageSize = 20)
    {
        var storyIds = await GetNewStoryIdsAsync();
        var pagedIds = storyIds
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        var stories = new List<Story>();
        foreach (var id in pagedIds)
        {
            var story = await GetStoryByIdAsync(id);
            if (story != null)
            {
                stories.Add(story);
            }
        }

        return stories;
    }

    public async Task<IEnumerable<Story>> SearchStoriesAsync(string query, int page = 1, int pageSize = 20)
    {
        var allStories = await GetNewestStoriesAsync(1, 200); // Get a larger set to search from
        return allStories
            .Where(s => s.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    public async Task<Story?> GetStoryByIdAsync(int id)
    {
        var cacheKey = $"story_{id}";
        if (_cache.TryGetValue<Story>(cacheKey, out var cachedStory))
        {
            return cachedStory;
        }

        var response = await _httpClient.GetAsync($"{BaseUrl}/item/{id}.json");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        var story = JsonSerializer.Deserialize<Story>(content);

        if (story != null)
        {
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheExpirationMinutes))
                .SetSlidingExpiration(TimeSpan.FromMinutes(CacheSlidingExpirationMinutes))
                .SetSize(1) // Each story takes 1 unit of cache size
                .SetPriority(CacheItemPriority.Normal);

            _cache.Set(cacheKey, story, cacheOptions);
            return story;
        }

        return null;
    }

    private async Task<IEnumerable<int>> GetNewStoryIdsAsync()
    {
        if (_cache.TryGetValue<int[]>(NewStoriesCacheKey, out var cachedIds))
        {
            return cachedIds;
        }

        var response = await _httpClient.GetAsync($"{BaseUrl}/newstories.json");
        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<int>();
        }

        var content = await response.Content.ReadAsStringAsync();
        var ids = JsonSerializer.Deserialize<int[]>(content) ?? Array.Empty<int>();

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheExpirationMinutes))
            .SetSlidingExpiration(TimeSpan.FromMinutes(CacheSlidingExpirationMinutes))
            .SetSize(1) // Story IDs list takes 1 unit of cache size
            .SetPriority(CacheItemPriority.High);

        _cache.Set(NewStoriesCacheKey, ids, cacheOptions);
        return ids;
    }
}