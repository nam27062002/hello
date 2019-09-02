#if UNITY_EDITOR
using SimpleJSON;
using System.Collections.Generic;

public class EditorAssetBundlesConfig
{
    private static string ATT_ENTRIES = "entries";

    public static JSONNode TranslateLocalAbListJSONToAbConfigJSON(JSONArray abListJSON)
    {
        Dictionary<string, EditorAssetBundlesConfigEntry> entries = new Dictionary<string, EditorAssetBundlesConfigEntry>();
        EditorAssetBundlesConfigEntry entry;
        if (abListJSON != null)
        {
            int count = abListJSON.Count;
            string abName;
            for (int i = 0; i < count; ++i)
            {
                abName = abListJSON[i];
                abName = abName.Trim();

                if (!entries.ContainsKey(abName))
                {
                    entry = new EditorAssetBundlesConfigEntry();
                    entry.Id = abName;
                    entry.Location = EditorAssetBundlesConfigEntry.ELocation.Local;
                    entries.Add(abName, entry);
                }
            }
        }

        JSONClass data = new JSONClass();
        data.Add(ATT_ENTRIES, EntriesToJSON(entries));
        return data;
    }

    private static JSONArray EntriesToJSON(Dictionary<string, EditorAssetBundlesConfigEntry> entries)
    {
        JSONArray data = new JSONArray();
        EditorAssetBundlesConfigEntry entry;
        foreach (KeyValuePair<string, EditorAssetBundlesConfigEntry> kvp in entries)
        {
            entry = kvp.Value;
            if (entry != null)
            {
                data.Add(entry.ToJSON());
            }
        }

        return data;
    }

    private List<string> m_localABList;

    private Dictionary<string, EditorAssetBundlesConfigEntry> m_entries;

    public EditorAssetBundlesConfig()
    {
        m_entries = new Dictionary<string, EditorAssetBundlesConfigEntry>();
        m_localABList = new List<string>();
    }

    public void Reset()
    {        
        m_entries.Clear();     
        m_localABList.Clear();
    }

    public void Load(JSONNode data, Logger logger)
    {
        Reset();
        Join(data, logger);
    }

    private void Join(JSONNode data, Logger logger)
    {        
        if (data != null)
        {
            LoadEntries(data[ATT_ENTRIES].AsArray, logger);            
        }     
    }

    private void LoadEntries(JSONArray entries, Logger logger)
    {        
        if (entries != null)
        {                    
            EditorAssetBundlesConfigEntry entry;
            int count = entries.Count;
            for (int i = 0; i < count; ++i)
            {
                entry = new EditorAssetBundlesConfigEntry();
                entry.Load(entries[i], logger);

                m_entries.Add(entry.Id, entry);

                if (entry.Location == EditorAssetBundlesConfigEntry.ELocation.Local && !m_localABList.Contains(entry.Id))
                {
                    m_localABList.Add(entry.Id);
                }
            }
        }        
    }
    
    public bool ContainsAssetBundle(string abId)
    {
        return m_entries.ContainsKey(abId);
    }   

    public EditorAssetBundlesConfigEntry.ELocation GetAssetBundleLocation(string abId)
    {
        EditorAssetBundlesConfigEntry entry = null;
        m_entries.TryGetValue(abId, out entry);
        return (entry == null) ? EditorAssetBundlesConfigEntry.ELocation.Remote : entry.Location;
    }

    public void SetAssetBundleLocation(string abId, EditorAssetBundlesConfigEntry.ELocation location, bool createIfNotExists=false)
    {
        bool prevIsLocal = m_localABList.Contains(abId);

        EditorAssetBundlesConfigEntry entry = null;
        m_entries.TryGetValue(abId, out entry);
        if (entry == null && createIfNotExists)
        {
            entry = new EditorAssetBundlesConfigEntry();
            m_entries.Add(abId, entry);
            entry.Id = abId;
        }
        
        if (entry != null)
        {
            entry.Location = location;

            bool currentIsLocal = location == EditorAssetBundlesConfigEntry.ELocation.Local;
            if (currentIsLocal != prevIsLocal)
            {
                if (currentIsLocal)
                {
                    m_localABList.Add(abId);
                }
                else
                {
                    m_localABList.Remove(abId);
                }
            }           
        }        
    }

    public void SetAddressablesMode(AddressablesManager.EMode mode)
    {
        m_localABList.Clear();

        foreach (KeyValuePair<string, EditorAssetBundlesConfigEntry> pair in m_entries)
        {
            pair.Value.SetAddressablesMode(mode);

            if (pair.Value.Location == EditorAssetBundlesConfigEntry.ELocation.Local)
            {
                m_localABList.Add(pair.Key);
            }
        }
    }   

    public void BanAssetBundles(List<string> abIds)
    {
        if (abIds != null)
        {
            string id;
            int count = abIds.Count;
            for (int i = 0; i < count; i++)
            {
                id = abIds[i];
                if (m_localABList.Contains(id))
                {
                    m_localABList.Remove(id);
                }

                if (m_entries.ContainsKey(id))
                {
                    m_entries.Remove(id);
                }
            }
        }        
    }

    public JSONClass ToJSON()
    {
        JSONClass data = new JSONClass();
        data.Add(ATT_ENTRIES, EntriesToJSON(m_entries));
        return data;
    }    

    public List<string> GetLocalABList()
    {
        return m_localABList;
    }
}
#endif