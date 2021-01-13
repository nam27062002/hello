using SimpleJSON;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LoadAssetBundleTestController : MonoBehaviour
{    
    void Start ()
    {
        Addressables_Init();   
    }

    void OnAddressablesInit()
    {
        Debug.Log("OnAddressablesInit");
        LoadCubeAsync();        
    }

    private void LoadCubeAsync()
    {
        AddressablesOp op = m_addressablesManager.LoadAssetAsync("UbiCube");
        op.OnDone = OnLoadedCube;        
    }  

    private void OnLoadedCube(AddressablesOp op)
    {
        GameObject prefab = op.GetAsset<GameObject>();
        if (prefab != null)
        {
            Instantiate(prefab);
        }
    }        
        
    void Update ()
    {    
        m_addressablesManager.Update();     
    }    

    void OnApplicationQuit()
    {
        m_addressablesManager.Reset();
    }    

    #region addressables
    private AddressablesManager m_addressablesManager = new AddressablesManager();

    private void Addressables_Init()
    {            
        Logger logger = new ConsoleLogger("Addressables");

        // Addressables catalog         
        string catalogAsText = GetAddressablesFileText("addressablesCatalog", true);
        JSONNode catalogASJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);

        // Downloadables catalog        
        catalogAsText = GetAddressablesFileText("downloadablesCatalog", true);        
        JSONNode downloadablesCatalogASJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);
        
        // Downloadables config        
        catalogAsText = GetAddressablesFileText("downloadablesConfig", false);
        JSONNode downloadablesConfigASJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);
        Downloadables.Config downloadablesConfig = new Downloadables.Config();
        downloadablesConfig.Load(downloadablesConfigASJSON, logger);        

        // AssetBundles catalog
        catalogAsText = GetAddressablesFileText("assetBundlesCatalog", true);
        JSONNode abCatalogASJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);

        Downloadables.Tracker tracker = new Downloadables.DummyTracker(downloadablesConfig, logger);
        m_addressablesManager.Initialize(catalogASJSON, abCatalogASJSON, downloadablesConfig, downloadablesCatalogASJSON, true, tracker, logger);
        
        OnAddressablesInit();
    }

    private string GetAddressablesFileText(string fileName, bool platformDependent)
    {
#if UNITY_EDITOR
        string path = "Assets/Editor/Addressables/generated/";
        if (platformDependent)
        {
            path += UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString() + "/";
        }

        path += fileName + ".json";
        return File.ReadAllText(path);
#else
        string path = "Addressables/" + fileName;                        
        TextAsset targetFile = Resources.Load<TextAsset>(path);
        return (targetFile == null) ? null : targetFile.text;
#endif
    }

    public void Addressables_Reset()
    {
        m_addressablesManager.Reset();     
    }
    #endregion
}
