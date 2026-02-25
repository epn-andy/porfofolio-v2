using Microsoft.EntityFrameworkCore;
using PortfolioAPI.Models;

namespace PortfolioAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<JobHistory> JobHistories => Set<JobHistory>();
    public DbSet<CvFile> CvFiles => Set<CvFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>()
            .HasIndex(a => a.Slug)
            .IsUnique();

        modelBuilder.Entity<Admin>()
            .HasIndex(a => a.Email)
            .IsUnique();

        // Store binary data as bytea in PostgreSQL
        modelBuilder.Entity<CvFile>()
            .Property(c => c.Data)
            .HasColumnType("bytea");
    }
}
