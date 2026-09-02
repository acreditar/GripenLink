namespace GripenLink.Core.Tracks;

/// <summary>
/// Uma pista aérea acompanhada pela estação de solo.
/// </summary>
public class Track
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string Callsign { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double AltitudeMeters { get; set; }

    public double HeadingDegrees { get; set; }

    public double SpeedMetersPerSecond { get; set; }

    public TrackState State { get; set; } = TrackState.Tentative;

    public DateTimeOffset LastUpdateUtc { get; set; }
}
