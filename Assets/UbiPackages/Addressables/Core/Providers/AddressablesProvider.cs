using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class AddressablesProvider
{
    public static Logger Logger { get; set; }
    public static bool CanLog()
    {
        return (Logger != null && Logger.CanLog());
    }

    public static void Log(string msg)
    {
        if (CanLog())
        {
            Logger.Log(msg);
        }
    }

    public virtual List<string> GetDependencyIds(AddressablesCatalogEntry entry)
    {
        return null;
    }

    public virtual bool IsResourceAvailable(AddressablesCatalogEntry entry, bool track = false)
    {
        // By default there's no dependencies so it's available
        return true;
    }

    public virtual AddressablesOp DownloadDependenciesAsync(AddressablesCatalogEntry entry)
    {
        // By default there's no dependencies
        AddressablesResultOp returnValue = new AddressablesResultOp();
        returnValue.Setup(null);

        return returnValue;
    }

    public virtual AddressablesOp LoadDependenciesAsync(AddressablesCatalogEntry entry)
    {
        // By default there's no dependencies
        AddressablesResultOp returnValue = new AddressablesResultOp();
        returnValue.Setup(null);

        return returnValue;
    }

    public virtual void UnloadDependencies(AddressablesCatalogEntry entry) {}

    public abstract bool LoadScene(AddressablesCatalogEntry entry, LoadSceneMode mode);
    public abstract AddressablesOp LoadSceneAsync(AddressablesCatalogEntry entry, LoadSceneMode mode);
    public abstract AddressablesOp UnloadSceneAsync(AddressablesCatalogEntry entry);

    public virtual T LoadAsset<T>(AddressablesCatalogEntry entry) where T : Object
    {
        return (T)System.Convert.ChangeType(LoadAsset(entry), typeof(T));
    }

    public abstract object LoadAsset(AddressablesCatalogEntry entry);
    public abstract AddressablesOp LoadAssetAsync(AddressablesCatalogEntry entry);

    protected AddressablesOp ProcessAsyncOperation(AsyncOperation op, AddressablesError.EType errorType)
    {
        AddressablesOp returnValue;
        if (op == null)
        {
            AddressablesResultOp opResult = new AddressablesResultOp();
            AddressablesError error = new AddressablesError(errorType);
            opResult.Setup(error);
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
