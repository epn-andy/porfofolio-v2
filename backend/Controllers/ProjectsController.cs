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
public class ProjectsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [EnableRateLimiting("public")]
    public async Task<IActionResult> GetAll()
    {
        var projects = await db.Projects
            .OrderBy(p => p.Order)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync();
        return Ok(projects);
    }

    [HttpGet("{id}")]
    [EnableRateLimiting("public")]
    public async Task<IActionResult> GetById(int id)
    {
        var project = await db.Projects.FindAsync(id);
        return project is null ? NotFound() : Ok(project);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProjectDto dto)
    {
        var project = new Project
        {
            Title = dto.Title,
            Description = dto.Description,
            TechStack = dto.TechStack,
            LiveUrl = dto.LiveUrl,
            GithubUrl = dto.GithubUrl,
            ImageUrl = dto.ImageUrl,
            Order = dto.Order
        };
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProjectDto dto)
    {
        var project = await db.Projects.FindAsync(id);
        if (project is null) return NotFound();

        project.Title = dto.Title;
        project.Description = dto.Description;
        project.TechStack = dto.TechStack;
        project.LiveUrl = dto.LiveUrl;
        project.GithubUrl = dto.GithubUrl;
        project.ImageUrl = dto.ImageUrl;
        project.Order = dto.Order;

        await db.SaveChangesAsync();
        return Ok(project);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await db.Projects.FindAsync(id);
        if (project is null) return NotFound();
        db.Projects.Remove(project);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
