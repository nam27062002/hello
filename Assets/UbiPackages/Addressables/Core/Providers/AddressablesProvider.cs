﻿using System.Collections.Generic;
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

    public virtual AddressablesOp LoadDependenciesAsync(AddressablesCatalogEntry entry)
    {
        // By default there's no dependencies
        AddressablesOpResult returnValue = new AddressablesOpResult();
        returnValue.Setup(null, null);

        return returnValue;
    }

    public virtual void UnloadDependencies(AddressablesCatalogEntry entry) {}

    public abstract bool LoadScene(AddressablesCatalogEntry entry, LoadSceneMode mode);
    public abstract AddressablesOp LoadSceneAsync(AddressablesCatalogEntry entry, LoadSceneMode mode);
    public abstract AddressablesOp UnloadSceneAsync(AddressablesCatalogEntry entry);

    public abstract T LoadAsset<T>(AddressablesCatalogEntry entry);
    public abstract AddressablesOp LoadAssetAsync(AddressablesCatalogEntry entry);

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
}
