﻿using Downloadables;
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;

public class UTDownloadablesDownloader : UnitTest
{
    private static string SCENE_CUBES = "scene_cubes";
    private static long SCENE_CUBES_CRC = 2172586036;    
    private static long SCENE_CUBES_SIZE = 126244;

    // Stored to be able to reestablish them after the unit test batch is done
    private static bool sm_isServerUp;
    private static string sm_serverDirectory;

    private static UnitTest sm_lastTest;    

    public static UnitTestBatch GetUnitTestBatch()
    {
        UnitTestBatch batch = new UnitTestBatch("UTDownloadablesDownloader");

        UTDownloadablesDownloader test;
        JSONNode jsonCatalogEntry;
        Queue<Attempt> queue;
        Attempt attempt;
        Script script;

        sm_isServerUp = AssetBundles.LaunchAssetBundleServer.IsRunning();
        sm_serverDirectory = AssetBundles.LaunchAssetBundleServer.GetRemoteAssetsFolderName();

        //
        // SUCCESS
        //                       
        
        // PURPOSE: Test error when accessing to disk to check if the Downloads directory exists        
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        attempt = new Attempt(MockDriver.EOp.Directory_Exists, "*", MockDriver.EExceptionType.IOException, Error.EType.Disk_IOException);
        queue.Enqueue(attempt);
        test.Setup("00", "00", false, SCENE_CUBES, jsonCatalogEntry, queue);
        batch.AddTest(test, true);

        // PURPOSE: Test IOException when accessing to disk to create the Downloads directory
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        attempt = new Attempt(MockDriver.EOp.Directory_CreateDirectory, "*", MockDriver.EExceptionType.IOException, Error.EType.Disk_IOException);
        queue.Enqueue(attempt);
        test.Setup("00", "00", false, SCENE_CUBES, jsonCatalogEntry, queue);        
        batch.AddTest(test, true);

        // PURPOSE: Test UnauthorizedAccessException when accessing to disk to create the Downloads directory
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        attempt = new Attempt(MockDriver.EOp.Directory_CreateDirectory, "*", MockDriver.EExceptionType.UnauthorizedAccess, Error.EType.Disk_UnauthorizedAccess);
        queue.Enqueue(attempt);
        test.Setup("00", "00", false, SCENE_CUBES, jsonCatalogEntry, queue);        
        batch.AddTest(test, true);

        // PURPOSE: Test UnauthorizedAccessException when accessing to disk to get the FileInfo of the file
        // INPUT: Empty
        // OUTPUT: Empty
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        attempt = new Attempt(MockDriver.EOp.File_GetInfo, "*", MockDriver.EExceptionType.UnauthorizedAccess, Error.EType.Disk_UnauthorizedAccess);
        queue.Enqueue(attempt);
        test.Setup("00", "00", false, SCENE_CUBES, jsonCatalogEntry, queue);        
        batch.AddTest(test, true);

        // PURPOSE: Test when creating http web request
        // INPUT: Empty
        // OUTPUT: Empty
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        attempt = new Attempt(MockDriver.EOp.CreateHttpWebRequest, "*", MockDriver.EExceptionType.UriFormatException, Error.EType.Network_Uri_Malformed);
        queue.Enqueue(attempt);
        test.Setup("00", "00", false, SCENE_CUBES, jsonCatalogEntry, queue);
        batch.AddTest(test, true);                        

        // PURPOSE: Test error when the server is down       
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        queue.Enqueue(new Attempt(Error.EType.Network_Web_Exception_Connect_Failure));
        test.Setup("00", "00", false, SCENE_CUBES, jsonCatalogEntry, queue);        
        batch.AddTest(test, true);                        
        
        // PURPOSE: Test success
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        queue.Enqueue(new Attempt(Error.EType.None));
        test.Setup("00", "00", true, SCENE_CUBES, jsonCatalogEntry, queue);
        batch.AddTest(test, true);        

        // PURPOSE: Test Download over carrier but no permission granted
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        queue.Enqueue(new Attempt(Error.EType.Network_Unauthorized_Reachability));

        script = new Script();
        script.AddAction(0, new ActionNetworkReachability(NetworkReachability.ReachableViaCarrierDataNetwork));
        test.Setup("00", "00", true, SCENE_CUBES, jsonCatalogEntry, queue, script);
        batch.AddTest(test, true);        

        // PURPOSE: Test Download over carrier because permission is granted
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        queue.Enqueue(new Attempt(Error.EType.None));        
        script = new Script();
        script.AddAction(0, new ActionNetworkReachability(NetworkReachability.ReachableViaCarrierDataNetwork));
        test.Setup("permission_g1_true", "00", true, SCENE_CUBES, jsonCatalogEntry, queue, script, "g1");
        batch.AddTest(test, true);        

        // PURPOSE: Test error when content from server is not sufficient       
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        queue.Enqueue(new Attempt(Error.EType.Network_Server_Size_Mismatch));
        script = new Script();
        script.AddAction(0, new ActionNetworkResponseLength(0));
        test.Setup("00", "00", true, SCENE_CUBES, jsonCatalogEntry, queue, script);
        batch.AddTest(test, true);                

        // PURPOSE: Test trying to resume a partial download unsuccessfully (the file is downloaded but from scratch)
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        queue.Enqueue(new Attempt(Error.EType.None));                
        test.Setup("scene_cubes_incomplete", "00", true, SCENE_CUBES, jsonCatalogEntry, queue);
        batch.AddTest(test, true);        
        
        // PURPOSE: Test the file is already downloaded so downloader must do nothing
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        queue.Enqueue(new Attempt(Error.EType.None));        
        test.Setup("scene_cubes_complete", "00", true, SCENE_CUBES, jsonCatalogEntry, queue);
        batch.AddTest(test, true);                

        // PURPOSE: Test: The downloaded file is bigger than the size stated by the manifest. It will be downloaded from scratch
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        queue.Enqueue(new Attempt(Error.EType.None));        
        test.Setup("scene_cubes_toobig", "00", true, SCENE_CUBES, jsonCatalogEntry, queue);
        batch.AddTest(test, true);        
        
        // PURPOSE: Test error when content from server is not accessible
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        queue.Enqueue(new Attempt(Error.EType.Network_Web_Exception_No_Access_To_Content));
        script = new Script();
        script.AddAction(0, new ActionNetworkResponseStatusCode(300));
        test.Setup("00", "00", true, SCENE_CUBES, jsonCatalogEntry, queue, script);
        batch.AddTest(test, true);        

        // PURPOSE: Test Download is interrumpted because there's no internet access
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        queue.Enqueue(new Attempt(Error.EType.Network_No_Reachability));
        script = new Script();
        script.AddAction(0, new ActionNetworkReachability(NetworkReachability.NotReachable));
        test.Setup("permission_g1_true", "00", true, SCENE_CUBES, jsonCatalogEntry, queue, script, "g1");
        batch.AddTest(test, true);         

        // PURPOSE: Test Download is interrumpted because there's no empty space in storage
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        queue.Enqueue(new Attempt(Error.EType.Disk_IOException));
        script = new Script();
        script.AddAction(0, new ActionDiskNoFreeSpace());
        test.Setup("permission_g1_true", "00", true, SCENE_CUBES, jsonCatalogEntry, queue, script, "g1");
        batch.AddTest(test, true);        

        // PURPOSE: Test Download is interrumpted because there's no permission to write in storage
        test = new UTDownloadablesDownloader();
        jsonCatalogEntry = UTDownloadablesHelper.GetEntryStatusManifestAsJSON(SCENE_CUBES_CRC, SCENE_CUBES_SIZE, 0, false);
        queue = new Queue<Attempt>();
        queue.Enqueue(new Attempt(MockDiskDriver.EOp.File_Open, "*", MockDriver.EExceptionType.UnauthorizedAccess, Error.EType.Disk_UnauthorizedAccess));        
        test.Setup("permission_g1_true", "00", true, SCENE_CUBES, jsonCatalogEntry, queue, null, "g1");
        batch.AddTest(test, true);

        sm_lastTest = test;

        return batch;
    }

    private string m_cacheFolder;
    private string m_downloadablesFolder;
    private bool m_isServerUp;

    private string m_entryId;
    private JSONNode m_entryJSON;
    private CatalogEntryStatus m_entry;

    private Disk m_disk;
    private MockDiskDriver m_diskDriver;
    private MockNetworkDriver m_network;
    private Downloader m_downloader;

    private Queue<Attempt> m_attempts;

    private bool m_success;

    private Script m_script;

    private float m_stateTimeAt;

    private float m_currenTime;

    private string m_entryGroupId;
    
    public void Setup(string cacheFolder, string downloadablesFolder, bool isServerUp, string entryId, JSONNode entryJSON, 
        Queue<Attempt> attempts, Script script = null, string entryGroupId = null)
    {
        m_cacheFolder = cacheFolder;
        m_downloadablesFolder = downloadablesFolder;
        m_isServerUp = isServerUp;
        m_entryId = entryId;
        m_entryJSON = entryJSON;
        m_attempts = attempts;
        m_script = script;
        m_entryGroupId = entryGroupId;
    }

    protected override void ExtendedPerform()
    {     
        if (AssetBundles.LaunchAssetBundleServer.IsRunning())
        {
            AssetBundles.LaunchAssetBundleServer.KillRunningAssetBundleServer();
        }

        AssetBundles.LaunchAssetBundleServer.SetRemoteAssetsFolderName("Assets/Editor/Downloadables/UnitTests/Downloadables/");

        if (m_isServerUp)
        {
            AssetBundles.LaunchAssetBundleServer.ToggleLocalAssetBundleServer();
        }
        
        UTDownloadablesHelper.PrepareCache(m_cacheFolder);

        Logger logger = new ConsoleLogger("UTDownloadablesDownloader");

        Config config = new Config();        
        m_network = new MockNetworkDriver(getExceptionToThrowDelegate);
        m_diskDriver = new MockDiskDriver(getExceptionToThrowDelegate);
        m_disk = new Disk(m_diskDriver, Manager.MANIFESTS_ROOT_PATH, Manager.DOWNLOADS_ROOT_PATH, Manager.GROUPS_ROOT_PATH, Manager.DUMP_ROOT_PATH, 0, null);
        UTTracker tracker = new UTTracker(config, logger);

        CatalogEntryStatus.StaticSetup(config, m_disk, tracker, OnDownloadEndCallback);

        m_downloader = new Downloader(null, m_network, m_disk, logger);

		try
		{
	        string url = AssetBundles.LaunchAssetBundleServer.GetServerURL() + m_downloadablesFolder + "/";
	        m_downloader.Initialize(url);

	        m_entry = new CatalogEntryStatus();
	        m_entry.LoadManifest(m_entryId, m_entryJSON);

            if (m_entryGroupId != null)
            {
                CatalogGroup.StaticSetup(m_disk);
                CatalogGroup entryGroup = new CatalogGroup();
                entryGroup.Setup(m_entryGroupId, new List<string> { m_entryId });
                m_entry.AddGroup(entryGroup);
            }

	        State = EState.PreparingEntry;

	        Action.mockNetworkDriver = m_network;
	        Action.downloader = m_downloader;
		}
		catch (System.Exception e) 
		{
            Debug.LogError("Exception " + e.ToString());
            InternalNotifyPasses(false);
		}
    }
    
    private enum EState
    {
        PreparingEntry,
        WaitingForServer,
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
                    InternalNotifyPasses(m_success);
                    m_downloader.Reset();
                    break;
            }

            m_stateTimeAt = m_currenTime;
        }
    }

    public override void Update()
    {
		if (HasStarted() && m_entry != null)
        {
            m_currenTime = Time.realtimeSinceStartup;

            if (m_script != null)
            {
                m_script.Update(m_currenTime - m_timeStartAt);
            }

            m_downloader.Update();
            CatalogEntryStatus.StaticUpdate(m_currenTime, m_network.CurrentNetworkReachability);

            m_entry.Update();

            switch (State)
            {                
                case EState.PreparingEntry:
                    if (m_entry.State == CatalogEntryStatus.EState.Available)
                    {
                        OnDone(true);
                    }
                    else if (m_entry.State == CatalogEntryStatus.EState.InQueueForDownload)
                    {
                        State = EState.WaitingForServer;
                    }
                    break;

                case EState.WaitingForServer:
                    if (m_currenTime - m_stateTimeAt >= 1f)
                    {
                        State = EState.Downloading;
                    }
                    break;

                case EState.PreparingForDone:
                    State = EState.Done;

                    // Reestablish server values
                    if (this == sm_lastTest)
                    {
                        if (AssetBundles.LaunchAssetBundleServer.IsRunning())
                        {
                            AssetBundles.LaunchAssetBundleServer.ToggleLocalAssetBundleServer();
                        }

                        if (sm_isServerUp)
                        {
                            AssetBundles.LaunchAssetBundleServer.SetRemoteAssetsFolderName(sm_serverDirectory);
                        }

                        if (sm_isServerUp != AssetBundles.LaunchAssetBundleServer.IsRunning())
                        {
                            AssetBundles.LaunchAssetBundleServer.ToggleLocalAssetBundleServer();
                        }                      
                    }
                    break;

                case EState.Done:                   
                    break;
            }            
        }
    }

    private void InternalNotifyPasses(bool success)
    {        
        NotifyPasses(success);
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
            // Verifies that the file has been downloaded
            bool success = true;
            if (errorType == Error.EType.None)
            {
                success = false;

                Error error;
                if (m_disk.File_Exists(Disk.EDirectoryId.Downloads, m_entryId, out error))
                {
                    System.IO.FileInfo fileInfo = m_disk.File_GetInfo(Disk.EDirectoryId.Downloads, m_entryId, out error);
                    if (fileInfo != null)
                    {
                        success = fileInfo.Length == m_entry.GetManifest().Size;
                    }
                }                                
            }

            OnDone(success);
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

    public abstract class Action
    {
        public static Downloader downloader;
        public static MockNetworkDriver mockNetworkDriver;

        public abstract void Perform();
    }

    public class ActionNetworkReachability : Action
    {
        private NetworkReachability m_reachability;

        public ActionNetworkReachability(NetworkReachability reachability)
        {
            m_reachability = reachability;
        }

        public override void Perform()
        {
            MockNetworkDriver.IsMockNetworkReachabilityEnabled = true;
            MockNetworkDriver.MockNetworkReachability = m_reachability;            
        }
    }    

    public class ActionNetworkResponseLength : Action
    {
        private long Length;

        public ActionNetworkResponseLength(long length)
        {
            Length = length;
        }

        public override void Perform()
        {
            mockNetworkDriver.IsMockResponseContentLengthEnabled = true;
            mockNetworkDriver.MockResponseContentLength = Length;
        }
    }

    public class ActionNetworkResponseStatusCode : Action
    {
        private int StatusCode;

        public ActionNetworkResponseStatusCode(int statusCode)
        {
            StatusCode = statusCode;
        }

        public override void Perform()
        {
            mockNetworkDriver.IsMockResponseStatusCodeEnabled = true;
            mockNetworkDriver.MockResponseStatusCode = StatusCode;
        }
    }

    public class ActionDiskNoFreeSpace : Action
    {                
        public override void Perform()
        {
            MockDiskDriver.IsNoFreeSpaceEnabled = true;            
        }
    }    

    public class Script
    {
        public Dictionary<float, List<Action>> Actions = new Dictionary<float, List<Action>>();
        public List<float> Times = new List<float>();

        public void AddAction(float atTime, Action action)
        {
            if (!Times.Contains(atTime))
            {
                Times.Add(atTime);
                Times.Sort();
            }

            if (!Actions.ContainsKey(atTime))
            {
                Actions.Add(atTime, new List<Action>());
            }

            Actions[atTime].Add(action);
        }

        public void Update(float time)
        {
            if (Times.Count > 0)
            {
                if (time >= Times[0])
                {
                    List<Action> actions = Actions[Times[0]];
                    if (actions != null)
                    {
                        int count = actions.Count;
                        for (int i = 0; i < count; i++)
                        {
                            actions[i].Perform();
                        }
                    }

                    Actions.Remove(Times[0]);
                    Times.RemoveAt(0);
                }
            }
        }
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
        public UTTracker(Config config, Logger logger) : base(config, logger)
        {
        }

        public override void TrackActionStart(EAction action, string downloadableId, long existingSizeAtStart, long totalSize)
        {
        }

        public override void TrackActionEnd(EAction action, string downloadableId, long existingSizeAtStart, long existingSizeAtEnd, long totalSize, int timeSpent,
                                             NetworkReachability reachabilityAtStart, NetworkReachability reachabilityAtEnd, Error.EType error, bool maxAttemptsReached)  
        {            
        }
    }
}
