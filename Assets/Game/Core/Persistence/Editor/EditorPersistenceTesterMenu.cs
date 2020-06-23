using UnityEditor;

public class EditorPersistenceTesterMenu
{
    private const string TESTER_MENU = EditorPersistenceMenu.PERSISTENCE_MENU + "/" + "Tester";

    private const string TESTER_MENU_NONE = TESTER_MENU + "/None";
    private const string TESTER_MENU_PERFORM_SELECTED = TESTER_MENU + "/Perform selected";
    private const string TESTER_MENU_01 = TESTER_MENU + "/01";
    private const string TESTER_MENU_02 = TESTER_MENU + "/02";
    private const string TESTER_MENU_03 = TESTER_MENU + "/03";
    private const string TESTER_MENU_04 = TESTER_MENU + "/04";
    private const string TESTER_MENU_05 = TESTER_MENU + "/05";
    private const string TESTER_MENU_06 = TESTER_MENU + "/06";
    private const string TESTER_MENU_07 = TESTER_MENU + "/07";
    private const string TESTER_MENU_08 = TESTER_MENU + "/08";
    private const string TESTER_MENU_09 = TESTER_MENU + "/09";
    private const string TESTER_MENU_10 = TESTER_MENU + "/10";


    private static PersistenceTester sm_persistenceTester = new PersistenceTester();

    private static int CurrentTestId
    {
        get
        {
            return sm_persistenceTester.CurrentTestId;           
        }

        set
        {
            sm_persistenceTester.BeginTest(value);
        }
    }

    [MenuItem(TESTER_MENU_NONE)]
    public static void Test_SetNone()
    {
        CurrentTestId = 0;
    }

    [MenuItem(TESTER_MENU_NONE, true)]
    public static bool Test_SetNoneValidate()
    {
        Menu.SetChecked(TESTER_MENU_NONE, CurrentTestId == 0);
        return true;
    }
    
    [MenuItem(TESTER_MENU_PERFORM_SELECTED)]
    public static void Test_PerformSelected()
    {
        sm_persistenceTester.BeginTest(CurrentTestId);
    }    

    [MenuItem(TESTER_MENU_01)]
    public static void Test_Set01()
    {
        CurrentTestId = 1;
    }

    [MenuItem(TESTER_MENU_01, true)]
    public static bool Test_Set01Validate()
    {
        Menu.SetChecked(TESTER_MENU_01, CurrentTestId == 1);
        return true;
    }

    [MenuItem(TESTER_MENU_02)]
    public static void Test_Set02()
    {
        CurrentTestId = 2;
    }

    [MenuItem(TESTER_MENU_02, true)]
    public static bool Test_Set02Validate()
    {
        Menu.SetChecked(TESTER_MENU_02, CurrentTestId == 2);
        return true;
    }

    [MenuItem(TESTER_MENU_03)]
    public static void Test_Set03()
    {
        CurrentTestId = 3;
    }

    [MenuItem(TESTER_MENU_03, true)]
    public static bool Test_Set03Validate()
    {
        Menu.SetChecked(TESTER_MENU_03, CurrentTestId == 3);
        return true;
    }

    [MenuItem(TESTER_MENU_04)]
    public static void Test_Set04()
    {
        CurrentTestId = 4;
    }

    [MenuItem(TESTER_MENU_04, true)]
    public static bool Test_Set04Validate()
    {
        Menu.SetChecked(TESTER_MENU_04, CurrentTestId == 4);
        return true;
    }

    [MenuItem(TESTER_MENU_05)]
    public static void Test_Set05()
    {
        CurrentTestId = 5;
    }

    [MenuItem(TESTER_MENU_05, true)]
    public static bool Test_Set05Validate()
    {
        Menu.SetChecked(TESTER_MENU_05, CurrentTestId == 5);
        return true;
    }

    [MenuItem(TESTER_MENU_06)]
    public static void Test_Set06()
    {
        CurrentTestId = 6;
    }

    [MenuItem(TESTER_MENU_06, true)]
    public static bool Test_Set06Validate()
    {
        Menu.SetChecked(TESTER_MENU_06, CurrentTestId == 6);
        return true;
    }

    [MenuItem(TESTER_MENU_07)]
    public static void Test_Set07()
    {
        CurrentTestId = 7;
    }

    [MenuItem(TESTER_MENU_07, true)]
    public static bool Test_Set07Validate()
    {
        Menu.SetChecked(TESTER_MENU_07, CurrentTestId == 7);
        return true;
    }

    [MenuItem(TESTER_MENU_08)]
    public static void Test_Set08()
    {
        CurrentTestId = 8;
    }

    [MenuItem(TESTER_MENU_08, true)]
    public static bool Test_Set08Validate()
    {
        Menu.SetChecked(TESTER_MENU_08, CurrentTestId == 8);
        return true;
    }

    [MenuItem(TESTER_MENU_09)]
    public static void Test_Set09()
    {
        CurrentTestId = 9;
    }

    [MenuItem(TESTER_MENU_09, true)]
    public static bool Test_Set09Validate()
    {
        Menu.SetChecked(TESTER_MENU_09, CurrentTestId == 9);
        return true;
    }

    [MenuItem(TESTER_MENU_10)]
    public static void Test_Set10()
    {
        CurrentTestId = 10;
    }

    [MenuItem(TESTER_MENU_10, true)]
    public static bool Test_Set10Validate()
    {
        Menu.SetChecked(TESTER_MENU_10, CurrentTestId == 10);
        return true;
    }
}
