// PopupShopOfferPack.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/02/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the Offer Pack Popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupShopOfferPack : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Economy/PF_PopupShopOfferPack";
	private const float REFRESH_FREQUENCY = 1f;	// Seconds
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] protected ShopMultiRewardPill m_rootPill = null;

	// Internal
	protected OfferPack m_pack = null;
	protected int m_initializationPending = -1;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to external events
		m_rootPill.OnPurchaseSuccess.AddListener(OnPurchaseSuccessful);
	}

	/// <summary>
	/// First update call.
	/// </summary>
	protected void Start() {
		InvokeRepeating("PeriodicRefresh", 0f, REFRESH_FREQUENCY);

    }

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		m_rootPill.OnPurchaseSuccess.RemoveListener(OnPurchaseSuccessful);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with a given pack's data.
	/// </summary>
	/// <param name="_pack">Pack.</param>
	public virtual void InitFromOfferPack(OfferPack _pack) {
		// Store pack
		m_pack = _pack;

		// Clear the pill
		m_rootPill.InitFromOfferPack(null);

		// Don't do anything else if pack is null (shouldn't happen though :s)
		if(m_pack == null) return;

		// Initialize pill with target offer pack
		// Delay until popup is ready
		m_initializationPending = 2;	// Delat some frames until popup is ready
		//m_rootPill.InitFromOfferPack(m_pack);
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Initialize visuals if needed
		if(m_initializationPending > 0) {
			m_initializationPending--;
			if(m_initializationPending <= 0) {
				Refresh();
				m_initializationPending = -1;
			}
		}
	}

    /// <summary>
    /// Refresh the view
    /// </summary>
    protected virtual void Refresh ()
    {
		m_rootPill.InitFromOfferPack(m_pack);
	}

	/// <summary>
	/// Called at regular intervals.
	/// </summary>
	private void PeriodicRefresh() {
		// Nothing if not enabled
		if(!this.isActiveAndEnabled) return;

		// Propagate to active pill
		m_rootPill.RefreshTimer();

		// If invalid pack or pack has expired, close popup
		if(m_pack == null || !m_pack.isActive) {
			GetComponent<PopupController>().Close(true);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The popup has been opened.
	/// </summary>
	public void OnShowPostAnimation() {
		// Update pack's view tracking
		m_pack.NotifyPopupDisplayed();
	}

	/// <summary>
	/// The shop button has been pressed.
	/// </summary>
	public void OnShopButton() {
		// Close this popup
		GetComponent<PopupController>().Close(true);

		// Open shop popup - unless already open
		if(PopupManager.GetOpenPopup(PopupShop.PATH) == null) {
			PopupController shopPopup = PopupManager.LoadPopup(PopupShop.PATH);
			shopPopup.GetComponent<PopupShop>().Init(ShopController.Mode.DEFAULT, "Featured_Offer");
			shopPopup.Open();
		}
	}

	/// <summary
	/// Successful purchase.
	/// </summary>
	/// <param name="_pill">The pill that triggered the event</param>
	private void OnPurchaseSuccessful(IShopPill _pill) {
		// Close popup
		GetComponent<PopupController>().Close(true);
	}

	//------------------------------------------------------------------------//
	// STATIC UTILS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Loads the popup layout best matching the given offer's content and initializes it.
	/// The popup WON'T be opened nor enqueued, the user is responsible to do so.
	/// </summary>
	/// <param name="_pack">The pack used to select the popup layout and initialize it.</param>
	/// <returns>The loaded popup instance.</returns>
	public static PopupController LoadPopupForOfferPack(OfferPack _pack) {
		// Check params
		if(_pack == null) return null;

		// Aux vars
		PopupController popup = null;

        
		if (_pack.type == OfferPack.Type.WELCOME_BACK)
		{
			// Does the pack contain more than 1 skin?
			if (_pack.GetDragonsSkinsCount() > 1)
			{
				// Yes!! Use skins popup layout
				popup = PopupManager.LoadPopup(PopupShopWelcomeBackOfferPackSkins.PATH);
				popup.GetComponent<PopupShopOfferPackSkins>().InitFromOfferPack(_pack);
			}
			else
			{
				// No, use the default offer layout
				popup = PopupManager.LoadPopup(PopupShopWelcomeBackOfferPack.PATH);
				popup.GetComponent<PopupShopOfferPack>().InitFromOfferPack(_pack);
			}
		}

        else
        {
			// Does the pack contain more than 1 skin?
			if (_pack.GetDragonsSkinsCount() > 1)
			{
				// Yes!! Use skins popup layout
				popup = PopupManager.LoadPopup(PopupShopOfferPackSkins.PATH);
				popup.GetComponent<PopupShopOfferPackSkins>().InitFromOfferPack(_pack);
			}
			else
			{
				// No, use the default offer layout
				popup = PopupManager.LoadPopup(PopupShopOfferPack.PATH);
				popup.GetComponent<PopupShopOfferPack>().InitFromOfferPack(_pack);
			}

		}



		// Done!
		return popup;
	}
}