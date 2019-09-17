// MenuDragonUnlock.cs
// Hungry Dragon
//
// Created by J.M Olea
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using System.Text;
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Control unlock/acquire/unavailable UI for classic dragons in the menu.
/// Depending on the selected dragon lockState, a different object will be displayed.
/// TODO:
/// - Check active dragon discounts
/// </summary>
public class MenuDragonUnlock : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public const string UNLOCK_WITH_HC_RESOURCES_FLOW_NAME = "UNLOCK_DRAGON_HC";	// Unlock and acquire a locked dragon using HC
	public const string UNLOCK_WITH_SC_RESOURCES_FLOW_NAME = "UNLOCK_DRAGON_SC";    // Acquire an already unlocked dragon using SC


    //------------------------------------------------------------------//
    // PROPERTIES														//
    //------------------------------------------------------------------//
    

    //------------------------------------------------------------------//
    // PROPERTIES														//
    //------------------------------------------------------------------//
    // Exposed
    [SerializeField] protected UIDragonPriceSetup m_hcPriceSetup = null;
	[SerializeField] protected UIDragonPriceSetup m_scPriceSetup = null;
	[Space]
	[SerializeField] protected Localizer m_unavailableInfoText = null;
	[Space]
	[SerializeField] protected ShowHideAnimator m_changeAnim = null;
	[Space]
	[SerializeField] protected GameObject m_hcRoot = null;
	[SerializeField] protected GameObject m_scRoot = null;
	[SerializeField] protected GameObject m_unavailableRoot = null;
    
    //Events
    [SerializeField] protected UnityEvent OnUnlockViaPCSuccessEvent = new UnityEvent();
    [SerializeField] protected UnityEvent OnUnlockViaSCSuccessEvent = new UnityEvent();

    // Internal
    protected bool m_firstEnablePassed = false;
    protected Coroutine m_delayedShowCoroutine = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Hide all elements and then apply for the first time with current values and without animation
		Toggle(m_hcRoot, false);
		Toggle(m_scRoot, false);
		Toggle(m_unavailableRoot, false);

        // Subscribe to external events
        Messenger.AddListener<IDragonData>(MessengerEvents.MODIFIER_ECONOMY_DRAGON_PRICE_CHANGED, OnModifierChanged);

        Refresh(InstanceManager.menuSceneController.selectedDragonData, false);
	}


	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Refresh! Don't animate if it's the first time
		Refresh(InstanceManager.menuSceneController.selectedDragonData, m_firstEnablePassed);
		m_firstEnablePassed = true;
	}

	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
        Messenger.RemoveListener<IDragonData>(MessengerEvents.MODIFIER_ECONOMY_DRAGON_PRICE_CHANGED, OnModifierChanged);
    }


	/// <summary>
	/// Refresh with data from given dragon and trigger animations.
	/// </summary>
	/// <param name="_data">The data of the selected dragon.</param>
	/// <param name="_animate">Whether to trigger animations or not.</param>
	public virtual void Refresh(IDragonData _data, bool _animate) {	}


    /// <summary>
    /// Checks whether the unavailable message should be displayed for the given dragon.
    /// </summary>
    /// <returns>Whether the given dragon is unavailable.</returns>
    /// <param name="_data">Dragon to evaluate.</param>
    public bool CheckUnavailable(IDragonData _data)
    {
        // Check lock state
        bool show = false;
        switch (_data.lockState)
        {
            case IDragonData.LockState.LOCKED_UNAVAILABLE:
                {
                    show = !_data.HasPriceModifier(UserProfile.Currency.HARD);  // If there is a discount for this dragon, don't show the unavailable message
                }
                break;
        }

        return show;
    }

    //------------------------------------------------------------------//
    // INTERNAL UTILS													//
    //------------------------------------------------------------------//
    /// <summary>
    /// Activate/deactivate a GameObject, checking its validity first.
    /// </summary>
    /// <param name="_target">Target GameObject.</param>
    /// <param name="_activate">Toggle on or off?</param>
    protected void Toggle(GameObject _target, bool _activate) {
		if(_target == null) return;
		_target.SetActive(_activate);
	}




    //------------------------------------------------------------------//
    // STATIC METHODS													//
    // These methods are defined static so they can be reused in other	//
    // areas of the game without duplicating code.						//
    //------------------------------------------------------------------//


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
        if (_data.CheckUnlockWithPC())
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
        if (_data.CheckUnlockWithSC())
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



    //------------------------------------------------------------------//
    // CALLBACKS														//
    //------------------------------------------------------------------//
   

    /// <summary>
    /// A modifider that changes the unlock/price of a dragon has changed.
    /// </summary>
    /// <param name="_data">Dragon Data.</param>
    public void OnModifierChanged(IDragonData _data) {
        if (InstanceManager.menuSceneController.selectedDragonData == _data) {
            Refresh(_data, false);
        }
    }

    /// <summary>
    /// Check whether this object can be displayed or not.
    /// </summary>
    /// <returns><c>true</c> if all the conditions to display this object are met, <c>false</c> otherwise.</returns>
    /// <param name="_dragonSku">Target dragon sku.</param>
    /// <param name="_screen">Target screen.</param>
    public bool OnShowCheck(string _dragonSku, MenuScreen _screen) {
		// Show only if the target dragon is locked
		IDragonData dragonData = DragonManager.GetDragonData(_dragonSku);
		return dragonData.lockState == IDragonData.LockState.LOCKED;
	}

	/// <summary>
	/// The unlock button has been pressed.
	/// </summary>
	public void OnUnlockWithPC() {
		// Use static method to unlock currently selected dragon
		UnlockWithPC(InstanceManager.menuSceneController.selectedDragonData, OnUnlockViaPCSuccess);
	}

    /// <summary>
    /// The dragon has been successfully unlocked with PC
    /// Method created to bypass the static methods, so it can call the event defined in the inspector
    /// </summary>
    public void OnUnlockViaPCSuccess (ResourcesFlow _flow)
    {
        OnUnlockSuccessMenuFeedback(_flow);
        OnUnlockViaPCSuccessEvent.Invoke();
    }

    /// <summary>
    /// The unlock button has been pressed.
    /// </summary>
    public void OnUnlockWithSC() {
		// Use static method to unlock currently selected dragon
		UnlockWithSC(InstanceManager.menuSceneController.selectedDragonData, OnUnlockViaSCSuccess);
	}

    /// <summary>
    /// The dragon has been successfully unlocked with SC
    /// /// Method created to bypass the static methods, so it can call the event defined in the inspector
    /// </summary>
    public void OnUnlockViaSCSuccess(ResourcesFlow _flow)
    {
        OnUnlockSuccessMenuFeedback(_flow);
        OnUnlockViaSCSuccessEvent.Invoke();
    }

    /// <summary>
    /// The unlock resources flow has been successful.
    /// </summary>
    /// <param name="_flow">The flow that triggered the event.</param>
    protected static void OnUnlockSuccess(ResourcesFlow _flow) {
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
    protected static void OnUnlockSuccessMenuFeedback(ResourcesFlow _flow) {
		// Aux vars
		IDragonData dragonData = DragonManager.GetDragonData(_flow.itemDef.sku);

		// Show a nice animation!
		// Different animations depending on whether the unlock was done via PC or SC
		MenuDragonScreenController screenController = InstanceManager.menuSceneController.GetScreenData(MenuScreen.DRAGON_SELECTION).ui.GetComponent<MenuDragonScreenController>();
		if(_flow.name == UNLOCK_WITH_HC_RESOURCES_FLOW_NAME)
        {
			screenController.LaunchUnlockAnim(dragonData.def.sku, 0.2f, 0.1f, true);
		}
        else if(_flow.name == UNLOCK_WITH_SC_RESOURCES_FLOW_NAME)
        {
			screenController.LaunchAcquireAnim(dragonData.def.sku);

            // In case of the special dragons, refresh the buttons to show the proper 
		}
	}
}
