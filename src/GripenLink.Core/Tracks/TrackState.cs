namespace GripenLink.Core.Tracks;

/// <summary>
/// Ciclo de vida de uma pista (track) numa estação de solo.
/// Espelha o conceito usado em sistemas de vigilância/C2:
/// Tentative → Confirmed → Coasting → Dropped.
/// </summary>
public enum TrackState
{
    /// <summary>Primeiro contato, ainda não correlacionado/confirmado.</summary>
    Tentative,

    /// <summary>Correlacionada por múltiplas atualizações.</summary>
    Confirmed,

    /// <summary>Sem contato recente (dead reckoning até nova detecção ou queda).</summary>
    Coasting,

    /// <summary>Removida por tempo de expiração.</summary>
    Dropped
}
