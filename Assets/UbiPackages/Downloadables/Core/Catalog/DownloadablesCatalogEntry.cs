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
                string key = ATT_CRC;
                if (data.ContainsKey(key))
                {
                    CRC = data[key].AsLong;
                }

                key = ATT_SIZE;
                if (data.ContainsKey(key))
                {
                    Size = data[key].AsLong;
                }
            }
        }

        public JSONClass ToJSON()
        {
            JSONClass data = new JSONClass();

            JSON_AddLongAtt(data, ATT_CRC, CRC);
            JSON_AddLongAtt(data, ATT_SIZE, Size);            

            return data;
        }       

        private void JSON_AddLongAtt(JSONClass data, string attName, long value)
        {
            JSONNumber jsonNumber = new JSONNumber(value);
            data.Add(attName, jsonNumber);
        }

        public override string ToString()
        {
            return string.Format("[{0}|{1}|", CRC, Size + "]");
        }
    }
}
