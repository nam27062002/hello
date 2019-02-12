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

    public override T LoadAsset<T>(AddressablesCatalogEntry entry)
    {
        Object o = LoadAssetObject(entry);
        return (T)System.Convert.ChangeType(o, typeof(T));        
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
        return Resources.Load(entry.AssetName);
    }
}

