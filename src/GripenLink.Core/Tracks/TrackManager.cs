using GripenLink.Core.Telemetry;

namespace GripenLink.Core.Tracks;

/// <summary>
/// Correlaciona amostras de telemetria em pistas e gerencia o ciclo de vida delas.
/// Fase atual: correlação simples por callsign (sem fusão sensorial ainda — entra na Fase 2).
/// </summary>
public class TrackManager
{
    private readonly Dictionary<string, Track> _tracks = new();

    public IReadOnlyCollection<Track> Tracks => _tracks.Values;

    /// <summary>Cria ou atualiza a pista do callsign e devolve a pista resultante.</summary>
    public Track Upsert(TelemetrySample sample)
    {
        if (!_tracks.TryGetValue(sample.Callsign, out var track))
        {
            track = new Track { Callsign = sample.Callsign };
            _tracks[sample.Callsign] = track;
            ApplyUpdate(track, sample);
            return track; // permanece Tentative no primeiro contato
        }

        ApplyUpdate(track, sample);

        if (track.State == TrackState.Tentative)
        {
            track.State = TrackState.Confirmed;
        }

        return track;
    }

    /// <summary>Marca como Coasting as pistas sem contato há mais de <paramref name="staleAfter"/>.</summary>
    public void Coast(DateTimeOffset now, TimeSpan staleAfter)
    {
        foreach (var track in _tracks.Values)
        {
            if (track.State != TrackState.Dropped && now - track.LastUpdateUtc > staleAfter)
            {
                track.State = TrackState.Coasting;
            }
        }
    }

    /// <summary>Remove pistas sem contato há mais de <paramref name="dropAfter"/>. Devolve quantas removeu.</summary>
    public int RemoveDropped(DateTimeOffset now, TimeSpan dropAfter)
    {
        var toRemove = _tracks.Values
            .Where(t => now - t.LastUpdateUtc > dropAfter)
            .Select(t => t.Callsign)
            .ToList();

        foreach (var callsign in toRemove)
        {
            if (_tracks.TryGetValue(callsign, out var track))
            {
                track.State = TrackState.Dropped;
            }
            _tracks.Remove(callsign);
        }

        return toRemove.Count;
    }

    private static void ApplyUpdate(Track track, TelemetrySample sample)
    {
        track.Latitude = sample.Latitude;
        track.Longitude = sample.Longitude;
        track.AltitudeMeters = sample.AltitudeMeters;
        track.AltitudeAglMeters = sample.AltitudeAglMeters;
        track.HeadingDegrees = sample.HeadingDegrees;
        track.SpeedMetersPerSecond = sample.SpeedMetersPerSecond;
        track.IndicatedAirSpeedMps = sample.IndicatedAirSpeedMps;
        track.MachNumber = sample.MachNumber;
        track.VerticalVelocityMps = sample.VerticalVelocityMps;
        track.AngleOfAttackDeg = sample.AngleOfAttackDeg;
        track.GLoad = sample.GLoad;
        track.PitchDeg = sample.PitchDeg;
        track.BankDeg = sample.BankDeg;
        track.FuelInternalKg = sample.FuelInternalKg;
        track.FuelExternalKg = sample.FuelExternalKg;
        track.EngineRpmLeft = sample.EngineRpmLeft;
        track.EngineRpmRight = sample.EngineRpmRight;
        track.LastUpdateUtc = sample.TimestampUtc;
    }
}
