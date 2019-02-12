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

    protected AddressablesOp ProcessAsyncOperation(AsyncOperation op, AddressablesError.EType errorType)
    {
        AddressablesOp returnValue;
        if (op == null)
        {
            AddressablesOpResult opResult = new AddressablesOpResult();
            AddressablesError error = new AddressablesError(errorType);
            opResult.Setup(error, null);
            returnValue = opResult;
        }
        else
        {
            AddressablesAsyncOp asyncOp = new AddressablesAsyncOp();
            asyncOp.Setup(new UbiUnityAsyncOperation(op));
            returnValue = asyncOp;
        }

        return returnValue;
    }

    public override T LoadAsset<T>(AddressablesCatalogEntry entry)
    {
        Object o = Resources.Load(entry.AssetName);
        return (T)System.Convert.ChangeType(o, typeof(T));        
    }

    public override AddressablesOp LoadAssetAsync(AddressablesCatalogEntry entry)
    {
        Object data = Resources.Load(entry.AssetName);
        AddressablesOp returnValue = new AddressablesOpResult();
        returnValue.Setup(null, data);

        return returnValue;
    }
}

