using System.Collections.Generic;

/// <summary>
/// This class is responsible for handling a batch of addressables, typically to make it easier to deal with its dependencies
/// </summary>
public class AddressablesBatchHandle
{
    public static AddressablesManager sm_manager;

    private List<string> GroupIds;
    public List<string> DependencyIds { get; set; }
    public List<string> MandatoryDependencyIds { get; set; }

    public void AddGroup(string groupId, bool mandatory=true)
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

                AddDependencyIds(ids, mandatory);                
            }
        }
    }

    public void AddAddressable(string addressableId, string variant=null, bool mandatory=true)
    {        
        List<string> ids = sm_manager.GetDependencyIds(addressableId, variant);
        AddDependencyIds(ids, mandatory);        
    }

    public void AddDependencyIds(List<string> dependencyIds, bool mandatory=true)
    {
        List<string> actualDependencyIds = dependencyIds;
        List<string> inexistentDependencyIds = null;
        if (dependencyIds != null)
        {
            int count = dependencyIds.Count;
            for (int i = 0; i < count; i++)
            {
                if (!sm_manager.ExistsDependencyId(dependencyIds[i]))
                {
                    if (inexistentDependencyIds == null)
                    {
                        inexistentDependencyIds  = new List<string>();
                    }

                    inexistentDependencyIds.Add(dependencyIds[i]);
                }
            }

            if (inexistentDependencyIds != null)
            {
                List<string> intersection;
                UbiListUtils.SplitIntersectionAndDisjoint(dependencyIds, inexistentDependencyIds, out intersection, out actualDependencyIds);

                if (AddressablesManager.CanLog())
                {
                    UbiListUtils.GetListAsString(inexistentDependencyIds);
                    AddressablesManager.LogWarning("Inexistent dependency ids: " + UbiListUtils.GetListAsString(inexistentDependencyIds) + " have been requested to be loaded");
                }
            }
        }

        DependencyIds = UbiListUtils.AddRange<string>(DependencyIds, actualDependencyIds, DependencyIds == null, true);

        if (mandatory)
        {
            MandatoryDependencyIds = UbiListUtils.AddRange<string>(MandatoryDependencyIds, actualDependencyIds, MandatoryDependencyIds == null, true);
        }
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