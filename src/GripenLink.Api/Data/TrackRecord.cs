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

    public double AltitudeAglMeters { get; set; }
    public double IndicatedAirSpeedMps { get; set; }
    public double MachNumber { get; set; }
    public double VerticalVelocityMps { get; set; }
    public double AngleOfAttackDeg { get; set; }
    public double GLoad { get; set; }
    public double PitchDeg { get; set; }
    public double BankDeg { get; set; }
    public double FuelInternalKg { get; set; }
    public double FuelExternalKg { get; set; }
    public double EngineRpmLeft { get; set; }
    public double EngineRpmRight { get; set; }

    public DateTimeOffset LastUpdateUtc { get; set; }
}
