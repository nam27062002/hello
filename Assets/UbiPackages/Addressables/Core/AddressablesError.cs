public class AddressablesError
{
    public enum EType
    {
        Error_Manager_Not_initialized,
        Error_Asset_Bundles,
    }

    public EType Type { get; set; }

    public AssetBundlesOp.EResult AssetBundleError;

    public AddressablesError(EType type, AssetBundlesOp.EResult abError = AssetBundlesOp.EResult.None)
    {
        Type = type;
        AssetBundleError = abError;
    }

    public AddressablesError(AssetBundlesOp.EResult abError)
    {
        Type = EType.Error_Asset_Bundles;
        AssetBundleError = abError;
    }
}
