public class PersistenceFacadeConfigDebug : PersistenceFacadeConfig
{
    public enum EUserCaseId
    {
        Production,

        Custom_Local,        

        Error_Local_Load_NotFound,
        Error_Local_Load_Permission,
        Error_Local_Load_Corrupted,
        Error_Local_Save_DiskSpace,
        Error_Local_Save_Permission,
        Error_Local_Load_Corrupted_Save_Permission,

        Error_Cloud_NotConnection,      
        Error_Cloud_Server_NotLogged,
        Error_Cloud_Server_Persistence,
        Error_Cloud_Social_NotLogged, 
    };

    private EUserCaseId UserCaseId { get; set;  }

    public PersistenceFacadeConfigDebug(EUserCaseId id)
    {
        UserCaseId = id;
    }

    protected override PersistenceSyncOpFactory GetFactory(EFactoryType type)
    {
        // Factories are regenerated so the same user case can be tested several times
        Setup();
        return base.GetFactory(type);
    }

    protected override void Setup()
    {        
        switch (UserCaseId)
        {
            case EUserCaseId.Production:
                base.Setup();
                break;

            case EUserCaseId.Custom_Local:
                SetupCustomLocal();
                break;

            case EUserCaseId.Error_Local_Load_NotFound:
                SetupErrorLocalLoadNotFound();
                break;

            case EUserCaseId.Error_Local_Load_Permission:
                SetupErrorLocalLoadPermission();
                break;

            case EUserCaseId.Error_Local_Load_Corrupted:
                SetupErrorLocalLoadCorrupted();
                break;

            case EUserCaseId.Error_Local_Save_DiskSpace:
                SetupErrorLocalSaveDiskSpace();
                break;

            case EUserCaseId.Error_Local_Save_Permission:
                SetupErrorLocalSavePermission();
                break;

            case EUserCaseId.Error_Local_Load_Corrupted_Save_Permission:
                SetupErrorLocalLoadCorruptedSavePermission();
                break;

            case EUserCaseId.Error_Cloud_NotConnection:
                SetupErrorCloudConnection(PersistenceStates.ESyncResult.Error_Cloud_NotConnection);
                break;

            case EUserCaseId.Error_Cloud_Server_NotLogged:
                SetupErrorCloudConnection(PersistenceStates.ESyncResult.Error_Cloud_Server_NotLogged);
                break;

            case EUserCaseId.Error_Cloud_Social_NotLogged:
                SetupErrorCloudConnection(PersistenceStates.ESyncResult.Error_Cloud_Social_NotLogged);
                break;

            case EUserCaseId.Error_Cloud_Server_Persistence:
                SetupErrorCloudConnection(PersistenceStates.ESyncResult.Error_Cloud_Server_Persistence);                
                break;
        }
    }

    private void SetupCustomLocal()
    {
        // From Launch        
        // The game loads with the persistence defined here
        // No login to fb (show incentive)
        // No cloud synced

        string persistence = "{\"userProfile\":{\"sc\":\"10000:0\",\"pc\":\"0:0\",\"keys\":\"3:0\",\"currentDragon\":\"dragon_baby\",\"currentLevel\":\"level_0\",\"timestamp\":\"09 / 06 / 2017 07:31:01\",\"gf\":\"0:0\",\"tutorialStep\":\"131070\",\"furyUsed\":\"False\",\"gamesPlayed\":\"0\",\"highScore\":\"0\",\"superFuryProgression\":\"0\"},\"dragons\":[{\"sku\":\"dragon_baby\",\"owned\":\"True\",\"xp\":\"0\",\"level\":\"0\",\"disguise\":\"dragon_baby_0\",\"pets\":[\"\"]}],\"User\":{\"NumGameLoops\":0},\"disguises\":[{\"disguise\":\"dragon_baby_0\",\"level\":\"3\"},{\"disguise\":\"dragon_crocodile_0\",\"level\":\"3\"},{\"disguise\":\"dragon_reptile_0\",\"level\":\"3\"},{\"disguise\":\"dragon_fat_0\",\"level\":\"3\"},{\"disguise\":\"dragon_bug_0\",\"level\":\"3\"},{\"disguise\":\"dragon_chinese_0\",\"level\":\"3\"},{\"disguise\":\"dragon_classic_0\",\"level\":\"3\"},{\"disguise\":\"dragon_devil_0\",\"level\":\"3\"},{\"disguise\":\"dragon_balrog_0\",\"level\":\"3\"},{\"disguise\":\"dragon_titan_0\",\"level\":\"3\"}],\"pets\":[],\"missions\":{\"activeMissions\":[{\"sku\":\"ftux1\",\"state\":\"3\",\"currentValue\":\"0\",\"targetValue\":\"42\",\"singleRun\":\"True\",\"cooldownStartTimestamp\":\"01 / 01 / 0001 00:00:00\"},{\"sku\":\"ftux2\",\"state\":\"3\",\"currentValue\":\"0\",\"targetValue\":\"100\",\"singleRun\":\"False\",\"cooldownStartTimestamp\":\"01 / 01 / 0001 00:00:00\"},{\"sku\":\"ftux3\",\"state\":\"3\",\"currentValue\":\"0\",\"targetValue\":\"20000\",\"singleRun\":\"False\",\"cooldownStartTimestamp\":\"01 / 01 / 0001 00:00:00\"}]},\"eggs\":{\"inventory\":[],\"incubationEndTimestamp\":\"01 / 01 / 0001 00:00:00\",\"collectedAmount\":\"0\",\"goldenEggsCollected\":\"0\"},\"chests\":{\"chests\":[{\"state\":\"1\",\"spawnPointID\":\" - \"},{\"state\":\"1\",\"spawnPointID\":\" - \"},{\"state\":\"1\",\"spawnPointID\":\" - \"},{\"state\":\"1\",\"spawnPointID\":\" - \"},{\"state\":\"1\",\"spawnPointID\":\" - \"}],\"resetTimestamp\":\"09 / 06 / 2017 17:12:42\"},\"dailyRemoveMissionAdTimestamp\":\"09 / 05 / 2017 17:12:39\",\"dailyRemoveMissionAdUses\":\"0\",\"mapResetTimestamp\":\"09 / 05 / 2017 17:12:39\",\"globalEvents\":[],\"Tracking\":{\"userID\":\"\",\"socialPlatform\":\"\",\"socialID\":\"\",\"accID\":0,\"sessionCount\":0,\"gameRoundCount\":0,\"totalPlaytime\":0,\"totalPurchases\":0,\"totalStoreVisits\":0,\"adsCount\":0,\"adsSessions\":0,\"firstLoading\":true},\"deviceName\":\"BCDTDAVIDGERM\",\"modifiedTime\":1504683061}";
        //string persistence = "{\'userProfile\':{\'sc\':\'10000:0\',\'pc\':\'0:0\',\'keys\':\'3:0\',\'currentDragon\':\'dragon_baby\',\'currentLevel\':\'level_0\',\'timestamp\':\'09 / 06 / 2017 07:31:01\',\'gf\':\'0:0\',\'tutorialStep\':\'131070\',\'furyUsed\':\'False\',\'gamesPlayed\':\'0\',\'highScore\':\'0\',\'superFuryProgression\':\'0\'},\'dragons\':[{\'sku\':\'dragon_baby\',\'owned\':\'True\',\'xp\':\'0\',\'level\':\'0\',\'disguise\':\'dragon_baby_0\',\'pets\':[\'\']}],\'User\':{\'NumGameLoops\':0},\'disguises\':[{\'disguise\':\'dragon_baby_0\',\'level\':\'3\'},{\'disguise\':\'dragon_crocodile_0\',\'level\':\'3\'},{\'disguise\':\'dragon_reptile_0\',\'level\':\'3\'},{\'disguise\':\'dragon_fat_0\',\'level\':\'3\'},{\'disguise\':\'dragon_bug_0\',\'level\':\'3\'},{\'disguise\':\'dragon_chinese_0\',\'level\':\'3\'},{\'disguise\':\'dragon_classic_0\',\'level\':\'3\'},{\'disguise\':\'dragon_devil_0\',\'level\':\'3\'},{\'disguise\':\'dragon_balrog_0\',\'level\':\'3\'},{\'disguise\':\'dragon_titan_0\',\'level\':\'3\'}],\'pets\':[],\'missions\':{\'activeMissions\':[{\'sku\':\'ftux1\',\'state\':\'3\',\'currentValue\':\'0\',\'targetValue\':\'42\',\'singleRun\':\'True\',\'cooldownStartTimestamp\':\'01 / 01 / 0001 00:00:00\'},{\'sku\':\'ftux2\',\'state\':\'3\',\'currentValue\':\'0\',\'targetValue\':\'100\',\'singleRun\':\'False\',\'cooldownStartTimestamp\':\'01 / 01 / 0001 00:00:00\'},{\'sku\':\'ftux3\',\'state\':\'3\',\'currentValue\':\'0\',\'targetValue\':\'20000\',\'singleRun\':\'False\',\'cooldownStartTimestamp\':\'01 / 01 / 0001 00:00:00\'}]},\'eggs\':{\'inventory\':[],\'incubationEndTimestamp\':\'01 / 01 / 0001 00:00:00\',\'collectedAmount\':\'0\',\'goldenEggsCollected\':\'0\'},\'chests\':{\'chests\':[{\'state\':\'1\',\'spawnPointID\':\' - \'},{\'state\':\'1\',\'spawnPointID\':\' - \'},{\'state\':\'1\',\'spawnPointID\':\' - \'},{\'state\':\'1\',\'spawnPointID\':\' - \'},{\'state\':\'1\',\'spawnPointID\':\' - \'}],\'resetTimestamp\':\'09 / 06 / 2017 17:12:42\'},\'dailyRemoveMissionAdTimestamp\':\'09 / 05 / 2017 17:12:39\',\'dailyRemoveMissionAdUses\':\'0\',\'mapResetTimestamp\':\'09 / 05 / 2017 17:12:39\',\'globalEvents\':[],\'Tracking\':{\'userID\':\'\',\'socialPlatform\':\'\',\'socialID\':\'\',\'accID\':0,\'sessionCount\':0,\'gameRoundCount\':0,\'totalPlaytime\':0,\'totalPurchases\':0,\'totalStoreVisits\':0,\'adsCount\':0,\'adsSessions\':0,\'firstLoading\':true},\'deviceName\':\'BCDTDAVIDGERM\',\'modifiedTime\':1504683061}";

        PersistenceSyncOpFactory productionFactory = GetProductionFactory();        

        PersistenceSyncOpFactory syncFactory = new PersistenceSyncOpFactoryDebug(null);
        PersistenceSyncOpFactoryDebug debugFactory = new PersistenceSyncOpFactoryDebug(syncFactory);
        PersistenceSyncOpDebug debugOp = debugFactory.RegisterOp(PersistenceSyncOpFactoryDebug.EOpType.LoadLocal);
        debugOp.Debug_RegisterData(0f, PersistenceStates.ESyncResult.Success, persistence);        

        SetFactoryToAllTypes(productionFactory);
        SyncFromLaunchFactory = debugFactory;        
    }

    private void SetupErrorLocalLoadNotFound()
    {
        // From Launch
        // No error popup shown (is silent)
        // The game loads with the default persistence
        // No login to fb (show incentive)
        // No cloud synced
        PersistenceSyncOpFactory productionFactory = GetProductionFactory();

        PersistenceSyncOpFactory syncFactory = new PersistenceSyncOpFactoryDebug(null);
        PersistenceSyncOpFactoryDebug debugFactory = new PersistenceSyncOpFactoryDebug(syncFactory);
        PersistenceSyncOpDebug debugOp = debugFactory.RegisterOp(PersistenceSyncOpFactoryDebug.EOpType.LoadLocal);
        for (int i = 0; i < 1; i++)
        {            
            debugOp.Debug_RegisterData(0f, PersistenceStates.ESyncResult.Error_Local_Load_NotFound, null);
        }

        SetFactoryToAllTypes(productionFactory);
        SyncFromLaunchFactory = debugFactory;
    }

    private void SetupErrorLocalLoadPermission()
    {
        // From Launch
        // An error popup notifying that there's not permission to load is shown. It has a retry button
        // When clicked the same popup is shown and when clicked again the game loads with the right persistence        
        // No login to fb (show incentive)
        // No cloud synced

        PersistenceSyncOpFactory productionFactory = GetProductionFactory();

        PersistenceSyncOpFactoryDebug syncFactory = new PersistenceSyncOpFactoryDebug(null);
        PersistenceSyncOpFactoryDebug debugFactory = new PersistenceSyncOpFactoryDebug(syncFactory);
        PersistenceSyncOpDebug debugOp = debugFactory.RegisterOp(PersistenceSyncOpFactoryDebug.EOpType.LoadLocal);
        for (int i = 0; i < 2; i++)
        {
            debugOp.Debug_RegisterData(0f, PersistenceStates.ESyncResult.Error_Local_Load_Permission, null);
        }

        SetFactoryToAllTypes(productionFactory);
        SyncFromLaunchFactory = debugFactory;
    }

    private void SetupErrorLocalLoadCorrupted()
    {
        // From Launch
        // An error popup notifying that local persistence is corrupted. It has a button to override it with default persistence
        // When clicked the default persistence should be loaded
        // No login to fb (show incentive)
        // No cloud synced

        PersistenceSyncOpFactory productionFactory = GetProductionFactory();

        PersistenceSyncOpFactoryDebug syncFactory = new PersistenceSyncOpFactoryDebug(null);
        PersistenceSyncOpFactoryDebug debugFactory = new PersistenceSyncOpFactoryDebug(syncFactory);
        PersistenceSyncOpDebug debugOp = debugFactory.RegisterOp(PersistenceSyncOpFactoryDebug.EOpType.LoadLocal);        
        debugOp.Debug_RegisterData(0f, PersistenceStates.ESyncResult.Error_Local_Load_Corrupted, null);

        SetFactoryToAllTypes(productionFactory);
        SyncFromLaunchFactory = debugFactory;
    }

    private void SetupErrorLocalSaveDiskSpace()
    {
        // Add money by using a cheat, which forces a save
        // An error popup notifying that there's not enough disk space to save is shown. It has a retry button
        // When clicked the same popup is shown and when clicked again the persistence is saved successfully. If we restart the game we should see the money updated
        // No login to fb (show incentive)
        // No cloud synced

        PersistenceSyncOpFactory productionFactory = GetProductionFactory();

        PersistenceSyncOpFactoryDebug syncFactory = new PersistenceSyncOpFactoryDebug(null);
        PersistenceSyncOpFactoryDebug debugFactory = new PersistenceSyncOpFactoryDebug(syncFactory);
        PersistenceSyncOpDebug debugOp = debugFactory.RegisterOp(PersistenceSyncOpFactoryDebug.EOpType.SaveLocal);
        for (int i = 0; i < 2; i++)
        {            
            debugOp.Debug_RegisterData(0f, PersistenceStates.ESyncResult.Error_Local_Save_DiskSpace, null);
        }

        SetFactoryToAllTypes(productionFactory);
        SaveFactory = debugFactory;
    }

    private void SetupErrorLocalSavePermission()
    {
        // Add money by using a cheat, which forces a save
        // An error popup notifying that there's not permission to save is shown. It has a retry button
        // When clicked the same popup is shown and when clicked again the persistence is saved successfully. If we restart the game we should see the money updated
        // No login to fb (show incentive)
        // No cloud synced

        PersistenceSyncOpFactory productionFactory = GetProductionFactory();

        PersistenceSyncOpFactoryDebug syncFactory = new PersistenceSyncOpFactoryDebug(null);
        PersistenceSyncOpFactoryDebug debugFactory = new PersistenceSyncOpFactoryDebug(syncFactory);
        PersistenceSyncOpDebug debugOp = debugFactory.RegisterOp(PersistenceSyncOpFactoryDebug.EOpType.SaveLocal);
        for (int i = 0; i < 2; i++)
        {
            debugOp.Debug_RegisterData(0f, PersistenceStates.ESyncResult.Error_Local_Save_Permission, null);
        }

        SetFactoryToAllTypes(productionFactory);
        SaveFactory = debugFactory;
    }

    private void SetupErrorLocalLoadCorruptedSavePermission()
    {
        // From Launch
        // An error popup notifying that local persistence is corrupted. It has a button to override it with default persistence
        // When clicked the default persistence is tried to be saved but it fails so a popup is shown with a retry button
        // When clicked the same popup is shown again and when clicked retry again the default persistence is saved and the game
        // should be loaded with that persistence
        // No login to fb (show incentive)
        // No cloud synced

        PersistenceSyncOpFactory productionFactory = GetProductionFactory();

        PersistenceSyncOpFactoryDebug syncFactory = new PersistenceSyncOpFactoryDebug(null);
        PersistenceSyncOpFactoryDebug debugFactory = new PersistenceSyncOpFactoryDebug(syncFactory);

        // Load local corrupted
        PersistenceSyncOpDebug debugOp = debugFactory.RegisterOp(PersistenceSyncOpFactoryDebug.EOpType.LoadLocal);
        debugOp.Debug_RegisterData(0f, PersistenceStates.ESyncResult.Error_Local_Load_Corrupted, null);

        // Save local error
        debugOp = syncFactory.RegisterOp(PersistenceSyncOpFactoryDebug.EOpType.SaveLocal);
        for (int i = 0; i < 2; i++)
        {
            debugOp.Debug_RegisterData(0f, PersistenceStates.ESyncResult.Error_Local_Save_Permission, null);
        }

        SetFactoryToAllTypes(productionFactory);
        SyncFromLaunchFactory = debugFactory;
    }

    private void SetupErrorCloudConnection(PersistenceStates.ESyncResult cloudResult)
    {
        // From Launch
        // The game should load with local persistence and no delays because of the cloud error connection        
        // No login to fb (show incentive)
        // No cloud synced

        PersistenceSyncOpFactory productionFactory = GetProductionFactory();

        PersistenceSyncOpFactoryDebug syncFactory = new PersistenceSyncOpFactoryDebug(null);
        PersistenceSyncOpFactoryDebug debugFactory = new PersistenceSyncOpFactoryDebug(syncFactory);

        // Load cloud with no connection
        PersistenceSyncOpDebug debugOp = debugFactory.RegisterOp(PersistenceSyncOpFactoryDebug.EOpType.LoadCloud);
        debugOp.Debug_RegisterData(5f, cloudResult, null);
        
        SetFactoryToAllTypes(productionFactory);
        SyncFromLaunchFactory = debugFactory;
        SyncFromSettingsFactory = debugFactory;
    }    
}
