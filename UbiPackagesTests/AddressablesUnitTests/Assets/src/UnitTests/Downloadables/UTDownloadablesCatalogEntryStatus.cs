using Downloadables;
using SimpleJSON;
using System.Collections.Generic;

public class UTDownloadablesCatalogEntryStatus : UnitTest
{
    private static long ASSET_CUBES_CRC = 2411361773;
    private static long ASSET_CUBES_NEW_CRC = 3411361773;
    private static long ASSET_CUBES_SIZE = 96157;

    public static UnitTestBatch GetUnitTestBatch()
    {
        UnitTestBatch batch = new UnitTestBatch("UTDownloadablesCatalogEntryStatus");
        MockDiskDriver diskDriver = new MockDiskDriver();
        JSONNode jsonCatalogEntry;
        JSONNode resultJsonCatalogEntry;
        List<string> resultFilesInManifests;
        List<string> resultFilesInDownloads;        
        string assetId = "asset_cubes";
        MockDiskDriver.ExceptionConf exceptionConf;
        CatalogEntryStatus.TIME_TO_WAIT_AFTER_ERROR = 1f;
        CatalogEntryStatus.TIME_TO_WAIT_BETWEEN_SAVES = 1f;

        UTDownloadablesCatalogEntryStatus test;

        //
        // SUCCESS
        //        
        
        // PURPOSE: Test READING_MANIFEST state: no data in cache
        // INPUT: 
        //      MANIFESTS: Empty
        //      DOWNLOADS: Empty
        //      FILE: asset_cubes
        // OUTPUT: 
        //      MANIFESTS: Empty
        //      DOWNLOADS: Empty
        test = new UTDownloadablesCatalogEntryStatus();

        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(ASSET_CUBES_CRC, ASSET_CUBES_SIZE, 0, false);

        resultFilesInManifests = new List<string>();
        resultFilesInDownloads = new List<string>();

        test.Setup(diskDriver, "00", assetId, jsonCatalogEntry, 
            CatalogEntryStatus.EState.ReadingManifest, CatalogEntryStatus.EState.ReadingDataInfo,
            null, 0, jsonCatalogEntry, resultFilesInManifests, resultFilesInDownloads);

        batch.AddTest(test, true);
                
        // PURPOSE: Test READING_MANIFEST: data in cache
        // INPUT: 
        //      MANIFESTS: assetId
        //      DOWNLOADS: assetId
        //      FILE: assetId (same CRC and SIZE as the one in MANIFESTS)
        // OUTPUT: 
        //      MANIFESTS: assetId
        //      DOWNLOADS: assetId
        test = new UTDownloadablesCatalogEntryStatus();

        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(ASSET_CUBES_CRC, ASSET_CUBES_SIZE, 0, false);

        resultFilesInManifests = new List<string>();
        resultFilesInManifests.Add(assetId);

        resultFilesInDownloads = new List<string>();
        resultFilesInDownloads.Add(assetId);

        test.Setup(diskDriver, "asset_cubes_orig", assetId, jsonCatalogEntry,
            CatalogEntryStatus.EState.ReadingManifest, CatalogEntryStatus.EState.ReadingDataInfo,
            null, 0f, jsonCatalogEntry, resultFilesInManifests, resultFilesInDownloads);

        batch.AddTest(test, true);        

        // PURPOSE: Test READING_MANIFEST: data in cache and exception when asking if the manifest exists in disk
        // INPUT: 
        //      MANIFESTS: assetId
        //      DOWNLOADS: assetId
        //      FILE: assetId (same CRC and SIZE as the one in MANIFESTS)
        // OUTPUT: 
        //      MANIFESTS: assetId
        //      DOWNLOADS: assetId
        test = new UTDownloadablesCatalogEntryStatus();

        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(ASSET_CUBES_CRC, ASSET_CUBES_SIZE, 0, false);
        exceptionConf = new MockDiskDriver.ExceptionConf(MockDiskDriver.EOp.File_Exists, Manager.MANIFESTS_ROOT_PATH + "/" + assetId, MockDiskDriver.EExceptionType.UnauthorizedAccess);

        resultFilesInManifests = new List<string>();
        resultFilesInManifests.Add(assetId);

        resultFilesInDownloads = new List<string>();
        resultFilesInDownloads.Add(assetId);

        test.Setup(diskDriver, "asset_cubes_orig", assetId, jsonCatalogEntry,
            CatalogEntryStatus.EState.ReadingManifest, CatalogEntryStatus.EState.ReadingDataInfo,
            exceptionConf, 3f, jsonCatalogEntry, resultFilesInManifests, resultFilesInDownloads);

        batch.AddTest(test, true);        

        // PURPOSE: Test READING_MANIFEST: data in cache and exception when retrieving the manifest in disk
        // INPUT: 
        //      MANIFESTS: assetId
        //      DOWNLOADS: assetId
        //      FILE: assetId (same CRC and SIZE as the one in MANIFESTS)
        // OUTPUT: 
        //      MANIFESTS: assetId
        //      DOWNLOADS: assetId
        test = new UTDownloadablesCatalogEntryStatus();

        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(ASSET_CUBES_CRC, ASSET_CUBES_SIZE, 0, false);
        exceptionConf = new MockDiskDriver.ExceptionConf(MockDiskDriver.EOp.File_ReadAllText, Manager.MANIFESTS_ROOT_PATH + "/" + assetId, MockDiskDriver.EExceptionType.UnauthorizedAccess);

        resultFilesInManifests = new List<string>();
        resultFilesInManifests.Add(assetId);

        resultFilesInDownloads = new List<string>();
        resultFilesInDownloads.Add(assetId);

        test.Setup(diskDriver, "asset_cubes_orig", assetId, jsonCatalogEntry,
            CatalogEntryStatus.EState.ReadingManifest, CatalogEntryStatus.EState.ReadingDataInfo,
            exceptionConf, 10f, jsonCatalogEntry, resultFilesInManifests, resultFilesInDownloads);

        batch.AddTest(test, true);        

        // PURPOSE: Test READING_MANIFEST: data in cache is outdated
        // INPUT: 
        //      MANIFESTS: assetId
        //      DOWNLOADS: assetId
        //      FILE: assetId (different CRC as the one in MANIFESTS)
        // OUTPUT: 
        //      MANIFESTS: assetId (with the new CRC)
        //      DOWNLOADS: Empty
        test = new UTDownloadablesCatalogEntryStatus();

        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(ASSET_CUBES_NEW_CRC, ASSET_CUBES_SIZE, 0, false);
        exceptionConf = new MockDiskDriver.ExceptionConf(MockDiskDriver.EOp.File_ReadAllText, Manager.MANIFESTS_ROOT_PATH + "/" + assetId, MockDiskDriver.EExceptionType.UnauthorizedAccess);

        resultFilesInManifests = new List<string>();
        resultFilesInManifests.Add(assetId);

        resultFilesInDownloads = new List<string>();        

        test.Setup(diskDriver, "asset_cubes_orig", assetId, jsonCatalogEntry,
            CatalogEntryStatus.EState.ReadingManifest, CatalogEntryStatus.EState.ReadingDataInfo,
            null, 0f, jsonCatalogEntry, resultFilesInManifests, resultFilesInDownloads);

        batch.AddTest(test, true);        

        // PURPOSE: Test READING_MANIFEST: data in cache is outdated and exception when searching for the downloaded file
        // INPUT: 
        //      MANIFESTS: assetId
        //      DOWNLOADS: assetId
        //      FILE: assetId (same CRC and SIZE as the one in MANIFESTS)
        // OUTPUT: 
        //      MANIFESTS: assetId (with the new CRC)
        //      DOWNLOADS: Empty
        test = new UTDownloadablesCatalogEntryStatus();

        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(ASSET_CUBES_NEW_CRC, ASSET_CUBES_SIZE, 0, false);
        exceptionConf = new MockDiskDriver.ExceptionConf(MockDiskDriver.EOp.File_Exists, Manager.DOWNLOADS_ROOT_PATH + "/" + assetId, MockDiskDriver.EExceptionType.UnauthorizedAccess);

        resultFilesInManifests = new List<string>();
        resultFilesInManifests.Add(assetId);

        resultFilesInDownloads = new List<string>();        

        test.Setup(diskDriver, "asset_cubes_orig", assetId, jsonCatalogEntry,
            CatalogEntryStatus.EState.ReadingManifest, CatalogEntryStatus.EState.ReadingDataInfo,
            exceptionConf, 10f, jsonCatalogEntry, resultFilesInManifests, resultFilesInDownloads);

        batch.AddTest(test, true);

        // PURPOSE: Test READING_MANIFEST: data in cache is outdated and exception when deleting the downloaded file
        // INPUT: 
        //      MANIFESTS: assetId
        //      DOWNLOADS: assetId
        //      FILE: assetId (same CRC and SIZE as the one in MANIFESTS)
        // OUTPUT: 
        //      MANIFESTS: assetId (with the new CRC)
        //      DOWNLOADS: Empty
        test = new UTDownloadablesCatalogEntryStatus();

        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(ASSET_CUBES_NEW_CRC, ASSET_CUBES_SIZE, 0, false);
        exceptionConf = new MockDiskDriver.ExceptionConf(MockDiskDriver.EOp.File_Delete, Manager.DOWNLOADS_ROOT_PATH + "/" + assetId, MockDiskDriver.EExceptionType.UnauthorizedAccess);

        resultFilesInManifests = new List<string>();
        resultFilesInManifests.Add(assetId);

        resultFilesInDownloads = new List<string>();

        test.Setup(diskDriver, "asset_cubes_orig", assetId, jsonCatalogEntry,
            CatalogEntryStatus.EState.ReadingManifest, CatalogEntryStatus.EState.ReadingDataInfo,
            exceptionConf, 3f, jsonCatalogEntry, resultFilesInManifests, resultFilesInDownloads);

        batch.AddTest(test, true);        
        
        // PURPOSE: Test READING_DATA_INFO: no data in cache
        // INPUT: 
        //      MANIFESTS: Empty
        //      DOWNLOADS: Empty
        //      FILE: assetId
        // OUTPUT: 
        //      MANIFESTS: Empty
        //      DOWNLOADS: Empty
        test = new UTDownloadablesCatalogEntryStatus();

        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(ASSET_CUBES_CRC, ASSET_CUBES_SIZE, 0, false);        
        exceptionConf = null;

        resultFilesInManifests = new List<string>();        
        resultFilesInDownloads = new List<string>();

        test.Setup(diskDriver, "00", assetId, jsonCatalogEntry, 
            CatalogEntryStatus.EState.ReadingDataInfo, CatalogEntryStatus.EState.InQueueForDownload,
            exceptionConf, 3f, jsonCatalogEntry, resultFilesInManifests, resultFilesInDownloads);

        batch.AddTest(test, true);                

        // PURPOSE: Test READING_DATA_INFO: no data in cache and exception when searching for data
        // INPUT: 
        //      MANIFESTS: Empty
        //      DOWNLOADS: Empty
        //      FILE: assetId
        // OUTPUT: 
        //      MANIFESTS: Empty
        //      DOWNLOADS: Empty
        test = new UTDownloadablesCatalogEntryStatus();

        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(ASSET_CUBES_CRC, ASSET_CUBES_SIZE, 0, false);
        exceptionConf = new MockDiskDriver.ExceptionConf(MockDiskDriver.EOp.File_Exists, Manager.DOWNLOADS_ROOT_PATH + "/" + assetId, MockDiskDriver.EExceptionType.UnauthorizedAccess);        

        resultFilesInManifests = new List<string>();
        resultFilesInDownloads = new List<string>();

        test.Setup(diskDriver, "00", assetId, jsonCatalogEntry,
            CatalogEntryStatus.EState.ReadingDataInfo, CatalogEntryStatus.EState.InQueueForDownload,
            exceptionConf, 3f, jsonCatalogEntry, resultFilesInManifests, resultFilesInDownloads);

        batch.AddTest(test, true);        

        // PURPOSE: Test READING_DATA_INFO: full download data in cache
        // INPUT: 
        //      MANIFESTS: asset_cubes (with verified = false)
        //      DOWNLOADS: asset_cubes
        //      FILE: assetId
        // OUTPUT: 
        //      MANIFESTS: asset_cubes (with verified = true)
        //      DOWNLOADS: asset_cubes
        test = new UTDownloadablesCatalogEntryStatus();

        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(ASSET_CUBES_CRC, ASSET_CUBES_SIZE, 0, false);        
        resultJsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(ASSET_CUBES_CRC, ASSET_CUBES_SIZE, 0, true);
        exceptionConf = null;

        resultFilesInManifests = new List<string>();
        resultFilesInManifests.Add(assetId);

        resultFilesInDownloads = new List<string>();
        resultFilesInDownloads.Add(assetId);

        test.Setup(diskDriver, "asset_cubes_orig", assetId, jsonCatalogEntry,
            CatalogEntryStatus.EState.ReadingDataInfo, CatalogEntryStatus.EState.Available,
            exceptionConf, 3f, resultJsonCatalogEntry, resultFilesInManifests, resultFilesInDownloads);

        batch.AddTest(test, true);         

        // PURPOSE: Test READING_DATA_INFO: full download data in cache, CRC mismatch
        // INPUT: 
        //      MANIFESTS: asset_cubes (with old CRC and verified = false)
        //      DOWNLOADS: asset_cubes
        //      FILE: assetId
        // OUTPUT: 
        //      MANIFESTS: asset_cubes (with verified = false and new CRC)
        //      DOWNLOADS
        test = new UTDownloadablesCatalogEntryStatus();

        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(ASSET_CUBES_NEW_CRC, ASSET_CUBES_SIZE, 0, false);
        resultJsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(ASSET_CUBES_NEW_CRC, ASSET_CUBES_SIZE, 0, false);
        exceptionConf = null;

        resultFilesInManifests = new List<string>();
        resultFilesInManifests.Add(assetId);

        resultFilesInDownloads = new List<string>();        

        test.Setup(diskDriver, "asset_cubes_orig", assetId, jsonCatalogEntry,
            CatalogEntryStatus.EState.ReadingDataInfo, CatalogEntryStatus.EState.InQueueForDownload,
            exceptionConf, 3f, resultJsonCatalogEntry, resultFilesInManifests, resultFilesInDownloads);

        batch.AddTest(test, true);        

        // PURPOSE: Test READING_DATA_INFO: full download data in cache, CRC mismatch
        // INPUT: 
        //      MANIFESTS: asset_cubes (with old CRC and verified = true)
        //      DOWNLOADS: asset_cubes
        //      FILE: assetId
        // OUTPUT: 
        //      MANIFESTS: asset_cubes (with verified = false and new CRC)
        //      DOWNLOADS
        test = new UTDownloadablesCatalogEntryStatus();

        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(ASSET_CUBES_NEW_CRC, ASSET_CUBES_SIZE, 0, true);
        resultJsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(ASSET_CUBES_NEW_CRC, ASSET_CUBES_SIZE, 0, false);
        exceptionConf = null;

        resultFilesInManifests = new List<string>();
        resultFilesInManifests.Add(assetId);

        resultFilesInDownloads = new List<string>();

        test.Setup(diskDriver, "asset_cubes_orig_verified", assetId, jsonCatalogEntry,
            CatalogEntryStatus.EState.ReadingDataInfo, CatalogEntryStatus.EState.InQueueForDownload,
            exceptionConf, 3f, resultJsonCatalogEntry, resultFilesInManifests, resultFilesInDownloads);

        batch.AddTest(test, true);        

        // PURPOSE: Test READING_DATA_INFO: full download data in cache, too big size
        // INPUT: 
        //      MANIFESTS: asset_cubes (verified = true)
        //      DOWNLOADS: asset_cubes (bigger than the size in manifest)
        //      FILE: assetId
        // OUTPUT: 
        //      MANIFESTS: asset_cubes (with verified = false)
        //      DOWNLOADS
        test = new UTDownloadablesCatalogEntryStatus();

        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(ASSET_CUBES_CRC, ASSET_CUBES_SIZE, 0, true);
        resultJsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(ASSET_CUBES_CRC, ASSET_CUBES_SIZE, 0, false);
        exceptionConf = null;

        resultFilesInManifests = new List<string>();
        resultFilesInManifests.Add(assetId);

        resultFilesInDownloads = new List<string>();

        test.Setup(diskDriver, "asset_cubes_orig_verified_too_big", assetId, jsonCatalogEntry,
            CatalogEntryStatus.EState.ReadingDataInfo, CatalogEntryStatus.EState.InQueueForDownload,
            exceptionConf, 3f, resultJsonCatalogEntry, resultFilesInManifests, resultFilesInDownloads);

        batch.AddTest(test, true);

        // PURPOSE: Test READING_DATA_INFO: full download data in cache, smaller size
        // INPUT: 
        //      MANIFESTS: asset_cubes (verified = true)
        //      DOWNLOADS: asset_cubes (smaller than the size in manifest)
        //      FILE: assetId
        // OUTPUT: 
        //      MANIFESTS: asset_cubes (with verified = true)
        //      DOWNLOADS
        test = new UTDownloadablesCatalogEntryStatus();

        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(ASSET_CUBES_CRC, ASSET_CUBES_SIZE, 0, true);
        resultJsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(ASSET_CUBES_CRC, ASSET_CUBES_SIZE, 0, true);
        exceptionConf = null;

        resultFilesInManifests = new List<string>();
        resultFilesInManifests.Add(assetId);

        resultFilesInDownloads = new List<string>();
        resultFilesInDownloads.Add(assetId);

        test.Setup(diskDriver, "asset_cubes_orig_verified_smaller", assetId, jsonCatalogEntry,
            CatalogEntryStatus.EState.ReadingDataInfo, CatalogEntryStatus.EState.InQueueForDownload,
            exceptionConf, 3f, resultJsonCatalogEntry, resultFilesInManifests, resultFilesInDownloads);

        batch.AddTest(test, true);
        return batch;
    }
    
    private MockDiskDriver m_diskDriver;
    private string m_cacheFolder;
    private CatalogEntryStatus m_entryStatus;    
    private CatalogEntryStatus.EState m_stateToTest;
    private CatalogEntryStatus.EState m_stateToExit;
    private bool m_testReallyStarted;
    private string m_resultManifest;
    private List<string> m_resultFilesInManifests;
    private List<string> m_resultFilesInDownloads;
    private MockDiskDriver.ExceptionConf m_exceptionConfAtStart;
    private float m_disableExceptionAt;
    private float m_timeReallyStartedAt;

    public void Setup(MockDiskDriver diskDriver, string cacheFolder, string id, JSONNode catalogJSON, 
        CatalogEntryStatus.EState stateToTest, CatalogEntryStatus.EState stateToExit,
        MockDiskDriver.ExceptionConf exceptionConfAtStart, float disableExceptionAt,
        JSONNode resultManifestJSON, List<string> resultManifests, List<string> resultDownloads)
    {
        Disk disk = new Disk(diskDriver, Manager.MANIFESTS_ROOT_PATH, Manager.DOWNLOADS_ROOT_PATH, 0, null);
        m_diskDriver = diskDriver;
        m_cacheFolder = cacheFolder;

        CatalogEntryStatus.sm_disk = disk;
        m_entryStatus = new CatalogEntryStatus();
        m_entryStatus.LoadManifest(id, catalogJSON);

        m_stateToTest = stateToTest;
        m_testReallyStarted = false;
        m_stateToExit = stateToExit;

        m_exceptionConfAtStart = exceptionConfAtStart;
        m_disableExceptionAt = disableExceptionAt;

        m_resultManifest = (resultManifestJSON == null) ? null : resultManifestJSON.ToString();
        m_resultFilesInManifests = resultManifests;
        m_resultFilesInDownloads = resultDownloads;
    }

    private void NotifyTestReallyStarted()
    {
        m_testReallyStarted = true;
        m_timeReallyStartedAt = UnityEngine.Time.realtimeSinceStartup;

        if (m_exceptionConfAtStart != null)
        {
            m_diskDriver.SetExceptionTypeToThrow(m_exceptionConfAtStart);
        }
    }

    protected override void ExtendedPerform()
    {     
        UTDownloadablesHelper.PrepareCache(m_cacheFolder);        
    }

    public override void Update()
    {
        if (HasStarted())
        {            
            if (!m_testReallyStarted && m_entryStatus.State == m_stateToTest)
            {
                NotifyTestReallyStarted();
            }

            float runningTime = UnityEngine.Time.realtimeSinceStartup - m_timeReallyStartedAt;

            if (m_testReallyStarted)
            {                
                if (m_disableExceptionAt >= 0)
                {
                    if (runningTime >= m_disableExceptionAt)
                    {
                        m_diskDriver.ClearAllExceptionTypeToThrow();
                        m_disableExceptionAt = -1f;
                    }
                }
            }

            m_entryStatus.Update();

            if (m_testReallyStarted && m_entryStatus.State != m_stateToTest && m_entryStatus.State == m_stateToExit)
            {
                JSONNode json = m_entryStatus.GetManifest().ToJSON();                
                bool passes = m_resultManifest.ToString() == json.ToString();

                if (passes)
                {
                    passes = UTDownloadablesHelper.CheckDisk(Manager.MANIFESTS_ROOT_PATH, m_resultFilesInManifests);
                    if (passes)
                    {
                        passes = UTDownloadablesHelper.CheckDisk(Manager.DOWNLOADS_ROOT_PATH, m_resultFilesInDownloads);
                    }

                    //Checks the contain of the manifest
                    if (passes)
                    {
                        string manifestInDisk = UTDownloadablesHelper.GetManifestFile(m_entryStatus.Id);
                        if (manifestInDisk != null)
                        {
                            passes = m_resultManifest.ToString() == manifestInDisk;
                        }
                    }

                    if (m_exceptionConfAtStart != null && passes)
                    {
                        passes = runningTime >= m_disableExceptionAt;
                    }
                }

                NotifyPasses(passes);
            }
        }                
    }
}