public class AddressablesError
{
    public enum EType
    {
        Error_Manager_Not_initialized,  // Triggered when the user tries to use AddressablesManager before calling AddressablesManager.Initialize()
        Error_Invalid_Scene,            // Triggered when a scene couldn't be loaded or unloaded because it has not been added to the build settings or the AssetBundle has not been loaded        
        Error_Asset_Bundles,            // Triggered by an error when trying to retrieve an asset stored in asset bundles. The particular error will be stored in AssetBundleError 
    }

    public EType Type { get; set; }

    public AssetBundlesOp.EResult AssetBundlesError;

    public AddressablesError(EType type, AssetBundlesOp.EResult abError = AssetBundlesOp.EResult.None)
    {
        Type = type;
        AssetBundlesError = abError;
    }

    public AddressablesError(AssetBundlesOp.EResult abError)
    {
        Type = EType.Error_Asset_Bundles;
        AssetBundlesError = abError;
    }

    public override string ToString()
    {
        string returnValue = "Error type: " + Type.ToString();
        if (Type == EType.Error_Asset_Bundles)
        {
            returnValue += " AssetBundlesError: " + AssetBundlesError.ToString();
        }

        return returnValue;
    }
}
