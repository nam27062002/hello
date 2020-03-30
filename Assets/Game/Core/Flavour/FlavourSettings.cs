/// <summary>
/// This class is responsible for storing a Flavour configuration. The main difference with <c>Flavour</c> is that
/// <c>Flavour</c> stores the data as the user expects it whereas <c>FlavourSettings</c> stores in so the discrete
/// values supported per setting are explicit.
/// </summary>
public class FlavourSettings
{
	public enum ESocialPlatform
	{
		Facebook,
		Weibo
	};

	private static SocialUtils.EPlatform ESocialPlatformToSocialUtilsEPlatform(ESocialPlatform value)
	{
		switch (value)
		{
			case ESocialPlatform.Facebook:
				return SocialUtils.EPlatform.Facebook;

			case ESocialPlatform.Weibo:
				return SocialUtils.EPlatform.Weibo;
		}

		return SocialUtils.EPlatform.None;
	}

	public enum EAddressablesVariant
	{
		WW,
		CN
	};

	public static EAddressablesVariant ADDRESSABLES_VARIANT_DEFAULT = EAddressablesVariant.WW;
	public static string ADDRESSABLES_VARIANT_DEFAULT_SKU = EAddressablesVariantToString(ADDRESSABLES_VARIANT_DEFAULT);

	public static string EAddressablesVariantToString(EAddressablesVariant value)
	{
		return value.ToString();
	}

	public enum EDevicePlatform
	{
		iOS,
		Android
	};

	public static string DEVICEPLATFORM_IOS = EDevicePlatform.iOS.ToString();
	public static string DEVICEPLATFORM_ANDROID = EDevicePlatform.Android.ToString();

	public ESocialPlatform SocialPlatform
	{
		get;
		private set;
	}

	public EAddressablesVariant AddressablesVariant
	{
		get;
		private set;
	}

	public bool IsSIWAEnabled
	{
		get;
		private set;
	}

    public FlavourSettings(ESocialPlatform socialPlatform, EAddressablesVariant addressablesVariant, bool isSIWAEnabled)
	{
		SocialPlatform = socialPlatform;
		AddressablesVariant = addressablesVariant;
		IsSIWAEnabled = isSIWAEnabled;
	}

    public void SetupFlavour(Flavour flavour, string sku)
	{
		flavour.Setup(
			sku: sku,
			socialPlatform: ESocialPlatformToSocialUtilsEPlatform(SocialPlatform),
			addressablesVariant: EAddressablesVariantToString(AddressablesVariant),
			isSIWAEnabled: IsSIWAEnabled);
	}
}
