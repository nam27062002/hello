﻿using SimpleJSON;

public class HDEditorAddressablesManager : EditorAddressablesManager
{
    public override void CustomizeEditorCatalog()
    {
        Debug.Log("HDEditorAddressablesManager Customizing editor catalog...");
    }

    protected override JSONNode GetExternalAddressablesCatalogJSON()
    {
#if UNITY_EDITOR
        // Scripts to generate game related asset bundles here
        return EditorAutomaticAddressables.BuildCatalog(true);
#else
        return null;
#endif
    }
}
