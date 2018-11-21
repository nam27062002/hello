// HDLogo.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/07/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple class to add logic to the Hungry Dragon logo.
/// </summary>
public class HDLogo : MonoBehaviour, IBroadcastListener {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	[System.Serializable]
	private class LogoData {
		[SkuList(DefinitionsCategory.LOCALIZATION, false)]
		public string languageSku = "";

		[FileList("Resources/UI/Common/Logo", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.png", false)]
		public string logoPath = "";
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private Image m_image = null;

	[FileList("Resources/UI/Common/Logo", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.png", false)]
	[SerializeField] private string m_defaultPath = "";

	[SerializeField] private LogoData[] m_paths = new LogoData[0];
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Broadcaster.AddListener(BroadcastEventType.LANGUAGE_CHANGED, this);

		// Make sure we have the right logo loaded
		Refresh();
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.LANGUAGE_CHANGED, this);
	}
    
    /// <summary>
    /// Ons the broadcast signal.
    /// </summary>
    /// <param name="eventType">Event type.</param>
    /// <param name="broadcastEventInfo">Broadcast event info.</param>
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.LANGUAGE_CHANGED:
            {
                OnLanguageChanged();
            }break;
        }
    }

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh logo.
	/// </summary>
	private void Refresh() {
		// Find logo path for the current language
		string langSku = LocalizationManager.SharedInstance.GetCurrentLanguageSKU();
		string path = m_defaultPath;
		for(int i = 0; i < m_paths.Length; ++i) {
			if(m_paths[i].languageSku == langSku) {
				path = m_paths[i].logoPath;
				break;
			}
		}

		// Check that the logo is no the same as already loaded
		string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
		if(m_image.sprite.name == fileName) return;

		// Load new image
		m_image.sprite = Resources.Load<Sprite>(path);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A new language has been selected.
	/// </summary>
	private void OnLanguageChanged() {
		Refresh();
	}
}