﻿using SimpleJSON;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class is responsible for storing the list of asset bundles available and its dependencies
/// </summary>
public class AssetBundlesCatalog
{
    public static string CATALOG_ATT_LOCAL_LIST = "local";
    public static string CATALOG_ATT_DEPENDENCIES = "dependencies";

    private List<string> m_ids;

    /// <summary>
    /// List of the asset bundles explicitly declared as local in the JSON loaded. They are stored in a different
    /// list because these will be the only ones included in the output json when ToJSON() is called.
    /// </summary>
    private List<string> m_explicitLocalList;

    /// <summary>
    /// List of all local asset bundles. Some of them are in m_explicitLocalList too, the rest are local because
    /// there are explit local asset bundles that depend on them.
    /// </summary>
    private List<string> m_localList;

    private Dictionary<string, List<string>> m_dependencies;

    public AssetBundlesCatalog()
    {
        m_ids = new List<string>();
        m_localList = new List<string>();
        m_explicitLocalList = new List<string>();
        m_dependencies = new Dictionary<string, List<string>>();
    }

    public void Reset()
    {
        m_ids.Clear();
        m_localList.Clear();
        m_explicitLocalList.Clear();
        m_dependencies.Clear();
    }

    public void Load(JSONNode json, Logger logger)
    {
        Reset();

        if (json != null)
        {
            UbiListUtils.JSONArrayToList(json[CATALOG_ATT_LOCAL_LIST].AsArray, m_explicitLocalList, true);            

            LoadDependencies(json[CATALOG_ATT_DEPENDENCIES], logger);            

            // Verifies that all asset bundles in local list are defined in dependencies and searches for
            // implicit local asset bundles, which are the dependencies of any explicit local asset bundles
            List<string> dependencies;
            int count = m_explicitLocalList.Count;
            for (int i = count - 1; i > -1; i--)
            {                
                if (m_dependencies.ContainsKey(m_explicitLocalList[i]))
                {
                    dependencies = GetAllDependencies(m_explicitLocalList[i]);
                    UbiListUtils.AddRange(m_localList, dependencies, false, true);
                }
                else
                {
                    // Any local asset bundle id that is not defined in dependencies is not considered a valid one
                    if (logger == null && logger.CanLog())
                    {
                        logger.LogError("Asset bundle id <" + m_explicitLocalList[i] + "> is defined as local but it's not present in dependencies");
                    }

                    m_explicitLocalList.RemoveAt(i);
                }
            }

            UbiListUtils.AddRange(m_localList, m_explicitLocalList, false, true);
        }
    }

    public JSONClass ToJSON()
    {
        JSONClass data = new JSONClass();

        // Only the explicit local asset bundles are included in the json because the rest can be
        // found out
        data.Add(CATALOG_ATT_LOCAL_LIST, UbiListUtils.ListToJSONArray(m_explicitLocalList));
        data.Add(CATALOG_ATT_DEPENDENCIES, DependenciesToJSON());        

        return data;
    }

    private void LoadDependencies(JSONNode dependencies, Logger logger)
    {
        if (dependencies != null)
        {
            string id;
            ArrayList keys = ((JSONClass)dependencies).GetKeys();
            int count = keys.Count;
            for (int i = 0; i < count; i++)
            {
                id = (string)keys[i];

                if (m_dependencies.ContainsKey(id))
                {
                    if (logger != null && logger.CanLog())
                    {
                        logger.LogError("Duplicate dependency " + id + " found in asset bundles catalog");
                    }
                }
                else
                {
                    m_ids.Add(id);
                    m_dependencies.Add(id, UbiListUtils.JSONArrayToList(dependencies[id].AsArray, true));
                }
            }
        }
    }

    private JSONNode DependenciesToJSON()
    {
        JSONClass data = new JSONClass();
        foreach (KeyValuePair<string, List<string>> pair in m_dependencies)
        {
            data.Add(pair.Key, UbiListUtils.ListToJSONArray(pair.Value));
        }

        return data;
    }

    public bool IsAssetBundleLocal(string id)
    {
        return m_localList.Contains(id);
    }

    public List<string> GetAllDependencies(string id)
    {
        List<string> returnValue = new List<string>();
        if (m_dependencies.ContainsKey(id))
        {
            int count = m_dependencies[id].Count;
            for (int i = 0; i < count; i++)
            {
                returnValue.Add(m_dependencies[id][i]);
                UbiListUtils.AddRange(returnValue, GetAllDependencies(m_dependencies[id][i]), false, true);                
            }
        }

        return returnValue;
    }

    public List<string> GetAllAssetBundleIds()
    {
        return m_ids;
    }
   
    public void AddDirectDependencies(string id, List<string> dependencies)
    {
        if (m_dependencies.ContainsKey(id))
        {
            m_dependencies[id] = dependencies;
        }
        else
        {
            m_dependencies.Add(id, dependencies);
        }
    }    
}
