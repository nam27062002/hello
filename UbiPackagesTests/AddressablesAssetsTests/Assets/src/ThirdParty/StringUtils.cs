public class StringUtils
{
    private static uint[] crc32Table = null;

    public static long CRC32(byte[] bytes)
    {
        if (crc32Table == null)
        {
            crc32Table = new uint[256];
            uint poly = 0xedb88320;
            uint temp = 0;
            for (uint i = 0; i < crc32Table.Length; ++i)
            {
                temp = i;
                for (int j = 8; j > 0; --j)
                {
                    if ((temp & 1) == 1)
                    {
                        temp = (uint)((temp >> 1) ^ poly);
                    }
                    else
                    {
                        temp >>= 1;
                    }
                }
                crc32Table[i] = temp;
            }
        }

        uint crc = 0xffffffff;
        for (int i = 0; i < bytes.Length; ++i)
        {
            byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
            crc = (uint)((crc >> 8) ^ crc32Table[index]);
        }

        return (long)~crc;
    }
}
