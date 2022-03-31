using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Listener.Models;

public class ListenerContext : DbContext
{
    public DbSet<Log> Log { get; set; }
    public DbSet<Node> Node { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(@"Data Source=MeshListener.db");
    }
}
