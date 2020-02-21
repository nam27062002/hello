// ShopScreenController.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 19/02/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Initializes the shop
/// </summary>
[RequireComponent(typeof(ShopController))]
public class ShopScreenController : MonoBehaviour {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    private MenuTransitionManager m_transitionManager;


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// First update call.
    /// </summary>
    protected void Start()
    {
        // Get a reference to the navigation system, which in this particular case should be a component in the menu scene controller
        m_transitionManager = InstanceManager.menuSceneController.transitionManager;
        Debug.Assert(m_transitionManager != null, "Required component missing!");

        // Initialize the shop
        GetComponent<ShopController>().Init(PopupShop.Mode.DEFAULT);


    }

    /// <summary>
    /// On Enabled
    /// </summary>
    private void OnEnable() {

        if (m_transitionManager!= null && m_transitionManager.prevScreen == MenuScreen.PENDING_REWARD)
        {
            // Do nothing. Let the shop to be opened in the same position that it was before the purchase.
        }
        else
        {
            // Move the scroll to the begining of the shop.
            GetComponent<ShopController>().ScrollToStart();
        }

        // In case the shop popup is waiting to open, cancel it
        InstanceManager.menuSceneController.interstitialPopupsController.SetFlag(MenuInterstitialPopupsController.StateFlag.OPEN_SHOP, false);

    }


	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}