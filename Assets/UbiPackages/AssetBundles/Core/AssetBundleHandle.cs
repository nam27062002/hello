using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class stores an asset bundle related information
/// </summary>
public class AssetBundleHandle
{
    public enum EState
    {
        None,
        ReadyToLoad,
        PendingToLoad,
        Loading,
        Loaded
    };

    private string m_id;
    public string Id
    {
        get { return m_id; }
        private set { m_id = value; }
    }

    private bool m_isRemote;

    /// List of asset bundles that this asset bundle depends on.IMPORTANT: This asset bundle id also included in this list in order to make the downloading logic easier.
    private List<string> m_dependenciesIncludingSelf;    

    private AssetBundle m_assetBundle;
    public AssetBundle AssetBundle
    {
        get { return m_assetBundle; }
        private set { m_assetBundle = value; }
    }
    

    /// <summary>
    /// Path where the asset bundle is stored in disk
    /// </summary>
    private string m_localPath;
    public string Path
    {
        get
        {
            return m_localPath;
        }

        private set
        {
            m_localPath = value;
        }
    }    
    
    public void Reset()
    {
        Id = null;
        Path = null;
        m_dependenciesIncludingSelf = null;
        AssetBundle = null;
    }

    public void Setup(string id, string path, List<string> dependencies, bool isRemote)
    {
        m_isRemote = isRemote;
        Path = path;

        m_id = id;
        m_dependenciesIncludingSelf = dependencies;
    }        

    public bool IsRemote()
    {
        return m_isRemote;
    }

    public bool IsDownloaded()
    {
        return false;
    }    
    
    public List<string> GetDependenciesIncludingSelf()
    {
        return m_dependenciesIncludingSelf;
    }    

    #region load    
    private AssetBundlesTypes.ELoadState m_loadState;
    public AssetBundlesTypes.ELoadState LoadState
    {
        get { return m_loadState; }
        set { m_loadState = value; }
    }

    public bool NeedsToRequestToLoad()
    {
        return !IsLoading() && !IsLoaded();
    }

    public bool IsLoading()
    {
        return LoadState == AssetBundlesTypes.ELoadState.Loading;
    }

    public bool IsLoaded()
    {
        return LoadState == AssetBundlesTypes.ELoadState.Loaded;
    }

    public void OnPendingToRequestToLoad()
    {
        LoadState = AssetBundlesTypes.ELoadState.PendingToRequest;            
    }

    public void OnLoadRequested()
    {
        LoadState = AssetBundlesTypes.ELoadState.Loading;
        AssetBundle = null;
    }

    public void OnLoaded(AssetBundle data)
    {
        if (IsLoading())
        {
            AssetBundle = data;
            if (AssetBundle == null)
            {
                LoadState = AssetBundlesTypes.ELoadState.Error;
            }
            else
            {
                LoadState = AssetBundlesTypes.ELoadState.Loaded;
            }
        }
    }

    public void Unload()
    {
        if (AssetBundle != null)
        {
            AssetBundle.Unload(true);
            AssetBundle = null;
        }

        if (IsLoaded() || IsLoading())
        {
            LoadState = AssetBundlesTypes.ELoadState.None;         
        }
    }
    #endregion
}
