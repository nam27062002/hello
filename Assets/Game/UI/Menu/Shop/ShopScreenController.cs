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

    // Internal references
    private NavigationShowHideAnimator m_animator = null;
    public NavigationShowHideAnimator animator
    {
        get
        {
            if (m_animator == null)
            {
                m_animator = GetComponent<NavigationShowHideAnimator>();
            }
            return m_animator;
        }
    }

	private ShopController m_shopController = null;

	// Tracking
	private string m_trackingOrigin = "";
	public string trackingOrigin {
		set { m_trackingOrigin = value; } 
	}

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//

    private void Awake()
    {
        animator.OnShowPreAnimation.AddListener(OnShowPreAnimation);
		m_shopController = GetComponent<ShopController>();
    }

    private void OnDestroy()
    {
        animator.OnShowPreAnimation.RemoveListener(OnShowPreAnimation);
    }



    /// <summary>
    /// First update call.
    /// </summary>
    protected void Start()
    {
        // Get a reference to the navigation system, which in this particular case should be a component in the menu scene controller
        m_transitionManager = InstanceManager.menuSceneController.transitionManager;
        Debug.Assert(m_transitionManager != null, "Required component missing!");



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
            m_shopController.ScrollToStart();
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

    /// <summary>
    /// Screen is about to be open.
    /// </summary>
    /// <param name="_animator">The animator that triggered the event.</param>
    public void OnShowPreAnimation(ShowHideAnimator _animator)
    {
        // Initialize the shop
        m_shopController.Init(PopupShop.Mode.DEFAULT);

		// Propagate event
		m_shopController.OnShopEnter(m_trackingOrigin);
    }
}