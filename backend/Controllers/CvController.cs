using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioAPI.Data;
using PortfolioAPI.Models;

namespace PortfolioAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CvController(AppDbContext db) : ControllerBase
{
    private static readonly string[] AllowedTypes = ["application/pdf"];
    private const long MaxBytes = 10 * 1024 * 1024; // 10 MB

    /// <summary>Returns metadata of the current CV (most recently uploaded).</summary>
    [HttpGet]
    public async Task<IActionResult> GetInfo()
    {
        var cv = await db.CvFiles
            .OrderByDescending(c => c.UploadedAt)
            .Select(c => new { c.Id, c.FileName, c.ContentType, c.UploadedAt })
            .FirstOrDefaultAsync();

        return cv is null ? NotFound() : Ok(cv);
    }

    /// <summary>Downloads the current CV file.</summary>
    [HttpGet("download")]
    public async Task<IActionResult> Download()
    {
        var cv = await db.CvFiles
            .OrderByDescending(c => c.UploadedAt)
            .FirstOrDefaultAsync();

        if (cv is null) return NotFound();

        return File(cv.Data, cv.ContentType, cv.FileName);
    }

    /// <summary>Uploads a new CV (PDF only, max 10 MB). Replaces any existing CV.</summary>
    [Authorize]
    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file provided." });

        if (!AllowedTypes.Contains(file.ContentType))
            return BadRequest(new { error = "Only PDF files are allowed." });

        if (file.Length > MaxBytes)
            return BadRequest(new { error = "File exceeds 10 MB limit." });

        // Remove previous CVs
        var existing = await db.CvFiles.ToListAsync();
        db.CvFiles.RemoveRange(existing);

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var cv = new CvFile
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            Data = ms.ToArray(),
        };

        db.CvFiles.Add(cv);
        await db.SaveChangesAsync();

        return Ok(new { cv.Id, cv.FileName, cv.ContentType, cv.UploadedAt });
    }

    /// <summary>Deletes the CV.</summary>
    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cv = await db.CvFiles.FindAsync(id);
        if (cv is null) return NotFound();
        db.CvFiles.Remove(cv);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
