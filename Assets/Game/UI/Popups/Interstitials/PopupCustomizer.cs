// CustomizerPopup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/04/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Popup controller for popups coming from the customizer.
/// </summary>
public class PopupCustomizer : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	[System.Serializable]
	public class Field {
		public enum Type {
			TEXT,
			IMAGE,
			BUTTON
		};

		public Type type = Type.TEXT;
		public UnityEngine.Object target = null;
	}

	[System.Serializable]
	public class FieldsDictionary : SerializableDictionary<string, Field> { }

	[System.Serializable]
	public class TextfieldsDictionary : SerializableDictionary<string, TextMeshProUGUI> { }
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// [AOC] TODO!! Support as many fields as desired. Not supported by Calety right now :(
	//[SerializeField] private FieldsDictionary m_dynamicFields = new FieldsDictionary();

	// Exposed
	[InfoBox("All fields optional")]
	[SerializeField] private GameObject m_closeButtonRoot = null;
	[SerializeField] private TextMeshProUGUI m_titleText = null;
	[SerializeField] private TextfieldsDictionary m_otherTexts = new TextfieldsDictionary();
	[SerializeField] private RawImage m_image = null;
	[SerializeField] private List<Button> m_buttons = new List<Button>();

	// Internal
	private CustomizerManager.CustomiserPopupConfig m_config = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	public void InitFromConfig(CustomizerManager.CustomiserPopupConfig _config) {
		// Store config
		m_config = _config;

		// Close button
		if(m_closeButtonRoot != null) m_closeButtonRoot.SetActive(_config.m_bHasCloseButton);

		// Texts

		// Image

		// Buttons
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}