// RemoteImageLoader.cs
// 
// Created by Alger Ortín Castellví on 12/07/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Load an image from a url and show a placeholder meanwhile.
/// </summary>
[RequireComponent(typeof(RawImage))]
public class RemoteImageLoader : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Comment("Optional URL")]
	[SerializeField] private string m_url = "";
	[SerializeField] private GameObject m_loadingPlaceholderPrefab = null;

	// Internal
	private RawImage m_targetImage = null;
	private GameObject m_loadingPlaceholderInstance = null;
	private bool m_dirty = false;

	private IEnumerator m_loadingCoroutine = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Mark as dirty if we have a URL predefined
		m_dirty = !string.IsNullOrEmpty(m_url);

		// Get target image
		m_targetImage = GetComponent<RawImage>();
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// If dirty, load image
		if(m_dirty) {
			Load(m_url);
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Load the image at the given url.
	/// If the component is disabled, the image will be loaded as soon as it is enabled.
	/// </summary>
	/// <param name="_url">Image URL, no validation done.</param>
	public void Load(string _url) {
		// Store url
		m_url = _url;

		// If we're disabled, mark as dirty (image will be loaded the next time we get enabled)
		if(!this.isActiveAndEnabled) {
			m_dirty = true;
			return;
		}

		// If we have a coroutine in progress, cancel it
		if(m_loadingCoroutine != null) {
			// StopCoroutine throws a nasty error on the console, fixed in 5.6 (https://issuetracker.unity3d.com/issues/www-getting-error-message-coroutine-continue-failure-when-stopping-a-www-coroutine)
			// Using StopAllCoroutines() in the meanwhile
			//StopCoroutine(m_loadingCoroutine);
			StopAllCoroutines();

			m_loadingCoroutine = null;
		}

		// Start the loading process!
		// Quick test from https://docs.unity3d.com/ScriptReference/WWW.LoadImageIntoTexture.html
		m_loadingCoroutine = LoadImageAsync(_url);
		StartCoroutine(m_loadingCoroutine);

		// Show loading placeholder
		if(m_loadingPlaceholderPrefab != null) {
			// If it's already instantiated, re-use it!
			if(m_loadingPlaceholderInstance != null) {
				m_loadingPlaceholderInstance.SetActive(true);
			} else {
				m_loadingPlaceholderInstance = GameObject.Instantiate<GameObject>(m_loadingPlaceholderPrefab, this.transform, false);
				RectTransform rt = m_loadingPlaceholderInstance.transform as RectTransform;
				//rt.anchorMin = Vector2.zero;
				//rt.anchorMax = Vector2.one;
				rt.anchoredPosition = Vector2.zero;
			}
		}

		// No longer dirty ^^
		m_dirty = false;
	}

	/// <summary>
	/// Async loading coroutine.
	/// </summary>
	/// <param name="_url">Image URL, no validation done.</param>
	private IEnumerator LoadImageAsync(string _url) {
		// Launch request
		WWW www = new WWW(_url);
		yield return www;

		// Store image into a new texture
		if(string.IsNullOrEmpty(www.error)) {
			Texture2D tex = new Texture2D(4, 4, TextureFormat.DXT1, false);	// Size doesn't matter, will be changed by WWW
			www.LoadImageIntoTexture(tex);

			// Assign new texture to the image
			m_targetImage.texture = tex;

			// Reset color modifier
			m_targetImage.color = Color.white;

			// Hide loading placeholder!
			if(m_loadingPlaceholderInstance != null) {
				m_loadingPlaceholderInstance.SetActive(false);
			}
		} else {
			// Hide image
			m_targetImage.color = Colors.transparentWhite;
		}

		// Clear coroutine reference
		m_loadingCoroutine = null;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}