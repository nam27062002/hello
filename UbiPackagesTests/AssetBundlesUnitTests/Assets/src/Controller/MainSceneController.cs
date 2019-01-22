using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneController : MonoBehaviour
{	    
	void Start ()
    {
        string localAssetBundlesPath = Application.streamingAssetsPath + "/AssetBundles";
        List<string> localAssetBundleIds = new List<string> { "01/cube" /*, "ab/logo", "ab/scene_cube"*/ };
        AssetBundlesManager.Instance.Initialize(localAssetBundleIds, localAssetBundlesPath, null);

        UT_Init();
    }	        

    public void OnAddCube()
    {
        //AddCubeFromResources();
        // AddCubeFromAddressable();
        //LoadAssetBundle();

        //LoadAsset_Start("ab/cube", "MyCube", ELoadAssetMode.AsyncFull);
        //LoadScene_Start();



        string abId = "01/cube";
        string resourceName = "MyCube";
        bool isAsset = true;
        LoadResource_Start(abId, resourceName, isAsset, ELoadResourceMode.AsyncFull);
        //LoadResource_Start(abId, resourceName, isAsset, ELoadResourceMode.Async);
        //LoadResource_Start(abId, resourceName, isAsset, ELoadResourceMode.Sync);

        abId = "ab/scene_cube";
        resourceName = "SC_MyCube";
        isAsset = false;
        LoadSceneMode loadSceneMode = LoadSceneMode.Additive;

        //LoadResource_Start(abId, resourceName, isAsset, ELoadResourceMode.AsyncFull, loadSceneMode);
        //LoadResource_Start(abId, resourceName, isAsset, ELoadResourceMode.Async, loadSceneMode);
        //LoadResource_Start(abId, resourceName, isAsset, ELoadResourceMode.Sync, loadSceneMode);
    }

    private void AddCubeFromAddressable()
    {
        /*
        AddressablesOp.OnDoneCallback onDone = delegate(AddressablesOp.EResult result, object data)
        {
            if (result == AddressablesOp.EResult.Success && data != null)
            {
                GameObject go = Instantiate(data as Object) as GameObject;
            }
            else
            {
                Debug.Log("Error " + result + " when loading asset");
            }
        };

        Addressables.Instance.LoadAsset<Object>("myCube", onDone);
        */
    }

    private void AddCubeFromResources()
    {
        Object prefab = Resources.Load("Cube/MyCube");
        if (prefab != null)
        {
            GameObject go = Instantiate(prefab) as GameObject;
        }
    }

    #region load_resource
    private enum ELoadResourceMode
    {
        Sync,
        Async,
        AsyncFull
    };

    private string m_loadResourceABId;
    private string m_loadResourceName;
    private bool m_loadResourceIsAsset;
    private ELoadResourceMode m_loadResourceMode;
    private LoadSceneMode m_loadResourceSceneMode;

    private void LoadResource_Start(string abId, string resourceName, bool isAsset, ELoadResourceMode mode, LoadSceneMode sceneMode = LoadSceneMode.Single)
    {
        m_loadResourceABId = abId;
        m_loadResourceName = resourceName;
        m_loadResourceIsAsset = isAsset;

        m_loadResourceMode = mode;
        m_loadResourceSceneMode = sceneMode;

        //string abId = "ab/logo";
        if (m_loadResourceMode != ELoadResourceMode.AsyncFull)
        {
            AssetBundlesManager.Instance.LoadAssetBundleAndDependencies(abId, LoadResource_OnAssetBundleLoaded);
        }
        else
        {            
            AssetBundlesManager.Instance.LoadResourceAsync(m_loadResourceABId, m_loadResourceName, m_loadResourceIsAsset, LoadResource_OnAssetLoaded, m_loadResourceSceneMode);            
        }
    }

    private void LoadResource_OnAssetBundleLoaded(AssetBundlesOp.EResult result, object data)
    {
        Debug.Log("LoadResource_OnAssetBundleLoaded " + result);
        if (result == AssetBundlesOp.EResult.Success)
        {
            if (m_loadResourceMode == ELoadResourceMode.Sync)
            {                
                Object asset = AssetBundlesManager.Instance.LoadResource(m_loadResourceABId, m_loadResourceName, m_loadResourceIsAsset, m_loadResourceSceneMode);
                if (asset != null && m_loadResourceIsAsset)
                {
                    GameObject go = Instantiate(asset) as GameObject;
                }
            }
            else if (m_loadResourceMode == ELoadResourceMode.Async)
            {
                AssetBundleHandle handle = AssetBundlesManager.Instance.GetAssetBundleHandle(m_loadResourceABId);
                AssetBundlesManager.Instance.LoadResourceFromAssetBundleAsync(handle.AssetBundle, m_loadResourceName, m_loadResourceIsAsset, LoadResource_OnAssetLoaded, m_loadResourceSceneMode);
            }
        }
    }

    private void LoadResource_OnAssetLoaded(AssetBundlesOp.EResult result, object data)
    {
        Debug.Log("On resource " + m_loadResourceName + " loaded from asset bundle " + m_loadResourceABId + " result = " + result);
        if (result == AssetBundlesOp.EResult.Success && data != null && m_loadResourceIsAsset)
        {
            GameObject go = Instantiate(data as Object) as GameObject;
        }
    }
    #endregion

    #region unit_tests
    private Queue<UnitTest> m_unitTests;

    public void UT_Init()
    {
        m_unitTests = new Queue<UnitTest>();

        string name = null;
        UnitTest ut;

        string abCubeId = "01/cube";
        string myCubeAssetName = "MyCube";

        /*name = "Sync load of an asset that exists from an asset bundle that exists";
        ut = new AssetBundlesUnitTest(name, abCubeId, myCubeAssetName, true, AssetBundleOp.EResult.Success, UT_OnDone);
        m_unitTests.Enqueue(ut);           
        
        
        name = "Sync load of an asset that doesn't exists from an asset bundle that exists";
        ut = new AssetBundlesUnitTest(name, abCubeId, "none", true, AssetBundleOp.EResult.Error_Asset_Not_Found_In_AB, UT_OnDone);
        m_unitTests.Enqueue(ut);        

        name = "Sync load of an asset that doesn't exists from an asset bundle that doesn't exists";
        ut = new AssetBundlesUnitTest(name, "none", "none", true, AssetBundleOp.EResult.Error_AB_Handle_Not_Found, UT_OnDone);
        m_unitTests.Enqueue(ut);
        
        name = "Async load of an asset that exists from an asset bundle that exists";
        ut = new AssetBundlesUnitTest(name, abCubeId, myCubeAssetName, false, AssetBundleOp.EResult.Success, UT_OnDone);
        m_unitTests.Enqueue(ut);
        */

        if (m_unitTests.Count > 0)
        {
            m_unitTests.Peek().Perform();
        }             
    }

    private void UT_OnDone(bool success)
    {
        if (m_unitTests != null)
        {
            m_unitTests.Dequeue();

            if (m_unitTests.Count > 0)
            {
                m_unitTests.Peek().Perform();
            }
        }
    }
    #endregion

    public void Update()
    {
        AssetBundlesManager.Instance.Update();
    }
}
