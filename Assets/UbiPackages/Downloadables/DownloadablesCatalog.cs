using SimpleJSON;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class is responsible for storing all downloadable entries.
/// </summary>
public class DownloadablesCatalog
{
    private Dictionary<string, DownloadablesCatalogEntry> Entries { get; set; }

    private static string CATALOG_ATT_ENTRIES = "assets";

    public DownloadablesCatalog()
    {
        Entries = new Dictionary<string, DownloadablesCatalogEntry>();
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
            DownloadablesCatalogEntry entry;
            JSONClass assets = (JSONClass)catalogJSON[CATALOG_ATT_ENTRIES];
            if (assets != null)
            {
                string id;
                ArrayList keys = assets.GetKeys();
                int count = keys.Count;
                for (int i = 0; i < count; i++)
                {
                    id = (string) keys[i];

                    if (Entries.ContainsKey(id))
                    {
                        if (logger != null && logger.CanLog())
                        {
                            logger.LogError("Duplicate entry " + id + " found in catalog");
                        }
                    }
                    else
                    {
                        entry = new DownloadablesCatalogEntry();
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

    public Dictionary<string, DownloadablesCatalogEntry> GetEntries()
    {
        return Entries;
    }    

    private JSONClass EntriesToJSON()
    {
        JSONClass data = new JSONClass();

        DownloadablesCatalogEntry entry;
        foreach (KeyValuePair<string, DownloadablesCatalogEntry> pair in Entries)
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
