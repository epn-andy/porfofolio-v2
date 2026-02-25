using Microsoft.EntityFrameworkCore;
using PortfolioAPI.Models;

namespace PortfolioAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<JobHistory> JobHistories => Set<JobHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>()
            .HasIndex(a => a.Slug)
            .IsUnique();

        modelBuilder.Entity<Admin>()
            .HasIndex(a => a.Email)
            .IsUnique();
    }
}
