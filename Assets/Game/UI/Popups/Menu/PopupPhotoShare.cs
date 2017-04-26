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

	// Internal
	private Texture2D m_photo = null;
	
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
	public void Init(Texture2D _photo) {
		// Skip if given photo is not valid
		if(_photo == null) return;

		// Store photo for future use
		m_photo = _photo;

		// Init image with the photo
		m_preview.texture = _photo;
		m_preview.color = Color.white;	// Removing placeholder color

		// Match photo's aspect ratio
		m_aspectRatioFitter.aspectRatio = (float)_photo.width/(float)_photo.height;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Share button has been pressed.
	/// </summary>
	public void OnShareButton() {
		// [AOC] TODO!! Share Flow
		UIFeedbackText.CreateAndLaunch(
			LocalizationManager.SharedInstance.Localize("TID_GEN_COMING_SOON"),
			new Vector2(0.5f, 0.5f),
			(RectTransform)this.GetComponentInParent<Canvas>().transform
		);
	}
}