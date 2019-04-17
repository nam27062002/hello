// PopupPhotoShare.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Popup to display once a photo of the dragon has been taken.
/// </summary>
public class PopupPhotoShare : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Menu/PF_PopupPhotoShare";
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private RawImage m_preview = null;
	[SerializeField] private AspectRatioFitter m_aspectRatioFitter = null;
	[SerializeField] private TextMeshProUGUI m_popupTitleText = null;

	// Internal
	private Texture2D m_photo = null;
	private string m_caption = "";
	private string m_trackingZone = "";
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Clear references
		m_photo = null;
		m_preview.texture = null;
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with the given photo.
	/// </summary>
	/// <param name="_photo">The photo that has been taken.</param>
	/// <param name="_caption">The caption to be posted.</param>
	/// <param name="_popupTitle">The title to be displayed on the popup, already localized.</param>
	/// <param name="_trackingZone">The id to be tracked when sharing.</param>
	public void Init(Texture2D _photo, string _caption, string _popupTitle, string _trackingZone) {
		// Skip if given photo is not valid
		if(_photo == null) return;

		// Set popup title
		if(m_popupTitleText != null) m_popupTitleText.text = _popupTitle;

		// Store photo and caption for future use
		m_photo = _photo;
		m_caption = _caption;

		CreateScreenshotFile();

		// Init image with the photo
		m_preview.texture = _photo;
		m_preview.color = Color.white;	// Removing placeholder color

		// Match photo's aspect ratio
		m_aspectRatioFitter.aspectRatio = (float)_photo.width/(float)_photo.height;

		// Store tracking zone for future use
		m_trackingZone = _trackingZone;
	}

	private void CreateScreenshotFile() {
		byte[] bytes = m_photo.EncodeToPNG();
		string filePath = Application.temporaryCachePath + "/Screenshot.png";
		if ( File.Exists(filePath) )
		{
			File.Delete(filePath);
		}
		File.WriteAllBytes( filePath, bytes);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Share button has been pressed.
	/// </summary>
	public void OnShareButton() {
		// Tracking
		HDTrackingManager.Instance.Notify_ShareScreen(m_trackingZone);

		// Share Flow - DO NOT REMOVE!
		string filePath = Application.temporaryCachePath + "/Screenshot.png";
	    PlatformUtils.Instance.ShareImage( 
			filePath, 
			m_caption
		);
	}
}