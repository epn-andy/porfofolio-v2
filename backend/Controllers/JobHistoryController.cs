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
public class JobHistoryController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [EnableRateLimiting("public")]
    public async Task<IActionResult> GetAll()
    {
        var jobs = await db.JobHistories
            .OrderBy(j => j.Order)
            .ThenByDescending(j => j.StartDate)
            .ToListAsync();
        return Ok(jobs);
    }

    [HttpGet("{id}")]
    [EnableRateLimiting("public")]
    public async Task<IActionResult> GetById(int id)
    {
        var job = await db.JobHistories.FindAsync(id);
        return job is null ? NotFound() : Ok(job);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] JobHistoryDto dto)
    {
        var job = new JobHistory
        {
            Company = dto.Company,
            Role = dto.Role,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsCurrentRole = dto.IsCurrentRole,
            Order = dto.Order
        };
        db.JobHistories.Add(job);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = job.Id }, job);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] JobHistoryDto dto)
    {
        var job = await db.JobHistories.FindAsync(id);
        if (job is null) return NotFound();

        job.Company = dto.Company;
        job.Role = dto.Role;
        job.Description = dto.Description;
        job.StartDate = dto.StartDate;
        job.EndDate = dto.EndDate;
        job.IsCurrentRole = dto.IsCurrentRole;
        job.Order = dto.Order;

        await db.SaveChangesAsync();
        return Ok(job);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var job = await db.JobHistories.FindAsync(id);
        if (job is null) return NotFound();
        db.JobHistories.Remove(job);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
