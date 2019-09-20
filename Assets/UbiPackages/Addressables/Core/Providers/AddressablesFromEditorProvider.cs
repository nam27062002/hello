#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine;

/// <summary>
/// This class is used as a provider when playing in editor.
/// </summary>
public class AddressablesFromEditorProvider : AddressablesProvider
{   
    private string GetPath(AddressablesCatalogEntry entry)
    {                
        return (entry == null) ? null : AssetDatabase.GUIDToAssetPath(entry.GUID);
    } 

    private string GetSceneName(AddressablesCatalogEntry entry)
    {
        return GetPath(entry);
    }

    public override bool LoadScene(AddressablesCatalogEntry entry, LoadSceneMode mode)
    {
        string sceneName = GetSceneName(entry);
        if (mode == LoadSceneMode.Additive)
        {
            EditorApplication.LoadLevelAdditiveInPlayMode(sceneName);
        }
        else
        {
            EditorApplication.LoadLevelInPlayMode(sceneName);
        }

        return true;
    }

    public override AddressablesOp LoadSceneAsync(AddressablesCatalogEntry entry, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        string sceneName = GetSceneName(entry);
        AsyncOperation op;
        if (mode == LoadSceneMode.Additive)
        {
            op = EditorApplication.LoadLevelAdditiveAsyncInPlayMode(sceneName);
        }
        else
        {
            op = EditorApplication.LoadLevelAsyncInPlayMode(sceneName);
        }
                
        return ProcessAsyncOperation(op, AddressablesError.EType.Error_Invalid_Scene);
    }

    public override T LoadAsset<T>(AddressablesCatalogEntry entry)
    {
        if (CanLog())
        {
            Log("LoadAsset by Editor provider " + entry.AssetName);
        }

        return AssetDatabase.LoadAssetAtPath<T>(GetPath(entry));
    }

    public override object LoadAsset(AddressablesCatalogEntry entry)
    {
        return LoadAssetObject(entry);        
    }

    public override AddressablesOp LoadAssetAsync(AddressablesCatalogEntry entry)
    {        
        AddressablesLoadFromEditorOp returnValue = new AddressablesLoadFromEditorOp();
        returnValue.Setup(GetPath(entry));

        return returnValue;
    }

    private object LoadAssetObject(AddressablesCatalogEntry entry)
    {
        if (CanLog())
        {
            Log("LoadAsset by Editor provider " + entry.AssetName);
        }

        return AssetDatabase.LoadMainAssetAtPath(GetPath(entry));
    }

    public override AddressablesOp UnloadSceneAsync(AddressablesCatalogEntry entry)
    {
        AsyncOperation op = SceneManager.UnloadSceneAsync(GetSceneName(entry));
        return ProcessAsyncOperation(op, AddressablesError.EType.Error_Invalid_Scene);
    }
}
#endif