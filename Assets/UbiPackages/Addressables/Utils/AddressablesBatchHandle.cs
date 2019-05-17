using System.Collections.Generic;

/// <summary>
/// This class is responsible for handling a batch of addressables, typically to make it easier to deal with its dependencies
/// </summary>
public class AddressablesBatchHandle
{
    public static AddressablesManager sm_manager;

    private List<string> GroupIds;
    public List<string> DependencyIds { get; set; }

    public void AddGroup(string groupId)
    {
        if (!string.IsNullOrEmpty(groupId))
        {
            if (GroupIds == null)
            {
                GroupIds = new List<string>();
            }

            if (!GroupIds.Contains(groupId))
            {
                GroupIds.Add(groupId);

                List<string> ids = sm_manager.GetAssetBundlesGroupDependencyIds(groupId);
                DependencyIds = UbiListUtils.AddRange<string>(DependencyIds, ids, DependencyIds == null, true);
            }
        }
    }

    public void AddAddressable(string addressableId, string variant=null)
    {        
        List<string> ids = sm_manager.GetDependencyIds(addressableId, variant);
        DependencyIds = UbiListUtils.AddRange<string>(DependencyIds, ids, DependencyIds == null, true);
    }

    public void AddDependencyIds(List<string> dependencyIds)
    {
        DependencyIds = UbiListUtils.AddRange<string>(DependencyIds, dependencyIds, DependencyIds == null, true);
    }

    public List<string> RemoteDependencyIds
    {
        get
        {
            List<string> returnValue = new List<string>();

            if (DependencyIds != null)
            {
                int count = DependencyIds.Count;
                for (int i = 0; i < count; i++)
                {
                    if (sm_manager.isDependencyIdDownloadable(DependencyIds[i]))
                    {
                        returnValue.Add(DependencyIds[i]);
                    }
                }
            }

            return returnValue;
        }
    }
}