using Microsoft.EntityFrameworkCore;
using VoiceShock.Models;

namespace VoiceShock.Data;

public class AppDbContext : DbContext
{
    public AppDbContext()
    {
        Database.EnsureCreated();
    }

    public DbSet<Word> Words => Set<Word>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<Shocker> Shockers => Set<Shocker>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=voiceshock.db");
    }
}
