using System.ComponentModel.DataAnnotations;
using DatabaseHandler.Data.Models.Database;
using DatabaseHandler.Data.Models.Database.MixedTables;
using DatabaseHandler.Data.Models.Database.ReferencingTables;
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
    public DbSet<Parameter> Parameters { get; set; }
    
    // Referencing Tables
    public DbSet<UserEmail> UserEmail { get; set; }
    public DbSet<UserPassword> UserPassword { get; set; }
    public DbSet<UserCreator> UserCreator { get; set; }
    public DbSet<UserActive> UserActive { get; set; }
    
    // "Mixed Tables"
    public DbSet<CompanyEndPoint> CompanyEndPoints { get; set; }
    public DbSet<UserCompany> UserCompanies { get; set; }
    public DbSet<CompanyRole> CompanyRoles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<ModuleEndPoint> ModuleEndPoints { get; set; }
    public DbSet<RoleEndPoint> RoleEndPoints { get; set; }
    public DbSet<EndPointParameter> EndPointParameters { get; set; }
    
    public DbSet<EndPointParameter> EndPointReturnValues { get; set; }
    


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Auth.db");
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}