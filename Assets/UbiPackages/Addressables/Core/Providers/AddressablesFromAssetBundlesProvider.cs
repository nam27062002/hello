﻿using SimpleJSON;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AddressablesFromAssetBundlesProvider : AddressablesProvider
{
    public void Initialize(string localAssetBundlesPath, Downloadables.Config downloadablesConfig, JSONNode downloadablesCatalog, bool useMockDrivers, Downloadables.Tracker tracker, Logger logger)
    {        
        AssetBundlesManager.Instance.Initialize(localAssetBundlesPath, downloadablesConfig, downloadablesCatalog, useMockDrivers, tracker, logger);
    }

    public void Reset()
    {
        AssetBundlesManager.Instance.Reset();
    }

    public override bool IsResourceAvailable(AddressablesCatalogEntry entry, bool track = false)
    {
        // A resource is available if its dependencies and itself are available
        List<string> dependencies = GetDependencyIds(entry);
        return AssetBundlesManager.Instance.IsAssetBundleListAvailable(dependencies, track);
    }

    public override List<string> GetDependencyIds(AddressablesCatalogEntry entry)
    {
        return AssetBundlesManager.Instance.GetDependenciesIncludingSelf(entry.AssetBundleName);       
    }

    public override AddressablesOp DownloadDependenciesAsync(AddressablesCatalogEntry entry)
    {
        AssetBundlesOpRequest request = AssetBundlesManager.Instance.DownloadAssetBundleAndDependencies(entry.AssetBundleName, null, true);
        return ProcessAssetBundlesOpRequest(request);
    }

    public override AddressablesOp LoadDependenciesAsync(AddressablesCatalogEntry entry)
    {
        AssetBundlesOpRequest request = AssetBundlesManager.Instance.LoadAssetBundleAndDependencies(entry.AssetBundleName, null, true);
        return ProcessAssetBundlesOpRequest(request);
    }

    public bool IsDependenciesListAvailable(List<string> dependencyIds)
    {
        return AssetBundlesManager.Instance.IsAssetBundleListAvailable(dependencyIds);
    }

    public List<string> GetDependenciesIncludingSelfList(List<string> dependencyIds)
    {
        return AssetBundlesManager.Instance.GetDependenciesIncludingSelfList(dependencyIds);
    }

    public AddressablesOp LoadDependencyIdAsync(string dependencyId)
    {
        AssetBundlesOpRequest request = AssetBundlesManager.Instance.LoadAssetBundleAndDependencies(dependencyId, null, true);
        return ProcessAssetBundlesOpRequest(request);        
    }

    public AddressablesOp LoadDependencyIdsListAsync(List<string> dependencyIds)
    {
        AssetBundlesOpRequest request = AssetBundlesManager.Instance.LoadAssetBundleList(dependencyIds, null, true);
        return ProcessAssetBundlesOpRequest(request);
    }

    public override void UnloadDependencies(AddressablesCatalogEntry entry)
    {
        List<string> dependenciesList = AssetBundlesManager.Instance.GetDependenciesIncludingSelf(entry.AssetBundleName);
        AssetBundlesManager.Instance.UnloadAssetBundleList(dependenciesList);        
    }

    public void UnloadDependencyId(string dependencyId, bool unloadItsDependenciesToo)
    {
        if (unloadItsDependenciesToo)
        {
            List<string> dependenciesList = AssetBundlesManager.Instance.GetDependenciesIncludingSelf(dependencyId);
            AssetBundlesManager.Instance.UnloadAssetBundleList(dependenciesList);
        }
        else
        {
            AssetBundlesManager.Instance.UnloadAssetBundle(dependencyId, null);
        }
    }

    public void UnloadDependencyIdsList(List<string> dependencyIds)
    {
        AssetBundlesManager.Instance.UnloadAssetBundleList(dependencyIds);
    }

    public void UnloadAllDependencies()
    {
        AssetBundlesManager.Instance.UnloadAllAssetBundles();
    }

    public override bool LoadScene(AddressablesCatalogEntry entry, LoadSceneMode mode)
    {
        AssetBundlesOp.EResult result = AssetBundlesManager.Instance.LoadScene(entry.AssetBundleName, entry.AssetName, mode);
        return result == AssetBundlesOp.EResult.Success;
    }

    public override AddressablesOp LoadSceneAsync(AddressablesCatalogEntry entry, LoadSceneMode mode)
    {
        AssetBundlesOpRequest request = AssetBundlesManager.Instance.LoadSceneAsync(entry.AssetBundleName, entry.AssetName, mode, null, true);
        return ProcessAssetBundlesOpRequest(request);
    }

    public override AddressablesOp UnloadSceneAsync(AddressablesCatalogEntry entry)
    {
        AssetBundlesOpRequest request = AssetBundlesManager.Instance.UnloadSceneAsync(entry.AssetBundleName, entry.AssetName, null, true);
        return ProcessAssetBundlesOpRequest(request);        
    }    

    public override object LoadAsset(AddressablesCatalogEntry entry)
    {
        if (CanLog())
        {
            Log("LoadAsset by FromAssetBundles provider");
        }

        return AssetBundlesManager.Instance.LoadAsset(entry.AssetBundleName, entry.AssetName);        
    }

    public override AddressablesOp LoadAssetAsync(AddressablesCatalogEntry entry)
    {
        if (CanLog())
        {
            Log("LoadAsset by FromAssetBundles provider");
        }
        
        AssetBundlesOpRequest request = AssetBundlesManager.Instance.LoadAssetAsync(entry.AssetBundleName, entry.AssetName, null, true);
        return ProcessAssetBundlesOpRequest(request);
    }

    private AddressablesOp ProcessAssetBundlesOpRequest(AssetBundlesOpRequest request)
    {
        AddressablesFromAssetBundlesOp op = new AddressablesFromAssetBundlesOp();
        op.Setup(request);
        return op;
    }

    public bool IsAutomaticDownloaderEnabled
    {
        get
        {
            return AssetBundlesManager.Instance.IsAutomaticDownloaderEnabled;
        }

        set
        {
            AssetBundlesManager.Instance.IsAutomaticDownloaderEnabled = value;
        }
    }

    public bool IsDownloaderEnabled
    {
        get
        {
            return AssetBundlesManager.Instance.IsDownloaderEnabled;
        }

        set
        {
            AssetBundlesManager.Instance.IsDownloaderEnabled = value;
        }
    }

    public AssetBundlesGroup GetAssetBundlesGroup(string groupId)
    {
        return AssetBundlesManager.Instance.GetAssetBundlesGroup(groupId);
    }
    
    public Downloadables.Handle CreateDownloadablesHandle(string groupId)
    {
        return AssetBundlesManager.Instance.CreateDownloadablesHandle(groupId);
    }

    public Downloadables.Handle CreateDownloadablesHandle(HashSet<string> groupIds)
    {
        return AssetBundlesManager.Instance.CreateDownloadablesHandle(groupIds);
    }

    public void SetDownloadablesGroupPriority(string groupId, int priority)
    {
        AssetBundlesManager.Instance.SetDownloadablesGroupPriority(groupId, priority);
    }

    public void Update()
    {
        AssetBundlesManager.Instance.Update();
    }
}
