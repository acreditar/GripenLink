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
    DateTimeOffset TimestampUtc,
    double AltitudeAglMeters = 0,
    double IndicatedAirSpeedMps = 0,
    double MachNumber = 0,
    double VerticalVelocityMps = 0,
    double AngleOfAttackDeg = 0,
    double GLoad = 0,
    double PitchDeg = 0,
    double BankDeg = 0,
    double FuelInternalKg = 0,
    double FuelExternalKg = 0,
    double EngineRpmLeft = 0,
    double EngineRpmRight = 0);
