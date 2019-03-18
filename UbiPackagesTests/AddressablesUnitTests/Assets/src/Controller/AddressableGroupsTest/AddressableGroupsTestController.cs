using System.IO;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class AddressableGroupsTestController : MonoBehaviour
{
    private Downloadables.MockHandle m_mockHandle;    

    void Start ()
    {
        Addressables_Init();
        Groups_Init();
    }

    void OnApplicationQuit()
    {
        m_addressablesManager.Reset();
    }    

    void Update()
    {
        m_addressablesManager.Update();

        if (m_groupsHandle != null)
        {
            m_groupsHandle.Update();
        }

        Ui_Update();

        if (m_mockHandle != null)
        {
            m_mockHandle.Update();

            Debug.Log(" progress bytes = " + m_mockHandle.GetDownloadedBytes() + " / " + m_mockHandle.GetTotalBytes() +
                         " progress % = " + m_mockHandle.Progress + " IsAvailable = " + m_mockHandle.IsAvailable() + " error = " + m_mockHandle.GetError());
        }

        if (Input.GetKeyDown(KeyCode.M))
        {            
            Debug_MockHandle();                                
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            m_addressablesManager.IsAutomaticDownloaderEnabled = true;            
        }
        else if (Input.GetKeyDown(KeyCode.R) && m_groupsHandle != null)
        {
            m_groupsHandle.Retry();
        }
    }

    private void Debug_MockHandle()
    {
        //m_mockHandle = new Downloadables.MockHandle(0, 100, 60, false, false, null);            
        //m_mockHandle = new Downloadables.MockHandle(100, 100, 60, false, false, null);
        //m_mockHandle = new Downloadables.MockHandle(50, 100, 20, false, false, null);                        
        
        m_mockHandle = new Downloadables.MockHandle(50, 100, 20, false, false, null);
        m_mockHandle.AddAction(5, Downloadables.Handle.EError.NO_CONNECTION);
        m_mockHandle.AddAction(35, Downloadables.Handle.EError.NONE);
    }    

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

    #region groups
    private static string GROUP_ID = "group_asset_cubes";

    private Downloadables.Handle m_groupsHandle;

    private void Groups_Init()
    {
        m_groupsHandle = m_addressablesManager.CreateDownloadablesHandle(GROUP_ID);
    }
    #endregion

    #region ui
    public Text m_uiProgress;
    public Text m_uiError;

    private void Ui_Update()
    {
        if (m_groupsHandle != null)
        {
            m_uiProgress.text = m_groupsHandle.Progress + "";

            Downloadables.Error.EType errorType = m_groupsHandle.GetErrorType();
            m_uiError.text = m_groupsHandle.GetError() + "(" + errorType + ", " + (int)errorType + ")";
        }
    }
    #endregion
}
