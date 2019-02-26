using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CPTabDownloadables : MonoBehaviour
{
    public class DownloadableView
    {
        private TMP_Text m_downloadedNameText;
        private Slider m_donwloadedSoFarSlider;
        private TMP_Text m_donwloadedSoFarText;

        Downloadables.CatalogEntryStatus m_entry;

        public void Setup(GameObject go, Downloadables.CatalogEntryStatus entry)
        {
            m_entry = entry;
            if (m_entry != null)
            {
                m_downloadedNameText = go.FindComponentRecursive<TMP_Text>("NameText");
                m_downloadedNameText.text = entry.Id;

                m_donwloadedSoFarSlider = go.FindComponentRecursive<Slider>("Slider");
                m_donwloadedSoFarSlider.minValue = 0f;
                m_donwloadedSoFarSlider.maxValue = m_entry.GetTotalBytes();

                m_donwloadedSoFarText = go.FindComponentRecursive<TMP_Text>("Text");
            }
        }

        public void Update()
        {
            if (m_donwloadedSoFarSlider != null)
            {
                m_donwloadedSoFarSlider.value = m_entry.GetBytesDownloadedSoFar();
                m_donwloadedSoFarText.text = m_entry.GetMbDownloadedSoFar() + "Mb";
            }
        }
    }

    private List<DownloadableView> m_views = new List<DownloadableView>();

    [SerializeField]
    private GameObject m_downloadableViewPrefab;

    [SerializeField]
    private Transform m_downloadableViewsRoot;

    void Start()
    {        
        DownloadableView view;
        GameObject go;
        Dictionary<string, Downloadables.CatalogEntryStatus> catalog = AssetBundlesManager.Instance.GetDownloadablesCatalog();
        if (catalog != null)
        {
            foreach (KeyValuePair<string, Downloadables.CatalogEntryStatus> pair in catalog)
            {
                go = Instantiate(m_downloadableViewPrefab, m_downloadableViewsRoot);
                go.SetActive(true);

                view = new DownloadableView();
                view.Setup(go, pair.Value);
                m_views.Add(view);
            }
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
