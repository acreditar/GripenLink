using System.Text.Json;
using GripenLink.Core.Telemetry;

namespace GripenLink.Ingest;

/// <summary>
/// Converte o JSON exportado pelo DCS (Export.lua) em <see cref="TelemetrySample"/>.
/// Contrato esperado (Fase 1): {"callsign":"...","latitude":...,"longitude":...,"altitude":...,"heading":...,"speed":...,"timestamp":"..."}
/// </summary>
public static class DcsTelemetryParser
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static TelemetrySample ParseJson(byte[] datagram)
    {
        var root = JsonDocument.Parse(datagram).RootElement;

        var callsign = root.TryGetProperty("callsign", out var cs) ? cs.GetString() : "UNKNOWN";
        var latitude = GetDouble(root, "latitude");
        var longitude = GetDouble(root, "longitude");
        var altitude = GetDouble(root, "altitude");
        var heading = GetDouble(root, "heading");
        var speed = GetDouble(root, "speed");

        var timestamp = root.TryGetProperty("timestamp", out var ts) && ts.TryGetDateTimeOffset(out var t)
            ? t
            : DateTimeOffset.UtcNow;

        return new TelemetrySample(callsign ?? "UNKNOWN", latitude, longitude, altitude, heading, speed, timestamp);
    }

    private static double GetDouble(JsonElement root, string property)
        => root.TryGetProperty(property, out var el) && el.ValueKind == JsonValueKind.Number
            ? el.GetDouble()
            : 0;
}
