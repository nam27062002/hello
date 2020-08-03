// PopupAssetsDownloadFlowErrorGeneric.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/03/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar popup to the Assets Download Flow.
/// </summary>
public class PopupAssetsDownloadFlowErrorGeneric : PopupAssetsDownloadFlow {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private TextMeshProUGUI m_errorCodeText = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh popup's visuals.
	/// </summary>
	public override void Refresh() {
		// Call parent
		base.Refresh();

		// Set error code
		if(m_errorCodeText != null) {
			if(m_handle != null) {
				m_errorCodeText.text = m_handle.GetErrorCode().ToString();
			} else {
				m_errorCodeText.text = "-";	// Shouldn't happen!
			}
		}
	}
}