/// <summary>
/// This class is responsible for storing a collection of settings for a particular flavour.
///
/// Following Android Studio approach (https://developer.android.com/studio/build/build-variants) for handling build variants
/// the app can have different flavours.
///
/// Some game features are configurable (Example: Splash screen, social platform, ...).
/// Every single configuration is called "flavour". 
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

    public bool IsSIWAEnabled
    {
        get;
        private set;
    }   

    public void Setup(string sku, SocialUtils.EPlatform socialPlatform, bool isSIWAEnabled)
    {
        Sku = sku;
        SocialPlatform = socialPlatform;
        IsSIWAEnabled = isSIWAEnabled;
    }
}
