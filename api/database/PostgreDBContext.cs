using Microsoft.EntityFrameworkCore;

public class PostgreDbContext : DbContext
{
    public PostgreDbContext(DbContextOptions<PostgreDbContext> options) 
        : base(options)
    {
        Database.Migrate();
    }

    public DbSet<Contact> Contacts { get; set; }
}