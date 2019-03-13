using System.IO;
using SimpleJSON;
using UnityEngine;

public class AddressableGroupsTestController : MonoBehaviour
{	
	void Start ()
    {
        Addressables_Init();
    }

    void OnApplicationQuit()
    {
        m_addressablesManager.Reset();
    }


    void Update()
    {
        m_addressablesManager.Update();

        if (Input.GetKeyDown(KeyCode.M))
        {
            /*bool available = m_addressablesManager.Groups_IsAvailable(GROUP_ID);
            string msg = "group is ";
            if (!available)
            {
                msg += "not ";
            }

            msg += "available";

            Debug.Log(msg);         
            */
        }
    }

    #region group
    private static string GROUP_ID = "group_asset_cubes";    
    #endregion

    #region addressables
    private AddressablesManager m_addressablesManager = new AddressablesManager();

    private void Addressables_Init()
    {
        Logger logger = new ConsoleLogger("Addressables");

        string addressablesPath = "Addressables";
        string assetBundlesPath = addressablesPath;
        string addressablesCatalogPath = Path.Combine(addressablesPath, "addressablesCatalog");

        // Addressables catalog 
        TextAsset targetFile = Resources.Load<TextAsset>(addressablesCatalogPath);
        string catalogAsText = (targetFile == null) ? null : targetFile.text;
        JSONNode catalogASJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);

        // Downloadables catalog
        string downloadablesPath = addressablesPath;
        string downloadablesCatalogPath = Path.Combine(downloadablesPath, "downloadablesCatalog");
        targetFile = Resources.Load<TextAsset>(downloadablesCatalogPath);
        catalogAsText = (targetFile == null) ? null : targetFile.text;
        JSONNode downloadablesCatalogASJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);

        // Downloadables config
        string downloadablesConfigPath = Path.Combine(downloadablesPath, "downloadablesConfig");
        targetFile = Resources.Load<TextAsset>(downloadablesConfigPath);
        catalogAsText = (targetFile == null) ? null : targetFile.text;
        JSONNode downloadablesConfigASJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);
        Downloadables.Config downloadablesConfig = new Downloadables.Config();
        downloadablesConfig.Load(downloadablesConfigASJSON, logger);

        Downloadables.Tracker tracker = new Downloadables.DummyTracker(downloadablesConfig, logger);
        m_addressablesManager.Initialize(catalogASJSON, assetBundlesPath, downloadablesConfig, downloadablesCatalogASJSON, tracker, logger);        
    }

    public void Addressables_Reset()
    {
        m_addressablesManager.Reset();
    }
    #endregion
}
