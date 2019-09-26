using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;

namespace Downloadables
{    
    /// <summary>
    /// This class is responsible for downloading remote assets (downloadables), storing them in disk and retrieving them on demand.
    /// </summary>
    public class Manager
    {
        public static bool USE_CRC_IN_URL = true;

#if UNITY_EDITOR
        private static bool USE_REMOTE_SERVER = true;
        public static string REMOTE_FOLDER = (USE_REMOTE_SERVER) ? "AssetBundles/" : "";
#else
        public static string REMOTE_FOLDER = "AssetBundles/";
#endif        
        public static JSONNode GetCatalogFromAssetsLUT(JSONNode assetsLUTJson, bool useProdUrl)
        {
            JSONNode returnValue = null;
            if (assetsLUTJson != null)
            {
                Catalog downloadablesCatalog = new Catalog();

                Catalog assetsLUTCatalog = new Catalog();
                assetsLUTCatalog.Load(assetsLUTJson, null);

#if UNITY_EDITOR
                string runtimePlatform = UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString();
#else
                string runtimePlatform = (Application.platform == RuntimePlatform.Android) ? "Android" : "iOS";
#endif
                string prefix = REMOTE_FOLDER + runtimePlatform + "/";
            
                downloadablesCatalog.UrlBase = assetsLUTCatalog.UrlBase;

                if (!USE_CRC_IN_URL)
                {
                    string key = "release";
                    if (assetsLUTJson.ContainsKey(key))
                    {
                        downloadablesCatalog.UrlBase += assetsLUTJson[key] + "/";
                    }

                    if (useProdUrl)
                    {
                        downloadablesCatalog.UrlBase += prefix;
                    }
                }

                // Deletes all asset bundle entries because we are going to reenter them
                Dictionary<string, CatalogEntry> entries = assetsLUTCatalog.GetEntries();
                foreach (KeyValuePair<string, CatalogEntry> pair in entries)
                {
                    if (pair.Key.Contains(prefix))
                    {
                        downloadablesCatalog.AddEntry(pair.Key.Replace(prefix, ""), pair.Value);
                    }
                }

                returnValue = downloadablesCatalog.ToJSON();                
            }

            return returnValue;
        }

        public static string GetPathToDownload(string id)
        {
            return DOWNLOADS_ROOT_PATH_WITH_SLASH + id;
        }

        public static ArrayList GetEntryIds(JSONNode catalogJSON)
        {
            ArrayList returnValue = null;
            if (catalogJSON != null)
            {
                JSONClass assets = (JSONClass)catalogJSON[Catalog.CATALOG_ATT_ENTRIES];
                if (assets != null)
                {
                    returnValue = assets.GetKeys();
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Removes the entry ids passed as an argument from <c>catalogJSON</c>
        /// </summary>
        /// <param name="catalogJSON">JSON containing downloadable entries</param>
        /// <param name="entryIdsToRemove">List of downoadable ids to remove from the catalog.</param>
        /// <returns>List with the downloadable ids actually removed from the catalog.</returns>
        public static List<string> RemoveEntryIds(JSONNode catalogJSON, List<string> entryIdsToRemove)
        {
            List<string> returnValue = null;
            if (catalogJSON != null && entryIdsToRemove != null && entryIdsToRemove.Count > 0)
            {
                JSONClass assets = (JSONClass)catalogJSON[Catalog.CATALOG_ATT_ENTRIES];
                if (assets != null)
                {
                    returnValue = new List<string>();

                    int count = entryIdsToRemove.Count;
                    for (int i = 0; i < count; i++)
                    {
                        if (assets.ContainsKey(entryIdsToRemove[i]))
                        {
                            returnValue.Add(entryIdsToRemove[i]);
                            assets.Remove(entryIdsToRemove[i]);
                        }
                    }                    
                }
            }

            return returnValue;
        }

        public static readonly string DESKTOP_DEVICE_STORAGE_PATH_SIMULATED = "DeviceStorageSimulated/";

        public static readonly string DOWNLOADABLES_FOLDER_NAME = "Downloadables";
        public static readonly string DOWNLOADABLESS_ROOT_PATH = FileUtils.GetDeviceStoragePath(DOWNLOADABLES_FOLDER_NAME, DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);

        public static readonly string MANIFESTS_FOLDER_NAME = Path.Combine(DOWNLOADABLES_FOLDER_NAME, "Metadata");
        public static readonly string MANIFESTS_ROOT_PATH = FileUtils.GetDeviceStoragePath(MANIFESTS_FOLDER_NAME, DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);

        public static readonly string DOWNLOADS_FOLDER_NAME = Path.Combine(DOWNLOADABLES_FOLDER_NAME, "Downloads");
        public static readonly string DOWNLOADS_ROOT_PATH = FileUtils.GetDeviceStoragePath(DOWNLOADS_FOLDER_NAME, DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);
        public static readonly string DOWNLOADS_ROOT_PATH_WITH_SLASH = DOWNLOADS_ROOT_PATH + "/";

        public static readonly string GROUPS_FOLDER_NAME = Path.Combine(MANIFESTS_FOLDER_NAME, "Groups");
        public static readonly string GROUPS_ROOT_PATH = FileUtils.GetDeviceStoragePath(GROUPS_FOLDER_NAME, DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);

        public static readonly string DUMP_FOLDER_NAME = Path.Combine(DOWNLOADABLES_FOLDER_NAME, "Dump");
        public static readonly string DUMP_ROOT_PATH = FileUtils.GetDeviceStoragePath(DUMP_FOLDER_NAME, DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);

        public static readonly string DOWNLOADABLES_CONFIG_FILENAME_NO_EXTENSION = "downloadablesConfig";
        public static readonly string DOWNLOADABLES_CONFIG_FILENAME = DOWNLOADABLES_CONFIG_FILENAME_NO_EXTENSION + ".json";        

        private bool IsInitialized { get; set; }

        private NetworkDriver m_network;
        private Disk m_disk;
        private Cleaner m_cleaner;
        private Downloader m_downloader;
        private Tracker m_tracker;

#if USE_DUMPER
        private Dumper m_dumper;
#endif

        /// <summary>
        /// When <c>true</c> all downloads will be downloaded automatically. Otherwise a downloadable will be downloaded only on demand (by calling Request)
        /// </summary>
        public bool IsAutomaticDownloaderEnabled { get; set; }

        private bool m_isEnabled;
        /// <summary>
        /// When <c>true</c> downloader is enabled. When disabled no downloadables, not even the ones requested explictily, will be downloaded.
        /// It will be disabled when high performance is required, typically when the user is playing the game.        
        /// </summary>
        public bool IsEnabled
        {
            get { return m_isEnabled; }
            set
            {
                m_isEnabled = value;
                if (!m_isEnabled)
                {
                    SetSpeed(0f);
                }
            }
        }

        private Config Config { get; set; }

        /// <summary>
        /// Current downloading speed 
        /// </summary>
        private float m_speed;

        private float m_nextUpdateAt;

        public Manager(Config config, NetworkDriver network, DiskDriver diskDriver, Disk.OnIssue onDiskIssueCallbak, Tracker tracker, Logger logger)
        {
            if (config == null)
            {
                config = new Config();
            }

            Config = config;

            sm_logger = logger;

            m_network = network;
            m_disk = new Disk(diskDriver, MANIFESTS_ROOT_PATH, DOWNLOADS_ROOT_PATH, GROUPS_ROOT_PATH, DUMP_ROOT_PATH, 180, onDiskIssueCallbak);
            m_tracker = tracker;

            CatalogEntryStatus.StaticSetup(config, m_disk, tracker);
            m_cleaner = new Cleaner(m_disk, 180);            
            m_downloader = new Downloader(this, network, m_disk, logger);
            CatalogGroup.StaticSetup(m_disk);

#if USE_DUMPER
            m_dumper = new Dumper();
#endif
            IsEnabled = true;

            Handle.StaticSetup(this, diskDriver);

            Reset();
        }

        public void Reset()
        {
            IsInitialized = false;
            IsAutomaticDownloaderEnabled = Config.IsAutomaticDownloaderEnabled;            
            m_cleaner.Reset();
            Groups_Reset();
            Catalog_Reset();
            m_downloader.Reset();
            SetSpeed(0f);
            m_nextUpdateAt = 0f;
        }        

        public void Initialize(JSONNode catalogJSON, Dictionary<string, CatalogGroup> groups)
        {
            Reset();          

            if (CanLog())
            {                
                Log("Initializing Downloadables manager..." );
            }            

            ProcessCatalog(catalogJSON, groups);

            // Groups need to be initialized after catalog has been loaded
            Groups_Init(groups);

#if USE_DUMPER
            m_dumper.Initialize(m_disk, sm_logger);
#endif
            IsInitialized = true;            

            if (catalogJSON != null)
            {
                string urlBase = catalogJSON[Catalog.CATALOG_ATT_URL_BASE];
                m_downloader.Initialize(urlBase);
            }
        }                

        private void ProcessCatalog(JSONNode catalogJSON, Dictionary<string, CatalogGroup> groups)
        {    
            if (catalogJSON != null)
            {
                List<string> ids = null;

                JSONClass assets = (JSONClass)catalogJSON[Catalog.CATALOG_ATT_ENTRIES];
                if (assets != null)
                {
                    ids = new List<string>();

                    string id;
                    ArrayList keys = assets.GetKeys();
                    int count = keys.Count;
                    for (int i = 0; i < count; i++)
                    {
                        id = (string)keys[i];
                        ids.Add(id);

                        Catalog_AddEntryStatus(id, assets[id]);                        
                    }                    
                }

                List<string> groupIds = null;
                if (groups != null)
                {
                    groupIds = new List<string>();
                    foreach (KeyValuePair<string, CatalogGroup> pair in groups)
                    {
                        groupIds.Add(pair.Key);
                    }

                    // Internal groups are included too
                    groupIds.Add(GROUPS_DEFAULT_ID);
                    groupIds.Add(GROUPS_ALL_ID);
                }

                // Cleans obsolete stuff
                m_cleaner.CleanAllExcept(ids, groupIds);                
            }            
        }                 
        
        public bool IsIdAvailable(string id, bool checkDisk = false, bool track = false)
        {
            bool returnValue = false;
            if (IsInitialized)
            {
                CatalogEntryStatus entry = Catalog_GetEntryStatus(id);
                returnValue = (entry != null && entry.IsAvailable(checkDisk));                
                
                if (track)
                {
                    long existingSize = GetIdBytesDownloadedSoFar(id);
                    NetworkReachability reachability = m_network.CurrentNetworkReachability;
                    Error.EType result = (returnValue) ? Error.EType.None : Error.EType.Internal_NotAvailable;
                    m_tracker.TrackActionEnd(Tracker.EAction.Load, id, existingSize, existingSize, GetIdTotalBytes(id), 0, reachability, reachability, result, false);
                }
            }

            return returnValue;
        }

        public bool IsIdsListAvailable(List<string> ids, bool checkDisk = false, bool track = false)
        {
            bool returnValue = true;
            if (ids != null)
            {
                int count = ids.Count;
                for (int i = 0; i < count && returnValue; i++)
                {
                    // IsIdAvailable() must be called for every id so that the result will be tracked if it needs to
                    returnValue = IsIdAvailable(ids[i], checkDisk, track) && returnValue;
                }
            }

            return returnValue;
        }

        public float GetIdMbDownloadedSoFar(string id)
        {
            float returnValue = float.MaxValue;
            if (IsInitialized)
            {
                returnValue = GetIdTotalMb(id) - GetIdMbLeftToDownload(id);                
            }

            return returnValue;
        }

        public float GetIdMbLeftToDownload(string id)
        {           
            float returnValue = float.MaxValue;
            if (IsInitialized)
            {
                CatalogEntryStatus entry = Catalog_GetEntryStatus(id);
                returnValue = (entry == null) ? 0 : entry.GetMbLeftToDownload();
            }

            return returnValue;
        }

        public float GetIdTotalMb(string id)
        {
            float returnValue = float.MaxValue;
            if (IsInitialized)
            {
                CatalogEntryStatus entry = Catalog_GetEntryStatus(id);
                returnValue = (entry == null) ? 0 : entry.GetTotalMb();
                if (entry != null)
                {
                    entry.GetTotalMb();
                }                
            }

            return returnValue;
        }

        public long GetIdBytesDownloadedSoFar(string id)
        {
            long returnValue = long.MaxValue;
            if (IsInitialized)
            {
                returnValue = GetIdTotalBytes(id) - GetIdBytesLeftToDownload(id);
            }

            return returnValue;
        }

        public long GetIdBytesLeftToDownload(string id)
        {
            long returnValue = long.MaxValue;
            if (IsInitialized)
            {
                CatalogEntryStatus entry = Catalog_GetEntryStatus(id);
                returnValue = (entry == null) ? 0 : entry.GetBytesLeftToDownload();
            }

            return returnValue;
        }

        public long GetIdTotalBytes(string id)
        {
            long returnValue = long.MaxValue;
            if (IsInitialized)
            {
                CatalogEntryStatus entry = Catalog_GetEntryStatus(id);
                returnValue = (entry == null) ? 0 : entry.GetTotalBytes();
                if (entry != null)
                {
                    entry.GetTotalBytes();
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Request a downloadable with <c>id</c> as an identifier to be downloaded.
        /// </summary>
        /// <param name="id">Downloadable identifier to download</param>
        public void RequestId(string id)
        {
            if (IsInitialized)
            {
                CatalogEntryStatus entry = Catalog_GetEntryStatus(id);
                if (entry == null && CanLog())
                {
                    LogError("Downloadable id " + id + " requested but it doesn't exist in catalog");
                }
                else if (entry.CanBeRequested())
                {
                    entry.Request();
                }
            }
        }

        /// <summary>
        /// Request a list of downloadables to be downloaded.
        /// </summary>
        /// <param name="id">List of downloadable identifiers to download</param>
        public void RequestIdList(List<string> ids)
        {
            if (IsInitialized)
            {
                if (ids != null)
                {
                    int count = ids.Count;
                    for (int i = 0; i < count; i++)
                    {
                        RequestId(ids[i]);
                    }
                }
            }
        }

        public List<string> GetIds()
        {
            List<string> returnValue = new List<string>();
            foreach (KeyValuePair<string, CatalogEntryStatus> pair in m_catalog)
            {
                returnValue.Add(pair.Key);
            }

            return returnValue;
        }

        public NetworkReachability GetCurrentNetworkReachability()
        {
            return m_network.CurrentNetworkReachability;
        }        

        /// <summary>
        /// Deletes all downloadables currently cached.
        /// </summary>
        public void ClearCache()
        {            
            m_downloader.AbortDownload();
            if (m_catalog != null)
            {
                foreach (KeyValuePair<string, CatalogEntryStatus> pair in m_catalog)
                {
                    pair.Value.DeleteDownload();                    
                }
            }

            FileUtils.RemoveDirectoryInDeviceStorage(DOWNLOADABLES_FOLDER_NAME, DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);

            Groups_ResetPermissions();
        }

        /// <summary>
        /// Returns current downloading speed in bytes/second 
        /// </summary>        
        public float GetSpeed()
        {
            return m_speed;
        }

        /// <summary>
        /// Sets downloading speed in bytes/second
        /// </summary>        
        public void SetSpeed(float value)
        {
            m_speed = value;
        }

        public bool IsAnyIdBeingDownloaded(List<string> ids)
        {
            bool returnValue = false;
            if (ids != null && m_downloader.IsDownloading && m_catalogEntryDownloading != null)
            {
                int count = ids.Count;
                for (int i = 0; i < count && !returnValue; i++)
                {
                    returnValue = (m_catalogEntryDownloading.Id == ids[i]);                    
                }
            }

            return returnValue;
        }

#if USE_DUMPER
        public Dumper GetDumper()
        {
            return m_dumper;
        }
#endif

        public void Update()
        {         
            if (IsInitialized && IsEnabled)
            {
                if (Time.realtimeSinceStartup >= m_nextUpdateAt)
                {
                    m_nextUpdateAt = Time.realtimeSinceStartup + 1f;

                    m_downloader.Update();
                    m_disk.Update();
                    m_cleaner.Update();
                    Catalog_Update();
                    Groups_Update();

#if USE_DUMPER
                    m_dumper.Update();
#endif
                } 
            }
        }               

#region catalog
        private Dictionary<string, CatalogEntryStatus> m_catalog;
        private CatalogEntryStatus m_catalogEntryDownloading;

        private void Catalog_Reset()
        {            
            if (m_catalog == null)
            {
                m_catalog = new Dictionary<string, CatalogEntryStatus>();
            }
            else
            {
                m_catalog.Clear();
            }

            m_catalogEntryDownloading = null;
        }

        private void Catalog_AddEntryStatus(string id, JSONNode json)
        {
            CatalogEntryStatus entry = new CatalogEntryStatus();
            entry.LoadManifest(id, json);
            m_catalog.Add(id, entry);
        }        

        public CatalogEntryStatus Catalog_GetEntryStatus(string id)
        {
            CatalogEntryStatus entry = null;
            m_catalog.TryGetValue(id, out entry);
            return entry;
        }  

        public bool Catalog_ContainsEntryStatus(string id)
        {
            return m_catalog.ContainsKey(id);            
        }
        
        public Dictionary<string, CatalogEntryStatus> Catalog_GetEntryStatusList()
        {
            return m_catalog;
        }

        private void Catalog_Update()
        {
            CatalogEntryStatus.StaticUpdate(Time.realtimeSinceStartup, m_network.CurrentNetworkReachability);            
            
            foreach (KeyValuePair<string, CatalogEntryStatus> pair in m_catalog)
            {
                pair.Value.Update();                
            }

            if (!m_downloader.IsDownloading)
            {
                m_catalogEntryDownloading = null;

                if (Groups_PrioritiesDirty)
                {
                    Groups_SetupPriorities();
                    Groups_PrioritiesDirty = false;
                }

                // Groups are sorted by priority, so as soon as we exit the loop as sson as we get an entry to download
                int count = m_groupsSortedByPriority.Count;
                CatalogEntryStatus entryToDownload = null;
                CatalogEntryStatus entryToSimulateDownload = null;
                for (int i = 0; i < count; i++)
                {
                    if (m_downloader.IsDownloadAllowed(Groups_GetIsPermissionRequested(m_groupsSortedByPriority[i].Id), Groups_GetIsPermissionGranted(m_groupsSortedByPriority[i].Id)))
                    {
                        if (FindEntryToDownloadInList(m_groupsSortedByPriority[i].EntryIds, ref entryToDownload, ref entryToSimulateDownload))
                        {
                            break;
                        }
                    }
                }

                // A simulation is performed only if there's no an actual download to perform
                if (entryToDownload == null)
                {
                    if (entryToSimulateDownload != null)
                    {
                        entryToSimulateDownload.SimulateDownload();
                    }

                    SetSpeed(0f);
                }
                else
                {
                    m_catalogEntryDownloading = entryToDownload;
                    m_downloader.StartDownloadThread(entryToDownload);
                }
            }            
        }

        private bool FindEntryToDownloadInList(List<string> entryIds, ref CatalogEntryStatus entryToDownload, ref CatalogEntryStatus entryToSimulateDownload)
        {            
            if (entryIds != null)
            {
                CatalogEntryStatus entry;
                int count = entryIds.Count;                
                for (int i = 0; i < count; i++)
                {
                    entry = Catalog_GetEntryStatus(entryIds[i]);
                    if (entry != null)
                    {
                        if (entry.State == CatalogEntryStatus.EState.InQueueForDownload)
                        {
                            if (m_downloader.ShouldDownloadWithCurrentConnection(entry))
                            {
                                ProcessCandidateToDownload(ref entryToDownload, entry, false);
                            }
                            else
                            {
                                ProcessCandidateToDownload(ref entryToSimulateDownload, entry, true);
                            }
                        }
                    }
                }                                
            }

            return entryToDownload != null || entryToSimulateDownload != null;
        }

        private void ProcessCandidateToDownload(ref CatalogEntryStatus candidateSoFar, CatalogEntryStatus entry, bool simulation)
        {
            if (candidateSoFar == null || !candidateSoFar.IsRequestRunning())
            {
                if (entry.IsRequestRunning())
                {
                    candidateSoFar = entry;
                }
                else if (IsAutomaticDownloaderEnabled && entry.CanAutomaticDownload(simulation))
                {
                    if (candidateSoFar == null || entry.IsRequestRunning())
                    {
                        candidateSoFar = entry;
                    }
                }
            }
        }        
#endregion

        // Region responsible for handling downloadable groups
#region groups
        /// <summary>
        /// Key of the default group used to store all downloadables that don't belong to any explicit group
        /// </summary>
        private const string GROUPS_DEFAULT_ID = "internalDefault";

        /// <summary>
        /// Key used for all downloadables, so if the user has granted permission for this group then all downloadables will always be downloaded
        /// </summary>
        private const string GROUPS_ALL_ID = "internalAll";

        private Dictionary<string, CatalogGroup> m_groups = new Dictionary<string, CatalogGroup>();
        private float m_groupsLastUpdateAt;

        private bool Groups_PrioritiesDirty { get; set; }

        private List<CatalogGroup> m_groupsSortedByPriority;

        private void Groups_Reset()
        {
            if (m_groups == null)
            {
                m_groups = new Dictionary<string, CatalogGroup>();
            }
            else
            {
                m_groups.Clear();
            }

            if (m_groupsSortedByPriority == null)
            {
                m_groupsSortedByPriority = new List<CatalogGroup>();
            }
            else
            {
                m_groupsSortedByPriority.Clear();
            }

            m_groupsLastUpdateAt = -1;
            Groups_PrioritiesDirty = false;
        }

        private void Groups_ResetPermissions()
        {
            foreach (KeyValuePair<string, CatalogGroup> pair in m_groups)
            {
                pair.Value.ResetPermissions();
            }
        }

        private void Groups_Init(Dictionary<string, CatalogGroup> groups)
        {
            int index = 0;
            Groups_Reset();

            if (groups != null)
            {
                foreach (KeyValuePair<string, CatalogGroup> pair in groups)
                {
                    m_groups.Add(pair.Key, pair.Value);
                    pair.Value.Index = index++;
                    m_groupsSortedByPriority.Add(pair.Value);
                }
            }

            Groups_Process();

            // Creates the default group, which will store all entries that don't belong to any other group
            List<string> entryIds = new List<string>();
            List<string> allEntryIds = new List<string>();
            CatalogGroup defaultGroup = new CatalogGroup();
            foreach (KeyValuePair<string, CatalogEntryStatus> pair in m_catalog)
            {
                allEntryIds.Add(pair.Value.Id);

                if (!pair.Value.BelongsToAnyGroup())
                {
                    pair.Value.AddGroup(defaultGroup);
                    entryIds.Add(pair.Key);                   
                }
            }

            defaultGroup.Setup(GROUPS_DEFAULT_ID, entryIds);            
            m_groups.Add(GROUPS_DEFAULT_ID, defaultGroup);
            defaultGroup.Index = index;
            m_groupsSortedByPriority.Add(defaultGroup);


            List<CatalogGroup> allOtherGroups = new List<CatalogGroup>();
            foreach (KeyValuePair<string, CatalogGroup> pair in m_groups)
            {
                allOtherGroups.Add(pair.Value);
            }

            // A group containing all entries is created. It's not added the groups sorted by priority list because it contains all asset bundles
            CatalogGroup allGroup = new CatalogGroup();
            allGroup.Setup(GROUPS_ALL_ID, allEntryIds, allOtherGroups);            
            m_groups.Add(GROUPS_ALL_ID, allGroup);

            // We make sure that groups will be sorted by priority as they've just been created
            Groups_PrioritiesDirty = true;
        }

        private void Groups_Process()
        {
            if (m_groups != null)
            {
                // Loops through every single group to link every entry status to the groups where they belong                
                int count;
                CatalogEntryStatus entryStatus;
                foreach (KeyValuePair<string, CatalogGroup> pair in m_groups)
                {                    
                    if (pair.Value.EntryIds != null)
                    {
                        count = pair.Value.EntryIds.Count;
                        for (int i = 0; i < count; i++)
                        {
                            entryStatus = Catalog_GetEntryStatus(pair.Value.EntryIds[i]);
                            if (entryStatus != null)
                            {
                                entryStatus.AddGroup(pair.Value);
                            }
                        }
                    }
                }
            }
        }

        public CatalogGroup Groups_GetGroup(string groupId)
        {
            CatalogGroup returnValue = null;
            if (!string.IsNullOrEmpty(groupId))
            {                
                m_groups.TryGetValue(groupId, out returnValue);
            }

            return returnValue;
        }

        public bool Groups_GetIsPermissionRequested(string groupId)
        {
            bool returnValue = true;

            CatalogGroup group;

            // For other groups than "all" group we need to check if the permission for "all" group has already been requested. If so then we extend that permission to every single group
            if (groupId != GROUPS_ALL_ID)
            {
                group = Groups_GetGroup(GROUPS_ALL_ID);
                if (group != null && group.PermissionRequested)
                {
                    return true;
                }
            }
             
            // Checks the permission for the group requested
            group = Groups_GetGroup(groupId);
            if (group != null)
            {
                returnValue = group.PermissionRequested;
            }            

			return returnValue;
        }

        public void Groups_SetIsPermissionRequested(string groupId, bool value)
        {                        
            if (groupId == GROUPS_ALL_ID)
            {
                foreach (KeyValuePair<string, CatalogGroup> pair in m_groups)
                {
                    pair.Value.PermissionRequested = value;
                }
            }
            else
            {
                CatalogGroup group = Groups_GetGroup(groupId);
                if (group != null)
                {
                    group.PermissionRequested = value;
                }
            }
		}

        public bool Groups_GetIsPermissionGranted(string groupId)
        {
            bool returnValue = true;

            CatalogGroup group;

            // For other groups than "all" group we need to check if the permission for "all" group has already been granted. If so then we extend that permission to every single group
            if (groupId != GROUPS_ALL_ID)
            {
                group = Groups_GetGroup(GROUPS_ALL_ID);
                if (group != null && group.PermissionOverCarrierGranted)
                {
                    return true;
                }
            }

            // Checks the permission for the group requested
            group = Groups_GetGroup(groupId);
            if (group != null)
            {
                returnValue = group.PermissionOverCarrierGranted;               
            }
			return returnValue;
        }

        public void Groups_SetIsPermissionGranted(string groupId, bool value)
        {
            if (groupId == GROUPS_ALL_ID)
            {
                foreach (KeyValuePair<string, CatalogGroup> pair in m_groups)
                {
                    pair.Value.PermissionOverCarrierGranted = value;
                }
            }
            else
            {
                CatalogGroup group = Groups_GetGroup(groupId);
                if (group != null)
                {
                    group.PermissionOverCarrierGranted = value;
                }
            }
        }

        private void Groups_Update()
        {            
            if (Time.realtimeSinceStartup - m_groupsLastUpdateAt >= 2f)
            {
                if (Groups_PrioritiesDirty)
                {
                    Groups_SetupPriorities();
                }

                foreach (KeyValuePair<string, CatalogGroup> pair in m_groups)
                {
                    pair.Value.Update();
                }

                m_groupsLastUpdateAt = Time.realtimeSinceStartup;
            }            
        }

        public Handle Groups_CreateHandle(string groupId)
        {
            ProductionHandle returnValue = new ProductionHandle();
            CatalogGroup group = Groups_GetGroup(groupId);
            if (group != null)
            {
                returnValue.Setup(groupId, group.EntryIds);                
            }

            return returnValue;
        }

        public Handle Groups_CreateHandle(HashSet<string> groupIds)
        {
            ProductionHandle returnValue = new ProductionHandle();
            if (groupIds != null)
            {
                CatalogGroup group;

                List<string> entryIds = new List<string>();
                foreach (string id in groupIds)
                {
                    group = Groups_GetGroup(id);
                    if (group != null)
                    {
                        UbiListUtils.AddRange(entryIds, group.EntryIds, false, true);
                    }
                }

                returnValue.Setup(groupIds, entryIds);
            }            
            
            return returnValue;
        }

        public Handle Groups_CreateAllGroupsHandle()
        {
            return Groups_CreateHandle(GROUPS_ALL_ID);
        }

        public void Groups_SetPriority(string groupId, int priority)
        {
            CatalogGroup group = Groups_GetGroup(groupId);
            if (group != null)
            {
                if (group.Priority != priority)
                {
                    group.Priority = priority;
                    Groups_PrioritiesDirty = true;
                }
            }
        }

        public void Groups_SetupPriorities()
        {
            m_groupsSortedByPriority.Sort(Groups_SortByPriority);
            Groups_PrioritiesDirty = false;
        }        

        private int Groups_SortByPriority(CatalogGroup x, CatalogGroup y)
        {
            if (x.Priority == y.Priority)
            {
                // Indices are also taken into consideration because List<T>.Sort is not stable (doesn't maintain the original order. We need to maintain the original order to make sure
                // that download resumes after sorting with the same group tha was already being downloaded if there's more than one group with the highest priority)
                // https://social.msdn.microsoft.com/Forums/vstudio/en-US/f5ea4976-1c3d-4e10-90e7-c7a0491fc28a/stable-sort-using-listlttgt?forum=netfxbcl
                if (x.Index == y.Index)
                {
                    return 0;
                }
                else if (x.Index > y.Index)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }

            }
            else if (x.Priority > y.Priority)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

        public List<CatalogGroup> Groups_GetSortedByPriority()
        {
            return m_groupsSortedByPriority;
        }

        public HashSet<string> Groups_GetAllGroupIds()
        {
            HashSet<string> returnValue = new HashSet<string>();
            if (m_groups != null)
            {
                foreach (KeyValuePair<string, CatalogGroup> pair in m_groups)
                {
                    returnValue.Add(pair.Key);
                }
            }

            return returnValue;
        }

        public CatalogGroup Groups_GetGroupAll()
        {
            CatalogGroup returnValue = null;
            if (m_groups != null)
            {
                m_groups.TryGetValue(GROUPS_ALL_ID, out returnValue);
            }

            return returnValue;
        }
#endregion

#region logger
        private static Logger sm_logger;

        public bool CanLog()
        {
            return sm_logger != null && sm_logger.CanLog();
        }

        public void Log(string msg)
        {
            if (CanLog())
            {
                sm_logger.Log(msg);
            }
        }

        public void LogWarning(string msg)
        {
            if (CanLog())
            {
                sm_logger.LogWarning(msg);
            }
        }

        public void LogError(string msg)
        {
            if (CanLog())
            {
                sm_logger.LogError(msg);
            }
        }        
#endregion
    }
}
