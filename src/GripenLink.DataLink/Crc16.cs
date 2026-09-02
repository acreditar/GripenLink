namespace GripenLink.DataLink;

/// <summary>
/// CRC-16-CCITT (polinômio 0x1021), init 0xFFFF — usado na checagem de integridade
/// das mensagens do data link tático (mesmo papel do CRC em ARINC 429/framing binário).
/// </summary>
public static class Crc16
{
    public static ushort Compute(ReadOnlySpan<byte> data)
    {
        ushort crc = 0xFFFF;
        foreach (var b in data)
        {
            crc ^= (ushort)(b << 8);
            for (var i = 0; i < 8; i++)
            {
                crc = (crc & 0x8000) != 0
                    ? (ushort)((crc << 1) ^ 0x1021)
                    : (ushort)(crc << 1);
            }
        }
        return crc;
    }
}
