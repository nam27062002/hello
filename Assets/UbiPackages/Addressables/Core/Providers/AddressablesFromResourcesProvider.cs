using UnityEngine;
using UnityEngine.SceneManagement;

public class AddressablesFromResourcesProvider : AddressablesProvider
{
    public AddressablesOp LoadDependenciesAsync(AddressablesCatalogEntry entry)
    {
        // There's no dependencies when an asset is loaded from resources
        AddressablesOpResult returnValue = new AddressablesOpResult();
        returnValue.Setup(null, null);

        return returnValue;
    }

    public void UnloadDependencies(AddressablesCatalogEntry entry)
    {
        // There's no dependencies when an asset is loaded from resources
    }

    public bool LoadScene(AddressablesCatalogEntry entry, LoadSceneMode mode)
    {
        SceneManager.LoadScene(entry.AssetName, mode);
        return true;
    }

    public AddressablesOp LoadSceneAsync(AddressablesCatalogEntry entry, LoadSceneMode mode)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(entry.AssetName, mode);
        return ProcessAsyncOperation(op, AddressablesError.EType.Error_Invalid_Scene);
    }

    public AddressablesOp UnloadSceneAsync(AddressablesCatalogEntry entry)
    {        
        AsyncOperation op = SceneManager.UnloadSceneAsync(entry.AssetName);
        return ProcessAsyncOperation(op, AddressablesError.EType.Error_Invalid_Scene);        
    }

    private AddressablesOp ProcessAsyncOperation(AsyncOperation op, AddressablesError.EType errorType)
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
}

