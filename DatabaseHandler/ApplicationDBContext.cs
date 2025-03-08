using System.ComponentModel.DataAnnotations;
using DatabaseHandler.Data.Models.Database;
using DatabaseHandler.Data.Models.Database.LookupTables;
using Microsoft.EntityFrameworkCore;

namespace DatabaseHandler;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
        
    }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // "Primitives"
    public DbSet<Company> Companies { get; set; }
    public DbSet<EndPoint> EndPoints { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Module> Modules { get; set; }
    
    // Referencing Tables
    
    
    // "Mixed Tables"
    public DbSet<CompanyEndPoint> CompanyEndPoints { get; set; }
    public DbSet<CompanyUser> CompanyUsers { get; set; }
    public DbSet<CompanyRole> CompanyRoles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }  

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Auth.db");
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}