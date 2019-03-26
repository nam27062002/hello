using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CPTabDownloadables : MonoBehaviour
{
    public class GroupView
    {
        private Toggle m_carrierPermissionToggle;                
        private List<AssetBundleView> m_views = new List<AssetBundleView>();
        private int m_priority;
        private TMP_Text m_priorityText;
        public Downloadables.CatalogGroup Group { get; private set; }
        public GameObject ViewGO { get; private set; }
        
        public void Setup(Downloadables.CatalogGroup group, GameObject groupViewGO, GameObject assetBundleViewPrefab)
        {
            Group = group;

            ViewGO = groupViewGO;

            TMP_Text nameText = groupViewGO.FindComponentRecursive<TMP_Text>("NameText");                            
            nameText.text = group.Id;            

            m_carrierPermissionToggle = groupViewGO.FindComponentRecursive<Toggle>("CarrierPermissionToggle");            
            
            Transform assetBundleViewsRoot = groupViewGO.FindComponentRecursive<Transform>("ItemsRoot");

            m_priorityText = ViewGO.FindComponentRecursive<TMP_Text>("PriorityText");
            UpdatePriorityText(true);

            if (group.EntryIds != null && group.EntryIds.Count > 0)
            {
                Dictionary<string, Downloadables.CatalogEntryStatus> catalog = AssetBundlesManager.Instance.GetDownloadablesCatalog();

                AssetBundleView view;
                GameObject assetBundleGO;
                Downloadables.CatalogEntryStatus entryStatus;
                int count = group.EntryIds.Count;               
                for (int i = 0; i < count; i++)
                {
                    catalog.TryGetValue(group.EntryIds[i], out entryStatus);    
                    if (entryStatus != null)
                    {
                        assetBundleGO = Instantiate(assetBundleViewPrefab, assetBundleViewsRoot);
                        assetBundleGO.SetActive(true);

                        view = new AssetBundleView();
                        view.Setup(assetBundleGO, entryStatus);
                        m_views.Add(view);
                    }
                }
            }
        }

        private void UpdatePriorityText(bool forced)
        {
            if (forced || m_priority != Group.Priority)
            {
                m_priority = Group.Priority;
                m_priorityText.text = "Priority:" + m_priority + "";
            }
        }
        
        public void Update()
        {
            if (m_views != null)
            {
                int count = m_views.Count;
                for (int i = 0; i < count; i++)
                {
                    m_views[i].Update();
                }
            }

            UpdatePriorityText(false);
            m_carrierPermissionToggle.isOn = Group.PermissionOverCarrierGranted;
        }
    }
    
    public class AssetBundleView
    {
        private TMP_Text m_nameText;
        private Slider m_donwloadedSoFarSlider;
        private TMP_Text m_donwloadedSoFarText;
        private TMP_Text m_errorText;
        private Downloadables.Error.EType m_errorType;
        private Downloadables.CatalogEntryStatus m_entry;

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
                m_errorText = go.FindComponentRecursive<TMP_Text>("ErrorText");
                UpdateErrorText(true);

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

        private void UpdateErrorText(bool forced)
        {
            Downloadables.Error.EType errorType = Downloadables.Error.EType.None;
            if (m_entry == null)
            {
                errorType = Downloadables.Error.EType.Internal_NotAvailable;
            }
            else if (m_entry.LatestError != null)
            {
                errorType = m_entry.LatestError.Type;
            }            

            if (!forced)
            {                           
                forced = errorType != m_errorType;                                
            }

            if (forced)
            {                
                m_errorType = errorType;
                m_errorText.text = "Error:" + errorType.ToString();
                m_errorText.color = GetColor(m_errorType);
            }
        }

        private static Color GetColor(Downloadables.Error.EType errorType)
        {
            return (errorType == Downloadables.Error.EType.None) ? Color.black : Color.red;
        }
        public void Update()
        {
            if (m_donwloadedSoFarSlider != null)
            {
                m_donwloadedSoFarSlider.value = m_entry.GetBytesDownloadedSoFar();
                m_donwloadedSoFarText.text = m_entry.GetMbDownloadedSoFar() + "Mb";                
            }            

            UpdateErrorText(false);
        }
    }
    
    private static Color COLOR_GROUP_BG_1 = new Color(167f/255f, 211f/255f, 255f/255f, 144f/255f);
    private static Color COLOR_GROUP_BG_2 = new Color(108f/255f, 251f/255f, 81f/255f, 144f/255f);    

    private List<GroupView> m_views = new List<GroupView>();
    private List<GroupView> m_viewsTemp = new List<GroupView>();

    [SerializeField]
    private GameObject m_assetBundleViewPrefab;

    [SerializeField]
    private GameObject m_groupViewPrefab;

    [SerializeField]
    private Transform m_groupViewsRoot;
    
    [SerializeField]
    private Toggle m_automaticDownloaderAllowedToggle;

    [SerializeField]
    private TMP_Dropdown m_networkSpeedDropdown;

    private List<Downloadables.CatalogGroup> m_groupsSortedByPriority;

    void Start()
    {        
        GroupView view;
        GameObject go;
        List<Downloadables.CatalogGroup> groupsSortedByPriority = AssetBundlesManager.Instance.GetDownloadablesGroupsSortedByPriority();        
        if (groupsSortedByPriority != null)
        {
            m_groupsSortedByPriority = groupsSortedByPriority.GetRange(0, groupsSortedByPriority.Count);
            int count = groupsSortedByPriority.Count;
            for (int i = 0; i < count; i++)
            {
                if (groupsSortedByPriority[i].EntryIds != null && groupsSortedByPriority[i].EntryIds.Count > 0)
                {
                    go = Instantiate(m_groupViewPrefab, m_groupViewsRoot);
                    go.SetActive(true);

                    view = new GroupView();
                    view.Setup(groupsSortedByPriority[i], go, m_assetBundleViewPrefab);
                    m_views.Add(view);

                    Image image = go.GetComponent<Image>();
                    if (image != null)
                    {
                        image.color = (i % 2 == 0) ? COLOR_GROUP_BG_1 : COLOR_GROUP_BG_2; 
                    }
                }
            }
        }        
    }    

    void OnEnable()
    {
        if (m_automaticDownloaderAllowedToggle != null)
        {
            m_automaticDownloaderAllowedToggle.isOn = HDAddressablesManager.Instance.IsAutomaticDownloaderAllowed();
        }        
    }

    void Update()
    {                        
        List<Downloadables.CatalogGroup> groupsSortedByPriority = AssetBundlesManager.Instance.GetDownloadablesGroupsSortedByPriority();
        int count = groupsSortedByPriority.Count;
        bool needsToUpdateGroupsOrder = count != m_groupsSortedByPriority.Count;
        if (!needsToUpdateGroupsOrder)
        {
            for (int i = 0; i < count && !needsToUpdateGroupsOrder; i++)
            {
                needsToUpdateGroupsOrder = m_groupsSortedByPriority[i] != groupsSortedByPriority[i];
            }
        }

        if (needsToUpdateGroupsOrder)
        {
            m_groupsSortedByPriority = groupsSortedByPriority.GetRange(0, groupsSortedByPriority.Count);
            UpdateGroupsOrder();
        }

        count = m_views.Count;
        for (int i = 0; i < count; i++)
        {
            m_views[i].Update();            
        }

#if UNITY_EDITOR
        m_networkSpeedDropdown.value = NETWORK_SPEED_SLEEP_TIME_BY_MODE.IndexOf(MockNetworkDriver.MockThrottleSleepTime);
#endif
    }

    private void UpdateGroupsOrder()
    {
        int count = m_views.Count;
        for (int i = 0; i < count; i++)
        {
            m_views[i].ViewGO.transform.parent = null;
        }
        
        count = m_groupsSortedByPriority.Count;
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < m_views.Count; j++)
            {
                if (m_views[j].Group.Id == m_groupsSortedByPriority[i].Id)
                {
                    m_views[j].ViewGO.transform.parent = m_groupViewsRoot;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Deletes all downloadables from device's storage. It's linked to UI in inspector
    /// </summary>
    public void DeleteAll()
    {
        AssetBundlesManager.Instance.DeleteAllDownloadables();        
    }

    private static List<int> NETWORK_SPEED_SLEEP_TIME_BY_MODE = new List<int>(new int[] { 0, 16, 80, 800 });

    public void NetworkSpeedSetOptionId(int optionId)
    {
        MockNetworkDriver.MockThrottleSleepTime = NETWORK_SPEED_SLEEP_TIME_BY_MODE[optionId];
    }
}
