using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UTAddressablesEditorParseABs : UnitTest
{
    private static string AB_SCENE_CUBES_NAME = "01/scene_cubes";
    private static string AB_AB_NAME = "01/asset_cubes";
    private static string AB_MATERIALS_NAME = "01/cubes/materials";
    private static string AB_UNKNOWN_NAME = "unknown";
    private static string AB_NOT_USED_NAME = "not_used";

    private static string ROOT_PATH = "Assets/Editor/Addressables/UnitTests";
    private static string AB_ROOT_PATH = ROOT_PATH + "/AssetBundles";
    private static string ADDRESSABLES_ROOT_PATH = ROOT_PATH + "/AddressablesCatalogs";

    public static UnitTestBatch GetUnitTestBatch()
    {
        UnitTestBatch batch = new UnitTestBatch("UTAddressablesEditorParseABs");

        EditorAddressablesManager.ParseAssetBundlesOutput output;
        UTAddressablesEditorParseABs test;

        //----------------------------------------------
        // SUCCESS
        //----------------------------------------------                
        
        // No folder with catalog and manifest
        output = new EditorAddressablesManager.ParseAssetBundlesOutput();
        test = new UTAddressablesEditorParseABs();
        test.Setup(null, null, output);
        batch.AddTest(test, true);

        // No catalog and no manifest files in an existing folder
        output = new EditorAddressablesManager.ParseAssetBundlesOutput();
        test = new UTAddressablesEditorParseABs();
        test.Setup("00", "00", output);
        batch.AddTest(test, true);

        // Two asset bundles used explicityly by addressables catalog, both remote because neither is defined as local 
        // in the catalog
        output = new EditorAddressablesManager.ParseAssetBundlesOutput();
        output.m_RemoteABList = new List<string>(new string[] { AB_SCENE_CUBES_NAME , AB_AB_NAME });
        test = new UTAddressablesEditorParseABs();
        test.Setup("01", "01", output);
        batch.AddTest(test, true);

        // The same as the above one but with the ab names list in reverse order to prove the order is not important
        output = new EditorAddressablesManager.ParseAssetBundlesOutput();
        output.m_RemoteABList = new List<string>(new string[] { AB_AB_NAME, AB_SCENE_CUBES_NAME });
        test = new UTAddressablesEditorParseABs();
        test.Setup("01", "01", output);
        batch.AddTest(test, true);

        // Only AB_SCENE_CUBES_NAME defined in addressables catalog and not defined as local in the catalog
        // AB_SCENE_CUBES_NAME depends on AB_AB_NAME
        output = new EditorAddressablesManager.ParseAssetBundlesOutput();
        output.m_RemoteABList = new List<string>(new string[] { AB_SCENE_CUBES_NAME, AB_AB_NAME });        
        test = new UTAddressablesEditorParseABs();
        test.Setup("01", "02", output);
        batch.AddTest(test, true);

        // Only AB_MATERIALS_NAME defined in addressables catalog and not defined as local in the catalog.
        // AB_MATERIALS_NAME has no dependencies in the manifest
        output = new EditorAddressablesManager.ParseAssetBundlesOutput();
        output.m_RemoteABList = new List<string>(new string[] { AB_MATERIALS_NAME });
        output.m_ABInManifestNotUsed = new List<string>(new string[] { AB_AB_NAME, AB_SCENE_CUBES_NAME });
        test = new UTAddressablesEditorParseABs();
        test.Setup("02", "03", output);
        batch.AddTest(test, true);

        // Only AB_MATERIALS_NAME defined in addressables catalog and not defined as local in the catalog
        // There's no manifest
        output = new EditorAddressablesManager.ParseAssetBundlesOutput();
        output.m_ABInUsedNotInManifest = new List<string>(new string[] { AB_MATERIALS_NAME });
        test = new UTAddressablesEditorParseABs();
        test.Setup("00", "03", output);
        batch.AddTest(test, true);

        // There's manifest but there's no addressables catalog
        output = new EditorAddressablesManager.ParseAssetBundlesOutput();
        output.m_ABInManifestNotUsed = new List<string>(new string[] { AB_SCENE_CUBES_NAME, AB_AB_NAME });
        test = new UTAddressablesEditorParseABs();
        test.Setup("01", "00", output);
        batch.AddTest(test, true);

        // AB_AB_NAME is required, which depends on AB_MATERIAL_NAME
        // AB_SCENE_CUBES_NAME is not used
        output = new EditorAddressablesManager.ParseAssetBundlesOutput();
        output.m_ABInManifestNotUsed = new List<string>(new string[] { AB_SCENE_CUBES_NAME });
        output.m_RemoteABList = new List<string>(new string[] { AB_AB_NAME, AB_MATERIALS_NAME });
        test = new UTAddressablesEditorParseABs();
        test.Setup("02", "04", output);
        batch.AddTest(test, true);

        // Only AB_SCENE_CUBES_NAME defined in addressables catalog and defined as local in the catalog
        // AB_SCENE_CUBES_NAME depends on AB_AB_NAME
        output = new EditorAddressablesManager.ParseAssetBundlesOutput();
        output.m_LocalABList = new List<string>(new string[] { AB_SCENE_CUBES_NAME, AB_AB_NAME });
        test = new UTAddressablesEditorParseABs();
        test.Setup("01", "02_local", output);
        batch.AddTest(test, true);        

        // Only AB_AB_NAME defined in addressables catalog and defined as local
        // AB_AB_NAME doesn't have any dependencies.
        // AB_SCENE_CUBES_NAME is not used
        output = new EditorAddressablesManager.ParseAssetBundlesOutput();
        output.m_ABInManifestNotUsed = new List<string>(new string[] { AB_SCENE_CUBES_NAME });
        output.m_LocalABList = new List<string>(new string[] { AB_AB_NAME });
        test = new UTAddressablesEditorParseABs();
        test.Setup("01", "04_local", output);
        batch.AddTest(test, true);        
        
        // Only AB_AB_NAME defined in addressables catalog and defined as local.
        // AB_AB_NAME depends on AB_MATERIALS_NAME
        // AB_SCENE_CUBES_NAME is not used but is defined in the list of local ABs
        output = new EditorAddressablesManager.ParseAssetBundlesOutput();
        output.m_LocalABNotUsedList = new List<string>(new string[] { });
        output.m_ABInManifestNotUsed = new List<string>(new string[] { });
        output.m_LocalABList = new List<string>(new string[] { AB_AB_NAME, AB_MATERIALS_NAME, AB_SCENE_CUBES_NAME });
        test = new UTAddressablesEditorParseABs();
        test.Setup("02", "05_local", output);
        batch.AddTest(test, true);
        
        // AB_AB_NAME and AB_SCENE_CUBES_NAME defined in addressables catalog
        // AB_AB_NAME defined as local.        
        // AB_AB_NAME depends on AB_MATERIALS_NAME        
        output = new EditorAddressablesManager.ParseAssetBundlesOutput();
        output.m_RemoteABList = new List<string>(new string[] { AB_SCENE_CUBES_NAME });
        output.m_LocalABList = new List<string>(new string[] { AB_AB_NAME, AB_MATERIALS_NAME });
        test = new UTAddressablesEditorParseABs();
        test.Setup("02", "06_local", output);
        batch.AddTest(test, true);

        // Only AB_AB_NAME and AB_SCENE_CUBES_NAME defined in addressables catalog
        // AB_AB_NAME defined as local.        
        // AB_AB_NAME depends on AB_MATERIALS_NAME        
        output = new EditorAddressablesManager.ParseAssetBundlesOutput();
        output.m_RemoteABList = new List<string>(new string[] { AB_SCENE_CUBES_NAME });
        output.m_LocalABList = new List<string>(new string[] { AB_AB_NAME, AB_MATERIALS_NAME });
        test = new UTAddressablesEditorParseABs();
        test.Setup("02", "06_local", output);
        batch.AddTest(test, true);

        // AB_AB_NAME, AB_SCENE_CUBES_NAME and AB_UNKNOWN_NAME defined in addressables catalog
        // AB_AB_NAME defined as local.        
        // AB_AB_NAME depends on AB_MATERIALS_NAME       
        // AB_UNKNOWN_NAME is not in manifest
        output = new EditorAddressablesManager.ParseAssetBundlesOutput();
        output.m_ABInUsedNotInManifest = new List<string>(new string[] { AB_UNKNOWN_NAME });        
        output.m_RemoteABList = new List<string>(new string[] { AB_SCENE_CUBES_NAME });
        output.m_LocalABList = new List<string>(new string[] { AB_AB_NAME, AB_MATERIALS_NAME });
        test = new UTAddressablesEditorParseABs();
        test.Setup("02", "07_local", output);
        batch.AddTest(test, true);        

        // AB_AB_NAME, AB_SCENE_CUBES_NAME and AB_UNKNOWN_NAME defined in addressables catalog
        // AB_AB_NAME and AB_NOT_USED_NAME defined as local.     
        // AB_AB_NAME doesn't depend on AB_MATERIALS_NAME    
        // AB_UNKNOWN_NAME and AB_NOT_USED_NAME  are not in manifest
        output = new EditorAddressablesManager.ParseAssetBundlesOutput();
        output.m_ABInUsedNotInManifest = new List<string>(new string[] { AB_UNKNOWN_NAME, AB_NOT_USED_NAME });
        output.m_LocalABNotUsedList = new List<string>(new string[] { });
        output.m_RemoteABList = new List<string>(new string[] { AB_SCENE_CUBES_NAME });
        output.m_LocalABList = new List<string>(new string[] { AB_AB_NAME });
        test = new UTAddressablesEditorParseABs();
        test.Setup("01", "08_local", output);
        batch.AddTest(test, true);

        //----------------------------------------------
        // FAIL
        //----------------------------------------------                
                
        // No catalog and no manifest files in an existing folder
        output = new EditorAddressablesManager.ParseAssetBundlesOutput();
        output.m_RemoteABList = new List<string>(new string[] { AB_SCENE_CUBES_NAME, AB_AB_NAME });
        test = new UTAddressablesEditorParseABs();
        test.Setup("00", "00", output);
        batch.AddTest(test, false);

        // Only AB_SCENE_CUBES_NAME defined in addressables catalog and not defined as local in the catalog
        // AB_SCENE_CUBES_NAME depends on AB_AB_NAME
        output = new EditorAddressablesManager.ParseAssetBundlesOutput();
        output.m_RemoteABList = new List<string>(new string[] { AB_SCENE_CUBES_NAME });
        test = new UTAddressablesEditorParseABs();
        test.Setup("01", "02", output);        
        batch.AddTest(test, false);               

        return batch;
    }

    private string m_abRootPath;
    private string m_addressablesRootPath;
    private List<string> m_localABList;
    private List<string> m_remoteABList;
    private List<string> m_ABInManifestNotUsed;
    private List<string> m_ABInUsedNotInManifest;
    private List<string> m_LocalABNotUsedList;    

    private void Reset()
    {
        m_abRootPath = null;
        m_addressablesRootPath = null;
        m_localABList = null;
        m_remoteABList = null;
        m_ABInManifestNotUsed = null;
        m_ABInUsedNotInManifest = null;
        m_LocalABNotUsedList = null;
    }

    public void Setup(string abRootPath, string addressablesRootPath, EditorAddressablesManager.ParseAssetBundlesOutput result)
    {
        Reset();
        
        m_abRootPath = (abRootPath == null) ? null : Path.Combine(AB_ROOT_PATH, abRootPath);
        m_addressablesRootPath = (addressablesRootPath == null) ? null : Path.Combine(ADDRESSABLES_ROOT_PATH, addressablesRootPath);
        m_localABList = result.m_LocalABList;
        m_remoteABList = result.m_RemoteABList;
        m_ABInManifestNotUsed = result.m_ABInManifestNotUsed;
        m_ABInUsedNotInManifest = result.m_ABInUsedNotInManifest;
        m_LocalABNotUsedList = result.m_LocalABNotUsedList;
    }

    protected override void ExtendedPerform()
    {
        AssetBundle.UnloadAllAssetBundles(true);

        AddressablesCatalog editorCatalog = null;
        if (m_addressablesRootPath != null)
        {                        
            string editorCatalogPath = m_addressablesRootPath + "/" + EditorAddressablesManager.ADDRESSABLES_EDITOR_CATALOG_FILENAME;
            if (File.Exists(editorCatalogPath))
            {
                editorCatalog = EditorAddressablesManager.GetCatalog(editorCatalogPath, true);
            }
        }

        AssetBundle manifestBundle = null;             
        AssetBundleManifest abManifest = null;
        if (m_abRootPath != null)
        {
            string abManifestPath = Path.Combine(m_abRootPath, "dependencies");
            abManifest = AssetBundlesManager.LoadManifest(abManifestPath, out manifestBundle);                                    
        }

        EditorAddressablesManager.ParseAssetBundlesOutput output = EditorAddressablesManager.ParseAssetBundles(editorCatalog, abManifest);

        if (manifestBundle != null)
        {
            manifestBundle.Unload(true);
        }

        bool passes = Equals(output.m_LocalABList, m_localABList) &&
                      Equals(output.m_RemoteABList, m_remoteABList) &&
                      Equals(output.m_ABInManifestNotUsed, m_ABInManifestNotUsed) &&
                      Equals(output.m_ABInUsedNotInManifest, m_ABInUsedNotInManifest) &&
                      Equals(output.m_LocalABNotUsedList, m_LocalABNotUsedList);

        NotifyPasses(passes);
    }

    private bool Equals(List<string> l1, List<string> l2)
    {
        return UbiListUtils.Compare(l1, l2);

        /*List<string> intersection;
        List<string> disjoint;

        UbiListUtils.SplitIntersectionAndDisjoint(l1, l2, out intersection, out disjoint);
        bool returnValue = (disjoint == null || disjoint.Count == 0);
        if (returnValue)
        {
            int intersectionCount = (intersection == null) ? 0 : intersection.Count;
            int localABListCount = (l1 == null) ? 0 : l1.Count;
            returnValue = intersectionCount == localABListCount;
        }

        return returnValue;
        */
    }
}