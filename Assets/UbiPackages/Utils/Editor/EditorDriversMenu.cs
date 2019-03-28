using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorDriversMenu : MonoBehaviour
{
    private const string DRIVERS_MENU = "Tech/Drivers";

#region network    
    private const string DRIVERS_NETWORK_MENU = DRIVERS_MENU + "/Network";
    private const string DRIVERS_NETWORK_REACHABILITY_MENU = DRIVERS_NETWORK_MENU + "/Reachability";
    private const string DRIVERS_NETWORK_SPEED_MENU = DRIVERS_NETWORK_MENU + "/Speed";

    private const string DRIVERS_NETWORK_REACHABILITY_PRODUCTION = DRIVERS_NETWORK_REACHABILITY_MENU + "/Production";
    private const string DRIVERS_NETWORK_REACHABILITY_NOT_REACHABLE = DRIVERS_NETWORK_REACHABILITY_MENU + "/Disconnected";
    private const string DRIVERS_NETWORK_REACHABILITY_VIA_CARRIER = DRIVERS_NETWORK_REACHABILITY_MENU + "/Carrier";
    private const string DRIVERS_NETWORK_REACHABILITY_VIA_WIFI = DRIVERS_NETWORK_REACHABILITY_MENU + "/Wifi";

    private const string DRIVERS_NETWORK_SPEED_PRODUCTION = DRIVERS_NETWORK_SPEED_MENU + "/Production";
    private const string DRIVERS_NETWORK_SPEED_MOCK_250 = DRIVERS_NETWORK_SPEED_MENU + "/Mock 250kbps";
    private const string DRIVERS_NETWORK_SPEED_MOCK_50 = DRIVERS_NETWORK_SPEED_MENU + "/Mock 50kbps";
    private const string DRIVERS_NETWORK_SPEED_MOCK_5 = DRIVERS_NETWORK_SPEED_MENU + "/Mock 5kbps";

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

    [MenuItem(DRIVERS_NETWORK_REACHABILITY_PRODUCTION)]
    public static void Network_SetReachabilityProduction()
    {
        Network_Reachability = ENetworkReachability.Production;
    }

    [MenuItem(DRIVERS_NETWORK_REACHABILITY_PRODUCTION, true)]
    public static bool Network_SetReachabilityProductionValidate()
    {
        Menu.SetChecked(DRIVERS_NETWORK_REACHABILITY_PRODUCTION, Network_Reachability == ENetworkReachability.Production);
        return true;
    }

    [MenuItem(DRIVERS_NETWORK_REACHABILITY_NOT_REACHABLE)]
    public static void Network_SetReachabilityNotReachable()
    {
        Network_Reachability = ENetworkReachability.NotReachable;
    }

    [MenuItem(DRIVERS_NETWORK_REACHABILITY_NOT_REACHABLE, true)]
    public static bool Network_SetReachabilityNotReachableValidate()
    {
        Menu.SetChecked(DRIVERS_NETWORK_REACHABILITY_NOT_REACHABLE, Network_Reachability == ENetworkReachability.NotReachable);
        return true;
    }

    [MenuItem(DRIVERS_NETWORK_REACHABILITY_VIA_CARRIER)]
    public static void Network_SetReachabilityViaCarrier()
    {
        Network_Reachability = ENetworkReachability.ReachableViaCarrierDataNetwork;
    }

    [MenuItem(DRIVERS_NETWORK_REACHABILITY_VIA_CARRIER, true)]
    public static bool Network_SetReachabilityViaCarrierValidate()
    {
        Menu.SetChecked(DRIVERS_NETWORK_REACHABILITY_VIA_CARRIER, Network_Reachability == ENetworkReachability.ReachableViaCarrierDataNetwork);
        return true;
    }

    [MenuItem(DRIVERS_NETWORK_REACHABILITY_VIA_WIFI)]
    public static void Network_SetReachabilityViaWifi()
    {
        Network_Reachability = ENetworkReachability.ReachableViaLocalAreaNetwork;
    }

    [MenuItem(DRIVERS_NETWORK_REACHABILITY_VIA_WIFI, true)]
    public static bool Network_SetReachabilityViaWifiValidate()
    {
        Menu.SetChecked(DRIVERS_NETWORK_REACHABILITY_VIA_WIFI, Network_Reachability == ENetworkReachability.ReachableViaLocalAreaNetwork);
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

    [MenuItem(DRIVERS_NETWORK_SPEED_PRODUCTION)]
    public static void Network_SpeedProduction()
    {
        Network_SpeedMode = ENetworkSpeedMode.Production;    
    }

    [MenuItem(DRIVERS_NETWORK_SPEED_PRODUCTION, true)]
    public static bool Network_SetSpeedProductionValidate()
    {
        Menu.SetChecked(DRIVERS_NETWORK_SPEED_PRODUCTION, Network_SpeedMode == ENetworkSpeedMode.Production);
        return true;
    }

    [MenuItem(DRIVERS_NETWORK_SPEED_MOCK_250)]
    public static void Network_SetSpeedMock250()
    {
        Network_SpeedMode = ENetworkSpeedMode.Mock250;
    }

    [MenuItem(DRIVERS_NETWORK_SPEED_MOCK_250, true)]
    public static bool Network_SetSpeedMock250Validate()
    {
        Menu.SetChecked(DRIVERS_NETWORK_SPEED_MOCK_250, Network_SpeedMode == ENetworkSpeedMode.Mock250);
        return true;
    }

    [MenuItem(DRIVERS_NETWORK_SPEED_MOCK_50)]
    public static void Network_SetSpeedMock50()
    {
        Network_SpeedMode = ENetworkSpeedMode.Mock50;
    }

    [MenuItem(DRIVERS_NETWORK_SPEED_MOCK_50, true)]
    public static bool Network_SetSpeedMock50Validate()
    {
        Menu.SetChecked(DRIVERS_NETWORK_SPEED_MOCK_50, Network_SpeedMode == ENetworkSpeedMode.Mock50);
        return true;
    }

    [MenuItem(DRIVERS_NETWORK_SPEED_MOCK_5)]
    public static void Network_SetSpeedMock5()
    {
        Network_SpeedMode = ENetworkSpeedMode.Mock5;
    }

    [MenuItem(DRIVERS_NETWORK_SPEED_MOCK_5, true)]
    public static bool Network_SetSpeedMock5Validate()
    {
        Menu.SetChecked(DRIVERS_NETWORK_SPEED_MOCK_5, Network_SpeedMode == ENetworkSpeedMode.Mock5);
        return true;
    }
#endregion

#region disk
    private const string DRIVERS_DISK_MENU = DRIVERS_MENU + "/Disk";
    private const string DRIVERS_DISK_PRODUCTION_MENU = DRIVERS_DISK_MENU + "/Production";
    private const string DRIVERS_DISK_NO_FREE_SPACE_MENU = DRIVERS_DISK_MENU + "/No free space";
    private const string DRIVERS_DISK_NO_PERMISSION_MENU = DRIVERS_DISK_MENU + "/No permission";

    private static bool Disk_IsProductionEnabled()
    {
        return !MockDiskDriver.IsNoFreeSpaceEnabled && !MockDiskDriver.IsNoAccessPermissionEnabled;
    }

    [MenuItem(DRIVERS_DISK_PRODUCTION_MENU)]
    public static void Disk_SetProduction()
    {
        bool production = !MockDiskDriver.IsNoFreeSpaceEnabled && !MockDiskDriver.IsNoAccessPermissionEnabled;
        if (!production)
        {
            MockDiskDriver.IsNoFreeSpaceEnabled = false;
            MockDiskDriver.IsNoAccessPermissionEnabled = false;
        }        
    }

    [MenuItem(DRIVERS_DISK_PRODUCTION_MENU, true)]
    public static bool Disk_SetProductionValidate()
    {
        Menu.SetChecked(DRIVERS_DISK_PRODUCTION_MENU, Disk_IsProductionEnabled());
        return !Disk_IsProductionEnabled();
    }

    [MenuItem(DRIVERS_DISK_NO_FREE_SPACE_MENU)]
    public static void Disk_SetIsNoFreeSpaceEnabled()
    {
        MockDiskDriver.IsNoFreeSpaceEnabled = !MockDiskDriver.IsNoFreeSpaceEnabled;
    }

    [MenuItem(DRIVERS_DISK_NO_FREE_SPACE_MENU, true)]
    public static bool Disk_SetIsNoFreeSpaceEnabledValidate()
    {
        Menu.SetChecked(DRIVERS_DISK_NO_FREE_SPACE_MENU, MockDiskDriver.IsNoFreeSpaceEnabled);
        return true;
    }

    [MenuItem(DRIVERS_DISK_NO_PERMISSION_MENU)]
    public static void Disk_SetIsNoPermissionEnabled()
    {
        MockDiskDriver.IsNoAccessPermissionEnabled = !MockDiskDriver.IsNoAccessPermissionEnabled;
    }

    [MenuItem(DRIVERS_DISK_NO_PERMISSION_MENU, true)]
    public static bool Disk_SetIsNoPermissionEnabledValidate()
    {
        Menu.SetChecked(DRIVERS_DISK_NO_PERMISSION_MENU, MockDiskDriver.IsNoAccessPermissionEnabled);
        return true;
    }
    #endregion
}