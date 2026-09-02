using GripenLink.Core.Telemetry;
using GripenLink.Core.Tracks;

namespace GripenLink.Tests;

public class TrackManagerTests
{
    private static TelemetrySample Sample(string callsign, DateTimeOffset? at = null) => new(
        callsign,
        Latitude: -23.1791,
        Longitude: -45.8870,
        AltitudeMeters: 3000,
        HeadingDegrees: 90,
        SpeedMetersPerSecond: 200,
        TimestampUtc: at ?? DateTimeOffset.UtcNow);

    [Fact]
    public void Upsert_FirstReport_CreatesTentativeTrack()
    {
        var manager = new TrackManager();
        var track = manager.Upsert(Sample("GRIPEN01"));

        Assert.Single(manager.Tracks);
        Assert.Equal(TrackState.Tentative, track.State);
    }

    [Fact]
    public void Upsert_SecondReport_ConfirmsTrack()
    {
        var manager = new TrackManager();
        manager.Upsert(Sample("GRIPEN01", DateTimeOffset.UtcNow.AddSeconds(-10)));
        var track = manager.Upsert(Sample("GRIPEN01"));

        Assert.Single(manager.Tracks);
        Assert.Equal(TrackState.Confirmed, track.State);
    }

    [Fact]
    public void RemoveDropped_RemovesStaleTracks()
    {
        var manager = new TrackManager();
        var now = DateTimeOffset.UtcNow;

        manager.Upsert(Sample("FRESH", now));
        manager.Upsert(Sample("STALE", now.AddMinutes(-10)));

        var removed = manager.RemoveDropped(now, TimeSpan.FromMinutes(5));

        Assert.Equal(1, removed);
        Assert.DoesNotContain(manager.Tracks, t => t.Callsign == "STALE");
    }
}
