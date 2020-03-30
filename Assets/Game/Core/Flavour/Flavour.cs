/// <summary>
/// Documentation: https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/%5BHD%5D+Flavours
/// 
/// This class is responsible for storing a collection of settings for a particular flavour.
///
/// </summary>
public class Flavour 
{    
    public string Sku
    {
        get;
        private set;
    }

    public SocialUtils.EPlatform SocialPlatform
    {
        get;
        private set;
    }

    public string AddressablesVariant
    {
        get;
        private set;
    }

    public bool IsSIWAEnabled
    {
        get;
        private set;
    }   
    
    public void Setup(string sku, SocialUtils.EPlatform socialPlatform, string addressablesVariant, bool isSIWAEnabled)
    {
        Sku = sku;
        SocialPlatform = socialPlatform;
        AddressablesVariant = addressablesVariant;
        IsSIWAEnabled = isSIWAEnabled;
    }    
}
