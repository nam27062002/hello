using UnityEngine;

public class AssetBundlesUnitTest : UnitTest
{
    private string m_assetBundleId;
    private string m_assetName;
    private bool m_loadSync;

    private AssetBundlesOp.EResult m_expectedResult;

    public AssetBundlesUnitTest(string name, string assetBundleId, string assetName, bool loadSync, AssetBundlesOp.EResult expectedResult, OnDoneCallback onDone)
    {
        m_assetBundleId = assetBundleId;
        m_assetName = assetName;
        m_loadSync = loadSync;
        m_expectedResult = expectedResult;

        Setup(name, onDone);        
    }

    protected override void ExtendedPerform()
    {
        AssetBundlesManager.Instance.LoadAssetBundleAndDependencies(m_assetBundleId, OnLoadDone);
    }    

    private void OnLoadDone(AssetBundlesOp.EResult result, object data)
    {
        if (result == AssetBundlesOp.EResult.Success)
        {
            if (m_loadSync)
            {
                Object asset = AssetBundlesManager.Instance.LoadAsset(m_assetBundleId, m_assetName);                
                switch (m_expectedResult)
                {
                    case AssetBundlesOp.EResult.Success:
                        NotifyPasses(asset != null);
                        break;

                    case AssetBundlesOp.EResult.Error_Asset_Not_Found_In_AB:
                        NotifyPasses(asset == null);
                        break;

                    default:
                        NotifyPasses(false);
                        break;
                }                
            }
        }
        else
        {
            NotifyPasses(m_expectedResult == result);
        }
    }
}
