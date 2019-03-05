using SimpleJSON;
using System.Collections.Generic;

/// <summary>
/// This class is responsible for storing a catalog of <c>AddressablesEntry</c> objects
/// </summary>
public class AddressablesCatalog
{
    private static string CATALOG_ATT_ENTRIES = "entries";
    public static string CATALOG_ATT_LOCAL_AB_LIST = "localAssetBundles";
    public static string CATALOG_ATT_AREAS = "areas";

    private Dictionary<string, AddressablesCatalogEntry> m_entries;

#if UNITY_EDITOR
    private List<string> m_localABList;
#endif

    private Dictionary<string, AddressablesCatalogArea> m_areas;

#if UNITY_EDITOR
    private bool m_editorMode;

    public AddressablesCatalog(bool editorMode=false) : this()
    {
        m_editorMode = editorMode;
    }
#endif
    
    public AddressablesCatalog()    
    {
        m_entries = new Dictionary<string, AddressablesCatalogEntry>();
        m_areas = new Dictionary<string, AddressablesCatalogArea>();

#if UNITY_EDITOR
        m_localABList = new List<string>();
#endif
    }

    public void Reset()
    {
        m_entries.Clear();      
        m_areas.Clear();

#if UNITY_EDITOR
        m_localABList.Clear();
#endif
    }

    public void Load(JSONNode catalogJSON, Logger logger)
    {
        Reset();

        if (catalogJSON != null)
        {
            LoadEntries(catalogJSON[CATALOG_ATT_ENTRIES].AsArray, logger);            
            LoadAreas(catalogJSON[CATALOG_ATT_AREAS].AsArray, logger);

#if UNITY_EDITOR
            if (m_editorMode)
            {
                LoadLocalABList(catalogJSON[CATALOG_ATT_LOCAL_AB_LIST].AsArray);
            }
#endif
        }
    }

    public JSONClass ToJSON()
    {        
        JSONClass data = new JSONClass();        
        data.Add(CATALOG_ATT_ENTRIES, EntriesToJSON());                
        data.Add(CATALOG_ATT_AREAS, AreasToJSON());

#if UNITY_EDITOR
        if (m_editorMode)
        {
            data.Add(CATALOG_ATT_LOCAL_AB_LIST, LocalABListToJSON());
        }
#endif

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
#if UNITY_EDITOR
                entry = new AddressablesCatalogEntry(m_editorMode);
#else
                entry = new AddressablesCatalogEntry();
#endif

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

#if UNITY_EDITOR
    public void LoadLocalABList(JSONArray entries)
    {
        if (entries != null)
        {            
            int count = entries.Count;
            string abName;
            for (int i = 0; i < count; ++i)
            {
                abName = entries[i];

                abName = abName.Trim();
                // Makes sure that there's no duplicates
                if (!string.IsNullOrEmpty(abName) && !m_localABList.Contains(abName))
                {
                    m_localABList.Add(abName);
                }
            }          
        }
    }

    private JSONArray LocalABListToJSON()
    {
        JSONArray data = new JSONArray();
        int count = m_localABList.Count;
        for (int i = 0; i < count; i++)
        {            
            if (!string.IsNullOrEmpty(m_localABList[i]))
            {
                data.Add(m_localABList[i]);
            }
        }

        return data;
    }

    public List<string> GetLocalABList()
    {
        return m_localABList;
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
#endif

    private void LoadAreas(JSONArray areas, Logger logger)
    {
        if (areas != null)
        {            
            AddressablesCatalogArea area;
            int count = areas.Count;
            for (int i = 0; i < count; ++i)
            {
                area = new AddressablesCatalogArea();
                area.Load(areas[i]);              
                if (m_areas.ContainsKey(area.Id))
                {
                    if (logger != null && logger.CanLog())
                    {
                        logger.LogError("Duplicate area " + area.Id + " found in catalog");
                    }
                }
                else
                {
                    m_areas.Add(area.Id, area);
                }                
            }
        }
    }

    private JSONArray AreasToJSON()
    {
        JSONArray data = new JSONArray();        
        foreach (KeyValuePair<string, AddressablesCatalogArea> pair in m_areas)
        {            
            data.Add(pair.Value.ToJSON());            
        }

        return data;
    }

    public Dictionary<string, AddressablesCatalogArea> GetAreas()
    {
        return m_areas;
    }

    public AddressablesCatalogArea GetArea(string areaId)
    {
        return (!string.IsNullOrEmpty(areaId) && m_areas.ContainsKey(areaId)) ? m_areas[areaId] : null;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Returns the entries grouped by asset bundle
    /// </summary>
    /// <returns>Returns a <c>Dictionary</c> which key is asset bundle name and the value is the list of <c>AddressablesCatalogEntry</c> objects that refer to that asset bundle.</returns>
    private Dictionary<string, List<AddressablesCatalogEntry>> GetEntriesGroupedByAssetBundle()
    {
        Dictionary<string, List<AddressablesCatalogEntry>> returnValue = new Dictionary<string, List<AddressablesCatalogEntry>>();
        
        foreach (KeyValuePair<string, AddressablesCatalogEntry> pair in m_entries)
        {
            if (pair.Value.LocationType == AddressablesTypes.ELocationType.AssetBundles && !string.IsNullOrEmpty(pair.Value.AssetBundleName))
            {
                if (!returnValue.ContainsKey(pair.Value.AssetBundleName))
                {
                    returnValue.Add(pair.Value.AssetBundleName, new List<AddressablesCatalogEntry>());
                }

                returnValue[pair.Value.AssetBundleName].Add(pair.Value);
            }
        }

        return returnValue;
    }        

    /// <summary>
    /// Optimizes the asset name field of all entries in this catalog
    /// </summary>
    public void OptimizeEntriesAssetNames()
    {        
        if (m_editorMode)
        {            
            Dictionary<string, AddressablesCatalogEntry> assetNames = new Dictionary<string, AddressablesCatalogEntry>();

            // Gets all entries grouped by asset bundle, then loops through them all and stores the ones which filename (which is the smallest possible name) is unique so 
            // their asset names can be changed to this smallest name. We need to keep the full path as an asset name for the ones that share filename
            string fileName;
            string entryPath;
            int count;
            Dictionary<string, List<AddressablesCatalogEntry>> entriesGroupedByAssetBundle = GetEntriesGroupedByAssetBundle();
            foreach (KeyValuePair<string, List<AddressablesCatalogEntry>> pair in entriesGroupedByAssetBundle)
            {                
                assetNames.Clear();
                count = pair.Value.Count;
                for (int i = 0; i < count; i++)
                {   
                    entryPath = UnityEditor.AssetDatabase.GUIDToAssetPath(pair.Value[i].GUID);
                    fileName = System.IO.Path.GetFileNameWithoutExtension(entryPath);
                    pair.Value[i].AssetName = entryPath;

                    if (assetNames.ContainsKey(fileName))
                    {
                        assetNames[fileName] = null;                        
                    }
                    else
                    {
                        assetNames.Add(fileName, pair.Value[i]);                        
                    }
                }

                foreach (KeyValuePair<string, AddressablesCatalogEntry> p in assetNames)
                {
                    if (p.Value != null)
                    {
                        p.Value.AssetName = p.Key;
                    }
                }
            }           
        }        
    }   
#endif
}
