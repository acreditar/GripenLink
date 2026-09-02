using System.Buffers.Binary;
using System.Text;
using GripenLink.Core.Tracks;

namespace GripenLink.DataLink;

/// <summary>
/// Mensagem binária de comprimento fixo (28 bytes) que codifica um relato de pista.
/// Estrutura: [magic 1][versão 1][callsign 8][lat 4][lon 4][alt 4][heading 2][speed 2][crc 2].
/// É um "data link tático de brinquedo": ensina framing, endianness e detecção de erro
/// com os mesmos princípios de protocolos reais, sem usar especificações restritas.
/// </summary>
public static class TrackReport
{
    public const byte Magic = 0xA5;
    public const byte Version = 1;
    public const int MessageSize = 28;

    private const int CallsignLength = 8;
    private const int LatOffset = 10;
    private const int LonOffset = 14;
    private const int AltOffset = 18;
    private const int HeadingOffset = 22;
    private const int SpeedOffset = 24;
    private const int CrcOffset = 26;
    private const double CoordinateScale = 1e7;

    public static byte[] Encode(Track track)
    {
        var buffer = new byte[MessageSize];
        buffer[0] = Magic;
        buffer[1] = Version;

        var callsignBytes = Encoding.ASCII.GetBytes(track.Callsign.PadRight(CallsignLength, '\0')[..CallsignLength]);
        callsignBytes.CopyTo(buffer, 2);

        BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(LatOffset), (int)Math.Round(track.Latitude * CoordinateScale));
        BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(LonOffset), (int)Math.Round(track.Longitude * CoordinateScale));
        BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(AltOffset), (int)Math.Round(track.AltitudeMeters));
        BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(HeadingOffset), (ushort)Math.Round(track.HeadingDegrees * 100));
        BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(SpeedOffset), (ushort)Math.Round(track.SpeedMetersPerSecond));

        BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(CrcOffset), Crc16.Compute(buffer.AsSpan(0, CrcOffset)));
        return buffer;
    }

    public static Track Decode(ReadOnlySpan<byte> data)
    {
        if (data.Length != MessageSize)
        {
            throw new ArgumentException($"Expected {MessageSize} bytes, got {data.Length}.", nameof(data));
        }
        if (data[0] != Magic)
        {
            throw new FormatException("Invalid magic byte.");
        }
        if (data[1] != Version)
        {
            throw new FormatException($"Unsupported version {data[1]}.");
        }

        var expected = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(CrcOffset));
        var actual = Crc16.Compute(data.Slice(0, CrcOffset));
        if (expected != actual)
        {
            throw new InvalidDataException("CRC mismatch: message is corrupted.");
        }

        var callsign = Encoding.ASCII.GetString(data.Slice(2, CallsignLength)).TrimEnd('\0');

        return new Track
        {
            Callsign = callsign,
            Latitude = BinaryPrimitives.ReadInt32BigEndian(data.Slice(LatOffset)) / CoordinateScale,
            Longitude = BinaryPrimitives.ReadInt32BigEndian(data.Slice(LonOffset)) / CoordinateScale,
            AltitudeMeters = BinaryPrimitives.ReadInt32BigEndian(data.Slice(AltOffset)),
            HeadingDegrees = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(HeadingOffset)) / 100.0,
            SpeedMetersPerSecond = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(SpeedOffset)),
            State = TrackState.Confirmed
        };
    }
}
