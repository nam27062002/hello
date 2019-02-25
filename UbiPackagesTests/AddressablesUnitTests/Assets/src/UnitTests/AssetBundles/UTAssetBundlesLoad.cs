using SimpleJSON;
using System.Collections.Generic;
using System.IO;

public class UTAssetBundlesLoad : UnitTest
{
    private Logger sm_logger = new ConsoleLogger("UTAssetBundlesLoad");

    public static UnitTestBatch GetUnitTestBatch()
    {        
        UnitTestBatch batch = new UnitTestBatch("UTAssetBundlesLoad");

        UTAssetBundlesLoad test;
        List<string> dependencies;

        //
        // SUCCESS
        //
                
        // PURPOSE: Test no catalog file
        // INPUT: 
        //      No assetBundlesCatalog.json        
        // OUTPUT: 
        //      isLocal = false;
        //      dependencies = empty
        test = new UTAssetBundlesLoad();
        dependencies = null;
        test.Setup("00", "asset_cubes", dependencies, false);
        batch.AddTest(test, true);

        // PURPOSE: Test empty catalog file
        // INPUT: 
        //      empty assetBundlesCatalog.json        
        // OUTPUT: 
        //      isLocal = false;
        //      dependencies = empty
        test = new UTAssetBundlesLoad();
        dependencies = null;
        test.Setup("01", "asset_cubes", dependencies, false);
        batch.AddTest(test, true);                

        // PURPOSE: Test a local asset bundle id is discarded if it's not defined in dependencies
        // INPUT: 
        //      LOCAL: asset_cubes
        //      DEPENDENCIES: Empty
        // OUTPUT: 
        //      isLocal = false; (asset_cubes is not a valid asset bundle because it's not present in dependencies)
        //      dependencies = empty
        test = new UTAssetBundlesLoad();
        dependencies = null;

        // checkJSON passes as false because "asset_cubes" will be discarded so catalog.ToJSON() won't be like the
        // input json and that's correct
        test.Setup("02", "asset_cubes", dependencies, false, false);
        batch.AddTest(test, true);
                
        // PURPOSE: Test empty local paragraph
        // INPUT: 
        //      LOCAL: empty
        //      DEPENDENCIES: asset_cubes:[]
        // OUTPUT: 
        //      isLocal = false; 
        //      dependencies = empty
        test = new UTAssetBundlesLoad();
        dependencies = null;
        test.Setup("03", "asset_cubes", dependencies, false);
        batch.AddTest(test, true);
                
        // PURPOSE: Simplest asset bundles catalog for local asset_cubes
        // INPUT: 
        //      LOCAL: asset_cubes
        //      DEPENDENCIES: asset_cubes:[]
        // OUTPUT: 
        //      isLocal = true; 
        //      dependencies = empty
        test = new UTAssetBundlesLoad();
        dependencies = null;
        test.Setup("04", "asset_cubes", dependencies, true);
        batch.AddTest(test, true);

        // PURPOSE: Simplest asset bundles catalog for local asset_cubes
        // INPUT: 
        //      LOCAL: asset_cubes
        //      DEPENDENCIES: "asset_cubes":["asset_materials"],
        //                    "asset_materials":["asset_textures"],
		//                    "asset_textures":["asset_colors","asset_textures_common"]
		//                    "asset_textures_common":[],
		//                    "asset_colors":[],
        // OUTPUT: 
        //      isLocal = true; 
        //      dependencies = [asset_materials, asset_textures, asset_colors, asset_textures_common]
        test = new UTAssetBundlesLoad();
        dependencies = new List<string>();
        dependencies.Add("asset_materials");
        dependencies.Add("asset_textures");
        dependencies.Add("asset_colors");
        dependencies.Add("asset_textures_common");

        test.Setup("05", "asset_cubes", dependencies, true);
        batch.AddTest(test, true);        

        // PURPOSE: An asset bundle that is a dependency of an asset bundle declared as local is considered local too
        // INPUT: 
        //      LOCAL: asset_cubes
        //      DEPENDENCIES: "asset_cubes":["asset_materials"],
        //                    "asset_materials":["asset_textures"],
        //                    "asset_textures":["asset_colors","asset_textures_common"]
        //                    "asset_textures_common":[],
        //                    "asset_colors":[],
        // OUTPUT: 
        //      isLocal = true; 
        //      dependencies = [asset_materials, asset_textures, asset_colors, asset_textures_common]
        test = new UTAssetBundlesLoad();
        dependencies = new List<string>();        
        dependencies.Add("asset_textures");
        dependencies.Add("asset_colors");
        dependencies.Add("asset_textures_common");

        test.Setup("05", "asset_materials", dependencies, true);
        batch.AddTest(test, true);                

        // PURPOSE: An asset bundle that is not defined as local is considered remote even thouth all its dependencies
        //          are local        
        // INPUT: 
        //      LOCAL: asset_materials
        //      DEPENDENCIES: "asset_cubes":["asset_materials"],
        //                    "asset_materials":["asset_textures"],
        //                    "asset_textures":["asset_colors","asset_textures_common"]
        //                    "asset_textures_common":[],
        //                    "asset_colors":[],
        // OUTPUT: 
        //      isLocal = false; 
        //      dependencies = [asset_materials, asset_textures, asset_colors, asset_textures_common]
        test = new UTAssetBundlesLoad();
        dependencies = new List<string>();
        dependencies.Add("asset_materials");
        dependencies.Add("asset_textures");
        dependencies.Add("asset_colors");
        dependencies.Add("asset_textures_common");

        test.Setup("06", "asset_cubes", dependencies, false);
        batch.AddTest(test, true);        

        // PURPOSE: An asset bundle that is not defined as local and it's not a dependency of any local asset bundle
        //          is not considered local
        // INPUT: 
        //      LOCAL: asset_materials
        //      DEPENDENCIES: "asset_cubes":[],
        //                    "asset_materials":["asset_textures"],
        //                    "asset_textures":["asset_colors","asset_textures_common"]
        //                    "asset_textures_common":[],
        //                    "asset_colors":[],
        // OUTPUT: 
        //      isLocal = false; 
        //      dependencies = []        
        test = new UTAssetBundlesLoad();
        dependencies = new List<string>();        

        test.Setup("07", "asset_cubes", dependencies, false);
        batch.AddTest(test, true);        
        
        // PURPOSE: Dealing with an asset bundle that is not defined in catalog
        // INPUT: 
        //      LOCAL: asset_materials
        //      DEPENDENCIES: "asset_cubes":[],
        //                    "asset_materials":["asset_textures"],
        //                    "asset_textures":["asset_colors","asset_textures_common"]
        //                    "asset_textures_common":[],
        //                    "asset_colors":[],
        // OUTPUT: 
        //      isLocal = false; 
        //      dependencies = []        
        test = new UTAssetBundlesLoad();
        dependencies = new List<string>();        

        test.Setup("07", "asset_cubes2", dependencies, false);
        batch.AddTest(test, true);

        //
        // FAIL
        //        
        
        // PURPOSE: Test CRC mismatch
        // INPUT: 
        //      No assetBundlesCatalog.json        
        // OUTPUT: 
        //      isLocal = true;
        //      dependencies = empty
        test = new UTAssetBundlesLoad();
        dependencies = null;
        test.Setup("00", "asset_cubes", dependencies, true);
        batch.AddTest(test, false);                

        return batch;
    }

    private string m_catalogPath;
    private string m_abId;
    private List<string> m_dependencies;
    private bool m_isLocal;
    private bool m_checkJSON;

    public void Setup(string catalogPath, string abId, List<string> dependencies, bool isLocal, bool checkJSON = true)
    {
        m_catalogPath = catalogPath;
        m_abId = abId;
        m_dependencies = dependencies;
        m_isLocal = isLocal;
        m_checkJSON = checkJSON;
    }

    protected override void ExtendedPerform()
    {
        string path = Directory.GetCurrentDirectory() + "/Assets/Editor/AssetBundles/UnitTests/" + m_catalogPath + "/assetBundlesCatalog.json";

        AssetBundlesCatalog catalog = new AssetBundlesCatalog();

        // Loads the catalog        
        JSONNode catalogJSON = null;                
        if (File.Exists(path))
        {
            StreamReader reader = new StreamReader(path);
            string content = reader.ReadToEnd();
            reader.Close();
            
            catalogJSON = JSON.Parse(content);
        }

        catalog.Load(catalogJSON, sm_logger);                

        bool passes = catalog.IsAssetBundleLocal(m_abId) == m_isLocal;
        if (passes)
        {
            List<string> dependencies = catalog.GetAllDependencies(m_abId);
            passes = UbiListUtils.Compare(m_dependencies, dependencies);

            if (passes && m_checkJSON)
            {
                if (catalogJSON == null)
                {
                    catalogJSON = new JSONClass();
                }

                if (!catalogJSON.ContainsKey("local"))
                {
                    catalogJSON.Add("local", new JSONArray());
                }

                if (!catalogJSON.ContainsKey("dependencies"))
                {
                    catalogJSON.Add("dependencies", new JSONClass());
                }

                passes = catalog.ToJSON().ToString() == catalogJSON.ToString();
            }
        }        

        NotifyPasses(passes);
    }
}
