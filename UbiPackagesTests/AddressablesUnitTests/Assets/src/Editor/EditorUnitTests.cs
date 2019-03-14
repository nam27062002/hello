using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class EditorUnitTests : MonoBehaviour
{
    static EditorUnitTests()
    {
        EditorApplication.update += Internal_Update;
    }

    private const string MENU = "Unit Tests";

    private const string MENU_ADDRESSABLES = MENU + "/Addressables";
    private const string MENU_ADDRESSABLES_PARSE_ABS = MENU_ADDRESSABLES + "/Parse ABs Test";
    private const string MENU_ADDRESSABLES_JOIN_CATALOGS = MENU_ADDRESSABLES + "/Join Catalogs";
    private static List<string> MENU_ADDRESSABLES_ALL_NAMES = new List<string>(new string[] { MENU_ADDRESSABLES_PARSE_ABS, MENU_UBI_LISTS_SPLIT, MENU_ADDRESSABLES_JOIN_CATALOGS });

    private const string MENU_ASSET_BUNDLES = MENU + "/AssetBundles";
    private const string MENU_ASSET_BUNDLES_LOAD = MENU_ASSET_BUNDLES + "/Load";
    private static List<string> MENU_ASSET_BUNDLES_ALL_NAMES = new List<string>(new string[] { MENU_ASSET_BUNDLES_LOAD });

    private const string MENU_DOWNLOADABLES = MENU + "/Downloadables";
    private const string MENU_DOWNLOADABLES_PARSE_CATALOG = MENU_DOWNLOADABLES + "/Parse Catalog";
    private const string MENU_DOWNLOADABLES_INITIALIZE = MENU_DOWNLOADABLES + "/Initialize";
    private const string MENU_DOWNLOADABLES_DISK = MENU_DOWNLOADABLES + "/Disk";
    private const string MENU_DOWNLOADABLES_CLEANER = MENU_DOWNLOADABLES + "/Cleaner";
    private const string MENU_DOWNLOADABLES_CATALOG_ENTRY_STATUS = MENU_DOWNLOADABLES + "/Catalog Entry Status";
    private const string MENU_DOWNLOADABLES_DOWNLOADER = MENU_DOWNLOADABLES + "/Downloader";
    private const string MENU_DOWNLOADABLES_All_TESTS = MENU_DOWNLOADABLES + "/All tests";
    private static List<string> MENU_DOWNLOADABLES_ALL_NAMES = new List<string>(new string[] { MENU_DOWNLOADABLES_PARSE_CATALOG, MENU_DOWNLOADABLES_INITIALIZE,
                                                                                               MENU_DOWNLOADABLES_DISK, MENU_DOWNLOADABLES_CLEANER, MENU_DOWNLOADABLES_CATALOG_ENTRY_STATUS,
                                                                                               MENU_DOWNLOADABLES_DOWNLOADER});

    private const string MENU_UBI_LISTS = MENU + "/UbiLists";
    private const string MENU_UBI_LISTS_ADD_RANGE = MENU_UBI_LISTS + "/AddRange Test";
    private const string MENU_UBI_LISTS_SPLIT = MENU_UBI_LISTS + "/Split Test";
    private const string MENU_UBI_LISTS_ALL = MENU_UBI_LISTS + "/All Tests";
    
    private static List<string> MENU_UBI_LISTS_ALL_NAMES = new List<string>(new string[] { MENU_UBI_LISTS_ADD_RANGE, MENU_UBI_LISTS_SPLIT });

    private const string MENU_ALL = MENU + "/All Tests";
    private const string MENU_RESET = MENU + "/Reset Tests";

    private static UnitTestBatch sm_unitTestBatch;

    private static UnitTestBatch GetUnitTestBatch(string key)
    {
        switch (key)
        {
            case MENU_ADDRESSABLES_PARSE_ABS:
                return UTAddressablesEditorParseABs.GetUnitTestBatch();

            case MENU_ADDRESSABLES_JOIN_CATALOGS:
                return UTAddressablesJoinCatalogs.GetUnitTestBatch();

            case MENU_ASSET_BUNDLES_LOAD:
                return UTAssetBundlesLoad.GetUnitTestBatch();

            case MENU_DOWNLOADABLES_PARSE_CATALOG:
                return UTLoadDownloadablesCatalog.GetUnitTestBatch();

            case MENU_DOWNLOADABLES_INITIALIZE:
                return UTDownloadablesInitialize.GetUnitTestBatch();

            case MENU_DOWNLOADABLES_DISK:
                return UTDownloadablesDisk.GetUnitTestBatch();

            case MENU_DOWNLOADABLES_CATALOG_ENTRY_STATUS:
                return UTDownloadablesCatalogEntryStatus.GetUnitTestBatch();

            case MENU_DOWNLOADABLES_DOWNLOADER:
                return UTDownloadablesDownloader.GetUnitTestBatch();                

            case MENU_DOWNLOADABLES_CLEANER:
                return UTDownloadablesCleaner.GetUnitTestBatch();

            case MENU_UBI_LISTS_ADD_RANGE:
                return UTListAddRange<string>.GetUnitTestBatch();

            case MENU_UBI_LISTS_SPLIT:
                return UTSplitIntersecionAndDisjoint<string>.GetUnitTestBatch();
        }

        return null;
    }

    [MenuItem(MENU_ADDRESSABLES_PARSE_ABS)]
    public static void UnitTests_Addressables_ParseABs()
    {
        PerformAllTests(MENU_ADDRESSABLES_PARSE_ABS);        
    }

    [MenuItem(MENU_ADDRESSABLES_JOIN_CATALOGS)]
    public static void UnitTests_Addressables_JoinCatalogs()
    {
        PerformAllTests(MENU_ADDRESSABLES_JOIN_CATALOGS);
    }

    [MenuItem(MENU_ASSET_BUNDLES_LOAD)]
    public static void UnitTests_AssetBundles_Load()
    {
        PerformAllTests(MENU_ASSET_BUNDLES_LOAD);
    }

    [MenuItem(MENU_DOWNLOADABLES_PARSE_CATALOG)]
    public static void UnitTests_Downloadables_ParseCatalog()
    {
        PerformAllTests(MENU_DOWNLOADABLES_PARSE_CATALOG);
    }

    [MenuItem(MENU_DOWNLOADABLES_INITIALIZE)]
    public static void UnitTests_Downloadables_Initialize()
    {
        PerformAllTests(MENU_DOWNLOADABLES_INITIALIZE);
    }

    [MenuItem(MENU_DOWNLOADABLES_DISK)]
    public static void UnitTests_Downloadables_Disk()
    {
        PerformAllTests(MENU_DOWNLOADABLES_DISK);
    }

    [MenuItem(MENU_DOWNLOADABLES_CATALOG_ENTRY_STATUS)]
    public static void UnitTests_Downloadables_CatalogEntryStatus()
    {
        PerformAllTests(MENU_DOWNLOADABLES_CATALOG_ENTRY_STATUS);
    }

    [MenuItem(MENU_DOWNLOADABLES_DOWNLOADER)]
    public static void UnitTests_Downloadables_Downloader()
    {
        PerformAllTests(MENU_DOWNLOADABLES_DOWNLOADER);
    }

    [MenuItem(MENU_DOWNLOADABLES_CLEANER)]
    public static void UnitTests_Downloadables_Cleaner()
    {
        PerformAllTests(MENU_DOWNLOADABLES_CLEANER);
    }
    
    [MenuItem(MENU_DOWNLOADABLES_All_TESTS)]
    public static void UnitTests_Downloadables_AllTests()
    {
        List<UnitTestBatch> batches = GetUnitTestBatchList(MENU_DOWNLOADABLES_ALL_NAMES);
        PerformUnitTestBatchList(batches);
    }

    [MenuItem(MENU_UBI_LISTS_ADD_RANGE)]
    public static void UnitTests_UbiLists_AddRange()
    {
        PerformAllTests(MENU_UBI_LISTS_ADD_RANGE);                
    }

    [MenuItem(MENU_UBI_LISTS_SPLIT)]
    public static void UnitTests_UbiLists_Split()
    {
        PerformAllTests(MENU_UBI_LISTS_SPLIT);        
    }

    [MenuItem(MENU_UBI_LISTS_ALL)]
    public static void UnitTests_UbiLists_All()
    {
        List<UnitTestBatch> batches = GetUnitTestBatchList(MENU_UBI_LISTS_ALL_NAMES);
        PerformUnitTestBatchList(batches);
    }

    [MenuItem(MENU_ALL)]
    public static void UnitTests_All()
    {
        List<string> keys = new List<string>();
        UbiListUtils.AddRange(keys, MENU_UBI_LISTS_ALL_NAMES, false, true);
        UbiListUtils.AddRange(keys, MENU_ADDRESSABLES_ALL_NAMES, false, true);
        UbiListUtils.AddRange(keys, MENU_ASSET_BUNDLES_ALL_NAMES, false, true);        
        UbiListUtils.AddRange(keys, MENU_DOWNLOADABLES_ALL_NAMES, false, true);

        List <UnitTestBatch> batches = GetUnitTestBatchList(keys);
        PerformUnitTestBatchList(batches);
    }

    [MenuItem(MENU_RESET)]
    private static void UnitTest_Reset()
    {
        sm_unitTestBatch = null;
    }

    private static void PerformAllTests(string key)
    {
        sm_unitTestBatch = GetUnitTestBatch(key);
        if (sm_unitTestBatch != null)
        {
            sm_unitTestBatch.PerformAllTests();
        }
    }

    private static List<UnitTestBatch> GetUnitTestBatchList(List<string> keys)
    {
        List<UnitTestBatch> batches = new List<UnitTestBatch>();
        if (keys != null)
        {
            int count = keys.Count;
            for (int i = 0; i < count; i++)
            {
                batches.Add(GetUnitTestBatch(keys[i]));
            }
        }

        return batches;
    }    

    private static void PerformUnitTestBatchList(List<UnitTestBatch> batches)
    {        
        if (batches != null)
        {
            sm_unitTestBatch = new UnitTestBatch("Batch List");

            int count = batches.Count;            
            for (int i = 0; i < count; i++)
            {
                sm_unitTestBatch.AddBatch(batches[i]);
            }

            sm_unitTestBatch.PerformAllTests();
        }        
    }   

    static void Internal_Update()
    {
        if (sm_unitTestBatch != null)
        {
            if (sm_unitTestBatch.Update())
            {
                Debug.Log("DONE");
                sm_unitTestBatch = null;
            }
        }
    }
}
