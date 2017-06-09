// CurrencyShopPopupLauncher.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Specialization of the popup launcher meant for the currency popup.
/// </summary>
public class CurrencyShopPopupLauncher : PopupLauncher {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private PopupCurrencyShop.Mode m_mode = PopupCurrencyShop.Mode.DEFAULT;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		OnPopupInit.AddListener(OnInitPopup);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		OnPopupInit.RemoveListener(OnInitPopup);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A popup has been destroyed.
	/// </summary>
	/// <param name="_popup">The popup that has just been destroyed.</param>
	private void OnInitPopup(PopupController _popup) {
		// Initialize target popup with the requested mode
		_popup.GetComponent<PopupCurrencyShop>().Init(m_mode);
	}
}