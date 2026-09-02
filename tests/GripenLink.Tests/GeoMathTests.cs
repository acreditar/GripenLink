using GripenLink.Core.Geometry;

namespace GripenLink.Tests;

public class GeoMathTests
{
    [Fact]
    public void Distance_ToSelf_IsZero()
    {
        var distance = GeoMath.DistanceMeters(-23.1791, -45.8870, -23.1791, -45.8870);
        Assert.True(distance < 1.0, $"Expected ~0 meters, got {distance}");
    }

    [Fact]
    public void Distance_SjcToSaoPaulo_IsAbout86Km()
    {
        // São José dos Campos → São Paulo (centro)
        var distance = GeoMath.DistanceMeters(-23.1791, -45.8870, -23.5505, -46.6333);
        Assert.InRange(distance, 80_000.0, 90_000.0);
    }

    [Fact]
    public void Bearing_IsInValidRange()
    {
        var bearing = GeoMath.BearingDegrees(-23.1791, -45.8870, -23.5505, -46.6333);
        Assert.InRange(bearing, 0.0, 360.0);
    }
}
