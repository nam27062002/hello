using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CPTabAssetBundles : MonoBehaviour
{
    public class AssetBundlleView
    {
        private TMP_Text m_nameText;
        private Slider m_donwloadedSoFarSlider;
        private TMP_Text m_donwloadedSoFarText;
        private TMP_Text m_abLoadedText;
        private TMP_Text m_sceneLoadedText;

        Downloadables.CatalogEntryStatus m_entry;

        public void Setup(GameObject go, Downloadables.CatalogEntryStatus entry)
        {
            m_entry = entry;
            if (m_entry != null)
            {
                m_nameText = go.FindComponentRecursive<TMP_Text>("NameText");
                m_nameText.text = entry.Id;

                m_donwloadedSoFarSlider = go.FindComponentRecursive<Slider>("Slider");
                m_donwloadedSoFarSlider.minValue = 0f;
                m_donwloadedSoFarSlider.maxValue = m_entry.GetTotalBytes();

                m_donwloadedSoFarText = go.FindComponentRecursive<TMP_Text>("Text");
                m_abLoadedText = go.FindComponentRecursive<TMP_Text>("ABLoadedText");
                m_sceneLoadedText = go.FindComponentRecursive<TMP_Text>("SceneLoadedText");

                Button button = go.FindComponentRecursive<Button>("DeleteButton");
                if (button != null)
                {
                    button.onClick.AddListener(OnDelete);
                }
            }
        }

        private void OnDelete()
        {
            if (m_entry != null)
            {
                m_entry.DeleteDownload();
            }
        }

        public void Update()
        {
            if (m_donwloadedSoFarSlider != null)
            {
                m_donwloadedSoFarSlider.value = m_entry.GetBytesDownloadedSoFar();
                m_donwloadedSoFarText.text = m_entry.GetMbDownloadedSoFar() + "Mb";
                
                AssetBundleHandle handle = AssetBundlesManager.Instance.GetAssetBundleHandle(m_entry.Id);
                m_abLoadedText.text = "AB Loaded: " + handle.IsLoaded().ToString();

                m_sceneLoadedText.text = "Scene Loaded: " + LevelManager.IsSceneLoaded("SO_Medieval_Castle");
            }
        }
    }

    private List<AssetBundlleView> m_views = new List<AssetBundlleView>();

    [SerializeField]
    private GameObject m_assetBundleViewPrefab;

    [SerializeField]
    private Transform m_assetBundleViewsRoot;

    [SerializeField]
    private TMP_Text m_isAutomaticDownloaderAllowed;

    void Start()
    {        
        AssetBundlleView view;
        GameObject go;
        Dictionary<string, Downloadables.CatalogEntryStatus> catalog = AssetBundlesManager.Instance.GetDownloadablesCatalog();
        if (catalog != null)
        {
            foreach (KeyValuePair<string, Downloadables.CatalogEntryStatus> pair in catalog)
            {
                go = Instantiate(m_assetBundleViewPrefab, m_assetBundleViewsRoot);
                go.SetActive(true);

                view = new AssetBundlleView();
                view.Setup(go, pair.Value);
                m_views.Add(view);
            }
        }
    }

    void OnEnable()
    {
        if (m_isAutomaticDownloaderAllowed != null)
        {
            m_isAutomaticDownloaderAllowed.text = "Automatic Downloader Allowed: " + HDAddressablesManager.Instance.IsAutomaticDownloaderAllowed();
        }
    }

    void Update()
    {        
        int count = m_views.Count;
        for (int i = 0; i < count; i++)
        {
            m_views[i].Update();
        }        
    }
}
