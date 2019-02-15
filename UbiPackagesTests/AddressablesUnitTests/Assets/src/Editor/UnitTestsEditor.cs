using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UnitTestsEditor : MonoBehaviour
{
    private const string MENU = "Unit Tests";

    private const string MENU_ADDRESSABLES = MENU + "/Addressables";
    private const string MENU_ADDRESSABLES_PARSE_ABS = MENU_ADDRESSABLES + "/Parse ABs Test";
    private static List<string> MENU_ADDRESSABLES_ALL_NAMES = new List<string>(new string[] { MENU_ADDRESSABLES_PARSE_ABS, MENU_UBI_LISTS_SPLIT });

    private const string MENU_UBI_DOWNLOADABLES = MENU + "/Downloadables";
    private const string MENU_DOWNLOADABLES_PARSE_CATALOG = MENU_UBI_DOWNLOADABLES + "/Parse Catalog";
    private static List<string> MENU_DOWNLOADABLES_ALL_NAMES = new List<string>(new string[] { MENU_DOWNLOADABLES_PARSE_CATALOG });

    private const string MENU_UBI_LISTS = MENU + "/UbiLists";
    private const string MENU_UBI_LISTS_ADD_RANGE = MENU_UBI_LISTS + "/AddRange Test";
    private const string MENU_UBI_LISTS_SPLIT = MENU_UBI_LISTS + "/Split Test";
    private const string MENU_UBI_LISTS_ALL = MENU_UBI_LISTS + "/All Tests";
    
    private static List<string> MENU_UBI_LISTS_ALL_NAMES = new List<string>(new string[] { MENU_UBI_LISTS_ADD_RANGE, MENU_UBI_LISTS_SPLIT });

    private const string MENU_ALL = MENU + "/All Tests";

    private static UnitTestBatch GetUnitTestBatch(string key)
    {
        switch (key)
        {
            case MENU_ADDRESSABLES_PARSE_ABS:
                return UTAddressablesEditorParseABs.GetUnitTestBatch();

            case MENU_DOWNLOADABLES_PARSE_CATALOG:
                return UTLoadDownloadablesCatalog.GetUnitTestBatch();
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

    [MenuItem(MENU_DOWNLOADABLES_PARSE_CATALOG)]
    public static void UnitTests_Downloadables_ParseCatalog()
    {
        PerformAllTests(MENU_DOWNLOADABLES_PARSE_CATALOG);
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
        UbiListUtils.AddRange(keys, MENU_DOWNLOADABLES_ALL_NAMES, false, true);

        List <UnitTestBatch> batches = GetUnitTestBatchList(keys);
        PerformUnitTestBatchList(batches);
    }

    private static void PerformAllTests(string key)
    {
        UnitTestBatch batch = GetUnitTestBatch(key);
        if (batch != null)
        {
            batch.PerformAllTests();
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
            int count = batches.Count;

            // First success tests
            UnitTestBatch.PrintSuccessHeader();
            for (int i = 0; i < count; i++)
            {
                batches[i].PerformSuccessTests();
            }

            UnitTestBatch.PrintFailHeader();
            for (int i = 0; i < count; i++)
            {
                batches[i].PerformFailTests();
            }
        }        
    }
}
