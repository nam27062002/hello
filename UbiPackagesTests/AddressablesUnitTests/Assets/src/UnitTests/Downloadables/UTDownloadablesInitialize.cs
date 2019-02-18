using Downloadables;
using SimpleJSON;
using System.Collections.Generic;
using System.IO;

public class UTDownloadablesInitialize : UnitTest
{    
    private static string ASSET_ID = "asset_cubes";    
    private static long ASSET_CRC_ORIG = 2411361792;
    private static long ASSET_SIZE_ORIG = 96157;
    private static long ASSET_CRC_NEW = 3411361792;
    private static long ASSET_SIZE_NEW = 96157;

    private static string OTHER_ASSET_ID = "material_cubes";
    private static long OTHER_ASSET_CRC_ORIG = 2924725248;
    private static long OTHER_ASSET_SIZE_ORIG = 41583;

    /*
    private static string OUTDATED_ASSET_ID = "scene_cubes";
    private static long OUTDATED_ASSET_CRC_ORIG = 1812583168;
    private static long OUTDATED_ASSET_SIZE_ORIG = 110924;
    */

    private Logger sm_logger = new ConsoleLogger("UTDownloadablesInitialize");

    public static UnitTestBatch GetUnitTestBatch()
    {        
        UnitTestBatch batch = new UnitTestBatch("UTDownloadablesInitialize");
        DiskDriver diskDriver = new DiskDriver();

        Dictionary<string, CatalogEntryStatus> resultInMemory;
        Dictionary<string, string> resultManifests;
        Dictionary<string, string> resultDownloads;
        UTDownloadablesInitialize test;
        string cachePath;
        
        //
        // SUCCESS
        //
        
        // PURPOSE: Test empty disk folders           
        // INPUT: 
        //      MANIFESTS: Empty
        //      DOWNLOADS: Empty
        //      CATALOG: ASSET
        // OUTPUT:
        //      MANIFESTS: Empty
        //      DOWNLOADS: Empty
        //      MEMORY: ASSET manifest loaded
        test = new UTDownloadablesInitialize();
        resultInMemory = new Dictionary<string, CatalogEntryStatus>();
        AddCatalogEntryStatusToCatalog(resultInMemory, ASSET_ID, ASSET_CRC_ORIG, ASSET_SIZE_ORIG, 0, 0);

        resultManifests = new Dictionary<string, string>();
        resultDownloads = new Dictionary<string, string>();

        test.Setup(diskDriver, "01", "00", resultInMemory, resultManifests, resultDownloads);
        batch.AddTest(test, true);
                
        // PURPOSE: Test manifests and catalog are loaded correctly
        // INPUT: 
        //      MANIFESTS: Only contains ASSET manifest
        //      DOWNLOADS: Empty
        //      CATALOG: ASSET and OTHER_ASSET
        // OUTPUT: 
        //      MANIFESTS: Only contains ASSET manifest
        //      DOWNLOADS: Empty
        //      MEMORY: ASSET and OTHER_ASSET manifests loaded
        test = new UTDownloadablesInitialize();
        resultInMemory = new Dictionary<string, CatalogEntryStatus>();
        AddCatalogEntryStatusToCatalog(resultInMemory, ASSET_ID, ASSET_CRC_ORIG, ASSET_SIZE_ORIG, 0, 0);
        AddCatalogEntryStatusToCatalog(resultInMemory, OTHER_ASSET_ID, OTHER_ASSET_CRC_ORIG, OTHER_ASSET_SIZE_ORIG, 0, 0);

        resultManifests = new Dictionary<string, string>();
        AddCatalogEntryStatusToCatalog(resultManifests, OTHER_ASSET_ID, OTHER_ASSET_CRC_ORIG, OTHER_ASSET_SIZE_ORIG, 0, 0);
        
        resultDownloads = new Dictionary<string, string>();
        test.Setup(diskDriver, "02", "01", resultInMemory, resultManifests, resultDownloads);
        batch.AddTest(test, true);                

        // PURPOSE: Test outdated manifest files in disk are deleted
        // INPUT: 
        //      MANIFESTS: It only contains OUTDATED_ASSET 
        //      DOWNLOADS: Empty
        //      CATALOG: only ASSET
        // OUTPUT: 
        //      MANIFESTS: Empty
        //      DOWNLOADS: Empty
        //      MEMORY: ASSET 
        test = new UTDownloadablesInitialize();
        resultInMemory = new Dictionary<string, CatalogEntryStatus>();
        AddCatalogEntryStatusToCatalog(resultInMemory, ASSET_ID, ASSET_CRC_ORIG, ASSET_SIZE_ORIG, 0, 0);

        // Empty
        resultManifests = new Dictionary<string, string>();        

        // Empty
        resultDownloads = new Dictionary<string, string>();
        test.Setup(diskDriver, "01", "03", resultInMemory, resultManifests, resultDownloads);
        batch.AddTest(test, true);        
        
        // PURPOSE: Test only manifest outdated files in disk are deleted
        // INPUT: 
        //      MANIFESTS: It contains ASSET and OUTDATED_ASSET
        //      DOWNLOADS: Empty
        //      CATALOG: only ASSET
        // OUTPUT: 
        //      MANIFESTS: ASSET
        //      DOWNLOADS: Empty
        //      MEMORY: ASSET 
        test = new UTDownloadablesInitialize();
        resultInMemory = new Dictionary<string, CatalogEntryStatus>();
        AddCatalogEntryStatusToCatalog(resultInMemory, ASSET_ID, ASSET_CRC_ORIG, ASSET_SIZE_ORIG, 0, 0);

        resultManifests = new Dictionary<string, string>();
        AddCatalogEntryStatusToCatalog(resultManifests, ASSET_ID, ASSET_CRC_ORIG, ASSET_SIZE_ORIG, 0, 0);

        // Empty
        resultDownloads = new Dictionary<string, string>();
        test.Setup(diskDriver, "01", "04", resultInMemory, resultManifests, resultDownloads);
        batch.AddTest(test, true);                

        // PURPOSE: Test CRC mismatch
        // INPUT: 
        //      MANIFESTS: It contains ASSET (former CRC) and OUTDATED_ASSET
        //      DOWNLOADS: It constains ASSET and OUTDATED_ASSET
        //      CATALOG: only ASSET (new CRC)
        // OUTPUT: 
        //      MANIFESTS: ASSET
        //      DOWNLOADS: Empty
        //      MEMORY: ASSET 
        test = new UTDownloadablesInitialize();
        cachePath = "05";
        resultInMemory = new Dictionary<string, CatalogEntryStatus>();
        AddCatalogEntryStatusToCatalog(resultInMemory, ASSET_ID, ASSET_CRC_ORIG, ASSET_SIZE_ORIG, 0, 0);

        resultManifests = new Dictionary<string, string>();
        AddCatalogEntryStatusToCatalog(resultManifests, ASSET_ID, ASSET_CRC_ORIG, ASSET_SIZE_ORIG, 0, 0);

        resultDownloads = new Dictionary<string, string>();        
        test.Setup(diskDriver, "01", cachePath, resultInMemory, resultManifests, resultDownloads);
        batch.AddTest(test, true);        
        
        
        // PURPOSE: Test manifest gets updated an asset's CRC doesn't match the one in catalog
        // INPUT: 
        //      MANIFESTS: It only contains ASSET (outdated CRC)
        //      DOWNLOADS: It constains ASSET (outdated) and OUTDATED_ASSET
        //      CATALOG: only ASSET (new CRC)
        // OUTPUT: 
        //      MANIFESTS: ASSET (new CRC)
        //      DOWNLOADS: Empty (ASSET is deleted because it was outdated and OUTDATED_ASSET was deleted because it's not in catalog anymore)
        //      MEMORY: ASSET (new CRC) 
        test = new UTDownloadablesInitialize();
        cachePath = "05";
        resultInMemory = new Dictionary<string, CatalogEntryStatus>();

        AddCatalogEntryStatusToCatalog(resultInMemory, ASSET_ID, ASSET_CRC_NEW, ASSET_SIZE_NEW, 0, 0);

        resultManifests = new Dictionary<string, string>();
        AddCatalogEntryStatusToCatalog(resultManifests, ASSET_ID, ASSET_CRC_NEW, ASSET_SIZE_NEW,0, 0);

        resultDownloads = new Dictionary<string, string>();        
        test.Setup(diskDriver, "03", cachePath, resultInMemory, resultManifests, resultDownloads);
        batch.AddTest(test, true);                
        
        // PURPOSE: Test manifest gets updated an asset's SIZE in Downloads is bigger than the one specified in catalog
        // INPUT: 
        //      MANIFESTS: It only contains ASSET
        //      DOWNLOADS: It constains ASSET (bigger size) and OUTDATED_ASSET
        //      CATALOG: only ASSET 
        // OUTPUT: 
        //      MANIFESTS: ASSET 
        //      DOWNLOADS: Empty (ASSET is deleted because it was outdated and OUTDATED_ASSET was deleted because it's not in catalog anymore)
        //      MEMORY: ASSET 
        test = new UTDownloadablesInitialize();
        cachePath = "06";
        resultInMemory = new Dictionary<string, CatalogEntryStatus>();

        AddCatalogEntryStatusToCatalog(resultInMemory, ASSET_ID, ASSET_CRC_ORIG, ASSET_SIZE_ORIG, 0, 0);

        resultManifests = new Dictionary<string, string>();
        AddCatalogEntryStatusToCatalog(resultManifests, ASSET_ID, ASSET_CRC_ORIG, ASSET_SIZE_ORIG, 0, 0);

        resultDownloads = new Dictionary<string, string>();
        test.Setup(diskDriver, "01", cachePath, resultInMemory, resultManifests, resultDownloads);
        batch.AddTest(test, true);                
        
        //
        // FAIL               

        return batch;
    }    

    private static void AddCatalogEntryStatusToCatalog(Dictionary<string, CatalogEntryStatus> catalog, string id, long crc, long size, int numDownloads, int verified, string cachePath = null)
    {
        catalog.Add(id, GetCatalogEntryStatus(id, crc, size, numDownloads, verified, cachePath));
    }

    private static void AddCatalogEntryStatusToCatalog(Dictionary<string, string> catalog, string id, long crc, long size, int numDownloads, int verified, string cachePath = null)
    {
        catalog.Add(id, GetCatalogEntryManifestAsString(crc, size, numDownloads, verified));        
    }           
    
    private static CatalogEntryStatus GetCatalogEntryStatus(string id, long crc, long size, int numDownloads, int verified, string cachePath)
    {
        string asString = GetCatalogEntryManifestAsString(crc, size, numDownloads, verified);
        JSONNode json = JSON.Parse(asString);
        
        CatalogEntryStatus returnValue = new CatalogEntryStatus();
        returnValue.LoadManifest(id, json);
        
        if (cachePath != null)
        {
            string path = Directory.GetCurrentDirectory() + "/" + UTDownloadablesHelper.ROOT_CACHES_PATH + "/" + cachePath + "/" + Manager.DOWNLOADS_FOLDER_NAME + "/" + id;
            if (File.Exists(path))
            {
                FileInfo fileInfo = new FileInfo(path);
                returnValue.DataInfo.Size = fileInfo.Length;
            }
        }

        return returnValue;
    }
   
    private static string GetCatalogEntryManifestAsString(long crc, long size)
    {
        return @"{""crc32"":" + crc + @",""size"":" + size + "}";
    }

    private static string GetCatalogEntryManifestAsString(long crc, long size, int numDownloads, int verified)
    {
        return @"{""crc32"":" + crc + @",""size"":" + size + @",""t"":" + numDownloads + @",""v"":" + verified + "}";
    }

    private Manager m_manager;

    private string m_catalogPath;
    private string m_cachePath;
    private Dictionary<string, CatalogEntryStatus> m_resultInMemory;
    private Dictionary<string, string> m_resultManifests;
    private Dictionary<string, string> m_resultDownloads;    

    public void Setup(DiskDriver diskDriver, string catalogPath, string cachePath, Dictionary<string, CatalogEntryStatus> resultInMemory, Dictionary<string, string> resultManifests, Dictionary<string, string> resultDownloads)
    {
        m_catalogPath = catalogPath;
        m_cachePath = cachePath;
        m_manager = new Manager(diskDriver, OnDiskIssue, sm_logger);
        m_resultInMemory = resultInMemory;
        m_resultManifests = resultManifests;
        m_resultDownloads = resultDownloads;        
    }

    private void OnDiskIssue(Error.EType type)
    {
        sm_logger.LogError("DiskIssue = " + type.ToString());
    }

    protected override void ExtendedPerform()
    {
        // Copy cache 
        UTDownloadablesHelper.PrepareCache(m_cachePath);        
        string path = UTDownloadablesHelper.ROOT_CATALOGS_PATH + "/" + m_catalogPath + "/downloadablesCatalog.json";

        // Loads the catalog        
        StreamReader reader = new StreamReader(path);
        string content = reader.ReadToEnd();
        reader.Close();

        JSONNode catalogJSON = JSON.Parse(content);        
        m_manager.Initialize(catalogJSON);        
    }

    private void OnDone()
    {        
        Dictionary<string, CatalogEntryStatus> managerCatalog = m_manager.Catalog_GetEntryStatusList();
        bool passes = CompareCatalogs(managerCatalog, m_resultInMemory);
        if (passes)
        {
            passes = CheckDisk(Manager.MANIFESTS_ROOT_PATH, m_resultManifests, true);
            if (passes)
            {
                passes = CheckDisk(Manager.DOWNLOADS_ROOT_PATH, m_resultDownloads, false);
            }
        }                    

        NotifyPasses(passes);
    }

    private bool CompareCatalogs(Dictionary<string, CatalogEntryStatus> c1, Dictionary<string, CatalogEntryStatus> c2)
    {
        bool returnValue = true;
        returnValue = c1.Keys.Count == c2.Keys.Count;
        if (returnValue)
        {
            CatalogEntryStatus e1, e2;
            foreach (KeyValuePair<string, CatalogEntryStatus> pair in c1)
            {
                if (c2.ContainsKey(pair.Key))
                {
                    e1 = pair.Value;
                    e2 = c2[pair.Key];
                    if (!e1.Compare(e2))
                    {
                        returnValue = false;
                        break;
                    }
                }
                else
                {
                    returnValue = false;
                    break;
                }
            }
        }

        return returnValue;
    }  

    private bool CheckDisk(string path, Dictionary<string, string> result, bool compareContent = true)
    {
        bool returnValue = true;

        List<string> files;

        // Get files in path
        if (System.IO.Directory.Exists(path))
        {
            files = new List<string>(System.IO.Directory.GetFiles(path));
        }
        else
        {
            files = new List<string>();
        }

        Dictionary<string, bool> ids = new Dictionary<string, bool>();
        int count = files.Count;
        string id;
        for (int i = 0; i < count && returnValue; i++)
        {
            id = Path.GetFileNameWithoutExtension(files[i]);
            if (result.ContainsKey(id))
            {
                ids.Add(id, true);
                if (compareContent)
                {
                    string content = File.ReadAllText(files[i]);
                    returnValue = (content.Equals(result[id]));
                }
            }
            else
            {
                returnValue = false;
            }
        }

        if (returnValue)
        {
            foreach (KeyValuePair<string, string> pair in result)
            {
                if (!ids.ContainsKey(pair.Key))
                {
                    returnValue = false;
                    break;
                }
            }
        }

        return returnValue;
    }

    public override void Update()
    {
        if (HasStarted())
        {
            m_manager.Update();

            if (UnityEngine.Time.realtimeSinceStartup - m_timeStartAt > 3f)
            {
                OnDone();
            }
        }
    }
}
