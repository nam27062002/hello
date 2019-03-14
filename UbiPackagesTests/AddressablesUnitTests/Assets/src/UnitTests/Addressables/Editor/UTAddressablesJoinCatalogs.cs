using SimpleJSON;
using System.IO;
using UnityEngine;

public class UTAddressablesJoinCatalogs : UnitTest
{
    private static string ROOT_PATH = "Assets/Editor/Addressables/UnitTests";
    private static string ADDRESSABLES_ROOT_PATH = ROOT_PATH + "/AddressablesCatalogs/joins/";

    public static UnitTestBatch GetUnitTestBatch()
    {
        UnitTestBatch batch = new UnitTestBatch("UTAddressablesEditorParseABs");

        UTAddressablesJoinCatalogs test;

        //----------------------------------------------
        // SUCCESS
        //----------------------------------------------                        
        
        // PURPOSE: Test joining 1 catalogs and null
        test = new UTAddressablesJoinCatalogs();
        test.Setup("1", "0", true);
        batch.AddTest(test, true);
        
        // PURPOSE: Test joining 1 catalogs and null
        test = new UTAddressablesJoinCatalogs();
        test.Setup("0", "1", true);
        batch.AddTest(test, true);

        // PURPOSE: Test joining two catalogs with no coincidences        
        test = new UTAddressablesJoinCatalogs();                
        test.Setup("1", "2", true);
        batch.AddTest(test, true);        

        // PURPOSE: Test joining two catalogs with a coincidence in localAssetBundles
        test = new UTAddressablesJoinCatalogs();
        test.Setup("1", "3", true);
        batch.AddTest(test, true);        

        // PURPOSE: Test joining two catalogs with a coincidence in localAssetBundles and groups
        test = new UTAddressablesJoinCatalogs();
        test.Setup("1", "4", true);
        batch.AddTest(test, true);        

        // PURPOSE: Test joining two catalogs with a coincidence in a group
        test = new UTAddressablesJoinCatalogs();
        test.Setup("1", "5", true);
        batch.AddTest(test, true);        

        // PURPOSE: Test joining two catalogs that are the same
        test = new UTAddressablesJoinCatalogs();
        test.Setup("1", "1", false);
        batch.AddTest(test, true);        

        return batch;
    }

    private string m_pathCatalog1;
    private string m_pathCatalog2;
    private string m_pathResult;
    private bool m_resultPasses;

    public void Setup(string catalogName1, string catalogName2, bool resultPasses)
    {
        m_pathCatalog1 = ADDRESSABLES_ROOT_PATH + catalogName1 + ".json";
        m_pathCatalog2 = ADDRESSABLES_ROOT_PATH + catalogName2 + ".json";
        m_pathResult = ADDRESSABLES_ROOT_PATH + catalogName1 + "_" + catalogName2 + ".json";
        m_resultPasses = resultPasses;
    }

    protected override void ExtendedPerform()
    {
        //string editorCatalogPath = m_addressablesRootPath + "/" + EditorAddressablesManager.ADDRESSABLES_EDITOR_CATALOG_FILENAME;

        AddressablesCatalog catalog1 = null, catalog2 = null;
        if (File.Exists(m_pathCatalog1))
        {
            catalog1 = EditorAddressablesManager.GetCatalog(m_pathCatalog1, true);
        }

        if (File.Exists(m_pathCatalog2))
        {
            catalog2 = EditorAddressablesManager.GetCatalog(m_pathCatalog2, true);
        }        

        bool passes = true;
        if (catalog1 == null)
        {
            catalog1 = catalog2;
            catalog2 = null;
        }

        string catalog1BeforeJoinText = null;
        string catalog1AfterJoinText = null;
        JSONNode catalog2JSON;

        if (catalog2 == null)
        {
            catalog2JSON = null;
        }
        else
        {
            catalog2JSON = catalog2.ToJSON();
        }

        if (catalog1 != null)
        {
            catalog1BeforeJoinText = catalog1.ToJSON().ToString();

            passes = catalog1.Join(catalog2JSON, new ConsoleLogger("UTAddressablesJoinCatalogs"));

            catalog1AfterJoinText = catalog1.ToJSON().ToString();
            Debug.Log(catalog1AfterJoinText);            
        }

        string catalogResult = null;
        if (catalog2 == null)
        {
            catalogResult = catalog1BeforeJoinText;
        }
        else if (File.Exists(m_pathResult))
        {
            catalogResult = File.ReadAllText(m_pathResult);
        }        

        passes = passes == m_resultPasses;

        if (passes)
        {
            passes = catalogResult == catalog1AfterJoinText;
        }

        NotifyPasses(passes);
    }
}
