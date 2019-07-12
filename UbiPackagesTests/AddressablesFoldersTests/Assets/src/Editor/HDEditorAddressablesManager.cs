﻿using SimpleJSON;

public class HDEditorAddressablesManager : EditorAddressablesManager
{
    public override void CustomizeEditorCatalog()
    {       
    }

    protected override JSONNode GetExternalAddressablesCatalogJSON()
    {
#if UNITY_EDITOR
        // Scripts to generate game related asset bundles here
        return EditorAutomaticAddressables.BuildCatalog(AddressablesManager.Mode == AddressablesManager.EMode.AllInLocalAssetBundles);
#else
        return null;
#endif
    }
}
