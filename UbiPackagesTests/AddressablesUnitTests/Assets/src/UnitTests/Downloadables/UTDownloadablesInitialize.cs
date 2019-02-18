using Downloadables;
using SimpleJSON;
using System.Collections.Generic;
using System.IO;

public class UTDownloadablesInitialize : UnitTest
{
    public static void CreateDirectory(string path)
    {
        if (!string.IsNullOrEmpty(path) && !System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }
    }

    public static void CopyDirectory(string srcPath, string dstPath)
    {       
        if (System.IO.Directory.Exists(srcPath))
        {
            if (!System.IO.Directory.Exists(dstPath))
            {
                CreateDirectory(dstPath);
            }

            string[] files = System.IO.Directory.GetFiles(srcPath);

            string fileName;
            string dstFile;

            // Copies the files and overwrite destination files if they already exist.           
            foreach (string s in files)
            {
                // Uses static Path methods to extract only the file name from the path.
                fileName = Path.GetFileName(s);
                dstFile = dstPath + "/" + fileName;

                if (Path.GetExtension(fileName) != ".meta")
                {
                    try
                    {
                        File.Copy(s, dstFile, true);
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogError(e.Message);
                    }
                }
            }

            // Copies the subdirectories            
            string dstSubdirectoryPath;
            string[] subdirectories = System.IO.Directory.GetDirectories(srcPath);
            foreach (string s in subdirectories)
            {
                // Uses static Path methods to extract only the file name from the path.
                fileName = Path.GetFileName(s);
                dstSubdirectoryPath = dstPath + "/" + fileName;
                CopyDirectory(s, dstSubdirectoryPath);
            }
        }
    }

    private static string ROOT_PATH = "Assets/Editor/Downloadables/UnitTests";
    private static string ROOT_CATALOGS_PATH = ROOT_PATH + "/" + "Catalogs";
    private static string ROOT_CACHES_PATH = ROOT_PATH + "/" + "Caches";    

    private static string ASSET_ID = "asset_cubes";    
    private static long ASSET_CRC_ORIG = 2411361792;
    private static long ASSET_SIZE_ORIG = 96157;
    private static long ASSET_CRC_NEW = 3411361792;
    private static long ASSET_SIZE_NEW = 96157;

    private static string OTHER_ASSET_ID = "material_cubes";
    private static long OTHER_ASSET_CRC_ORIG = 2924725248;
    private static long OTHER_ASSET_SIZE_ORIG = 41583;

    private static string OUTDATED_ASSET_ID = "scene_cubes";
    private static long OUTDATED_ASSET_CRC_ORIG = 1812583168;
    private static long OUTDATED_ASSET_SIZE_ORIG = 110924;

    private Logger sm_logger = new ConsoleLogger("UTDownloadablesInitialize");

    public static UnitTestBatch GetUnitTestBatch()
    {        
        UnitTestBatch batch = new UnitTestBatch("UTDownloadablesInitialize");
        DiskDriver diskDriver = new DiskDriver();

        Dictionary<string, EntryStatus> resultInMemory;
        Dictionary<string, string> resultManifests;
        Dictionary<string, string> resultDownloads;
        UTDownloadablesInitialize test;
        string cachePath;
        
        //
        // SUCCESS
        //

        /*
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
        resultInMemory = new Dictionary<string, EntryStatus>();
        AddEntryStatusToCatalog(resultInMemory, ASSET_ID, ASSET_CRC_ORIG, ASSET_SIZE_ORIG, 0, 0);

        resultManifests = new Dictionary<string, string>();
        resultDownloads = new Dictionary<string, string>();

        test.Setup(diskDriver, "01", "00", resultInMemory, resultManifests, resultDownloads, Error.EType.None);
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
        resultInMemory = new Dictionary<string, EntryStatus>();
        AddEntryStatusToCatalog(resultInMemory, ASSET_ID, ASSET_CRC_ORIG, ASSET_SIZE_ORIG, 0, 0);
        AddEntryStatusToCatalog(resultInMemory, OTHER_ASSET_ID, OTHER_ASSET_CRC_ORIG, OTHER_ASSET_SIZE_ORIG, 0, 0);

        resultManifests = new Dictionary<string, string>();
        AddEntryStatusToCatalog(resultManifests, OTHER_ASSET_ID, OTHER_ASSET_CRC_ORIG, OTHER_ASSET_SIZE_ORIG, 0, 0);
        
        resultDownloads = new Dictionary<string, string>();
        test.Setup(diskDriver, "02", "01", resultInMemory, resultManifests, resultDownloads, Error.EType.None);
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
        resultInMemory = new Dictionary<string, EntryStatus>();
        AddEntryStatusToCatalog(resultInMemory, ASSET_ID, ASSET_CRC_ORIG, ASSET_SIZE_ORIG, 0, 0);

        // Empty
        resultManifests = new Dictionary<string, string>();        

        // Empty
        resultDownloads = new Dictionary<string, string>();
        test.Setup(diskDriver, "01", "03", resultInMemory, resultManifests, resultDownloads, Error.EType.None);
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
        resultInMemory = new Dictionary<string, EntryStatus>();
        AddEntryStatusToCatalog(resultInMemory, ASSET_ID, ASSET_CRC_ORIG, ASSET_SIZE_ORIG, 0, 0);

        resultManifests = new Dictionary<string, string>();
        AddEntryStatusToCatalog(resultManifests, ASSET_ID, ASSET_CRC_ORIG, ASSET_SIZE_ORIG, 0, 0);

        // Empty
        resultDownloads = new Dictionary<string, string>();
        test.Setup(diskDriver, "01", "04", resultInMemory, resultManifests, resultDownloads, Error.EType.None);
        batch.AddTest(test, true);                

        // PURPOSE: Test only manifest and downloaded outdated files in disk are deleted
        // INPUT: 
        //      MANIFESTS: It contains ASSET and OUTDATED_ASSET
        //      DOWNLOADS: It constains ASSET and OUTDATED_ASSET
        //      CATALOG: only ASSET
        // OUTPUT: 
        //      MANIFESTS: ASSET
        //      DOWNLOADS: ASSET
        //      MEMORY: ASSET 
        test = new UTDownloadablesInitialize();
        cachePath = "05";
        resultInMemory = new Dictionary<string, EntryStatus>();
        AddEntryStatusToCatalog(resultInMemory, ASSET_ID, ASSET_CRC_ORIG, ASSET_SIZE_ORIG, 0, 0, cachePath);

        resultManifests = new Dictionary<string, string>();
        AddEntryStatusToCatalog(resultManifests, ASSET_ID, ASSET_CRC_ORIG, ASSET_SIZE_ORIG, 0, 0);

        resultDownloads = new Dictionary<string, string>();
        AddEntryStatusToCatalog(resultDownloads, ASSET_ID, ASSET_CRC_ORIG, ASSET_SIZE_ORIG, 0, 0);
        test.Setup(diskDriver, "01", cachePath, resultInMemory, resultManifests, resultDownloads, Error.EType.None);
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
        resultInMemory = new Dictionary<string, EntryStatus>();
        
        AddEntryStatusToCatalog(resultInMemory, ASSET_ID, ASSET_CRC_NEW, ASSET_SIZE_NEW, 0, 0, cachePath);

        resultManifests = new Dictionary<string, string>();
        AddEntryStatusToCatalog(resultManifests, ASSET_ID, ASSET_CRC_NEW, ASSET_SIZE_NEW,0, 0);

        resultDownloads = new Dictionary<string, string>();
        AddEntryStatusToCatalog(resultDownloads, ASSET_ID, ASSET_CRC_NEW, ASSET_SIZE_NEW, 0, 0);
        test.Setup(diskDriver, "03", cachePath, resultInMemory, resultManifests, resultDownloads, Error.EType.None);
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
        resultInMemory = new Dictionary<string, EntryStatus>();

        AddEntryStatusToCatalog(resultInMemory, ASSET_ID, ASSET_CRC_ORIG, ASSET_SIZE_ORIG, 0, 0);

        resultManifests = new Dictionary<string, string>();
        AddEntryStatusToCatalog(resultManifests, ASSET_ID, ASSET_CRC_ORIG, ASSET_SIZE_ORIG, 0, 0);

        resultDownloads = new Dictionary<string, string>();        
        test.Setup(diskDriver, "01", cachePath, resultInMemory, resultManifests, resultDownloads, Error.EType.None);
        batch.AddTest(test, true);        
        */

        /*
        //
        // FAIL
        //        
        // PURPOSE: Test Initialize Error
        // INPUT:
        //      MANIFESTS: Empty
        //      DOWNLOADS: Empty
        //      CATALOG: ASSET
        // OUTPUT:
        //      MANIFESTS: Empty
        //      DOWNLOADS: Empty
        //      MEMORY: ASSET manifest
        //      INITIALIZE_ERROR: None != Disk_IOException
        test = new UTDownloadablesInitialize();
        resultInMemory = new Dictionary<string, EntryStatus>();
        AddEntryStatusToCatalog(resultInMemory, ASSET_ID, ASSET_CRC_ORIG, ASSET_SIZE_ORIG, 0, 0);

        resultManifests = new Dictionary<string, string>();
        resultDownloads = new Dictionary<string, string>();

        test.Setup(diskDriver, "01", "00", resultInMemory, resultManifests, resultDownloads, Error.EType.Disk_IOException);
        batch.AddTest(test, false);   
        */                 

        return batch;
    }    

    private static void AddEntryStatusToCatalog(Dictionary<string, EntryStatus> catalog, string id, long crc, long size, int numDownloads, int verified, string cachePath = null)
    {
        catalog.Add(id, GetEntryStatus(id, crc, size, numDownloads, verified, cachePath));
    }

    private static void AddEntryStatusToCatalog(Dictionary<string, string> catalog, string id, long crc, long size, int numDownloads, int verified, string cachePath = null)
    {
        catalog.Add(id, GetEntryStatusAsString(crc, size, numDownloads, verified));        
    }           
    
    private static EntryStatus GetEntryStatus(string id, long crc, long size, int numDownloads, int verified, string cachePath)
    {
        string asString = GetEntryStatusAsString(crc, size, numDownloads, verified);
        JSONNode json = JSON.Parse(asString);
        
        EntryStatus returnValue = new EntryStatus();
        returnValue.Load(json, null);
        
        if (cachePath != null)
        {
            string path = System.IO.Directory.GetCurrentDirectory() + "/" + ROOT_CACHES_PATH + "/" + cachePath + "/" + Manager.DOWNLOADS_FOLDER_NAME + "/" + id;
            if (File.Exists(path))
            {
                FileInfo fileInfo = new FileInfo(path);
                returnValue.DataEntry.Size = fileInfo.Length;
            }
        }

        return returnValue;
    }
   
    private static string GetEntryStatusAsString(long crc, long size)
    {
        return @"{""crc32"":" + crc + @",""size"":" + size + "}";
    }

    private static string GetEntryStatusAsString(long crc, long size, int numDownloads, int verified)
    {
        return @"{""crc32"":" + crc + @",""size"":" + size + @",""t"":" + numDownloads + @",""v"":" + verified + "}";
    }

    private Manager m_manager;

    private string m_catalogPath;
    private string m_cachePath;
    private Dictionary<string, EntryStatus> m_resultInMemory;
    private Dictionary<string, string> m_resultManifests;
    private Dictionary<string, string> m_resultDownloads;
    private Error.EType m_initializeErrorType;

    public void Setup(DiskDriver diskDriver, string catalogPath, string cachePath, Dictionary<string, EntryStatus> resultInMemory, Dictionary<string, string> resultManifests, Dictionary<string, string> resultDownloads, Error.EType initializeErrorType)
    {
        m_catalogPath = catalogPath;
        m_cachePath = cachePath;
        m_manager = new Manager(diskDriver, sm_logger);
        m_resultInMemory = resultInMemory;
        m_resultManifests = resultManifests;
        m_resultDownloads = resultDownloads;
        m_initializeErrorType = initializeErrorType;
    }

    protected override void ExtendedPerform()
    {
        // Copy cache 
        string downloadablesFolder = Manager.DOWNLOADABLES_FOLDER_NAME;
        string srcCachePath = System.IO.Directory.GetCurrentDirectory() + "/" + ROOT_CACHES_PATH + "/" +  m_cachePath;
        string dstCachePath = System.IO.Directory.GetCurrentDirectory() + "/" + Manager.DESKTOP_DEVICE_STORAGE_PATH_SIMULATED;
        FileUtils.RemoveDirectoryInDeviceStorage(downloadablesFolder, Manager.DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);
        CopyDirectory(srcCachePath, dstCachePath);

        string path = ROOT_CATALOGS_PATH + "/" + m_catalogPath + "/downloadablesCatalog.json";

        // Loads the catalog        
        StreamReader reader = new StreamReader(path);
        string content = reader.ReadToEnd();
        reader.Close();

        JSONNode catalogJSON = JSON.Parse(content);
        Catalog catalog = new Catalog();
        catalog.Load(catalogJSON, sm_logger);

        m_manager.Initialize(catalogJSON, OnInitialized);        
    }

    private void OnInitialized(Error error)
    {
        bool passes = ((error == null && m_initializeErrorType == Error.EType.None) || (error != null && error.Type == m_initializeErrorType));
        
        if (passes)
        {
            Dictionary<string, EntryStatus> managerCatalog = m_manager.CatalogStatus_GetCatalog();
            passes = CompareCatalogs(managerCatalog, m_resultInMemory);
            if (passes)
            {
                passes = CheckDisk(Manager.MANIFESTS_ROOT_PATH, m_resultManifests, true);
                if (passes)
                {
                    passes = CheckDisk(Manager.DOWNLOADS_ROOT_PATH, m_resultDownloads, false);
                }
            }            
        }

        NotifyPasses(passes);
    }

    private bool CompareCatalogs(Dictionary<string, EntryStatus> c1, Dictionary<string, EntryStatus> c2)
    {
        bool returnValue = true;
        returnValue = c1.Keys.Count == c2.Keys.Count;
        if (returnValue)
        {
            EntryStatus e1, e2;
            foreach (KeyValuePair<string, EntryStatus> pair in c1)
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
}
