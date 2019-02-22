using UnityEngine;

/// <summary>
/// This class is responsible for defining the interface of the class that will be notified every time a relevant action related to asset bundles happens, typically it's used to track
/// asset bundles sytem's behavior
/// </summary>
public abstract class AssetBundlesTracker
{
    public enum EAction
    {
        Download,         
        Update,
        Load
    };

    public abstract void Track_ActionStart(EAction action, string assetBundleName, float existingSizeMb, float totalSizeMb, NetworkReachability reachability);
    public abstract void Track_ActionEnd(EAction action, string assetBundleName, float existingSizeMb, float totalSizeMb, int timeSpent, NetworkReachability reachability, AssetBundlesOp.EResult result);    
}
