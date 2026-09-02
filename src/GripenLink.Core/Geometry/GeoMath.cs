namespace GripenLink.Core.Geometry;

/// <summary>
/// Cálculos geodésicos básicos para navegação aérea.
/// Referência: fórmulas de Haversine e de bearing (rumo inicial) no modelo esférico WGS84.
/// </summary>
public static class GeoMath
{
    public const double EarthRadiusMeters = 6371000.0;

    /// <summary>Distância em metros entre dois pontos (grande círculo).</summary>
    public static double DistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusMeters * c;
    }

    /// <summary>Rumo inicial em graus (0..360), sentido horário a partir do norte.</summary>
    public static double BearingDegrees(double lat1, double lon1, double lat2, double lon2)
    {
        var phi1 = ToRadians(lat1);
        var phi2 = ToRadians(lat2);
        var deltaLon = ToRadians(lon2 - lon1);

        var y = Math.Sin(deltaLon) * Math.Cos(phi2);
        var x = Math.Cos(phi1) * Math.Sin(phi2) - Math.Sin(phi1) * Math.Cos(phi2) * Math.Cos(deltaLon);

        return (ToDegrees(Math.Atan2(y, x)) + 360) % 360;
    }

    public static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    public static double ToDegrees(double radians) => radians * 180.0 / Math.PI;
}
