using Downloadables;
using SimpleJSON;
using System.Collections.Generic;
using System.IO;

public class UTDownloadablesPermission : UnitTest
{    
    private Logger sm_logger = new ConsoleLogger("UTDownloadablesPermission");

    public static UnitTestBatch GetUnitTestBatch()
    {
        UnitTestBatch batch = new UnitTestBatch("UTDownloadablesPermission");
        ProductionDiskDriver diskDriver = new ProductionDiskDriver();        
        UTDownloadablesPermission test;        
        Dictionary<string, List<string>> abGroups;

        //
        // SUCCESS
        //

        // PURPOSE: An ab only belongs to a group, which permission granted
        // INPUT: 
        //      g1: asset_cubes pg: 1                
        test = new UTDownloadablesPermission();
        abGroups = new Dictionary<string, List<string>>();
        abGroups.Add("g1", new List<string> { "asset_cubes" });        
        test.Setup(diskDriver, "01", "permission_g1_true", abGroups, "asset_cubes", true);
        batch.AddTest(test, true);

        // PURPOSE: An ab only belongs to a group, which permission not granted
        // INPUT: 
        //      g1: asset_cubes pg: 0                
        test = new UTDownloadablesPermission();
        abGroups = new Dictionary<string, List<string>>();
        abGroups.Add("g1", new List<string> { "asset_cubes" });
        test.Setup(diskDriver, "01", "permission_g1_false", abGroups, "asset_cubes", false);
        batch.AddTest(test, true);

        // PURPOSE: An ab only belongs to a group with no permission file
        // INPUT: 
        //      g1: asset_cubes                
        test = new UTDownloadablesPermission();
        abGroups = new Dictionary<string, List<string>>();
        abGroups.Add("g1", new List<string> { "asset_cubes" });
        test.Setup(diskDriver, "01", "asset_cubes_orig", abGroups, "asset_cubes", false);
        batch.AddTest(test, true);

        // PURPOSE: An ab belongs to two groups with no permission file
        // INPUT: 
        //      g1: asset_cubes                
        //      g2: asset_cubes
        test = new UTDownloadablesPermission();
        abGroups = new Dictionary<string, List<string>>();
        abGroups.Add("g1", new List<string> { "asset_cubes" });
        abGroups.Add("g2", new List<string> { "asset_cubes" });
        test.Setup(diskDriver, "01", "asset_cubes_orig", abGroups, "asset_cubes", false);
        batch.AddTest(test, true);        

        // PURPOSE: An ab belongs to two groups
        // INPUT: 
        //      g1: asset_cubes pg:1
        //      g2: asset_cubes no permissions file               
        test = new UTDownloadablesPermission();
        abGroups = new Dictionary<string, List<string>>();
        abGroups.Add("g1", new List<string> { "asset_cubes" });
        abGroups.Add("g2", new List<string> { "asset_cubes" });
        test.Setup(diskDriver, "01", "permission_g1_true", abGroups, "asset_cubes", true);
        batch.AddTest(test, true);

        // PURPOSE: An ab belongs to two groups
        // INPUT: 
        //      g1: asset_cubes pg:0
        //      g2: asset_cubes no permissions file               
        test = new UTDownloadablesPermission();
        abGroups = new Dictionary<string, List<string>>();
        abGroups.Add("g1", new List<string> { "asset_cubes" });
        abGroups.Add("g2", new List<string> { "asset_cubes" });
        test.Setup(diskDriver, "01", "permission_g1_false_g2_false", abGroups, "asset_cubes", false);
        batch.AddTest(test, true);        

        // PURPOSE: An ab belongs to two groups
        // INPUT: 
        //      g1: asset_cubes pg:0
        //      g2: asset_cubes pg:1
        test = new UTDownloadablesPermission();
        abGroups = new Dictionary<string, List<string>>();
        abGroups.Add("g1", new List<string> { "asset_cubes" });
        abGroups.Add("g2", new List<string> { "asset_cubes" });
        test.Setup(diskDriver, "01", "permission_g1_false_g2_true", abGroups, "asset_cubes", true);
        batch.AddTest(test, true);                
        
        // PURPOSE: An ab only belongs to a group, which permission has been granted but it's revoked
        // INPUT: 
        //      g1: asset_cubes pg: 1                
        test = new UTDownloadablesPermission();
        abGroups = new Dictionary<string, List<string>>();
        abGroups.Add("g1", new List<string> { "asset_cubes" });
        test.Setup(diskDriver, "01", "permission_g1_true", abGroups, "asset_cubes", false, "g1", false);
        batch.AddTest(test, true);        
        
        // PURPOSE: An ab only belongs to a group, which permission has not been granted and then it's granted
        // INPUT: 
        //      g1: asset_cubes pg: 0                
        test = new UTDownloadablesPermission();
        abGroups = new Dictionary<string, List<string>>();
        abGroups.Add("g1", new List<string> { "asset_cubes" });
        test.Setup(diskDriver, "01", "permission_g1_false", abGroups, "asset_cubes", true, "g1", true);
        batch.AddTest(test, true);        

        // PURPOSE: An ab belongs to two groups. Permission is granted only for one of them but it's revoked
        // INPUT: 
        //      g1: asset_cubes pg:0
        //      g2: asset_cubes pg:1
        test = new UTDownloadablesPermission();
        abGroups = new Dictionary<string, List<string>>();
        abGroups.Add("g1", new List<string> { "asset_cubes" });
        abGroups.Add("g2", new List<string> { "asset_cubes" });
        test.Setup(diskDriver, "01", "permission_g1_false_g2_true", abGroups, "asset_cubes", false, "g2", false);
        batch.AddTest(test, true);        

        // PURPOSE: An ab belongs to two groups. Permission is granted for both groups and the permission is revoked for
        // one of them which doens't revoke the permission for the asset bundle because the other group is still granted
        // INPUT: 
        //      g1: asset_cubes pg:1
        //      g2: asset_cubes pg:1
        test = new UTDownloadablesPermission();
        abGroups = new Dictionary<string, List<string>>();
        abGroups.Add("g1", new List<string> { "asset_cubes" });
        abGroups.Add("g2", new List<string> { "asset_cubes" });
        test.Setup(diskDriver, "01", "permission_g1_true_g2_true", abGroups, "asset_cubes", true, "g2", false);
        batch.AddTest(test, true);
        
        //
        // FAIL               

        return batch;
    }    

    private Manager m_manager;
    private string m_catalogPath;
    private string m_cachePath;

    private Dictionary<string, List<string>> m_abGroups;
    private string m_entryId;
    private bool m_resultPermission;
    private string m_groupToSetPermission;
    private bool m_permissionToSet;

    public void Setup(DiskDriver diskDriver, string catalogPath, string cachePath, Dictionary<string, List<string>> abGroups,
        string entryId, bool resultPermission, string groupToSetPermission = null, bool permissionToSet = false)
    {
        m_catalogPath = catalogPath;
        m_cachePath = cachePath;

        Config config = new Config();
        config.IsAutomaticDownloaderEnabled = false;
        m_manager = new Manager(config, new ProductionNetworkDriver(), diskDriver, OnDiskIssue, null, sm_logger);

        m_abGroups = abGroups;
        m_entryId = entryId;
        m_resultPermission = resultPermission;
        m_groupToSetPermission = groupToSetPermission;
        m_permissionToSet = permissionToSet;
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
        m_manager.Initialize(catalogJSON, UTDownloadablesHelper.GetGroups(m_abGroups));

        if (m_groupToSetPermission != null)
        {
            m_manager.Groups_SetIsPermissionGranted(m_groupToSetPermission, m_permissionToSet);
        }
    }

    private void OnDone()
    {
        CatalogEntryStatus entryStatus = m_manager.Catalog_GetEntryStatus(m_entryId);
        bool passes = entryStatus != null;
        if (passes)
        {
            passes = m_resultPermission == entryStatus.GetPermissionOverCarrierGranted();
        }

        if (passes && m_groupToSetPermission != null)
        {
            passes = m_permissionToSet == m_manager.Groups_GetIsPermissionGranted(m_groupToSetPermission);
        }

        NotifyPasses(passes);
    }

    public override void Update()
    {
        if (HasStarted())
        {
            m_manager.Update();

            if (UnityEngine.Time.realtimeSinceStartup - m_timeStartAt > 1f)
            {
                OnDone();
            }
        }
    }
}
