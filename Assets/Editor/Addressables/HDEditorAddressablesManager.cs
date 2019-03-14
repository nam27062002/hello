using SimpleJSON;

public class HDEditorAddressablesManager : EditorAddressablesManager
{
    public override void CustomizeEditorCatalog()
    {
        Debug.Log("HDEditorAddressablesManager Customizing editor catalog...");
    }

    protected override JSONNode GetExternalAddressablesCatalogJSON()
    {
        // Scripts to generate game related asset bundles here
        return null;
    }
}
