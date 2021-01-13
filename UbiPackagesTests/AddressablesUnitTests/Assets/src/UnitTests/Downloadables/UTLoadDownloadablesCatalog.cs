using SimpleJSON;
using System.IO;

public class UTLoadDownloadablesCatalog : UnitTest
{
    private static string ROOT_PATH = "Assets/Editor/Downloadables/UnitTests/Catalogs/00";    

    private static Logger sm_logger = new ConsoleLogger("UTLoadDownloadablesCatalog");

    public static UnitTestBatch GetUnitTestBatch()
    {
        UnitTestBatch batch = new UnitTestBatch("UTLoadDownloadablesCatalog");

        UTLoadDownloadablesCatalog test = new UTLoadDownloadablesCatalog();
        test.Setup("downloadablesCatalog.json");
        batch.AddTest(test, true);

        test = new UTLoadDownloadablesCatalog();
        test.Setup("downloadablesWithDuplicatesCatalog.json");
        batch.AddTest(test, true);        

        return batch;
    }

    private string m_path;

    public void Setup(string path)
    {
        m_path = path;
    }

    protected override void ExtendedPerform()
    {
        string path = Path.Combine(ROOT_PATH, m_path);

        // Loads the catalog        
        StreamReader reader = new StreamReader(path);
        string content = reader.ReadToEnd();
        reader.Close();

        JSONNode catalogJSON = JSON.Parse(content);
        Downloadables.Catalog catalog = new Downloadables.Catalog();
        catalog.Load(catalogJSON, sm_logger);

        JSONNode jsonAfterLoading = catalog.ToJSON();
        sm_logger.Log("Catalog after loading: " + jsonAfterLoading.ToString());

        NotifyPasses(true);
    }
}
