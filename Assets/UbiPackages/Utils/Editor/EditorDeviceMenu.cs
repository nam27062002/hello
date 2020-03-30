using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorDeviceMenu : MonoBehaviour
{
    private const string DEVICE_MENU = "Tech/Device";

#region network    
    private const string DEVICE_NETWORK_MENU = DEVICE_MENU + "/Network";
    private const string DEVICE_NETWORK_REACHABILITY_MENU = DEVICE_NETWORK_MENU + "/Reachability";
    private const string DEVICE_NETWORK_SPEED_MENU = DEVICE_NETWORK_MENU + "/Speed";

    private const string DEVICE_NETWORK_REACHABILITY_PRODUCTION = DEVICE_NETWORK_REACHABILITY_MENU + "/Production";
    private const string DEVICE_NETWORK_REACHABILITY_NOT_REACHABLE = DEVICE_NETWORK_REACHABILITY_MENU + "/Disconnected";
    private const string DEVICE_NETWORK_REACHABILITY_VIA_CARRIER = DEVICE_NETWORK_REACHABILITY_MENU + "/Carrier";
    private const string DEVICE_NETWORK_REACHABILITY_VIA_WIFI = DEVICE_NETWORK_REACHABILITY_MENU + "/Wifi";

    private const string DEVICE_NETWORK_SPEED_PRODUCTION = DEVICE_NETWORK_SPEED_MENU + "/Production";
    private const string DEVICE_NETWORK_SPEED_MOCK_250 = DEVICE_NETWORK_SPEED_MENU + "/Mock 250kbps";
    private const string DEVICE_NETWORK_SPEED_MOCK_50 = DEVICE_NETWORK_SPEED_MENU + "/Mock 50kbps";
    private const string DEVICE_NETWORK_SPEED_MOCK_5 = DEVICE_NETWORK_SPEED_MENU + "/Mock 5kbps";

    private enum ENetworkReachability
    {
        Production = -1,
        NotReachable = 0,
        //
        // Resumen:
        //     Network is reachable via carrier data network.
        ReachableViaCarrierDataNetwork = 1,
        //
        // Resumen:
        //     Network is reachable via WiFi or cable.
        ReachableViaLocalAreaNetwork = 2,               
    };

    private static ENetworkReachability Network_Reachability
    {
        get
        {
            int index = MockNetworkDriver.MockNetworkReachabilityAsInt;            
            return (ENetworkReachability)index;
        }

        set
        {
            MockNetworkDriver.MockNetworkReachabilityAsInt = (int)value;
        }
    }

    [MenuItem(DEVICE_NETWORK_REACHABILITY_PRODUCTION)]
    public static void Network_SetReachabilityProduction()
    {
        Network_Reachability = ENetworkReachability.Production;
    }

    [MenuItem(DEVICE_NETWORK_REACHABILITY_PRODUCTION, true)]
    public static bool Network_SetReachabilityProductionValidate()
    {
        Menu.SetChecked(DEVICE_NETWORK_REACHABILITY_PRODUCTION, Network_Reachability == ENetworkReachability.Production);
        return true;
    }

    [MenuItem(DEVICE_NETWORK_REACHABILITY_NOT_REACHABLE)]
    public static void Network_SetReachabilityNotReachable()
    {
        Network_Reachability = ENetworkReachability.NotReachable;
    }

    [MenuItem(DEVICE_NETWORK_REACHABILITY_NOT_REACHABLE, true)]
    public static bool Network_SetReachabilityNotReachableValidate()
    {
        Menu.SetChecked(DEVICE_NETWORK_REACHABILITY_NOT_REACHABLE, Network_Reachability == ENetworkReachability.NotReachable);
        return true;
    }

    [MenuItem(DEVICE_NETWORK_REACHABILITY_VIA_CARRIER)]
    public static void Network_SetReachabilityViaCarrier()
    {
        Network_Reachability = ENetworkReachability.ReachableViaCarrierDataNetwork;
    }

    [MenuItem(DEVICE_NETWORK_REACHABILITY_VIA_CARRIER, true)]
    public static bool Network_SetReachabilityViaCarrierValidate()
    {
        Menu.SetChecked(DEVICE_NETWORK_REACHABILITY_VIA_CARRIER, Network_Reachability == ENetworkReachability.ReachableViaCarrierDataNetwork);
        return true;
    }

    [MenuItem(DEVICE_NETWORK_REACHABILITY_VIA_WIFI)]
    public static void Network_SetReachabilityViaWifi()
    {
        Network_Reachability = ENetworkReachability.ReachableViaLocalAreaNetwork;
    }

    [MenuItem(DEVICE_NETWORK_REACHABILITY_VIA_WIFI, true)]
    public static bool Network_SetReachabilityViaWifiValidate()
    {
        Menu.SetChecked(DEVICE_NETWORK_REACHABILITY_VIA_WIFI, Network_Reachability == ENetworkReachability.ReachableViaLocalAreaNetwork);
        return true;
    }

    private enum ENetworkSpeedMode
    {
        Production,
        Mock250,
        Mock50,
        Mock5
    };

    private static List<int> NETWORK_SPEED_SLEEP_TIME_BY_MODE = new List<int>(new int[] { 0, 16, 80, 800 });
    
    private static ENetworkSpeedMode Network_SpeedMode
    {
        get
        {
            int index = NETWORK_SPEED_SLEEP_TIME_BY_MODE.IndexOf(MockNetworkDriver.MockThrottleSleepTime);
            if (index == -1)
            {
                index = 0;
            }
            
            return (ENetworkSpeedMode)index;
        }

        set
        {            
            MockNetworkDriver.MockThrottleSleepTime = NETWORK_SPEED_SLEEP_TIME_BY_MODE[(int)value];
        }
    }

    [MenuItem(DEVICE_NETWORK_SPEED_PRODUCTION)]
    public static void Network_SpeedProduction()
    {
        Network_SpeedMode = ENetworkSpeedMode.Production;    
    }

    [MenuItem(DEVICE_NETWORK_SPEED_PRODUCTION, true)]
    public static bool Network_SetSpeedProductionValidate()
    {
        Menu.SetChecked(DEVICE_NETWORK_SPEED_PRODUCTION, Network_SpeedMode == ENetworkSpeedMode.Production);
        return true;
    }

    [MenuItem(DEVICE_NETWORK_SPEED_MOCK_250)]
    public static void Network_SetSpeedMock250()
    {
        Network_SpeedMode = ENetworkSpeedMode.Mock250;
    }

    [MenuItem(DEVICE_NETWORK_SPEED_MOCK_250, true)]
    public static bool Network_SetSpeedMock250Validate()
    {
        Menu.SetChecked(DEVICE_NETWORK_SPEED_MOCK_250, Network_SpeedMode == ENetworkSpeedMode.Mock250);
        return true;
    }

    [MenuItem(DEVICE_NETWORK_SPEED_MOCK_50)]
    public static void Network_SetSpeedMock50()
    {
        Network_SpeedMode = ENetworkSpeedMode.Mock50;
    }

    [MenuItem(DEVICE_NETWORK_SPEED_MOCK_50, true)]
    public static bool Network_SetSpeedMock50Validate()
    {
        Menu.SetChecked(DEVICE_NETWORK_SPEED_MOCK_50, Network_SpeedMode == ENetworkSpeedMode.Mock50);
        return true;
    }

    [MenuItem(DEVICE_NETWORK_SPEED_MOCK_5)]
    public static void Network_SetSpeedMock5()
    {
        Network_SpeedMode = ENetworkSpeedMode.Mock5;
    }

    [MenuItem(DEVICE_NETWORK_SPEED_MOCK_5, true)]
    public static bool Network_SetSpeedMock5Validate()
    {
        Menu.SetChecked(DEVICE_NETWORK_SPEED_MOCK_5, Network_SpeedMode == ENetworkSpeedMode.Mock5);
        return true;
    }
#endregion

#region disk
    private const string DEVICE_DISK_MENU = DEVICE_MENU + "/Disk";
    private const string DEVICE_DISK_PRODUCTION_MENU = DEVICE_DISK_MENU + "/Production";
    private const string DEVICE_DISK_NO_FREE_SPACE_MENU = DEVICE_DISK_MENU + "/No free space";
    private const string DEVICE_DISK_NO_PERMISSION_MENU = DEVICE_DISK_MENU + "/No permission";

    private static bool Disk_IsProductionEnabled()
    {
        return !MockDiskDriver.IsNoFreeSpaceEnabled && !MockDiskDriver.IsNoAccessPermissionEnabled;
    }

    [MenuItem(DEVICE_DISK_PRODUCTION_MENU)]
    public static void Disk_SetProduction()
    {
        bool production = !MockDiskDriver.IsNoFreeSpaceEnabled && !MockDiskDriver.IsNoAccessPermissionEnabled;
        if (!production)
        {
            MockDiskDriver.IsNoFreeSpaceEnabled = false;
            MockDiskDriver.IsNoAccessPermissionEnabled = false;
        }        
    }

    [MenuItem(DEVICE_DISK_PRODUCTION_MENU, true)]
    public static bool Disk_SetProductionValidate()
    {
        Menu.SetChecked(DEVICE_DISK_PRODUCTION_MENU, Disk_IsProductionEnabled());
        return !Disk_IsProductionEnabled();
    }

    [MenuItem(DEVICE_DISK_NO_FREE_SPACE_MENU)]
    public static void Disk_SetIsNoFreeSpaceEnabled()
    {
        MockDiskDriver.IsNoFreeSpaceEnabled = !MockDiskDriver.IsNoFreeSpaceEnabled;
    }

    [MenuItem(DEVICE_DISK_NO_FREE_SPACE_MENU, true)]
    public static bool Disk_SetIsNoFreeSpaceEnabledValidate()
    {
        Menu.SetChecked(DEVICE_DISK_NO_FREE_SPACE_MENU, MockDiskDriver.IsNoFreeSpaceEnabled);
        return true;
    }

    [MenuItem(DEVICE_DISK_NO_PERMISSION_MENU)]
    public static void Disk_SetIsNoPermissionEnabled()
    {
        MockDiskDriver.IsNoAccessPermissionEnabled = !MockDiskDriver.IsNoAccessPermissionEnabled;
    }

    [MenuItem(DEVICE_DISK_NO_PERMISSION_MENU, true)]
    public static bool Disk_SetIsNoPermissionEnabledValidate()
    {
        Menu.SetChecked(DEVICE_DISK_NO_PERMISSION_MENU, MockDiskDriver.IsNoAccessPermissionEnabled);
        return true;
    }
 #endregion

 #region country
    private const string DEVICE_COUNTRY_ON_INSTALL_MENU = DEVICE_MENU + "/Country At Install";
    private const string DEVICE_COUNTRY_ON_INSTALL_NONE = DEVICE_COUNTRY_ON_INSTALL_MENU + "/None";
    private const string DEVICE_COUNTRY_ON_INSTALL_WW = DEVICE_COUNTRY_ON_INSTALL_MENU + "/Worldwide";
    private const string DEVICE_COUNTRY_ON_INSTALL_CHINA = DEVICE_COUNTRY_ON_INSTALL_MENU + "/China";

    [MenuItem(DEVICE_COUNTRY_ON_INSTALL_NONE)]
    public static void CountryOnInstall_SetNone()
    {
        PersistencePrefs.CountryCodeOnInstall = "";
    }

    [MenuItem(DEVICE_COUNTRY_ON_INSTALL_NONE, true)]
    public static bool CountryOnInstall_SetNoneValidate()
    {
        Menu.SetChecked(DEVICE_COUNTRY_ON_INSTALL_NONE, string.IsNullOrEmpty(PersistencePrefs.CountryCodeOnInstall));
        return true;
    }

    [MenuItem(DEVICE_COUNTRY_ON_INSTALL_WW)]
    public static void CountryOnInstall_SetWW()
    {
        PersistencePrefs.CountryCodeOnInstall = PlatformUtils.COUNTRY_CODE_WW_DEFAULT;        
    }

    [MenuItem(DEVICE_COUNTRY_ON_INSTALL_WW, true)]
    public static bool CountryOnInstall_SetWWValidate()
    {
        Menu.SetChecked(DEVICE_COUNTRY_ON_INSTALL_WW, PersistencePrefs.CountryCodeOnInstall == PlatformUtils.COUNTRY_CODE_WW_DEFAULT);
        return true;
    }

    [MenuItem(DEVICE_COUNTRY_ON_INSTALL_CHINA)]
    public static void CountryOnInstall_SetChina()
    {
        PersistencePrefs.CountryCodeOnInstall = PlatformUtils.COUNTRY_CODE_CHINA;
    }

    [MenuItem(DEVICE_COUNTRY_ON_INSTALL_CHINA, true)]
    public static bool CountryOnInstall_SetChinaValidate()
    {
        Menu.SetChecked(DEVICE_COUNTRY_ON_INSTALL_CHINA, PersistencePrefs.CountryCodeOnInstall == PlatformUtils.COUNTRY_CODE_CHINA);
        return true;
    }
#endregion

#region country
    private const string DEVICE_PLATFORM_MENU = DEVICE_MENU + "/Platform";
    private const string DEVICE_PLATFORM_AS_BUILD_TARGET = DEVICE_PLATFORM_MENU + "/As Build Target";
    private const string DEVICE_PLATFORM_IOS = DEVICE_PLATFORM_MENU + "/iOS";
    private const string DEVICE_PLATFORM_ANDROID = DEVICE_PLATFORM_MENU + "/Android";    

    private static string Platform_GetDevicePlatformAsString()
    {
        return FlavourManager.Prefs_GetDevicePlatform();
    }

    private static void Platform_SetDevicePlatformString(string value)
    {
        FlavourManager.Prefs_SetDevicePlatform(value);
    }

    [MenuItem(DEVICE_PLATFORM_AS_BUILD_TARGET)]
    public static void Platform_SetAsBuildTarget()
    {
        Platform_SetDevicePlatformString("");
    }

    [MenuItem(DEVICE_PLATFORM_AS_BUILD_TARGET, true)]
    public static bool Platform_SetAsBuildTargetValidate()
    {
        string platform = Platform_GetDevicePlatformAsString();
        Menu.SetChecked(DEVICE_PLATFORM_AS_BUILD_TARGET, string.IsNullOrEmpty(Platform_GetDevicePlatformAsString()));
        return true;
    }

    [MenuItem(DEVICE_PLATFORM_IOS)]
    public static void Platform_SetIOS()
    {
        Platform_SetDevicePlatformString(Flavour.DEVICEPLATFORM_IOS);
    }

    [MenuItem(DEVICE_PLATFORM_IOS, true)]
    public static bool Platform_SetIOSValidate()
    {
        Menu.SetChecked(DEVICE_PLATFORM_IOS, Platform_GetDevicePlatformAsString() == Flavour.DEVICEPLATFORM_IOS);
        return true;
    }

    [MenuItem(DEVICE_PLATFORM_ANDROID)]
    public static void Platform_SetAndroid()
    {
        Platform_SetDevicePlatformString(Flavour.DEVICEPLATFORM_ANDROID);
    }

    [MenuItem(DEVICE_PLATFORM_ANDROID, true)]
    public static bool Platform_SetAndroidValidate()
    {
        string devicePlatform = Platform_GetDevicePlatformAsString();
        bool boolValue = devicePlatform == Flavour.DEVICEPLATFORM_ANDROID;
        Menu.SetChecked(DEVICE_PLATFORM_ANDROID, Platform_GetDevicePlatformAsString() == Flavour.DEVICEPLATFORM_ANDROID);
        return true;
    }
#endregion
}