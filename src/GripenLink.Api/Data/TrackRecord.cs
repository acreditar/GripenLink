namespace GripenLink.Api.Data;

/// <summary>
/// Entidade persistida de uma pista (mapeada para o banco).
/// </summary>
public class TrackRecord
{
    public Guid Id { get; set; }

    public required string Callsign { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double AltitudeMeters { get; set; }

    public double HeadingDegrees { get; set; }

    public double SpeedMetersPerSecond { get; set; }

    public DateTimeOffset LastUpdateUtc { get; set; }
}
