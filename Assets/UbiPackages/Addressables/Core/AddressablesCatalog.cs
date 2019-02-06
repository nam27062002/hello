using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for storing a catalog of <c>AddressablesEntry</c> objects
/// </summary>
public class AddressablesCatalog
{
    private static string CATALOG_ATT_ENTRIES = "entries";

    private Dictionary<string, AddressablesCatalogEntry> m_entries;

    public AddressablesCatalog()
    {
        m_entries = new Dictionary<string, AddressablesCatalogEntry>();
    }

    public void Reset()
    {
        if (m_entries != null)
        {
            m_entries.Clear();
        }
    }

    public void Load(JSONNode catalogJSON, Logger logger)
    {
        Reset();

        if (catalogJSON != null)
        {
            LoadEntries(catalogJSON[CATALOG_ATT_ENTRIES].AsArray, logger);
        }
    }

    public JSONClass ToJSON()
    {
        // Create new object
        JSONClass data = new JSONClass();        
        data.Add(CATALOG_ATT_ENTRIES, EntriesToJSON());

        return data;
    }

    private void LoadEntries(JSONArray entries, Logger logger)
    {
        if (entries != null)
        {
            AddressablesCatalogEntry.sm_logger = logger;

            AddressablesCatalogEntry entry;
            int count = entries.Count;
            for (int i = 0; i < count; ++i)
            {
                entry = new AddressablesCatalogEntry();
                entry.Load(entries[i]);
                if (entry.IsValid())
                {
                    if (m_entries.ContainsKey(entry.Id))
                    {
                        if (logger != null && logger.CanLog())
                        {
                            logger.LogError("Duplicate entry " + entry.Id + " found in catalog");
                        }
                    }
                    else
                    {
                        m_entries.Add(entry.Id, entry);
                    }
                }              
            }
        }
    }

    private JSONArray EntriesToJSON()
    {
        JSONArray data = new JSONArray();
        if (m_entries != null)
        {
            AddressablesCatalogEntry entry;
            foreach (KeyValuePair<string, AddressablesCatalogEntry> kvp in m_entries)
            {
                entry = kvp.Value;
                if (entry != null && entry.IsValid())
                {
                    data.Add(entry.ToJSON());
                }
            }
        }

        return data;
    }

    public AddressablesCatalogEntry GetEntry(string id)
    {
        return (m_entries != null && m_entries.ContainsKey(id)) ? m_entries[id] : null;
    }

    public bool TryGetEntry(string id, out AddressablesCatalogEntry entry)
    {
        if (m_entries != null)
        {
            return m_entries.TryGetValue(id, out entry);
        }
        else
        {
            entry = null;
            return false;
        }
    }

    public Dictionary<string, AddressablesCatalogEntry> GetEntries()
    {
        return m_entries;
    }
}
