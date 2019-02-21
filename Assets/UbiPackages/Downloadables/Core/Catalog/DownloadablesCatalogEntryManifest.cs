using SimpleJSON;

namespace Downloadables
{
    /// <summary>
    /// This class is responsible for storing information of an entry. This information is useful to know what the
    /// catalog entry was like when the downloadable data stored in Downloads was requested.
    /// </summary>
    public class CatalogEntryManifest
    {
        private static string ATT_DOWNLOADED_TIMES = "t";
        private static string ATT_VERIFIED = "v";

        public CatalogEntry CatalogEntry { get; set; }

        public long CRC
        {
            get
            {
                return CatalogEntry.CRC;
            }

            set
            {
                CatalogEntry.CRC = value;
                NeedsToSave = true;
            }
        }

        public long Size
        {
            get
            {
                return CatalogEntry.Size;
            }

            set
            {
                CatalogEntry.Size = value;
                NeedsToSave = true;
            }
        }

        /// <summary>
        /// Whether or not this manifest should be saved to disk
        /// </summary>
        private bool m_needsToSave;
        public bool NeedsToSave
        {
            get { return m_needsToSave; }
            set
            {
                m_needsToSave = value;
            }
        }

        /// <summary>
        /// Amount of times it's been downloaded successfully
        /// </summary>
        private int m_downloadedTimes;
        private int DownloadedTimes
        {
            get { return m_downloadedTimes; }

            set
            {
                m_downloadedTimes = value;
                NeedsToSave = true;
            }
        }

        /// <summary>
        /// Whether or not its CRC was already calculated and matched with the one in the catalog
        /// </summary>
        private bool m_isVerified;
        public bool IsVerified
        {
            get { return m_isVerified; }
            set
            {
                m_isVerified = value;
                NeedsToSave = true;
            }
        }

        public CatalogEntryManifest()
        {
            CatalogEntry = new CatalogEntry();

            Reset();
        }

        public void Reset()
        {
            CatalogEntry.Reset();

            DownloadedTimes = 0;
            IsVerified = false;
            NeedsToSave = false;
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
                CatalogEntry.Load(data);

                m_downloadedTimes = data[ATT_DOWNLOADED_TIMES].AsInt;
                m_isVerified = GetAttAsBool(data, ATT_VERIFIED);                
            }
        }

        public JSONClass ToJSON()
        {
            JSONClass data = CatalogEntry.ToJSON();

            data[ATT_DOWNLOADED_TIMES] = DownloadedTimes;
            AddAttAsInt(data, ATT_VERIFIED, IsVerified);            

            return data;
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

        public bool Compare(CatalogEntryManifest manifest)
        {
            return manifest != null && CatalogEntry.Compare(manifest.CatalogEntry) &&
                   IsVerified == manifest.IsVerified && DownloadedTimes == manifest.DownloadedTimes;
        }

    }
}
