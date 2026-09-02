using Microsoft.EntityFrameworkCore;

namespace GripenLink.Api.Data;

public class GripenLinkDbContext : DbContext
{
    public GripenLinkDbContext(DbContextOptions<GripenLinkDbContext> options) : base(options)
    {
    }

    public DbSet<TrackRecord> Tracks => Set<TrackRecord>();
}
