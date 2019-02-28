using Downloadables;
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;

public class UTDownloadablesDownloader : UnitTest
{
    private static string SCENE_CUBES = "scene_cubes";
    private static long SCENE_CUBES_CRC = 2172586036;    
    private static long SCENE_CUBES_SIZE = 126244;

    public static UnitTestBatch GetUnitTestBatch()
    {
        UnitTestBatch batch = new UnitTestBatch("UTDownloadablesDownloader");

        UTDownloadablesDownloader test;
        JSONNode jsonCatalogEntry;
        Queue<Attempt> queue;
        Attempt attempt;

        //
        // SUCCESS
        //        

        /*
        // PURPOSE: Test error when accessing to disk to check if the Downloads directory exists        
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        attempt = new Attempt(MockDriver.EOp.Directory_Exists, "*", MockDriver.EExceptionType.IOException, Error.EType.Disk_IOException);
        queue.Enqueue(attempt);
        test.Setup("00", SCENE_CUBES, jsonCatalogEntry, queue);       

        // PURPOSE: Test IOException when accessing to disk to create the Downloads directory
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        attempt = new Attempt(MockDriver.EOp.Directory_CreateDirectory, "*", MockDriver.EExceptionType.IOException, Error.EType.Disk_IOException);
        queue.Enqueue(attempt);
        test.Setup("00", SCENE_CUBES, jsonCatalogEntry, queue);        

        // PURPOSE: Test UnauthorizedAccessException when accessing to disk to create the Downloads directory
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        attempt = new Attempt(MockDriver.EOp.Directory_CreateDirectory, "*", MockDriver.EExceptionType.UnauthorizedAccess, Error.EType.Disk_UnauthorizedAccess);
        queue.Enqueue(attempt);
        test.Setup("00", SCENE_CUBES, jsonCatalogEntry, queue);        

        // PURPOSE: Test UnauthorizedAccessException when accessing to disk to get the FileInfo of the file
        // INPUT: Empty
        // OUTPUT: Empty
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        attempt = new Attempt(MockDriver.EOp.File_GetInfo, "*", MockDriver.EExceptionType.UnauthorizedAccess, Error.EType.Disk_UnauthorizedAccess);
        queue.Enqueue(attempt);
        test.Setup("00", SCENE_CUBES, jsonCatalogEntry, queue);
        */

        // PURPOSE: Test when creating http web request
        // INPUT: Empty
        // OUTPUT: Empty
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        attempt = new Attempt(MockDriver.EOp.CreateHttpWebRequest, "*", MockDriver.EExceptionType.UriFormatException, Error.EType.Network_Uri_Malformed);
        queue.Enqueue(attempt);
        test.Setup("00", SCENE_CUBES, jsonCatalogEntry, queue);

        /*
        // PURPOSE: Test error when the server is down       
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        queue.Enqueue(new Attempt(Error.EType.Network_Web_Exception_Connect_Failure));
        test.Setup("00", SCENE_CUBES, jsonCatalogEntry, queue);
        */

        batch.AddTest(test, true);

        return batch;
    }

    private string m_cacheFolder;
    private string m_entryId;
    private JSONNode m_entryJSON;
    private CatalogEntryStatus m_entry;

    private MockDiskDriver m_disk;
    private MockNetworkDriver m_network;
    private Downloader m_downloader;

    private Queue<Attempt> m_attempts;

    private bool m_success;

    public void Setup(string cacheFolder, string entryId, JSONNode entryJSON, Queue<Attempt> attempts)
    {
        m_cacheFolder = cacheFolder;
        m_entryId = entryId;
        m_entryJSON = entryJSON;
        m_attempts = attempts;
    }

    protected override void ExtendedPerform()
    {
        UTDownloadablesHelper.PrepareCache(m_cacheFolder);

        Logger logger = new ConsoleLogger("UTDownloadablesDownloader");

        m_network = new MockNetworkDriver(getExceptionToThrowDelegate);
        m_disk = new MockDiskDriver(getExceptionToThrowDelegate);
        Disk disk = new Disk(m_disk, Manager.MANIFESTS_ROOT_PATH, Manager.DOWNLOADS_ROOT_PATH, 0, null);
        UTTracker tracker = new UTTracker(5, null, logger);

        CatalogEntryStatus.StaticSetup(disk, tracker, OnDownloadEndCallback);

        m_downloader = new Downloader(m_network, disk, logger);
        string url = "http://192.168.1.2:7888/StandaloneWindows64/";
        m_downloader.Initialize(url);

        m_entry = new CatalogEntryStatus();
        m_entry.LoadManifest(m_entryId, m_entryJSON);

        State = EState.PreparingEntry;
    }

    private enum EState
    {
        PreparingEntry,
        Downloading,
        PreparingForDone,
        Done
    };

    private EState m_state;
    private EState State
    {
        get
        {
            return m_state;
        }

        set
        {
            m_state = value;

            switch (m_state)
            {
                case EState.Downloading:
                    m_downloader.StartDownloadThread(m_entry);
                    break;

                case EState.Done:
                    NotifyPasses(m_success);
                    m_downloader.Reset();
                    break;
            }
        }
    }

    public override void Update()
    {
        if (HasStarted())
        {
            CatalogEntryStatus.StaticUpdate(Time.realtimeSinceStartup, m_network.CurrentNetworkReachability);

            m_entry.Update();

            switch (State)
            {
                case EState.PreparingEntry:
                    if (m_entry.State == CatalogEntryStatus.EState.InQueueForDownload)
                    {
                        State = EState.Downloading;
                    }
                    break;

                case EState.PreparingForDone:
                    State = EState.Done;
                    break;
            }            
        }
    }

    private void OnDownloadEndCallback(CatalogEntryStatus entry, Error.EType errorType)
    {
        Attempt attempt = m_attempts.Dequeue();
        if (attempt.Result != errorType)
        {
            OnDone(false);
        }
        else if (m_attempts.Count == 0)
        {
            OnDone(true);
        }
        else
        {
            State = EState.Downloading;
        }
    }

    private void OnDone(bool success)
    {
        m_success = success;
        State = EState.PreparingForDone;
    }

    private MockDriver.EExceptionType getExceptionToThrowDelegate(MockDriver.EOp op, string parameter)
    {
        MockDriver.EExceptionType returnValue = MockDriver.EExceptionType.None;
        
        if (State == EState.Downloading && m_attempts != null && m_attempts.Count > 0)
        {
            Attempt attempt = m_attempts.Peek();
            if (attempt.canPerform(op, parameter))
            {
                returnValue = attempt.ExceptionType;
            }
        }

        return returnValue;
    }

    public class Attempt
    {
        public MockDriver.EOp Op { get; set; }
        public string Parameter { get; set; }
        public MockDriver.EExceptionType ExceptionType { get; set; }

        public Error.EType Result { get; set; }

        public Attempt(MockDriver.EOp op, string parameter, MockDriver.EExceptionType exceptionType, Error.EType result)
        {
            Setup(op, parameter, exceptionType, result);
        }

        public Attempt(Error.EType result)
        {
            Setup(MockDriver.EOp.None, null, MockDriver.EExceptionType.None, result);
        }

        public void Setup(MockDriver.EOp op, string parameter, MockDriver.EExceptionType exceptionType, Error.EType result)
        {
            Op = op;
            Parameter = parameter;
            ExceptionType = exceptionType;
            Result = result;
        }

        public bool canPerform(MockDriver.EOp op, string parameter)
        {
            return Op == op && (Parameter == null || Parameter == parameter || Parameter == "*");                        
        }
    }

    private class UTTracker : Tracker
    {
        public UTTracker(int maxAttempts, Dictionary<Error.EType, int> maxPerErrorType, Logger logger) : base(maxAttempts, maxPerErrorType, logger)
        {
        }

        public override void TrackActionEnd(EAction action, string downloadableId, float existingSizeMbAtStart, float existingSizeMbAtEnd, float totalSizeMb, int timeSpent,
                                             NetworkReachability reachabilityAtStart, NetworkReachability reachabilityAtEnd, Error.EType error, bool maxAttemptsReached)  
        {
            int i = 10;
        }
    }
}
