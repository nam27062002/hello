using System.Collections.Generic;

/// <summary>
/// This class is responsible for waiting until a list of asset bundles are downloaded or until any of those 
/// downloads fail
/// </summary>
public class DownloadAssetBundleListOp : AssetBundlesOp
{
    private List<string> m_assetBundleIds;

    protected override void ExtendedReset()
    {
        m_assetBundleIds = null;
    }

    public void Setup(List<string> assetBundleIds, OnDoneCallback onDone)
    {
        base.Setup(onDone);

        m_assetBundleIds = assetBundleIds;
    }

    protected override void ExtendedPerform()
    {
        if (m_assetBundleIds == null || m_assetBundleIds.Count == 0)
        {
            NotifySuccess(null);
        }
    }

    protected override void ExtendedUpdate()
    {
        int absDownloaded = 0;

        AssetBundlesManager abManager = AssetBundlesManager.Instance;

        AssetBundleHandle handle;
        string abName;
        EResult result = EResult.Success;        
        int count = m_assetBundleIds.Count;
        for (int i = 0; i < count && result == EResult.Success; i++)
        {
            abName = m_assetBundleIds[i];
            handle = abManager.GetAssetBundleHandle(abName);
            if (handle == null)
            {
                result = EResult.Error_AB_Handle_Not_Found;
            }
            else
            {
                if (abManager.IsAssetBundleAvailable(abName))
                {
                    absDownloaded++;
                }
                else
                {
                    Downloadables.CatalogEntryStatus entry = abManager.GetDownloadablesCatalogEntryStatus(abName);
                    if (entry == null)
                    {
                        result = EResult.Error_AB_Is_Not_Downloadable;
                    }
                    else if (!entry.IsRequestRunning())
                    {
                        
                        result = EResult.Error_AB_Download_Internal;
                        
                        Downloadables.Error error = entry.RequestError;

                        // If the request is done here then it has to be because there was an error when proocessing the request (otherwise abManager.IsAssetBundleAvailable(abName) above had been true)
                        if (error == null || error.Type == Downloadables.Error.EType.None)
                        {
                            // Unexpected error because an error is expected to be found here
                            result = EResult.Error_AB_Download_Internal;
                        }
                        else 
                        {
                            switch (error.Type)
                            {
                                case Downloadables.Error.EType.Disk_IOException:
                                    result = EResult.Error_AB_Disk_IOException;
                                    break;

                                case Downloadables.Error.EType.Disk_UnauthorizedAccess:
                                    result = EResult.Error_AB_Disk_UnauthorizedAccess;
                                    break;

                                default:
                                    result = EResult.Error_AB_Download_Internal;
                                    break;
                            }                            
                        }
                    }
                }             
            }
        }

        if (result == EResult.Success)
        {
            if (absDownloaded == count)
            {
                NotifySuccess(null);
            }
        }
        else
        {
            NotifyError(result);
        }
    }

    protected override float ExtendedProgress
    {
        get
        {
            float soFar = 0;
            int count = m_assetBundleIds.Count;
            for (int i = 0; i < count; i++)
            {
                AssetBundleHandle handle = AssetBundlesManager.Instance.GetAssetBundleHandle(m_assetBundleIds[i]);
                if (handle != null)
                {
                    soFar += AssetBundlesManager.Instance.Loader_GetProgress(handle.Id);
                }
            }

            return (count == 0) ? 1f : soFar / count;
        }
    }
}
