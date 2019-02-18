using SimpleJSON;

namespace Downloadables
{
    /// <summary>
    /// This class is responsible for storing information related to the downloading status of a downloadable item.
    /// </summary>
    public class EntryStatus
    {
        private static string ATT_DOWNLOADED_TIMES = "t";
        private static string ATT_VERIFIED = "v";

        /// <summary>
        /// CRC and size of the downloadable in the catalog when this item started being downloaded
        /// </summary>
        public CatalogEntry ManifestEntry { get; set; }

        /// <summary>
        /// Error when doing an I/O operation with the manifest
        /// </summary>
        public Error ManifestError { get; set; }

        /// <summary>
        /// CRC and size of the downloadable in disk so far
        /// </summary>
        public CatalogEntry DataEntry { get; set; }

        /// <summary>
        /// Error when managing this entry data
        /// </summary>
        public Error DataError { get; set; }

        /// <summary>
        /// Amount of times it's been downloaded so far
        /// </summary>
        private int DownloadedTimes { get; set; }        

        public EntryStatus()
        {
            Reset();
        }

        private void Reset()
        {
            if (ManifestEntry == null)
            {
                ManifestEntry = new CatalogEntry();
            }
            else
            {
                ManifestEntry.Reset();
            }

            ManifestError = null;

            if (DataEntry == null)
            {
                DataEntry = new CatalogEntry();
            }
            else
            {
                DataEntry.Reset();
            }

            DataError = null;

            DownloadedTimes = 0;            
        }

        public void Load(JSONNode data, Error manifestError)
        {
            Reset();

            ManifestError = manifestError;

            if (data != null)
            {
                ManifestEntry.Load(data);
                DownloadedTimes = data[ATT_DOWNLOADED_TIMES].AsInt;

                if (GetAttAsBool(data, ATT_VERIFIED))
                {
                    DataEntry.CRC = ManifestEntry.CRC;
                }                
            }
        }        

        public JSONClass ToJSON()
        {
            JSONClass data = ManifestEntry.ToJSON();
            data.Add(ATT_DOWNLOADED_TIMES, DownloadedTimes);
            AddAttAsInt(data, ATT_VERIFIED, IsVerified());            

            return data;
        }

        /// <summary>
        /// Whether or not this downloadable is verified (which means its CRC was ever calculated and matched its manifest's one
        /// </summary>
        private bool IsVerified()
        {            
            return (ManifestEntry != null && DataEntry != null && ManifestEntry.CRC == DataEntry.CRC && ManifestEntry.CRC != 0);
        }

        private bool GetAttAsBool(JSONNode data, string attName)
        {
            return data != null && data[attName].AsInt > 0;
        }

        private void AddAttAsInt(JSONNode data, string attName, bool value)
        {
            if (data != null)
            {
                data[attName] = (value) ? 1 : 0;
            }
        }

        public bool Compare(EntryStatus other)
        {
            return other != null && IsVerified() == other.IsVerified() && DownloadedTimes == other.DownloadedTimes && ManifestEntry.Compare(other.ManifestEntry) && DataEntry.Compare(other.DataEntry);
        }
    }
}
