using UnityEngine;
using UnityEngine.SceneManagement;

public class AddressablesFromResourcesProvider : AddressablesProvider
{
    public bool LoadScene(AddressablesCatalogEntry entry, LoadSceneMode mode)
    {
        SceneManager.LoadScene(entry.AssetName, mode);
        return true;
    }

    public AddressablesOp UnloadSceneAsync(AddressablesCatalogEntry entry)
    {
        AddressablesOp returnValue;

        AsyncOperation op = SceneManager.UnloadSceneAsync(entry.AssetName);
        if (op == null)
        {
            AddressablesOpResult opResult = new AddressablesOpResult();
            AddressablesError error = new AddressablesError(AddressablesError.EType.Error_Manager_Not_initialized);
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

