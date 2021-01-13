// PopupTermsAndConditionsMoreInfo.cs
// Hungry Dragon
//
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// COPPA/GDPR More info popup, showing the GDPR Checkboxes.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupConsentMoreInfo : IPopupConsentGDPR {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Message/Consent/PF_PopupConsentMoreInfo";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Public properties
	private PopupController m_popupController = null;
	public PopupController popupController {
		get {
			if(m_popupController == null) {
				m_popupController = GetComponent<PopupController>();
			}
			return m_popupController;
		}
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}
