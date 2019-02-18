using SimpleJSON;
using System.Collections;
using System.Collections.Generic;

namespace Downloadables
{
    /// <summary>
    /// This class is responsible for storing all downloadable entries.
    /// </summary>
    public class Catalog
    {
        private Dictionary<string, CatalogEntry> Entries { get; set; }

        private static string CATALOG_ATT_ENTRIES = "assets";

        public Catalog()
        {
            Entries = new Dictionary<string, CatalogEntry>();
        }

        public void Reset()
        {
            Entries.Clear();
        }

        /// <summary>
        /// Loads data from a json, which must fulfill the following structure: http://dragon.ubi.com/dragon/assetsLUT?combined
        /// </summary>
        /// <param name="catalogJSON">Json file that must fulfill http://dragon.ubi.com/dragon/assetsLUT?combined</param>
        /// <param name="logger">Logger</param>
        public void Load(JSONNode catalogJSON, Logger logger)
        {
            Reset();

            if (catalogJSON != null)
            {
                CatalogEntry entry;
                JSONClass assets = (JSONClass)catalogJSON[CATALOG_ATT_ENTRIES];
                if (assets != null)
                {
                    string id;
                    ArrayList keys = assets.GetKeys();
                    int count = keys.Count;
                    for (int i = 0; i < count; i++)
                    {
                        id = (string)keys[i];

                        if (Entries.ContainsKey(id))
                        {
                            if (logger != null && logger.CanLog())
                            {
                                logger.LogError("Duplicate entry " + id + " found in catalog");
                            }
                        }
                        else
                        {
                            entry = new CatalogEntry();
                            entry.Load(assets[id]);
                            Entries.Add(id, entry);
                        }
                    }
                }
            }
        }

        public JSONClass ToJSON()
        {
            JSONClass data = new JSONClass();
            data.Add(CATALOG_ATT_ENTRIES, EntriesToJSON());

            return data;
        }

        public CatalogEntry GetEntry(string id)
        {
            return (Entries.ContainsKey(id)) ? Entries[id] : null;
        }

        public Dictionary<string, CatalogEntry> GetEntries()
        {
            return Entries;
        }

        public void AddEntry(string id, CatalogEntry entry)
        {
            if (!Entries.ContainsKey(id))
            {
                Entries.Add(id, entry);
            }
        }

        private JSONClass EntriesToJSON()
        {
            JSONClass data = new JSONClass();

            CatalogEntry entry;
            foreach (KeyValuePair<string, CatalogEntry> pair in Entries)
            {
                entry = pair.Value;
                if (entry != null)
                {
                    data.Add(pair.Key, entry.ToJSON());
                }
            }

            return data;
        }
    }
}
