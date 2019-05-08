using SimpleJSON;
using System.Collections.Generic;

/// <summary>
/// This class is responsible for storing a catalog of <c>AddressablesEntry</c> objects
/// </summary>
public class AddressablesCatalog
{
    private static string CATALOG_ATT_ENTRIES = "entries";
    public static string CATALOG_ATT_LOCAL_AB_LIST = "localAssetBundles";
    public static string CATALOG_ATT_GROUPS = "groups";

    /// <summary>
    /// Key: Addressable Id, Value: Addressable entry. This dictionary only contains entries with no variants
    /// </summary>
    private Dictionary<string, AddressablesCatalogEntry> m_entriesNoVariants;

    /// <summary>
    /// Key: Addressable Id, Value: Dictionary where Key:Variant Value: Addressable entry. This dictionary only contains entries with variants
    /// </summary>
    private Dictionary<string, Dictionary<string, AddressablesCatalogEntry>> m_entriesWithVariants;

#if UNITY_EDITOR
    private List<string> m_localABList;

    private Dictionary<string, AssetBundlesGroup> m_groups;

    private bool m_editorMode;

    public AddressablesCatalog(bool editorMode=false) : this()
    {
        m_editorMode = editorMode;
    }
#endif
    
    public AddressablesCatalog()    
    {
        m_entriesNoVariants = new Dictionary<string, AddressablesCatalogEntry>();
        m_entriesWithVariants = new Dictionary<string, Dictionary<string, AddressablesCatalogEntry>>();

#if UNITY_EDITOR
        m_groups = new Dictionary<string, AssetBundlesGroup>();
        m_localABList = new List<string>();
#endif
    }

    public void Reset()
    {
        m_entriesNoVariants.Clear();
        m_entriesWithVariants.Clear();

#if UNITY_EDITOR
        m_groups.Clear();
        m_localABList.Clear();
#endif
    }

    public void Load(JSONNode catalogJSON, Logger logger)
    {
        Reset();
        Join(catalogJSON, logger, true);        
    }

    public bool Join(JSONNode catalogJSON, Logger logger, bool strictMode=false)
    {
        bool success = true;
        if (catalogJSON != null)
        {
            success = LoadEntries(catalogJSON[CATALOG_ATT_ENTRIES].AsArray, logger);

#if UNITY_EDITOR
            if (m_editorMode)
            {
                LoadGroups(catalogJSON[CATALOG_ATT_GROUPS].AsArray, logger, strictMode);
                LoadLocalABList(catalogJSON[CATALOG_ATT_LOCAL_AB_LIST].AsArray);

                if (AddressablesManager.EffectiveMode == AddressablesManager.EMode.AllInLocalAssetBundles)
                {
                    SetAllAssetBundlesAsLocal();
                }
            }
#endif
        }

        return success;
    }   

    public JSONClass ToJSON()
    {        
        JSONClass data = new JSONClass();        
        data.Add(CATALOG_ATT_ENTRIES, EntriesToJSON());                

#if UNITY_EDITOR
        if (m_editorMode)
        {            
            data.Add(CATALOG_ATT_LOCAL_AB_LIST, LocalABListToJSON());
            data.Add(CATALOG_ATT_GROUPS, GroupsToJSON());
        }
#endif

        return data;
    }   

    private bool LoadEntries(JSONArray entries, Logger logger)
    {
        bool success = true;
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
                    if (string.IsNullOrEmpty(entry.Variant))
                    {
                        success = EntriesNoVariants_AddEntry(entry, logger) && success;
                    }                    
                    else
                    {
                        success = EntriesWithVariants_AddEntry(entry, logger) && success;
                    }
                }              
            }
        }

        return success;
    }    

    private bool EntriesNoVariants_AddEntry(AddressablesCatalogEntry entry, Logger logger)
    {
        bool success = !m_entriesNoVariants.ContainsKey(entry.Id);
        if (success)
        {
            m_entriesNoVariants.Add(entry.Id, entry);
        }
        else
        {
            if (logger != null && logger.CanLog())
            {
                logger.LogError("Duplicate entry " + entry.Id + " found in catalog");
            }
        }

        return success;
    }

    private bool EntriesWithVariants_AddEntry(AddressablesCatalogEntry entry, Logger logger)
    {        
        if (!m_entriesWithVariants.ContainsKey(entry.Id))
        {
            m_entriesWithVariants.Add(entry.Id, new Dictionary<string, AddressablesCatalogEntry>());
        }

        Dictionary<string, AddressablesCatalogEntry> entries = m_entriesWithVariants[entry.Id];

        bool success = !entries.ContainsKey(entry.Variant);
        if (success)
        {
            entries.Add(entry.Variant, entry);
        }
        else
        {            
            if (logger != null && logger.CanLog())
            {
                logger.LogError("Duplicate entry " + entry.Id + ", " + entry.Variant + " found in catalog");
            }
        }

        return success;  
    }

    private JSONArray EntriesToJSON()
    {
        JSONArray data = new JSONArray();
        AddEntriesToJSON(data, m_entriesNoVariants);        

        foreach (KeyValuePair<string, Dictionary<string, AddressablesCatalogEntry>> pair in m_entriesWithVariants)
        {
            AddEntriesToJSON(data, pair.Value);
        }

        return data;
    }    

    private void AddEntriesToJSON(JSONArray data, Dictionary<string, AddressablesCatalogEntry> entries)
    {        
        AddressablesCatalogEntry entry;
        foreach (KeyValuePair<string, AddressablesCatalogEntry> kvp in entries)
        {
            entry = kvp.Value;
            if (entry != null && entry.IsValid())
            {
                data.Add(entry.ToJSON());
            }
        }
    }

    public AddressablesCatalogEntry GetEntry(string id, string variant = null)
    {
        AddressablesCatalogEntry returnValue = null;
        if (string.IsNullOrEmpty(variant))
        {
            m_entriesNoVariants.TryGetValue(id, out returnValue);            
        }        
        else
        {
            Dictionary<string, AddressablesCatalogEntry> entries = null;
            if (m_entriesWithVariants.TryGetValue(id, out entries))
            {
                entries.TryGetValue(variant, out returnValue);
            }
        }

        return returnValue;
    }
    
    public List<AddressablesCatalogEntry> GetEntries()
    {
        List<AddressablesCatalogEntry> returnValue = new List<AddressablesCatalogEntry>();
        AddEntriesToList(returnValue, m_entriesNoVariants);

        foreach (KeyValuePair<string, Dictionary<string, AddressablesCatalogEntry>> pair in m_entriesWithVariants)
        {
            AddEntriesToList(returnValue, pair.Value);
        }

        return returnValue;
    }

    private void AddEntriesToList(List<AddressablesCatalogEntry> list, Dictionary<string, AddressablesCatalogEntry> entries)
    {
        foreach (KeyValuePair<string, AddressablesCatalogEntry> pair in entries)
        {
            list.Add(pair.Value);
        }
    }

    public void ClearEntries()
    {
        m_entriesNoVariants.Clear();
        m_entriesWithVariants.Clear();
    }

    public void AddEntry(AddressablesCatalogEntry entry)
    {
        if (entry != null)
        {            
            if (string.IsNullOrEmpty(entry.Variant))
            {
                m_entriesNoVariants.Add(entry.Id, entry);
            }
            else
            {                
                if (!m_entriesWithVariants.ContainsKey(entry.Id))
                {
                    m_entriesWithVariants.Add(entry.Id, new Dictionary<string, AddressablesCatalogEntry>());
                }

                m_entriesWithVariants[entry.Id].Add(entry.Variant, entry);
            }
        }
    }

    public bool HasEntryVariants(string id)
    {
        return m_entriesWithVariants.ContainsKey(id);
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

    private void SetAllAssetBundlesAsLocal()
    {
        string abName;
        foreach (KeyValuePair<string, AddressablesCatalogEntry> pair in m_entriesNoVariants)
        {
            abName = pair.Value.AssetBundleName;
            if (!string.IsNullOrEmpty(abName) && !m_localABList.Contains(abName))
            {
                m_localABList.Add(abName);
            }
        }

        foreach (KeyValuePair<string, Dictionary<string, AddressablesCatalogEntry>> p in m_entriesWithVariants)
        {
            foreach (KeyValuePair<string, AddressablesCatalogEntry> pair in p.Value)
            {
                abName = pair.Value.AssetBundleName;
                if (!string.IsNullOrEmpty(abName) && !m_localABList.Contains(abName))
                {
                    m_localABList.Add(abName);
                }
            }
        }
    }

    public List<string> GetUsedABList()
    {
        List<string> returnValue = new List<string>();

        List<AddressablesCatalogEntry> entries = GetEntries();

        AddressablesCatalogEntry entry;
        int count = entries.Count;
        for (int i = 0; i < count; i++)
        {
            entry = entries[i];
            if (entry != null && entry.IsValid())
            {
                if (entry.LocationType == AddressablesTypes.ELocationType.AssetBundles &&
                    !returnValue.Contains(entry.AssetBundleName))
                {
                    returnValue.Add(entry.AssetBundleName);
                }                
            }
        }

        // We consider that the asset bundles defined in groups and in localAB are necessary
        if (m_groups != null)
        {
            foreach (KeyValuePair<string, AssetBundlesGroup> pair in m_groups)
            {
                UbiListUtils.AddRange(returnValue, pair.Value.AssetBundleIds, false, true);
            }
        }

        UbiListUtils.AddRange(returnValue, m_localABList, false, true);

        return returnValue;
    }

    private void LoadGroups(JSONArray groups, Logger logger, bool strictMode)
    {        
        if (groups != null)
        {
            AssetBundlesGroup group;
            int count = groups.Count;
            for (int i = 0; i < count; ++i)
            {
                group = new AssetBundlesGroup();
                group.Load(groups[i]);              
                if (m_groups.ContainsKey(group.Id))
                {
                    m_groups[group.Id].Join(groups[i]);
                    
                    if (strictMode && logger != null && logger.CanLog())
                    {
                        logger.LogError("Duplicate group " + group.Id + " found in catalog");
                    }                    
                }
                else
                {
                    m_groups.Add(group.Id, group);
                }                
            }
        }
    }

    private JSONArray GroupsToJSON()
    {
        JSONArray data = new JSONArray();        
        foreach (KeyValuePair<string, AssetBundlesGroup> pair in m_groups)
        {            
            data.Add(pair.Value.ToJSON());            
        }

        return data;
    }
    
    public Dictionary<string, AssetBundlesGroup> GetGroups()
    {
        return m_groups;
    }

    public AssetBundlesGroup GetGroup(string groupId)
    {
        return (!string.IsNullOrEmpty(groupId) && m_groups.ContainsKey(groupId)) ? m_groups[groupId] : null;
    }    

    /// <summary>
    /// Returns the entries grouped by asset bundle
    /// </summary>
    /// <returns>Returns a <c>Dictionary</c> which key is asset bundle name and the value is the list of <c>AddressablesCatalogEntry</c> objects that refer to that asset bundle.</returns>
    private Dictionary<string, List<AddressablesCatalogEntry>> GetEntriesGroupedByAssetBundle()
    {
        Dictionary<string, List<AddressablesCatalogEntry>> returnValue = new Dictionary<string, List<AddressablesCatalogEntry>>();

        List<AddressablesCatalogEntry> entries = GetEntries();

        AddressablesCatalogEntry entry;
        int count = entries.Count;
        for (int i = 0; i < count; i++)
        {
            entry = entries[i];            
            if (entry.LocationType == AddressablesTypes.ELocationType.AssetBundles && !string.IsNullOrEmpty(entry.AssetBundleName))
            {
                if (!returnValue.ContainsKey(entry.AssetBundleName))
                {
                    returnValue.Add(entry.AssetBundleName, new List<AddressablesCatalogEntry>());
                }

                returnValue[entry.AssetBundleName].Add(entry);
            }
        }

        return returnValue;
    }

    private static bool OPTIMIZE_ASSET_NAMES_ENABLED = true;

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

                    if (OPTIMIZE_ASSET_NAMES_ENABLED)
                    {
                        if (assetNames.ContainsKey(fileName))
                        {
                            assetNames[fileName] = null;
                        }
                        else
                        {
                            assetNames.Add(fileName, pair.Value[i]);
                        }
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
