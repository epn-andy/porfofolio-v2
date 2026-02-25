using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PortfolioAPI.Data;
using PortfolioAPI.DTOs;
using PortfolioAPI.Models;

namespace PortfolioAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [EnableRateLimiting("public")]
    public async Task<IActionResult> GetAll()
    {
        var articles = await db.Articles
            .Where(a => a.Published)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new { a.Id, a.Title, a.Slug, a.Excerpt, a.CreatedAt })
            .ToListAsync();
        return Ok(articles);
    }

    [HttpGet("{slug}")]
    [EnableRateLimiting("public")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var article = await db.Articles.FirstOrDefaultAsync(a => a.Slug == slug && a.Published);
        return article is null ? NotFound() : Ok(article);
    }

    [Authorize]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllAdmin()
    {
        var articles = await db.Articles
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
        return Ok(articles);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ArticleDto dto)
    {
        var article = new Article
        {
            Title = dto.Title,
            Slug = dto.Slug,
            Content = dto.Content,
            Excerpt = dto.Excerpt,
            Published = dto.Published
        };
        db.Articles.Add(article);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBySlug), new { slug = article.Slug }, article);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ArticleDto dto)
    {
        var article = await db.Articles.FindAsync(id);
        if (article is null) return NotFound();

        article.Title = dto.Title;
        article.Slug = dto.Slug;
        article.Content = dto.Content;
        article.Excerpt = dto.Excerpt;
        article.Published = dto.Published;
        article.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(article);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var article = await db.Articles.FindAsync(id);
        if (article is null) return NotFound();
        db.Articles.Remove(article);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
