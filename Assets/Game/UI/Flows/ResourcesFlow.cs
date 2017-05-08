// CurrencyFlow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 27/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// This class encapsulates the whole logic of purchasing anything in the game with
/// either SC or PC. Manages the check of required currencies, computes the required
/// PC to top up price and opens the PC shop if needed.
/// If the flow is completed, performs the currency transaction on the profile and
/// invokes the corresponding Callback to notify its result so item transaction can
/// be completed.
/// </summary>
public class ResourcesFlow {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// States
	public enum State {
		INIT,

		SHOWING_MISSING_CURRENCY,
		SHOWING_MISSING_EXTRA_PC,
		SHOWING_PC_SHOP,

		ASKING_BIG_AMOUNT_CONFIRMATION,

		FINISHED_SUCCESS,
		FINISHED_CANCELED,
		FINISHED_ERROR
	}

	// Constant values
	public const long PC_CONFIRMATION_POPUP_THRESHOLD = 20;	// Show confirmation popup for PC purchases bigger than this threshold

	// Custom events
	public class ResourcesFlowEvent : UnityEvent<ResourcesFlow> { };
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Flow data
	private string m_name = "";	// Identifier, mostly for debug purposes
	public string name {
		get { return m_name; }
		set { m_name = value; }
	}

	// Transaction Data
	private DefinitionNode m_itemDef = null;	// Optional, item to be purchased
	public DefinitionNode itemDef {
		get { return m_itemDef; }
		set { m_itemDef = value; }
	}

	// Base prices
	private UserProfile.Currency m_currency = 0;
	public UserProfile.Currency currency {
		get { return m_currency; }
	}

	private long m_originalAmount = 0;
	public long originalAmount {
		get { return m_originalAmount; }
	}
	
	// Missing resources
	private long m_missingAmount = 0;
	public long missingAmount {
		get { return m_missingAmount; }
	}

	// Actual amount considering current resources
	private long m_finalAmount = 0;
	public long finalAmount {
		get { return m_finalAmount; }
	}

	// Total extra PC cost of buying missing resources
	private long m_extraPCCost = 0;
	public long extraPCCost {
		get { return m_extraPCCost; }
	}

	// Missing PC for the extra cost
	private long m_missingExtraPC = 0;
	public long missingExtraPC {
		get { return m_missingExtraPC; }
	}

	private DefinitionNode m_recommendedPCPackDef = null;
	public DefinitionNode recommendedPCPackDef {
		get { return m_recommendedPCPackDef; }
	}

	// Popups
	private List<PopupController> m_popups = new List<PopupController>();

	// Events
	public ResourcesFlowEvent OnSuccess = new ResourcesFlowEvent();
	public ResourcesFlowEvent OnCancel = new ResourcesFlowEvent();
	public ResourcesFlowEvent OnFinished = new ResourcesFlowEvent();	// The flow has finished, regardless of its result (success, cancel, error, etc.)

	// Internal logic
	private State m_previousState = State.INIT;
	private State m_state = State.INIT;
	public State state {
		get { return m_state; }
	}

	public bool successful {
		get { return m_state == State.FINISHED_SUCCESS; }
	}

	// Internal data
	private List<DefinitionNode> m_pcPackDefinitions = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	/// <param name="_name">Optional identifier to be given to the flow, mostly for debugging.</param>
	public ResourcesFlow(string _name = "") {
		// Init internal vars
		m_name = _name;

		// Subscribe to external events
		Messenger.AddListener<PopupController>(EngineEvents.POPUP_CLOSED, OnPopupClosed);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	~ResourcesFlow() {
		// Unsubscribe from external events
		Messenger.RemoveListener<PopupController>(EngineEvents.POPUP_CLOSED, OnPopupClosed);
	}

	/// <summary>
	/// Start the flow.
	/// Prices should have been set.
	/// </summary>
	/// <param name="_targetAmount">How much are we trying to spend?</param>
	/// <param name="_currency">Which currency are we trying to spend?</param>
	/// <param name="_itemDef">Optional, which item are we trying to buy? (Only for visual purposes)</param>
	public void Begin(long _targetAmount, UserProfile.Currency _currency, DefinitionNode _itemDef) {
		// Only from Init state!
		if(m_state != State.INIT) return;

		// Initialize internal vars
		m_originalAmount = _targetAmount;
		m_currency = _currency;
		m_itemDef = _itemDef;

		// Try it now!
		TryTransaction(true);
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Change state and store previous state.
	/// </summary>
	/// <param name="_newState">New state.</param>
	private void ChangeState(State _newState) {
		m_previousState = m_state;
		m_state = _newState;
	}

	/// <summary>
	/// Close any popups/UI opened by this flow.
	/// </summary>
	private void Close() {
		// Close all popups opened by this resources flow instance
		ClosePopups();

		// Notify listeners
		OnFinished.Invoke(this);
	}

	/// <summary>
	/// Cancel the flow.
	/// </summary>
	private void Cancel() {
		// Change state
		ChangeState(State.FINISHED_CANCELED);

		// Notify!
		OnCancel.Invoke(this);

		// Close flow
		Close();
	}

	/// <summary>
	/// Attempt to do the transaction based on data stored in the m_originalAmount, 
	/// m_currency and m_itemDef variables.
	/// Missing currency will be computed, required popups will be opened and flow's 
	/// state will be updated according to that.
	/// If everything is ok, transaction will be executed.
	/// </summary>
	/// <param name="_askConfirmationForBigPCAmounts">If set to true, a confirmation popup will be triggered for big PC amounts and the transaction wont happen until the popup is confirmed.</param>
	private void TryTransaction(bool _askConfirmationForBigPCAmounts) {
		// Close any popup opened by this resources flow instance
		ClosePopups();

		// Currency amounts
		m_missingAmount = System.Math.Max(0, m_originalAmount - UsersManager.currentUser.GetCurrency(m_currency));	// Non-negative!
		m_finalAmount = m_originalAmount - m_missingAmount;

		// Extra PC price of missing resources
		// Depends on currency!
		switch(m_currency) {
			case UserProfile.Currency.SOFT: {
				m_extraPCCost = GameSettings.ComputePCForCoins(m_missingAmount);
			} break;

			default:{ 
				m_extraPCCost = 0;
			} break;
		}
		m_missingExtraPC = System.Math.Max(0, m_extraPCCost - UsersManager.currentUser.pc);	// Non-negative!

		// If a resource has failed, decide what to do
		if(m_missingAmount > 0) {
			// Depends on currency
			switch(m_currency) {
				case UserProfile.Currency.SOFT: {
					// Open the popup
					OpenMissingSCPopup(m_missingAmount, m_extraPCCost);
				} break;

				case UserProfile.Currency.HARD: {
					// Show confirmation popup?
					if(_askConfirmationForBigPCAmounts && (m_originalAmount > PC_CONFIRMATION_POPUP_THRESHOLD)) {
						// Final PC amount over threshold!
						// Show confirmation popup
						OpenBigAmountConfirmationPopup(m_originalAmount, () => { OpenMissingPCPopup(m_missingAmount); });	// If confirmed, open missing PC popup
					} else {
						// Directly show missing PC popup
						OpenMissingPCPopup(m_missingAmount);
					}
				} break;
			}
		}

		// Everything ok! Do the transaction
		else {
			// Everything ok!
			DoTransaction(_askConfirmationForBigPCAmounts);
		}
	}

	/// <summary>
	/// Perform the transaction! Should only be called once when all the checks
	/// have been passed (all missing resources purchased).
	/// It will actually subtract m_finalAmount from the target currency plus m_extraPCCost 
	/// from PC, so there is no need to manually do intermediate transactions 
	/// other than purchasing more PC for the extra cost.
	/// </summary>
	/// <param name="_askConfirmationForBigPCAmounts">If set to true, a confirmation popup will be triggered for big PC amounts and the transaction wont happen until the popup is confirmed.</param>
	private void DoTransaction(bool _askConfirmationForBigPCAmounts) {
		// Confirmation required?
		if(_askConfirmationForBigPCAmounts) {
			// Final PC cost?
			long finalPCAmount = 0;

			// a) Purchasing with PC (no extra PC cost when purchasing with PC)
			if(m_currency == UserProfile.Currency.HARD) {
				finalPCAmount = m_finalAmount;
			}

			// b) Not enough resources
			else {
				finalPCAmount = m_extraPCCost;
			}

			// Final PC amount over threshold?
			if(finalPCAmount > PC_CONFIRMATION_POPUP_THRESHOLD) {
				// Show confirmation popup
				OpenBigAmountConfirmationPopup(finalPCAmount, () => { DoTransaction(false); });	// Do the transaction on success
				return;	// Don't do anything else until confirmed by user
			}
		}

		// Transaction confirmed!
		// Just in case, doublecheck that the player has enough currencies
		if(m_finalAmount > UsersManager.currentUser.GetCurrency(m_currency)
		|| m_extraPCCost > UsersManager.currentUser.pc) {
			// Move to error state
			ChangeState(State.FINISHED_ERROR);

			// Show default message
			UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_RESOURCES_FLOW_UNKNOWN_ERROR"), new Vector2(0.5f, 0.33f), PopupManager.canvas.transform as RectTransform);	// Use popup's canvas

			// Close the flow
			Close();
			return;
		}

		// [AOC] TODO!! Tracking

		// Currency transaction
		if(m_finalAmount > 0) {
			UsersManager.currentUser.AddCurrency(-m_finalAmount, m_currency);
		}

		// Extra PC Cost Transaction
		if(m_extraPCCost > 0) {
			UsersManager.currentUser.AddPC(-m_extraPCCost);
		}

		// Change state
		ChangeState(State.FINISHED_SUCCESS);

		// Notify!
		OnSuccess.Invoke(this);

		// Close any open popups
		Close();
	}

	/// <summary>
	/// Find the closest PC pack covering a given amount.
	/// </summary>
	/// <returns>The definition of the recommended PC pack.</returns>
	/// <param name="_pcAmount">Target PC amount.</param>
	private DefinitionNode FindRecommendedPCPack(long _pcAmount) {
		// If not already done, initialize definitions list
		if(m_pcPackDefinitions == null) {
			// Get all PC pack definitions and sort them by PC amount given
			m_pcPackDefinitions = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.SHOP_PACKS, "type", "hc");	// Hard currency packs only
			DefinitionsManager.SharedInstance.SortByProperty(ref m_pcPackDefinitions, "amount", DefinitionsManager.SortType.NUMERIC);
		}

		// Definitions are sorted, should be easy to find the right pack!
		for(int i = 0; i < m_pcPackDefinitions.Count; i++) {
			// Is it the first pack covering our target amount?
			if(m_pcPackDefinitions[i].GetAsLong("amount") >= _pcAmount) {
				// Yes!! Return it
				return m_pcPackDefinitions[i];
			}
		}

		// No pack was found, return biggest available pack
		return m_pcPackDefinitions.Last();
	}

	//------------------------------------------------------------------------//
	// POPUP MANAGEMENT METHODS												  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Close all popups opened by this resources flow.
	/// </summary>
	private void ClosePopups() {
		// Reverse order for better visual effect
		for(int i = m_popups.Count - 1; i >= 0; i--) {
			m_popups[i].Close(false);	// Don't destroy, let's reuse popups from flow to flow! (popup manager will take care of that)
		}
		m_popups.Clear();
	}

	/// <summary>
	/// Find a popup of a specific type in the list of popups opened by this flow.
	/// </summary>
	/// <returns>The last opened popup of the given type. <c>null</c> if no popup of the requested type could be found.</returns>
	/// <typeparam name="T">Type of the popup we are looking for.</typeparam>
	private T GetPopup<T>() where T : Object {
		// Reverse search
		T targetPopup = null;
		for(int i = m_popups.Count - 1; i >= 0 && targetPopup == null; i--) {
			targetPopup = m_popups[i].GetComponent<T>();
		}
		return targetPopup;
	}

	/// <summary>
	/// Open the missing PC popup.
	/// Will initialize the m_recommendedPCPackDef variable.
	/// </summary>
	/// <param name="_amount">Amount of missing PC.</param>
	private void OpenMissingPCPopup(long _amount) {
		// Find recommended shop pack for the missing amount
		m_recommendedPCPackDef = FindRecommendedPCPack(_amount);

		// Show popup to buy missing PC
		PopupController popup = PopupManager.LoadPopup(ResourcesFlowMissingPCPopup.PATH);
		ResourcesFlowMissingPCPopup pcPopup = popup.GetComponent<ResourcesFlowMissingPCPopup>();
		pcPopup.Init(m_recommendedPCPackDef);

		pcPopup.OnRecommendedPackPurchased.RemoveAllListeners();	// We're recycling popups, so we don't want events to be added twice!
		pcPopup.OnRecommendedPackPurchased.AddListener(OnPCPackPurchased);

		pcPopup.OnGoToShop.RemoveAllListeners();	// We're recycling popups, so we don't want events to be added twice!
		pcPopup.OnGoToShop.AddListener(OnGoToPCShop);

		pcPopup.OnCancel.RemoveAllListeners();	// We're recycling popups, so we don't want events to be added twice!
		pcPopup.OnCancel.AddListener(Cancel);

		popup.Open();
		m_popups.Add(popup);

		// Change state - is PC the main currency?
		if(m_currency == UserProfile.Currency.HARD) {
			ChangeState(State.SHOWING_MISSING_CURRENCY);
		} else {
			ChangeState(State.SHOWING_MISSING_EXTRA_PC);
		}
	}

	/// <summary>
	/// Open the missing SC popup.
	/// </summary>
	private void OpenMissingSCPopup(long _missingAmount, long _extraPCCost) {
		// Show popup to buy missing resources with PC
		PopupController popup = PopupManager.LoadPopup(ResourcesFlowMissingSCPopup.PATH);
		ResourcesFlowMissingSCPopup coinsPopup = popup.GetComponent<ResourcesFlowMissingSCPopup>();
		coinsPopup.Init(_missingAmount, _extraPCCost);

		coinsPopup.OnAccept.RemoveAllListeners();	// We're recycling popups, so we don't want events to be added twice!
		coinsPopup.OnAccept.AddListener(OnBuyMissingCoins);

		coinsPopup.OnCancel.RemoveAllListeners();	// We're recycling popups, so we don't want events to be added twice!
		coinsPopup.OnCancel.AddListener(Cancel);

		popup.Open();
		m_popups.Add(popup);

		// Change state
		ChangeState(State.SHOWING_MISSING_CURRENCY);
	}

	/// <summary>
	/// Open the big PC amount confirmation popup.
	/// </summary>
	/// <param name="_amount">Amount of PC to be spent.</param>
	/// <param name="_onSuccess">Action to be performed on confirmation sucess.</param>
	private void OpenBigAmountConfirmationPopup(long _amount, UnityAction _onSuccess) {
		// Open and initialize the popup
		PopupController popup = PopupManager.LoadPopup(ResourcesFlowBigAmountConfirmationPopup.PATH);
		ResourcesFlowBigAmountConfirmationPopup confirmationPopup = popup.GetComponent<ResourcesFlowBigAmountConfirmationPopup>();
		confirmationPopup.Init(_amount);

		confirmationPopup.OnAccept.RemoveAllListeners();	// We're recycling popups, so we don't want events to be added twice!
		confirmationPopup.OnAccept.AddListener(_onSuccess);

		confirmationPopup.OnCancel.RemoveAllListeners();	// We're recycling popups, so we don't want events to be added twice!
		confirmationPopup.OnCancel.AddListener(() => { Cancel(); });	// Cancel flow

		popup.Open();
		m_popups.Add(popup);

		// Change state
		ChangeState(State.ASKING_BIG_AMOUNT_CONFIRMATION);
	}

	/// <summary>
	/// Open the PC shop.
	/// </summary>
	private void OpenPCShopPopup() {
		// [AOC] We could show only the packs covering the required amount
		PopupController popup = PopupManager.LoadPopup(PopupCurrencyShop.PATH);
		PopupCurrencyShop shopPopup = popup.GetComponent<PopupCurrencyShop>();
		shopPopup.Init(PopupCurrencyShop.Mode.PC_ONLY);
		shopPopup.closeAfterPurchase = true;

		// Wait for the popup to close
		popup.OnClosePostAnimation.AddListener(OnPCShopClosed);

		// Open the popup!
		popup.Open();
		m_popups.Add(popup);

		// Change state
		ChangeState(State.SHOWING_PC_SHOP);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A popup has been closed.
	/// </summary>
	/// <param name="_popup">The closed popup.</param>
	private void OnPopupClosed(PopupController _popup) {
		// If the popup is on the list, remove it
		m_popups.Remove(_popup);
	}

	/// <summary>
	/// Confirmation from the player to buy missing coins with PC.
	/// </summary>
	private void OnBuyMissingCoins() {
		// If the player doesn't have enough PC, let him buy some!
		if(m_missingExtraPC > 0) {
			// Open popup
			OpenMissingPCPopup(m_missingExtraPC);
		}

		// Otherwise buy the missing currency with PC
		else {
			// Transaction will do everything! ^_^
			DoTransaction(false);	// Ask confirmation? No, looks weird after having purchased the gems
		}
	}

	/// <summary>
	/// The recommended PC pack has been purchased.
	/// </summary>
	private void OnPCPackPurchased() {
		// We should have enough PC to complete the transaction now, do it!
		DoTransaction(false);	// Ask confirmation? No, looks weird after having purchased the pack
	}

	/// <summary>
	/// While buying PC, the player asks for more offers.
	/// </summary>
	private void OnGoToPCShop() {
		// Open PC shop
		OpenPCShopPopup();
	}

	/// <summary>
	/// The PC Shop popup has closed.
	/// </summary>
	private void OnPCShopClosed() {
		// Get the popup from the list
		PopupCurrencyShop shopPopup = GetPopup<PopupCurrencyShop>();	// [AOC] Should never be null
		PopupController shopPopupController = shopPopup.GetComponent<PopupController>();

		// Remove the listener
		shopPopupController.OnClosePostAnimation.RemoveListener(OnPCShopClosed);

		// Remove the popup from the list
		m_popups.Remove(shopPopupController);

		// Several possible results:
		if(shopPopup.packsPurchased.Count > 0) {
			// PC pack bought
			// Was PC the main currency or were we buying extra PC?
			if(m_currency == UserProfile.Currency.HARD) {
				// Try transaction again
				TryTransaction(false);	// Ask confirmation? No, looks weird after having purchased the gems
			} else {
				// Have we bought enough extra PC?
				if(m_extraPCCost <= UsersManager.currentUser.pc) {
					// Yes! Complete transaction
					DoTransaction(false);	// Ask confirmation? No, looks weird after having purchased the gems
				} else {
					// No! Refresh the MISSING_EXTRA_PC popup
					ResourcesFlowMissingPCPopup pcPopup = GetPopup<ResourcesFlowMissingPCPopup>();
					PopupController pcPopupController = pcPopup.GetComponent<PopupController>();
					pcPopupController.Reopen();

					// Update the missing extra PC and the recommended pack
					m_missingExtraPC = System.Math.Max(0, m_extraPCCost - UsersManager.currentUser.pc);	// Non-negative!
					m_recommendedPCPackDef = FindRecommendedPCPack(m_missingExtraPC);
					pcPopup.Init(m_recommendedPCPackDef);

					// Change state
					ChangeState(State.SHOWING_MISSING_EXTRA_PC);
				}
			}
		} else {
			// PC pack not bought
			// CancelFlow or stay in current state?
			// Let's stay in current state for now, feels weird to cancel the whole flow
			ChangeState(m_previousState);
		}
	}
}