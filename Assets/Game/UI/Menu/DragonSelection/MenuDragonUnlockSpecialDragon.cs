// MenuDragonUnlockGoldenFragments.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using UnityEngine.Events;



//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Unlock the selected special dragon.
/// </summary>
public class MenuDragonUnlockSpecialDragon : MonoBehaviour {

    //------------------------------------------------------------------//
    // CONSTANTS														//
    //------------------------------------------------------------------//
    public const string UNLOCK_WITH_HC_RESOURCES_FLOW_NAME = "UNLOCK_SPECIAL_DRAGON_PC";    // Unlock and acquire a locked dragon using HC
    public const string UNLOCK_WITH_SC_RESOURCES_FLOW_NAME = "UNLOCK_SPECIAL_DRAGON_SC";    // Acquire an already unlocked dragon using SC



    //------------------------------------------------------------------//
    // PROPERTIES														//
    //------------------------------------------------------------------//
    // Exposed
    [SerializeField] private UIDragonPriceSetup m_hcPriceSetup = null;
    [SerializeField] private UIDragonPriceSetup m_scPriceSetup = null;
    [Space]
    [SerializeField] private Localizer m_unavailableInfoText = null;
    [Space]
    [SerializeField] private ShowHideAnimator m_changeAnim = null;
    [Space]
    [SerializeField] private GameObject m_hcRoot = null;
    [SerializeField] private GameObject m_scRoot = null;
    [SerializeField] private GameObject m_unavailableRoot = null;

    // Internal
    private bool m_firstEnablePassed = false;
    private Coroutine m_delayedShowCoroutine = null;

   
    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake()
    {
        // Hide all elements and then apply for the first time with current values and without animation
        Toggle(m_hcRoot, false);
        Toggle(m_scRoot, false);
        Toggle(m_unavailableRoot, false);
        Refresh(InstanceManager.menuSceneController.selectedDragonData, false);
    }

    /// <summary>
    /// First update
    /// </summary>
    private void Start() {
        // Subscribe to external events
        Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
        Messenger.AddListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);
        Messenger.AddListener<IDragonData>(MessengerEvents.MODIFIER_ECONOMY_DRAGON_PRICE_CHANGED, OnModifierChanged);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
        // Unsubscribe from external events
        Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
        Messenger.RemoveListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);
        Messenger.RemoveListener<IDragonData>(MessengerEvents.MODIFIER_ECONOMY_DRAGON_PRICE_CHANGED, OnModifierChanged);
    }

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Refresh with data from given dragon and trigger animations.
    /// </summary>
    /// <param name="_data">The data of the selected dragon.</param>
    /// <param name="_animate">Whether to trigger animations or not.</param>
    public void Refresh(IDragonData _data, bool _animate)
    {
        // Stop any pending coroutines
        if (m_delayedShowCoroutine != null)
        {
            StopCoroutine(m_delayedShowCoroutine);
            m_delayedShowCoroutine = null;
        }

        // Trigger animation?
        if (_animate && m_changeAnim != null)
        {
            // If object is visible, hide first and Refresh the info when the object is hidden
            if (m_changeAnim.visible)
            {
                // Trigger hide animation
                m_changeAnim.Hide();

                // Refresh info once the object is hidden
                m_delayedShowCoroutine = UbiBCN.CoroutineManager.DelayedCall(() => {
                    // Refresh info
                    RefreshInfo(_data);

                    // Trigger show animation
                    m_changeAnim.Show();
                }, m_changeAnim.tweenDuration); // Use hide animation duration as delay for the coroutine
            }
            else
            {
                //  Object already hidden, refresh info and trigger show animation
                RefreshInfo(_data);
                m_changeAnim.Show();
            }
        }
        else
        {
            // Just refresh info immediately
            RefreshInfo(_data);
            if (m_changeAnim != null) m_changeAnim.Show(false);
        }
    }


    /// <summary>
	/// Refresh texts and visibility to match given dragon.
	/// Doesn't trigger any animation.
	/// </summary>
	/// <param name="_data">Data.</param>
	private void RefreshInfo(IDragonData _data)
    {
        // Aux vars
        bool show = true;

        // Update hc unlock button
        // Display?
        show = MenuDragonUnlockClassicDragon.CheckUnlockWithPC(_data);
        Toggle(m_hcRoot, show);

        // Refresh info
        if (show && m_hcPriceSetup != null)
        {
            // [AOC] UIDragonPriceSetup makes it easy for us!
            m_hcPriceSetup.InitFromData(_data, UserProfile.Currency.HARD);
        }

        // Update sc unlock button
        // Display?
        show = MenuDragonUnlockClassicDragon.CheckUnlockWithSC(_data);
        Toggle(m_scRoot, show);

        // Refresh info
        if (show && m_scPriceSetup != null)
        {
            // [AOC] UIDragonPriceSetup makes it easy for us!
            m_scPriceSetup.InitFromData(_data, UserProfile.Currency.SOFT);
        }

        // Update unavailable info
        if (m_unavailableInfoText != null)
        {
            // Display?
            show = MenuDragonUnlockClassicDragon.CheckUnavailable(_data);
            Toggle(m_unavailableRoot, show);

            // Refresh info
            if (show)
            {
                // Check the minimum dragon required 
                string unlockFromDragon = _data.def.Get("unlockFromDragon");
                if (unlockFromDragon != null)
                {
                    IDragonData requiredDragon = DragonManager.GetDragonData(unlockFromDragon);

                    // If the player doesnt have this dragon or any bigger, show the unavailable text
                    if (DragonManager.biggestOwnedDragon.tier < requiredDragon.tier)
                    {

                        // Set text
                        m_unavailableInfoText.Localize(
                            m_unavailableInfoText.tid,
                            requiredDragon.def.GetLocalized("tidName")
                        );
                    }
                }
            }
        }
    }

    /// <summary>
	/// Unlocks and acquires the given dragon using PC.
	/// Will check that the given dragon can actually be acquired using PC.
	/// Will perform all needed post actions such as tracking, saving persistence, etc, but not visual feedback.
	/// Use the <paramref name="_onSuccess"/> parameter to do so.
	/// </summary>
	/// <param name="_data">Data of the dragon to be unlocked.</param>
	/// <param name="_onSuccess">Callback to be invoked when unlock was successful. </param>
	public static void UnlockWithPC(IDragonData _data, UnityAction<ResourcesFlow> _onSuccess)
    {
        // Make sure the dragon we unlock is the currently selected one and that we can actually do it
        if (MenuDragonUnlockClassicDragon.CheckUnlockWithPC(_data))
        {
            // Get price and start purchase flow
            ResourcesFlow purchaseFlow = new ResourcesFlow(UNLOCK_WITH_HC_RESOURCES_FLOW_NAME);
            purchaseFlow.OnSuccess.AddListener(OnUnlockSuccess);
            if (_onSuccess != null) purchaseFlow.OnSuccess.AddListener(_onSuccess);
            purchaseFlow.Begin(
                _data.GetPriceModified(UserProfile.Currency.HARD),
                UserProfile.Currency.HARD,
                HDTrackingManager.EEconomyGroup.UNLOCK_DRAGON,
                _data.def
            );
        }
    }

    /// <summary>
    /// Unlocks and acquires the given dragon using SC.
    /// Will check that the given dragon can actually be acquired using SC.
    /// Will perform all needed post actions such as tracking, saving persistence, etc, but not visual feedback.
    /// Use the <paramref name="_onSuccess"/> parameter to do so.
    /// </summary>
    /// <param name="_data">Data of the dragon to be unlocked.</param>
    /// <param name="_onSuccess">Callback to be invoked when unlock was successful. </param>
    public static void UnlockWithSC(IDragonData _data, UnityAction<ResourcesFlow> _onSuccess)
    {
        // Make sure the dragon we unlock is the currently selected one and that we can actually do it
        if (MenuDragonUnlockClassicDragon.CheckUnlockWithSC(_data))
        {
            // [AOC] From 1.18 on, don't trigger the missing SC flow for dragon
            //		 purchases (we are displaying the HC button next to it)
            // Check whether we have enough SC
            long priceSC = _data.GetPriceModified(UserProfile.Currency.SOFT);
            if (priceSC > UsersManager.currentUser.coins)
            {
                // Not enough SC! Show a message
                UIFeedbackText.CreateAndLaunch(
                    LocalizationManager.SharedInstance.Localize("TID_SC_NOT_ENOUGH"),   // [AOC] TODO!! Improve text?
                    GameConstants.Vector2.center,
                    InstanceManager.menuSceneController.hud.transform as RectTransform,
                    "NotEnoughSCError"
                );
            }
            else
            {
                // There shouldn't be any problem to perform the transaction, do
                // it via a ResourcesFlow to avoid duplicating code / missing steps
                ResourcesFlow purchaseFlow = new ResourcesFlow(UNLOCK_WITH_SC_RESOURCES_FLOW_NAME);
                purchaseFlow.OnSuccess.AddListener(OnUnlockSuccess);
                if (_onSuccess != null) purchaseFlow.OnSuccess.AddListener(_onSuccess);
                purchaseFlow.Begin(
                    priceSC,
                    UserProfile.Currency.SOFT,
                    HDTrackingManager.EEconomyGroup.UNLOCK_DRAGON,
                    _data.def
                );
            }
        }
    }

    //------------------------------------------------------------------------//
    // INTERNAL METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Gets the price of unlocking the current selected dragon in the target currency.
    /// </summary>
    /// <returns>The price.</returns>
    /// <param name="_currency">Target currency.</param>
    private long GetPrice(UserProfile.Currency _currency) {
		// Choos currency
		string priceProperty = "";
		switch(_currency) {
			case UserProfile.Currency.SOFT: priceProperty = "unlockPriceCoins"; break;
			case UserProfile.Currency.HARD: priceProperty = "unlockPricePC"; break;
		}

		// Get price from the definition of the currently selected dragon.
		IDragonData dragonData = DragonManager.GetDragonData(InstanceManager.menuSceneController.selectedDragon);
		return dragonData.def.GetAsLong(priceProperty, 0);
	}




    //------------------------------------------------------------------//
    // INTERNAL UTILS													//
    //------------------------------------------------------------------//
    /// <summary>
    /// Activate/deactivate a GameObject, checking its validity first.
    /// </summary>
    /// <param name="_target">Target GameObject.</param>
    /// <param name="_activate">Toggle on or off?</param>
    private void Toggle(GameObject _target, bool _activate)
    {
        if (_target == null) return;
        _target.SetActive(_activate);
    }


   

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Selected dragon has changed.
    /// </summary>
    /// <param name="_dragonSku">Selected dragon sku.</param>
    public void OnDragonSelected(string _dragonSku)
    {
        Refresh(DragonManager.GetDragonData(_dragonSku), true);
    }

    /// <summary>
    /// A dragon has been acquired
    /// </summary>
    /// <param name="_data">The data of the acquired dragon.</param>
    public void OnDragonAcquired(IDragonData _data)
    {
        Refresh(_data, true);
    }

    /// <summary>
    /// A modifider that changes the unlock/price of a dragon has changed.
    /// </summary>
    /// <param name="_data">Dragon Data.</param>
    public void OnModifierChanged(IDragonData _data)
    {
        if (InstanceManager.menuSceneController.selectedDragonData == _data)
        {
            Refresh(_data, false);
        }
    }
    
    
    /// <summary>
	/// The unlock button has been pressed.
	/// </summary>
	public void OnUnlockWithPC()
    {
        // Use static method to unlock currently selected dragon
        UnlockWithPC(InstanceManager.menuSceneController.selectedDragonData, OnUnlockSuccessMenuFeedback);
    }

    /// <summary>
    /// The unlock button has been pressed.
    /// </summary>
    public void OnUnlockWithSC()
    {
        // Use static method to unlock currently selected dragon
        UnlockWithSC(InstanceManager.menuSceneController.selectedDragonData, OnUnlockSuccessMenuFeedback);
    }

    /// <summary>
	/// The unlock resources flow has been successful.
	/// </summary>
	/// <param name="_flow">The flow that triggered the event.</param>
	private static void OnUnlockSuccess(ResourcesFlow _flow)
    {
        // Aux vars
        IDragonData dragonData = DragonManager.GetDragonData(_flow.itemDef.sku);

        // Just acquire target dragon!
        dragonData.Acquire();

        // Track
        HDTrackingManager.Instance.Notify_DragonUnlocked(dragonData.def.sku, dragonData.GetOrder());

        // Save persistence!
        PersistenceFacade.instance.Save_Request();
    }

    /// <summary>
    /// The unlock resources flow has been successful.
    /// </summary>
    /// <param name="_flow">The flow that triggered the event.</param>
    private static void OnUnlockSuccessMenuFeedback(ResourcesFlow _flow)
    {
        // Aux vars
        IDragonData dragonData = DragonManager.GetDragonData(_flow.itemDef.sku);

        // Show a nice animation!
        // Different animations depending on whether the unlock was done via PC or SC
        MenuDragonScreenController screenController = InstanceManager.menuSceneController.GetScreenData(MenuScreen.DRAGON_SELECTION).ui.GetComponent<MenuDragonScreenController>();
        if (_flow.name == UNLOCK_WITH_HC_RESOURCES_FLOW_NAME)
        {
            screenController.LaunchUnlockAnim(dragonData.def.sku, 0.2f, 0.1f, true);
        }
        else if (_flow.name == UNLOCK_WITH_SC_RESOURCES_FLOW_NAME)
        {
            screenController.LaunchAcquireAnim(dragonData.def.sku);
        }
    }

}
