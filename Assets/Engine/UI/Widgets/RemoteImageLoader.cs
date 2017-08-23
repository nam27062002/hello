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
using System.Collections.Generic;

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
	private const float REQUEST_TIMEOUT = 10f;	// Seconds

	public enum State {
		IDLE,
		QUEUE_PENDING,
		QUEUE,
		LOADING,
		POST_LOADING
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Comment("Optional URL")]
	[SerializeField] private string m_url = "";
	[SerializeField] private Texture2D m_placeholderImage = null;
	[SerializeField] private GameObject m_loadingPlaceholderPrefab = null;

	// Internal logic
	private State m_state = State.IDLE;
	public State state {
		get { return m_state; }
		set { ChangeState(value); }
	}

	// Internal refs
	private RawImage m_targetImage = null;
	private GameObject m_loadingPlaceholderInstance = null;

	// Others
	private float m_timer = 0f;
	private IEnumerator m_loadingCoroutine = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Mark as queue pending if we have a URL predefined
		if(!string.IsNullOrEmpty(m_url)) {
			m_state = State.QUEUE_PENDING;
		}

		// Get target image
		m_targetImage = GetComponent<RawImage>();
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// If dirty, load image
		if(m_state == State.QUEUE_PENDING) {
			Load(m_url);
		}
	}

	/// <summary>
	/// Update call.
	/// </summary>
	private void Update() {
		switch(m_state) {
			case State.IDLE:
			case State.QUEUE: {
				// Nothing to do!
			} break;

			case State.QUEUE_PENDING: {
				// Put into the queue
				ChangeState(State.QUEUE);
			} break;

			case State.LOADING: {
				// Check for timeout
				m_timer += Time.unscaledDeltaTime;
				if(m_timer >= REQUEST_TIMEOUT) {
					// Cancel request and pull off the queue
					ChangeState(State.IDLE);
				}
			} break;

			case State.POST_LOADING: {
				// Wait a couple of frames before going back to idle
				m_timer += 1f;
				if(m_timer >= 2) {
					ChangeState(State.IDLE);
				}
			} break;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Load the image at the stored url.
	/// If the component is disabled, the image will be loaded as soon as it is enabled.
	/// </summary>
	public void Load() {
		Load(m_url);
	}

	/// <summary>
	/// Load the image at the given url.
	/// If the component is disabled, the image will be loaded as soon as it is enabled.
	/// </summary>
	/// <param name="_url">Image URL, no validation done.</param>
	public void Load(string _url) {
		// Store url
		m_url = _url;

		// If we're disabled, mark as queue pending (image will be loaded the next time we get enabled)
		if(!this.isActiveAndEnabled) {
			ChangeState(State.QUEUE_PENDING);
			return;
		}

		// Otherwise put it straight to the queue
		ChangeState(State.QUEUE);
	}

	/// <summary>
	/// Change to a target state.
	/// </summary>
	/// <param name="_newState">New state.</param>
	public void ChangeState(State _newState) {
		// Actions to perform when leaving a state
		switch(m_state) {
			case State.LOADING: {
				// If interrupting the normal flow, do some extra stuff
				if(_newState != State.POST_LOADING) {
					// If we have a coroutine in progress, cancel it
					if(m_loadingCoroutine != null) {
						StopCoroutine(m_loadingCoroutine);
						m_loadingCoroutine = null;
					}

					// Remove ourselves from the manager
					RemoteImageLoaderManager.Remove(this);
				}

				// Show image
				ToggleImage(true);
			} break;

			case State.POST_LOADING: {
				// Clear coroutine reference
				m_loadingCoroutine = null;

				// Remove ourselves from the manager
				RemoteImageLoaderManager.Remove(this);
			} break;
		}

		// Save new state
		m_state = _newState;

		// Actions to perform when entering a state
		switch(_newState) {
			case State.QUEUE: {
				// Optimization: If url is empty, directly show the placeholder (no need to add it to the queue)
				if(string.IsNullOrEmpty(m_url)) {
					// Assign new texture to the image
					m_targetImage.texture = m_placeholderImage;

					// Change state
					ChangeState(State.POST_LOADING);
				} else {
					// Show loading
					ToggleImage(false);

					// Add ourselves to the manager
					RemoteImageLoaderManager.Add(this);
				}
			} break;

			case State.LOADING: {
				// Start the loading async task
				// See https://docs.unity3d.com/ScriptReference/WWW.LoadImageIntoTexture.html
				m_loadingCoroutine = LoadImageAsync(m_url);
				StartCoroutine(m_loadingCoroutine);

				// Reset timer
				m_timer = 0f;
			} break;

			case State.POST_LOADING: {
				// Reset timer
				m_timer = 0f;
			} break;
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Async loading coroutine.
	/// </summary>
	/// <param name="_url">Image URL, no validation done.</param>
	private IEnumerator LoadImageAsync(string _url) {
		// Aux vars
		Texture2D tex = null;

		// If URL is empty, use placeholder instead and finish
		if(string.IsNullOrEmpty(_url)) {
			// Don't yield
			tex = m_placeholderImage;
		} else {
			// Launch request
			WWW www = new WWW(_url);
			yield return www;

			// Store image into a new texture
			if(string.IsNullOrEmpty(www.error)) {
				tex = new Texture2D(4, 4, TextureFormat.DXT1, false);	// Size doesn't matter, will be changed by WWW
				www.LoadImageIntoTexture(tex);
			} else {
				// Put placeholder instead
				tex = m_placeholderImage;
			}
		}

		// Assign new texture to the image
		m_targetImage.texture = tex;

		// Change state
		ChangeState(State.POST_LOADING);
	}

	/// <summary>
	/// Show/hide the image or the loading placeholder instead.
	/// </summary>
	/// <param name="_show">Show the image?.</param>
	private void ToggleImage(bool _show) {
		if(_show) {
			// Show target image
			m_targetImage.color = Colors.white;

			// Hide loading placeholder!
			if(m_loadingPlaceholderInstance != null) {
				m_loadingPlaceholderInstance.SetActive(false);
			}
		} else {
			// Hide target image while loading
			m_targetImage.color = Colors.transparentWhite;

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
		}
	}
}