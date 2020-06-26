using UnityEditor;

public class EditorPersistenceMenu
{
    public const string PERSISTENCE_MENU = "Tech/Persistence";

    private const string PERSISTENCE_MENU_CORRUPT_LOCAL_PROGRESS = PERSISTENCE_MENU + "/Corrupt Local Progress";

    private static PersistenceLocalDriver m_localDriver;
    private static PersistenceLocalDriver LocalDriver
    {
        get
        {
            if (m_localDriver == null)
            {
                m_localDriver = new PersistenceLocalDriver();
            }

            return m_localDriver;
        }
    }

    [MenuItem(PERSISTENCE_MENU_CORRUPT_LOCAL_PROGRESS)]
    public static void Test_CorruptLocalProgress()
    {
        LocalDriver.OverrideWithCorruptProgress(null);
    }
}