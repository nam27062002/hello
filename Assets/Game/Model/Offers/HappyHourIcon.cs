// HappyHourIcon.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 20/09/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using TMPro;
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class HappyHourIcon : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float REFRESH_FREQUENCY = 1f; // Seconds

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField]
    private TextMeshProUGUI m_timeLeftText;
    [SerializeField]
    private ShowHideAnimator m_animationRoot;

    // Internal
    private HappyHourOffer m_happyHour;
    private bool m_happyHourActive = false;


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		InvokeRepeating("UpdatePeriodic", 0f, REFRESH_FREQUENCY);
        m_happyHour = OffersManager.instance.happyHour;
        m_animationRoot.Hide();
    }

	/// <summary>
	/// Called at regular intervals.
	/// </summary>
	private void UpdatePeriodic() {
        // Refresh offers periodically for better performance
        Refresh();
    }

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		CancelInvoke("UpdatePeriodic");
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	/// <summary>
	/// Refresh visuals
	/// [JOM] It should be optimized to activate/deactivate this icon through events instead of polling every second
	/// </summary>
	private void Refresh()
    {
        // Refresh the happy hour panel
        if (m_happyHour != null)
        {
            if (m_happyHour.IsActive())
            {
                // Run this code when happy hour starts
                if (!m_happyHourActive)
                {
                    // Enable the icon
                    m_animationRoot.Show();
                    m_happyHourActive = true;
                }

                // Show time left in the proper format (1h 20m 30s)
                string timeLeft = TimeUtils.FormatTime(m_happyHour.TimeLeftSecs(), TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES, 3);
                m_timeLeftText.text = timeLeft;
            }
            else
            {
                // Run this code when happy hour finish
                if (m_happyHourActive)
                {
                    // Disable the icon object
                    m_animationRoot.Hide();
                    m_happyHourActive = false;
                }

            }
        }

    }
    /// <summary>
    /// Open the PC shop.
    /// </summary>
    private void OpenPCShopPopup()
    {
        PopupController popup = PopupManager.LoadPopup(PopupShop.PATH);
        PopupShop shopPopup = popup.GetComponent<PopupShop>();

        // Show the gems tab
        shopPopup.Init(PopupShop.Mode.PC_FIRST, "Happy_Hour_Icon");
        shopPopup.closeAfterPurchase = true;

        // Open the shop popup!
        popup.Open();

    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Player clicks on the icon
    /// </summary>
    public void OnClick()
    {
        // open the shop in the gems tab
        OpenPCShopPopup();
    }
}