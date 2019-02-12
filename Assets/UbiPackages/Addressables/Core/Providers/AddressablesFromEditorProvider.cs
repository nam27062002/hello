#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class is used as a provider when playing in editor. It exteds AddressablesFromResourcesProvider to reuse scenes related stuff
/// </summary>
public class AddressablesFromEditorProvider : AddressablesFromResourcesProvider
{    
    public override bool LoadScene(AddressablesCatalogEntry entry, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive)
        {
            EditorApplication.LoadLevelAdditiveInPlayMode(entry.AssetName);
        }
        else
        {
            EditorApplication.LoadLevelInPlayMode(entry.AssetName);
        }        
        
        return true;
    }

    public override AddressablesOp LoadSceneAsync(AddressablesCatalogEntry entry, LoadSceneMode mode)
    {
        AsyncOperation op;
        if (mode == LoadSceneMode.Additive)
        { 
            op = EditorApplication.LoadLevelAdditiveAsyncInPlayMode(entry.AssetName);
        }
        else
        {
            op = EditorApplication.LoadLevelAsyncInPlayMode(entry.AssetName);
        }

        return ProcessAsyncOperation(op, AddressablesError.EType.Error_Invalid_Scene);
    }   
}
#endif