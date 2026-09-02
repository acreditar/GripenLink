using GripenLink.Core.Tracks;
using GripenLink.DataLink;

namespace GripenLink.Tests;

public class DataLinkTests
{
    [Fact]
    public void EncodeDecode_RoundTrip_PreservesFields()
    {
        var original = new Track
        {
            Callsign = "GRIPEN01",
            Latitude = -23.1791,
            Longitude = -45.8870,
            AltitudeMeters = 3500,
            HeadingDegrees = 125.5,
            SpeedMetersPerSecond = 250,
            State = TrackState.Confirmed
        };

        var bytes = TrackReport.Encode(original);
        var decoded = TrackReport.Decode(bytes);

        Assert.Equal("GRIPEN01", decoded.Callsign);
        Assert.Equal(-23.1791, decoded.Latitude, precision: 5);
        Assert.Equal(-45.8870, decoded.Longitude, precision: 5);
        Assert.Equal(3500, decoded.AltitudeMeters, precision: 0);
        Assert.Equal(125.5, decoded.HeadingDegrees, precision: 1);
        Assert.Equal(250, decoded.SpeedMetersPerSecond, precision: 0);
    }

    [Fact]
    public void Decode_CorruptedMessage_ThrowsCrcError()
    {
        var track = new Track
        {
            Callsign = "VULCAN",
            Latitude = 0,
            Longitude = 0,
            AltitudeMeters = 100,
            HeadingDegrees = 90,
            SpeedMetersPerSecond = 150
        };

        var bytes = TrackReport.Encode(track);
        bytes[18] ^= 0xFF; // corrompe o byte de altitude (dentro da região protegida pelo CRC)

        Assert.Throws<InvalidDataException>(() => TrackReport.Decode(bytes));
    }

    [Fact]
    public void Decode_WrongSize_Throws()
    {
        Assert.Throws<ArgumentException>(() => TrackReport.Decode(new byte[10]));
    }
}
