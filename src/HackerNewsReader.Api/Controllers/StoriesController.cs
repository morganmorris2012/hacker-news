using HackerNewsReader.Core.Interfaces;
using HackerNewsReader.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace HackerNewsReader.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoriesController : ControllerBase
{
    private readonly IHackerNewsService _hackerNewsService;

    public StoriesController(IHackerNewsService hackerNewsService)
    {
        _hackerNewsService = hackerNewsService;
    }

    [HttpGet("newest")]
    public async Task<IActionResult> GetNewestStories([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var stories = await _hackerNewsService.GetNewestStoriesAsync(page, pageSize);
        return Ok(stories);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchStories([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var stories = await _hackerNewsService.SearchStoriesAsync(query, page, pageSize);
        return Ok(stories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStoryById(int id)
    {
        var story = await _hackerNewsService.GetStoryByIdAsync(id);
        if (story == null)
        {
            return NotFound();
        }
        return Ok(story);
    }
}