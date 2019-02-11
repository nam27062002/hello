using SimpleJSON;

public class DownloadablesCatalogEntry
{        
    private static string ATT_CRC = "crc32";
    private static string ATT_SIZE = "size";

    public long CRC { get; private set; }
    public long FileSizeBytes { get; private set; }

    public void Reset()
    {
        CRC = 0L;
        FileSizeBytes = 0L;
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
            FileSizeBytes = data[ATT_SIZE].AsLong;
        }
    }

    public JSONClass ToJSON()
    {
        JSONClass data = new JSONClass();

        data[ATT_CRC] = CRC;
        data[ATT_SIZE] = FileSizeBytes;        

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
        return string.Format("[{0}|{1}|", CRC, FileSizeBytes + "]");
    }
}
