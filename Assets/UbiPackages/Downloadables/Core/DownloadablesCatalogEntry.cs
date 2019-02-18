using SimpleJSON;

namespace Downloadables
{
    public class CatalogEntry
    {
        private static string ATT_CRC = "crc32";
        private static string ATT_SIZE = "size";

        public long CRC { get; set; }
        public long Size { get; set; }

        public void Reset()
        {
            CRC = 0L;
            Size = 0L;
        }

        public void Setup(long crc, long size)
        {
            CRC = crc;
            Size = size;
        }

        public bool Compare(CatalogEntry other)
        {
            return other != null && CRC == other.CRC && Size == other.Size;
        }

        /// <summary>
        /// Loads an entry from JSON
        /// </summary>
        /// <param name="data">JSON to load into this entry</param>
        /// <returns></returns>
        public void Load(JSONNode data)
        {
            Reset();

            if (data != null)
            {
                CRC = data[ATT_CRC].AsLong;
                Size = data[ATT_SIZE].AsLong;
            }
        }

        public JSONClass ToJSON()
        {
            JSONClass data = new JSONClass();

            data[ATT_CRC] = CRC;
            data[ATT_SIZE] = Size;

            return data;
        }

        private void AddToJSON(JSONClass data, string attId, string value)
        {
            if (data != null && !string.IsNullOrEmpty(attId) && value != null)
            {
                data[attId] = value;
            }
        }

        public override string ToString()
        {
            return string.Format("[{0}|{1}|", CRC, Size + "]");
        }
    }
}
