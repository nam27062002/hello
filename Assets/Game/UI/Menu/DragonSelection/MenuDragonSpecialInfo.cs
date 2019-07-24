// MenuDragonSpecialLevelBar.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 22/07/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class MenuDragonSpecialInfo : MonoBehaviour {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//

    [SerializeField] private Localizer m_dragonNameText;

    [SerializeField] private Localizer m_dragonDescText;

    [SerializeField] private LabDragonBar m_specialDragonLevelBar;

    [SerializeField] private LabStatUpgrader[] m_stats = new LabStatUpgrader[0];

    // Internal
    private DragonDataSpecial m_dragonData = null;	// Last used dragon data

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() {
        // Subscribe to external events
        Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);

    }

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
        
        // Do a first refresh
        Refresh(InstanceManager.menuSceneController.selectedDragon, 0.25f);
    }

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//



    /// <summary>
    /// Refresh with data from a target dragon.
    /// </summary>
    /// <param name="_sku">The sku of the dragon whose data we want to use to initialize the bar.</param>
    /// <param name="_delay">Optional delay before refreshing the data. Useful to sync with other UI animations.</param>
    virtual public void Refresh(string _sku, float _delay = -1f)
    {

        // Only show special dragons bar
        bool special = DragonManager.GetDragonData(_sku).type == IDragonData.Type.SPECIAL;
        gameObject.SetActive(special);

        // Nope
        if (!special) return;

        // Get new dragon's data from the dragon manager
        DragonDataSpecial data = DragonManager.GetDragonData(_sku) as DragonDataSpecial;
        if (data == null) return;

        // Things to update only when target dragon has changed
        if (m_dragonData != data)
        {
            // Dragon Name
            if (m_dragonNameText != null)
            {
                switch (data.GetLockState())
                {
                    case DragonDataSpecial.LockState.SHADOW:
                    case DragonDataSpecial.LockState.REVEAL:
                        m_dragonNameText.Localize("TID_SELECT_DRAGON_UNKNOWN_NAME");
                        break;
                    default:
                        m_dragonNameText.Localize(data.def.GetAsString("tidName"));
                        break;
                }
            }



            if (m_dragonDescText != null)
            {
                m_dragonDescText.Localize(data.def.GetAsString("tidDesc"));
            }

            // Update level bar
            m_specialDragonLevelBar.BuildFromDragonData(data);

            // Upgrade buttons
            for (int i = 0; i < m_stats.Length; ++i)
            {
                m_stats[i].InitFromData(m_dragonData);
            }

            // Store new dragon data
            m_dragonData = data;

        }
    }


    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// A new dragon has been selected.
    /// </summary>
    /// <param name="_sku">The sku of the selected dragon.</param>
    private void OnDragonSelected(string _sku)
    {
        // Refresh after some delay to let the animation finish
        Refresh(_sku, 0.25f);
    }


    /// <summary>
    /// Info button has been pressed.
    /// </summary>
    public void OnInfoButton()
    {
        // Skip if dragon data is not valid
        if (m_dragonData == null) return;

        // Tracking
        string popupName = System.IO.Path.GetFileNameWithoutExtension(PopupSpecialDragonInfo.PATH);
        HDTrackingManager.Instance.Notify_InfoPopup(popupName, "info_button");

        // Open the dragon info popup and initialize it with the current dragon's data
        PopupSpecialDragonInfo popup = PopupManager.OpenPopupInstant(PopupSpecialDragonInfo.PATH).GetComponent<PopupSpecialDragonInfo>();
        popup.Init(m_dragonData);
    }

}