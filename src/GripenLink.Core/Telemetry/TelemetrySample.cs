namespace GripenLink.Core.Telemetry;

/// <summary>
/// Uma amostra de telemetria de uma aeronave.
/// É o contrato de entrada da estação (vem do Export.lua do DCS ou de qualquer fonte).
/// </summary>
public record TelemetrySample(
    string Callsign,
    double Latitude,
    double Longitude,
    double AltitudeMeters,
    double HeadingDegrees,
    double SpeedMetersPerSecond,
    DateTimeOffset TimestampUtc);
