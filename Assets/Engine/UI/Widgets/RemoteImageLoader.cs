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
/// Has a small manager integrated to avoid having too many images loading at the same time.
/// </summary>
[RequireComponent(typeof(RawImage))]
public class RemoteImageLoader : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const int MAX_SIMULTANEOUS_LOADS = 3;
	private static List<RemoteImageLoader> s_queue = new List<RemoteImageLoader>();
	private static HashSet<RemoteImageLoader> s_loading = new HashSet<RemoteImageLoader>();
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Comment("Optional URL")]
	[SerializeField] private string m_url = "";
	[SerializeField] private Texture2D m_placeholderImage = null;
	[SerializeField] private GameObject m_loadingPlaceholderPrefab = null;

	// Internal
	private RawImage m_targetImage = null;
	private GameObject m_loadingPlaceholderInstance = null;
	private bool m_queuePending = false;

	private IEnumerator m_loadingCoroutine = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Mark as dirty if we have a URL predefined
		m_queuePending = !string.IsNullOrEmpty(m_url);

		// Get target image
		m_targetImage = GetComponent<RawImage>();
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// If dirty, load image
		if(m_queuePending) {
			Load(m_url);
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

		// If we're disabled, mark as dirty (image will be loaded the next time we get enabled)
		if(!this.isActiveAndEnabled) {
			m_queuePending = true;
			return;
		}

		// If we have a coroutine in progress, cancel it
		if(m_loadingCoroutine != null) {
			// StopCoroutine throws a nasty error on the console, fixed in 5.6 (https://issuetracker.unity3d.com/issues/www-getting-error-message-coroutine-continue-failure-when-stopping-a-www-coroutine)
			// Using StopAllCoroutines() in the meanwhile
			//StopCoroutine(m_loadingCoroutine);
			StopAllCoroutines();
			m_loadingCoroutine = null;

			// Remove ourselves from the queue and the loading set
			s_queue.Remove(this);
			s_loading.Remove(this);
		}

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

		// Add ourselves to the queue
		s_queue.Add(this);
		Debug.Log("<color=blue>Enqueuing " + _url + "</color>" + " <color=orange>(" + s_queue.Count + ", " + s_loading.Count + ")</color>");

		// Check the queue
		CheckQueue();

		// No longer dirty ^^
		m_queuePending = false;
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Starts the loading async task, doesn't check the queue.
	/// </summary>
	private void StartLoadingTask() {
		// Just do it
		// See https://docs.unity3d.com/ScriptReference/WWW.LoadImageIntoTexture.html
		m_loadingCoroutine = LoadImageAsync(m_url);
		StartCoroutine(m_loadingCoroutine);
	}

	/// <summary>
	/// Async loading coroutine.
	/// </summary>
	/// <param name="_url">Image URL, no validation done.</param>
	private IEnumerator LoadImageAsync(string _url) {
		Debug.Log("<color=yellow>Starting " + _url + "</color>" + " <color=orange>(" + s_queue.Count + ", " + s_loading.Count + ")</color>");

		// Aux vars
		Texture2D tex = null;

		// If URL is empty, use placeholder instead and finish
		if(string.IsNullOrEmpty(_url)) {
			// Don't yield
			tex = m_placeholderImage;
		} else {
			// Launch request
			Debug.Log("<color=yellow>Starting WWW request</color>");
			WWW www = new WWW(_url);
			yield return www;

			// Store image into a new texture
			if(string.IsNullOrEmpty(www.error)) {
				Debug.Log("<color=green>WWW Success!</color>");
				tex = new Texture2D(4, 4, TextureFormat.DXT1, false);	// Size doesn't matter, will be changed by WWW
				www.LoadImageIntoTexture(tex);
			} else {
				// Put placeholder instead
				Debug.Log("<color=red>WWW Error!</color>");
				tex = m_placeholderImage;
			}
		}

		Debug.Log("<color=green>Received " + _url + "</color>" + " <color=orange>(" + s_queue.Count + ", " + s_loading.Count + ")</color>");

		// Assign new texture to the image
		m_targetImage.texture = tex;

		// Loading finished, show back
		m_targetImage.color = Colors.white;

		// Hide loading placeholder!
		if(m_loadingPlaceholderInstance != null) {
			m_loadingPlaceholderInstance.SetActive(false);
		}

		// Wait a couple of frames before updating the queue

		// Clear coroutine reference
		Debug.Log("<color=green>FINISHED! " + _url + "</color>" + " <color=orange>(" + s_queue.Count + ", " + s_loading.Count + ")</color>");
		m_loadingCoroutine = null;

		// Free loading slot and check queue
		s_loading.Remove(this);
		CheckQueue();
	}

	//------------------------------------------------------------------------//
	// MANAGER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether we can start loading the first element in the queue.
	/// </summary>
	private static void CheckQueue() {
		// Nothing to do if queue is empty
		if(s_queue.Count == 0) return;

		// Nothing to do either if all loading slots are full
		if(s_loading.Count >= MAX_SIMULTANEOUS_LOADS) return;

		Debug.Log("<color=orange>Checking queue: " + s_queue.Count + ", " + s_loading.Count + "</color>");

		// Pop the first element in the queue into the loading collection
		RemoteImageLoader loader = s_queue[0];
		s_loading.Add(loader);
		s_queue.RemoveAt(0);

		// Start the loading process!
		loader.StartLoadingTask();

		// Check whether we can load next element in the queue
		CheckQueue();
	}
}