// FontReplacer.cs
// 
// Created by Alger Ortín Castellví on 08/03/2018
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

using TMPro;

using System.Globalization;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Font management for TMP textfields.
/// Known issues:
/// - Doesn't support submeshes with different fonts/materials than the root one
/// - Newly instantiated TextFields wont get the replacement!
/// </summary>
//[RequireComponent(typeof(TextMeshProUGUI))]
public class FontReplacer : MonoBehaviour, IBroadcastListener {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	// References
	private TMP_Text m_text = null;
	public TMP_Text text {
		get { 
			if(m_text == null) m_text = GetComponent<TMP_Text>();
			return m_text; 
		}
	}

	// Internal
	private string m_originalFontName = "";
	private string m_originalMaterialID = "";

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Awake() {
		// Get text reference
		m_text = GetComponent<TextMeshProUGUI>();
		Debug.Assert(m_text != null, "Required member!");

		// Store original setup
		m_originalFontName = m_text.font.name;
		m_originalMaterialID = FontManager.instance.GetMaterialIDFromName(m_originalFontName, m_text.fontSharedMaterial.name);
		FontManager.instance.RegisterDummyMaterial(m_originalFontName, m_originalMaterialID, m_text.fontSharedMaterial);

		// We want to listen to these events even when disabled!!
		Broadcaster.AddListener(BroadcastEventType.FONT_CHANGE_STARTED, this);
		Broadcaster.AddListener(BroadcastEventType.FONT_CHANGE_FINISHED, this);

		// Make sure we start with the right font!
		// [AOC] Unfortunately, wrong font is already loaded into memory :(
		//		 We'll workaround this issue by using a small font asset for edition, 
		//		 one that we can afford to have always in memory.
		if(FontManager.instance.isReady) {
			OnFontChangeStarted();	// This will at least loose reference to any instantiated font
		}
	}

	/// <summary>
	/// First update.
	/// </summary>
	public void Start() {
		if(FontManager.instance.isReady) {
			OnFontChangeFinished();	// Make sure we have the right font loaded!
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public void OnDestroy() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.FONT_CHANGE_STARTED, this);
		Broadcaster.RemoveListener(BroadcastEventType.FONT_CHANGE_FINISHED, this);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.FONT_CHANGE_STARTED:
            {
                OnFontChangeStarted();
            }break;
            case BroadcastEventType.FONT_CHANGE_FINISHED:
            {
                OnFontChangeFinished();
            }break;
        }
    }
    
	/// <summary>
	/// 
	/// </summary>
	private void OnFontChangeStarted() {
		// Simulate what the textfield should do by itself
		// Clear old font reference
		m_text.font = FontManager.instance.dummyFont;	// Text must always have a font assigned, so use dummy (low memory usage) while we unload old font and load new one
		m_text.fontSharedMaterial = m_text.font.material;
		m_text.fontMaterial = m_text.font.material;

		// Clear references from submeshes as well
		TMP_SubMeshUI[] submeshes = m_text.GetComponentsInChildren<TMP_SubMeshUI>();
		for(int i = 0; i < submeshes.Length; ++i) {
			submeshes[i].fontAsset = FontManager.instance.dummyFont;
			submeshes[i].sharedMaterial = FontManager.instance.dummyFont.material;
			submeshes[i].material = FontManager.instance.dummyFont.material;
			submeshes[i].RefreshMaterial();
		}

		// Disable text component while the font is being loaded
		m_text.enabled = false;
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnFontChangeFinished() {
		// Simulate what the textfield should do by itself
		// If original font asset doesn't match any of the font assets for the current font group, the default for this font group will be returned instead
		m_text.font = FontManager.instance.GetFontAsset(m_originalFontName);

		// Match material!
		FontManager.instance.ApplyFontMaterial(
			ref m_text, 
			m_originalMaterialID, 
			m_text.font.name, 
			m_originalFontName
		);

		// Enable text component once the font has been loaded
		m_text.enabled = true;
	}
}
