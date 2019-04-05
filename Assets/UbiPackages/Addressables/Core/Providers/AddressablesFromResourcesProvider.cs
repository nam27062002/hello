﻿using UnityEngine;
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

    public override AddressablesOp LoadAssetAsync(AddressablesCatalogEntry entry)
    {
        Object data = LoadAssetObject(entry);
        AddressablesOp returnValue = new AddressablesOpResult();
        returnValue.Setup(null, data);

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

