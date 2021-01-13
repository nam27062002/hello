using UnityEngine;
using UnityEngine.SceneManagement;

public class AddressablesFromResourcesProvider : AddressablesProvider
{    
    public override bool LoadScene(AddressablesCatalogEntry entry, LoadSceneMode mode)
    {
        SceneManager.LoadScene(entry.AssetName, mode);
        return true;
    }

    public override AddressablesOp LoadSceneAsync(AddressablesCatalogEntry entry, LoadSceneMode mode)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(entry.AssetName, mode);
        return ProcessAsyncOperation(op, AddressablesError.EType.Error_Invalid_Scene);
    }

    public override AddressablesOp UnloadSceneAsync(AddressablesCatalogEntry entry)
    {        
        AsyncOperation op = SceneManager.UnloadSceneAsync(entry.AssetName);
        return ProcessAsyncOperation(op, AddressablesError.EType.Error_Invalid_Scene);        
    }    

    public override object LoadAsset(AddressablesCatalogEntry entry)
    {
        return LoadAssetObject(entry);        
    }

	public override T LoadAsset<T>(AddressablesCatalogEntry entry)
	{		
		return Resources.Load<T>(entry.AssetName);
	}

    public override AddressablesOp LoadAssetAsync(AddressablesCatalogEntry entry)
    {        
        AddressablesLoadFromResourcesOp returnValue = new AddressablesLoadFromResourcesOp();
        returnValue.Setup(entry.AssetName);     
        return returnValue;
    }

    private Object LoadAssetObject(AddressablesCatalogEntry entry)
    {
        if (CanLog())
        {
            Log("LoadAsset by FromResources provider");
        }

        return Resources.Load(entry.AssetName);
    }
}

