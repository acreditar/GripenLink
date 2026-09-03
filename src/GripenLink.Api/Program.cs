using GripenLink.Api.Data;
using GripenLink.Api.Hubs;
using GripenLink.Api.Services;
using GripenLink.Core.Telemetry;
using GripenLink.Core.Tracks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GripenLinkDbContext>(options =>
    options.UseSqlite("Data Source=gripenlink.db"));

builder.Services.AddSingleton<TrackManager>();
builder.Services.AddHostedService<TelemetryIngestService>();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GripenLinkDbContext>();
    db.Database.EnsureCreated();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();

app.MapHub<TracksHub>("/tracksHub");

app.MapGet("/", () => Results.Ok(new
{
    name = "GripenLink",
    description = "Mini estação de solo / C2 alimentada por telemetria (DCS)",
    status = "operational"
}));

app.MapGet("/health", () => Results.Ok(new { status = "ok", utc = DateTimeOffset.UtcNow }));

app.MapGet("/tracks", (TrackManager manager) => Results.Ok(manager.Tracks));

app.MapPost("/telemetry", (TelemetrySample sample, TrackManager manager, GripenLinkDbContext db) =>
{
    var track = manager.Upsert(sample);

    var record = db.Tracks.Find(track.Id);
    if (record is null)
    {
        record = new TrackRecord { Id = track.Id, Callsign = track.Callsign };
        db.Tracks.Add(record);
    }

    record.Latitude = track.Latitude;
    record.Longitude = track.Longitude;
    record.AltitudeMeters = track.AltitudeMeters;
    record.HeadingDegrees = track.HeadingDegrees;
    record.SpeedMetersPerSecond = track.SpeedMetersPerSecond;
    record.LastUpdateUtc = track.LastUpdateUtc;

    db.SaveChanges();

    return Results.Created($"/tracks/{track.Id}", track);
});

app.Run();
