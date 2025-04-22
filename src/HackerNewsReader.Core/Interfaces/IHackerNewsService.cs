using HackerNewsReader.Core.Models;

namespace HackerNewsReader.Core.Interfaces;

public interface IHackerNewsService
{
    Task<IEnumerable<Story>> GetNewestStoriesAsync(int page = 1, int pageSize = 20);
    Task<IEnumerable<Story>> SearchStoriesAsync(string query, int page = 1, int pageSize = 20);
    Task<Story?> GetStoryByIdAsync(int id);
}