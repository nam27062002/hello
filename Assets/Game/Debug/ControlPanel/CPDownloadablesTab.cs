using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CPDownloadablesTab : MonoBehaviour
{
    // Exposed references
	[Separator("Setup")]
	[SerializeField] private Color[] m_groupBgColors = new Color[] {
		Colors.WithAlpha(Colors.skyBlue, 0.25f),
		Colors.WithAlpha(Colors.teal, 0.25f)
	};

	[Separator("Groups References")]
	[SerializeField] private GameObject m_groupViewPrefab = null;
    [SerializeField] private Transform m_groupViewsRoot = null;

	[Separator("Other References")]
	[SerializeField] private Toggle m_automaticDownloaderAllowedToggle = null;
    [SerializeField] private TMP_Dropdown m_networkSpeedDropdown = null;
    [SerializeField] private Toggle m_downloadCompletedPopupToggle = null;

    // Internal
    private List<Downloadables.CatalogGroup> m_groupsSortedByPriority;
	private List<CPDownloadablesGroupView> m_views = new List<CPDownloadablesGroupView>();
	private List<CPDownloadablesGroupView> m_viewsTemp = new List<CPDownloadablesGroupView>();

    [SerializeField] private GameObject m_dumperRoot;
    [SerializeField] private TMP_Text m_dumperFillUpProgress;
    [SerializeField] private Button m_dumperFillUpDisk;
    [SerializeField] private Button m_dumperCancelFillUpDisk;

    private List<Downloadables.CatalogGroup> m_groupsSortedByPriorityFromManager;

    /// <summary>
    /// 
    /// </summary>
    private void Start() { 
		// Create group views     
        CPDownloadablesGroupView groupView = null;
        GameObject newInstance = null;

        // Only one handle for all downloadables is used. 
        m_groupsSortedByPriorityFromManager = new List<Downloadables.CatalogGroup>();
        m_groupsSortedByPriorityFromManager.Add(AssetBundlesManager.Instance.Groups_GetGroupAll());
        //m_groupsFromManager = AssetBundlesManager.Instance.GetDownloadablesGroupsSortedByPriority();                                
                
        if (m_groupsSortedByPriorityFromManager != null) {
			// Cache data for further use
            m_groupsSortedByPriority = m_groupsSortedByPriorityFromManager.GetRange(0, m_groupsSortedByPriorityFromManager.Count);

			// Create a new view for each group
            int count = m_groupsSortedByPriorityFromManager.Count;
            for(int i = 0; i < count; ++i) {
				// Skip if the group doesn't have any bundle assinged
                if(m_groupsSortedByPriorityFromManager[i].EntryIds != null && m_groupsSortedByPriorityFromManager[i].EntryIds.Count > 0) {
					// Create instacne
                    newInstance = Instantiate(m_groupViewPrefab, m_groupViewsRoot);
                    newInstance.SetActive(true);

					// Initialize group view
					groupView = newInstance.GetComponent<CPDownloadablesGroupView>();
					groupView.InitWithGroup(m_groupsSortedByPriorityFromManager[i]);

					// Set alternate row colors
					groupView.SetBackgroundColor(m_groupBgColors[i % m_groupBgColors.Length]);

					// Store reference for future use
                    m_views.Add(groupView);
                }
            }
        }

		// Hide prefab instance
		m_groupViewPrefab.SetActive(false);

        Dumper_Init();
    }       

	/// <summary>
	/// 
	/// </summary>
    private void OnEnable()
    {
        if (m_automaticDownloaderAllowedToggle != null)
        {
            m_automaticDownloaderAllowedToggle.isOn = HDAddressablesManager.Instance.IsAutomaticDownloaderAllowed();
        }



    }    

	/// <summary>
	/// 
	/// </summary>
    private void Update()
    {
        // Do groups need to be reordered?
        List<Downloadables.CatalogGroup> groupsSortedByPriority = m_groupsSortedByPriorityFromManager;

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

        if (m_downloadCompletedPopupToggle != null)
        {
            m_downloadCompletedPopupToggle.isOn = Prefs.GetBoolPlayer(AssetsDownloadFlowSettings.OTA_SHOW_DOWNLOAD_COMPLETE_POPUP, false);
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
            m_views[i].transform.parent = null;
        }
        
        count = m_groupsSortedByPriority.Count;
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < m_views.Count; j++)
            {
                if (m_views[j].Group.Id == m_groupsSortedByPriority[i].Id)
                {
                    m_views[j].transform.parent = m_groupViewsRoot;
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

    /// <summary>
    /// Sets the player pref value for "download completed popup already shown"
    /// </summary>
    public void OnDownloadCompletedPopupToggleClick()
    {
        bool newValue = m_downloadCompletedPopupToggle.isOn;
        Prefs.SetBoolPlayer(AssetsDownloadFlowSettings.OTA_SHOW_DOWNLOAD_COMPLETE_POPUP, newValue);
    }

    #region dumper
    private float m_dumperTimeAtFillUp;

    private void Dumper_Init()
    {
#if USE_DUMPER
        Dumper_RefreshUI();
#else
        m_dumperRoot.SetActive(false);
#endif
    }

#if USE_DUMPER    
    private void Dumper_RefreshUI()
    {
        Downloadables.Dumper dumper = AssetBundlesManager.Instance.GetDownloadablesDumper();
        if (dumper != null)
        {
            if (dumper.IsFillingUp)
            {                                
                m_dumperFillUpDisk.interactable = false;
                m_dumperCancelFillUpDisk.interactable = true;
                m_dumperFillUpProgress.enabled = true;
            }
            else
            {                
                m_dumperFillUpDisk.interactable = true;
                m_dumperCancelFillUpDisk.interactable = false;
                m_dumperFillUpProgress.enabled = false;
            }
        }
    }   
#endif

    public void Dumper_FillUpDisk()
    {
#if USE_DUMPER
        Downloadables.Dumper dumper = AssetBundlesManager.Instance.GetDownloadablesDumper();
        if (dumper != null)
        {
            dumper.IsFillingUp = true;
        }

        m_dumperTimeAtFillUp = Time.realtimeSinceStartup;        

        Dumper_RefreshUI();
#endif
    }

    public void Dumper_CancelFillUpDisk()
    {
#if USE_DUMPER
        Downloadables.Dumper dumper = AssetBundlesManager.Instance.GetDownloadablesDumper();
        if (dumper != null)
        {
            dumper.IsFillingUp = false;
        }

        Dumper_RefreshUI();
#endif
    }

    public void Dumper_CleanUpDisk()
    {
#if USE_DUMPER
        Downloadables.Dumper dumper = AssetBundlesManager.Instance.GetDownloadablesDumper();
        if (dumper != null)
        {
            dumper.CleanUp();
        }

        Dumper_RefreshUI();
#endif
    }   
#endregion
}
