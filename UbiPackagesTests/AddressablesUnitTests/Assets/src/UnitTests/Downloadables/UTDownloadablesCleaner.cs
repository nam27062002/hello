using Downloadables;
using System.Collections.Generic;

public class UTDownloadablesCleaner : UnitTest
{
    public static UnitTestBatch GetUnitTestBatch()
    {
        UnitTestBatch batch = new UnitTestBatch("UTDownloadablesCleaner");
        MockDiskDriver diskDriver = new MockDiskDriver(null);        
        List<string> idsToDelete;
        List<string> resultFilesInManifests;
        List<string> resultFilesInDownloads;

        UTDownloadablesCleaner test;

        //
        // SUCCESS
        //        

        // PURPOSE: Deletes an id
        // INPUT: 
        //      MANIFESTS: asset_cubes, scene_cubes
        //      DOWNLOADS: asset_cubes, scene_cubes
        //      IDS TO KEEP: scene_cubes
        // OUTPUT: 
        //      MANIFESTS: scene_cubes
        //      DOWNLOADS: scene_cubes        
        test = new UTDownloadablesCleaner();

        idsToDelete = new List<string>();
        idsToDelete.Add("scene_cubes");

        resultFilesInManifests = new List<string>();
        resultFilesInManifests.Add("scene_cubes");

        resultFilesInDownloads = new List<string>();
        resultFilesInDownloads.Add("scene_cubes");

        test.Setup("05", diskDriver, idsToDelete, false, 0f, resultFilesInManifests, resultFilesInDownloads);
        batch.AddTest(test, true);        

        // PURPOSE: Deletes an id after a exception
        // INPUT: 
        //      MANIFESTS: asset_cubes, scene_cubes
        //      DOWNLOADS: asset_cubes, scene_cubes
        //      IDS TO KEEP: scene_cubes
        // OUTPUT: 
        //      MANIFESTS: scene_cubes
        //      DOWNLOADS: scene_cubes        
        test = new UTDownloadablesCleaner();

        idsToDelete = new List<string>();
        idsToDelete.Add("scene_cubes");

        resultFilesInManifests = new List<string>();
        resultFilesInManifests.Add("scene_cubes");

        resultFilesInDownloads = new List<string>();
        resultFilesInDownloads.Add("scene_cubes");

        test.Setup("05", diskDriver, idsToDelete, true, 3f, resultFilesInManifests, resultFilesInDownloads);
        batch.AddTest(test, true);

        // PURPOSE: Tries to deletes an id that doesn't exist in disk
        // INPUT: 
        //      MANIFESTS: Empty
        //      DOWNLOADS: scene_cubes
        //      IDS TO KEEP: scene_cubes
        // OUTPUT: 
        //      MANIFESTS: Empty
        //      DOWNLOADS: scene_cubes
        test = new UTDownloadablesCleaner();

        idsToDelete = new List<string>();
        idsToDelete.Add("scene_cubes");

        resultFilesInManifests = new List<string>();
        resultFilesInManifests.Add("scene_cubes");

        resultFilesInDownloads = new List<string>();        

        test.Setup("04", diskDriver, idsToDelete, false, 0f, resultFilesInManifests, resultFilesInDownloads);
        batch.AddTest(test, true);

        //
        // FAIL
        //

        return batch;
    }

    private MockDiskDriver m_diskDriver;
    private Cleaner m_cleaner;
    private string m_cacheFolder;
    private List<string> m_idsToKeep;
    private List<string> m_resultFilesInManifests;
    private List<string> m_resultFilesInDownloads;
    private bool m_enableExceptionAtStart;
    private float m_disableExceptionAt;    

    public void Setup(string cacheFolder, MockDiskDriver diskDriver, List<string> idsToKeep, bool enableExceptionAtStart, float disableExceptionAt,
        List<string> resultFilesInManifests, List<string> resultFilesInDownloads)
    {
        m_cacheFolder = cacheFolder;

        m_diskDriver = diskDriver;
        Disk disk = new Disk(diskDriver, Manager.MANIFESTS_ROOT_PATH, Manager.DOWNLOADS_ROOT_PATH, 0, null);
        m_cleaner = new Cleaner(disk, 1f);
        m_idsToKeep = idsToKeep;
        m_enableExceptionAtStart = enableExceptionAtStart;
        m_disableExceptionAt = disableExceptionAt;

        m_resultFilesInManifests = resultFilesInManifests;
        m_resultFilesInDownloads = resultFilesInDownloads;
    }

    protected override void ExtendedPerform()
    {
        if (m_enableExceptionAtStart)
        {
            m_diskDriver.SetExceptionTypeToThrow(MockDiskDriver.EExceptionType.UnauthorizedAccess);
        }

        // Copy cache 
        UTDownloadablesHelper.PrepareCache(m_cacheFolder);
        m_cleaner.CleanAllExcept(m_idsToKeep);
    }

    public override void Update()
    {
        float runningTime = UnityEngine.Time.realtimeSinceStartup - m_timeStartAt;

        if (m_disableExceptionAt >= 0)
        {            
            if (runningTime >= m_disableExceptionAt)
            {
                m_diskDriver.SetExceptionTypeToThrow(MockDiskDriver.EExceptionType.None);
                m_disableExceptionAt = -1f;
            }
        }

        m_cleaner.Update();
        if (m_cleaner.IsDone())
        {
            bool passes = UTDownloadablesHelper.CheckDisk(Manager.MANIFESTS_ROOT_PATH, m_resultFilesInManifests);
            if (passes)
            {
                passes = UTDownloadablesHelper.CheckDisk(Manager.DOWNLOADS_ROOT_PATH, m_resultFilesInDownloads);
            }

            if (m_enableExceptionAtStart && passes)
            {
                passes = runningTime >= m_disableExceptionAt;
            }

            NotifyPasses(passes);
        }
    }       
}
