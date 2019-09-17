// PopupShop.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Temp popup to "purchase" currencies.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupShop : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Economy/PF_PopupShop";

	public enum Mode {
		DEFAULT,
		SC_ONLY,
		PC_ONLY,
		OFFERS_FIRST
	};

	// Order must match tab system setup!
	public enum Tabs {
		OFFERS,
		PC,
		SC,
		COUNT
	};

	private Tabs DEFAULT_INITIAL_TAB = Tabs.OFFERS;

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed Setup
	[SerializeField] private TabSystem m_tabs = null;
	public TabSystem tabs {
		get { return m_tabs; }
	}

	[Space]
	[SerializeField] private float m_pillAnimDelay = 0.075f;
	[SerializeField] private float[] m_pillRotationSequence = new float[0];
	[SerializeField] private GameObject m_tabButtonsContainer = null;

	[Space]
	[SerializeField] private TextMeshProUGUI m_offersCount;

    [Space]
    [SerializeField] private GameObject m_happyHourPanel;
    [SerializeField] private TextMeshProUGUI m_happyHourTimer;


    // Other setup parameters
    private bool m_closeAfterPurchase = false;
	public bool closeAfterPurchase {
		get { return m_closeAfterPurchase; }
		set { m_closeAfterPurchase = value; }
	}

	private Tabs m_initialTab = Tabs.COUNT;
	public Tabs initialTab {
		get { return m_initialTab; }
		set { m_initialTab = value; }
	}

	// Data
	private List<DefinitionNode> m_packsPurchased = new List<DefinitionNode>();
	public List<DefinitionNode> packsPurchased {
		get { return m_packsPurchased; }
	}

    protected string m_openOrigin = "";
    protected bool m_trackScreenChange = false;
    protected int m_lastTrackedScreen = -1;

    //Internal
    private float m_timer = 0; // Refresh timer
    private HappyHourOffer m_happyHour;

    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//
    /// <summary>
    /// Initialization
    /// </summary>
    void Awake() {
		// Check required fields
		Debug.Assert(m_tabs != null, "Missing required reference!");

		// Create pills for each tab
		for(int i = 0; i < (int)Tabs.COUNT; ++i) {
			// Get target tab
			IPopupShopTab tab = m_tabs[i] as IPopupShopTab;
			Debug.Assert(tab != null, "Unknown tab type!");
			tab.Init();

			// Do some extra initialization on the pills
			float totalDelay = 0f;
			for(int j = 0; j < tab.pills.Count; ++j) {
				// Get pill
				IPopupShopPill pill = tab.pills[j];

				// Subscribe to purchase events
				pill.OnPurchaseSuccess.AddListener(OnPurchaseSuccessful);

				// Apply "random" rotation to the pill
				if(m_pillRotationSequence.Length > 0) {
					pill.transform.localRotation = Quaternion.Euler(0f, 0f, m_pillRotationSequence[j % m_pillRotationSequence.Length]);
				}

				// Animation!
				ShowHideAnimator anim = pill.GetComponent<ShowHideAnimator>();
				if(anim != null) {
					anim.tweenDelay += totalDelay;	// Keep original anim delay
					totalDelay += m_pillAnimDelay;
				}
			}
		}
	}

	/// <summary>
	/// Initialize the popup with the requested mode. Should be called before opening the popup.
	/// </summary>
	/// <param name="_mode">Target mode.</param>
	public void Init(Mode _mode, string _origin ) {
        m_openOrigin = _origin;
        // Refresh pills?

        // Get the current happy hour instance
        m_happyHour = OffersManager.instance.happyHour;

        // Reset scroll lists and hide all tabs
        for (int i = 0; i < (int)Tabs.COUNT; ++i) {
			(m_tabs[i] as IPopupShopTab).scrollList.horizontalNormalizedPosition = 0f;
			m_tabs[i].Hide(NavigationScreen.AnimType.NONE);
		}

		// If required, hide tab buttons
		m_tabButtonsContainer.SetActive(
			_mode == Mode.DEFAULT
		 || _mode == Mode.OFFERS_FIRST
		);

		// Select initial tab
		Tabs goToTab = DEFAULT_INITIAL_TAB;
		switch(_mode) {
			default:
			case Mode.DEFAULT: {
				// Is initial tab overriden?
				if(m_initialTab != Tabs.COUNT) {
					goToTab = m_initialTab;
				} else {
					goToTab = DEFAULT_INITIAL_TAB;	// Default behaviour
				}
			} break;

			case Mode.SC_ONLY: {
				goToTab = Tabs.SC; 
			} break;

			case Mode.PC_ONLY: {
				goToTab = Tabs.PC;
			} break;

			case Mode.OFFERS_FIRST: {
				goToTab = Tabs.OFFERS;
			} break;
		}

		// If initial tab is set to offers, but there are no active offers, fallback to PC tab
		if(goToTab == Tabs.OFFERS && OffersManager.activeOffers.Count == 0) {
			goToTab = Tabs.PC;
		}

		m_trackScreenChange = false;
        m_lastTrackedScreen = -1;

		// Go to initial tab
		m_tabs.GoToScreen(-1, NavigationScreen.AnimType.NONE);	// [AOC] The shop popup is kept cached, so if the last open tab matches the initial tab, animation wont be triggered. Force it by doing this.
		m_tabs.GoToScreen((int)goToTab);
	}


    public void Update()
    {
            // Refresh offers periodically for better performance
            if (m_timer <= 0)
            {
                m_timer = 1f; // Refresh every second
                Refresh();
            }
            m_timer -= Time.deltaTime;
    }

    private void Refresh ()
    {

        // Refresh the happy hour panel
        if (m_happyHour != null)
        {
            // If show the happy hour panel only if the offer is active        
            m_happyHourPanel.SetActive(m_happyHour.IsActive());

            if (m_happyHour.IsActive())
            {
                // Show time left in the proper format (1h 20m 30s)
                string timeLeft = TimeUtils.FormatTime(m_happyHour.TimeLeftSecs(), TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES, 3);
                m_happyHourTimer.text = LocalizationManager.SharedInstance.Localize("TID_REFERRAL_DAYS_LEFT", timeLeft);

            }
        }

    }

    //------------------------------------------------------------------//
    // CALLBACKS														//
    //------------------------------------------------------------------//
    /// <summary
    /// Successful purchase.
    /// </summary>
    /// <param name="_pill">The pill that triggered the event</param>
    private void OnPurchaseSuccessful(IPopupShopPill _pill) {
		// Add to purchased packs list
		m_packsPurchased.Add(_pill.def);

		// Close popup?
		if(m_closeAfterPurchase) GetComponent<PopupController>().Close(false);
	}

	/// <summary>
	/// The popup is about to be been opened.
	/// </summary>
	public void OnOpenPreAnimation() {
		HDTrackingManager.Instance.Notify_StoreVisited( m_openOrigin );
        // Track initial section
        m_lastTrackedScreen = m_tabs.currentScreenIdx;
        string tabName = m_tabs.GetScreen(m_tabs.currentScreenIdx).screenName;
        HDTrackingManager.Instance.Notify_StoreSection(tabName);
        m_trackScreenChange = true;
		m_offersCount.text = OffersManager.activeOffers.Count.ToString();

        // Reset packs purchased list
        m_packsPurchased.Clear();
	}

	/// <summary>
	/// The popup has just been opened.
	/// </summary>
	public void OnOpenPostAnimation() {
		
	}

	/// <summary>
	/// Popup is about to be closed.
	/// </summary>
	public void OnClosePreAnimation() {
		
	}
    
    public void OnScreenChanged( NavigationScreenSystem.ScreenChangedEventData changedEventData )
    {
        if (m_trackScreenChange && m_lastTrackedScreen != changedEventData.toScreenIdx )
        {
            m_lastTrackedScreen = changedEventData.toScreenIdx;
            HDTrackingManager.Instance.Notify_StoreSection( changedEventData.toScreen.screenName );
            Debug.Log( changedEventData.ToString() );
        }
        
    }
}
