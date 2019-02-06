using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AddressablesFromAssetBundlesProvider : AddressablesProvider
{
    public void Initialize(string assetBundlesManifestPath, Logger logger)
    {
        List<string> localAssetBundleIds = new List<string> { "01/scene_cubes", "01/asset_cubes" };
        //localAssetBundleIds = null;
        AssetBundlesManager.Instance.Initialize(localAssetBundleIds, assetBundlesManifestPath, logger);
    }

    public void Reset()
    {
        AssetBundlesManager.Instance.Reset();
    }

    public AddressablesOp LoadDependenciesAsync(AddressablesCatalogEntry entry)
    {
        AssetBundlesOpRequest request = AssetBundlesManager.Instance.LoadAssetBundleAndDependencies(entry.AssetBundleName, null, true);
        return ProcessAssetBundlesOpRequest(request);
    }

    public void UnloadDependencies(AddressablesCatalogEntry entry)
    {
        List<string> dependenciesList = AssetBundlesManager.Instance.GetDependenciesIncludingSelf(entry.AssetBundleName);
        AssetBundlesManager.Instance.UnloadAssetBundleList(dependenciesList, null);        
    }

    public bool LoadScene(AddressablesCatalogEntry entry, LoadSceneMode mode)
    {
        AssetBundlesOp.EResult result = AssetBundlesManager.Instance.LoadScene(entry.AssetBundleName, entry.AssetName, mode);
        return result == AssetBundlesOp.EResult.Success;
    }

    public AddressablesOp LoadSceneAsync(AddressablesCatalogEntry entry, LoadSceneMode mode)
    {
        AssetBundlesOpRequest request = AssetBundlesManager.Instance.LoadSceneAsync(entry.AssetBundleName, entry.AssetName, mode, null, true);
        return ProcessAssetBundlesOpRequest(request);
    }

    public AddressablesOp UnloadSceneAsync(AddressablesCatalogEntry entry)
    {
        AssetBundlesOpRequest request = AssetBundlesManager.Instance.UnloadSceneAsync(entry.AssetBundleName, entry.AssetName, null, true);
        return ProcessAssetBundlesOpRequest(request);        
    }  
    
    private AddressablesOp ProcessAssetBundlesOpRequest(AssetBundlesOpRequest request)
    {
        AddressablesFromAssetBundlesOp op = new AddressablesFromAssetBundlesOp();
        op.Setup(request);
        return op;
    }  

    public void Update()
    {
        AssetBundlesManager.Instance.Update();
    }
}
