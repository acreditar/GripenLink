using GripenLink.Api.Data;
using GripenLink.Core.Tracks;
using GripenLink.Ingest;
using Microsoft.EntityFrameworkCore;

namespace GripenLink.Api.Services;

/// <summary>
/// Escuta telemetria UDP do DCS (Export.lua → 127.0.0.1:5310), converte em TelemetrySample
/// e alimenta o TrackManager + SQLite. Fase 1.
/// </summary>
public sealed class TelemetryIngestService : BackgroundService
{
    private readonly TrackManager _manager;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelemetryIngestService> _logger;
    private UdpTelemetryReceiver? _receiver;

    private const int Port = 5310;

    public TelemetryIngestService(
        TrackManager manager,
        IServiceScopeFactory scopeFactory,
        ILogger<TelemetryIngestService> logger)
    {
        _manager = manager;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _receiver = new UdpTelemetryReceiver(Port);
        _receiver.DatagramReceived += OnDatagram;
        _receiver.Start();

        _logger.LogInformation("GripenLink ingest ouvindo em UDP {Port}", Port);

        stoppingToken.Register(() =>
        {
            _receiver.Dispose();
            _logger.LogInformation("GripenLink ingest parado");
        });

        return Task.CompletedTask;
    }

    private void OnDatagram(byte[] buffer)
    {
        try
        {
            var sample = DcsTelemetryParser.ParseJson(buffer);
            var track = _manager.Upsert(sample);

            // Persiste de forma best-effort (não bloqueia o loop UDP)
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<GripenLinkDbContext>();
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
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao persistir track {Callsign}", sample.Callsign);
            }

            _logger.LogDebug("Track {Callsign} @ {Lat:F5},{Lon:F5} hdg {Hdg:F0} spd {Spd:F0}",
                track.Callsign, track.Latitude, track.Longitude, track.HeadingDegrees, track.SpeedMetersPerSecond);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Datagrama inválido ({Bytes} bytes)", buffer.Length);
        }
    }
}
