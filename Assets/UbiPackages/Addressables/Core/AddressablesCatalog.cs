using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for storing a catalog of <c>AddressablesEntry</c> objects
/// </summary>
public class AddressablesCatalog
{
    private static string CATALOG_ATT_ENTRIES = "entries";
    public static string CATALOG_ATT_LOCAL_AB_LIST = "localAssetBundles";

    private Dictionary<string, AddressablesCatalogEntry> m_entries;
    private List<string> m_localABList;    

    public AddressablesCatalog()
    {
        m_entries = new Dictionary<string, AddressablesCatalogEntry>();
        m_localABList = new List<string>();
    }

    public void Reset()
    {
        m_entries.Clear();
        m_localABList.Clear();
    }

    public void Load(JSONNode catalogJSON, Logger logger)
    {
        Reset();

        if (catalogJSON != null)
        {
            LoadEntries(catalogJSON[CATALOG_ATT_ENTRIES].AsArray, logger);
            LoadLocalABList(catalogJSON[CATALOG_ATT_LOCAL_AB_LIST]);
        }
    }

    public JSONClass ToJSON()
    {
        // Create new object
        JSONClass data = new JSONClass();        
        data.Add(CATALOG_ATT_ENTRIES, EntriesToJSON());        
        data.Add(CATALOG_ATT_LOCAL_AB_LIST, UbiListUtils.GetListAsString(m_localABList));

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
        AddressablesCatalogEntry entry;
        foreach (KeyValuePair<string, AddressablesCatalogEntry> kvp in m_entries)
        {
            entry = kvp.Value;
            if (entry != null && entry.IsValid())
            {
                data.Add(entry.ToJSON());
            }
        }        

        return data;
    }    

    public AddressablesCatalogEntry GetEntry(string id)
    {
        return (m_entries.ContainsKey(id)) ? m_entries[id] : null;
    }

    public bool TryGetEntry(string id, out AddressablesCatalogEntry entry)
    {
        return m_entries.TryGetValue(id, out entry);        
    }

    public Dictionary<string, AddressablesCatalogEntry> GetEntries()
    {
        return m_entries;
    }

    public void LoadLocalABList(string list)
    {
        if (list != null)
        {
            string[] abList = list.Split(',');
            int count = abList.Length;
            string abName;
            for (int i = 0; i < count; i++)
            {
                abName = abList[i].Trim();

                // Makes sure that there's no duplicates
                if (!string.IsNullOrEmpty(abName) && !m_localABList.Contains(abName))
                {
                    m_localABList.Add(abName);
                }
            }
        }
    }

    public List<string> GetLocalABList()
    {
        return m_localABList;
    }

    public void SetLocalABList(List<string> value)
    {
        m_localABList = value;
    }

    public List<string> GetUsedABList()
    {
        List<string> returnValue = new List<string>();

        AddressablesCatalogEntry entry;
        foreach (KeyValuePair<string, AddressablesCatalogEntry> kvp in m_entries)
        {
            entry = kvp.Value;
            if (entry != null && entry.IsValid())
            {
                if (entry.LocationType == AddressablesTypes.ELocationType.AssetBundles &&
                    !returnValue.Contains(entry.AssetBundleName))
                {
                    returnValue.Add(entry.AssetBundleName);
                }                
            }
        }

        return returnValue;
    }
}
