// RemoteImageLoaderManager.cs
// 
// Created by Alger Ortín Castellví on 23/08/2017.
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
/// Simple manager to avoid having too many images loading at the same time.
/// </summary>
public class RemoteImageLoaderManager : UbiBCN.SingletonMonoBehaviour<RemoteImageLoaderManager> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const int MAX_SIMULTANEOUS_LOADS = 3;

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Queues
	private List<RemoteImageLoader> m_queue = new List<RemoteImageLoader>();
	private HashSet<RemoteImageLoader> m_loading = new HashSet<RemoteImageLoader>();

	// Internal
	private bool m_dirty = true;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// Update call.
	/// </summary>
	private void Update() {
		// Process queue
		if(m_dirty) {
			ProcessQueue();
		}
	}

	//------------------------------------------------------------------------//
	// PUBLIC MANAGER METHODS												  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Register a loader in the manager.
	/// </summary>
	/// <param name="_loader">Loader to be registered.</param>
	public static void Add(RemoteImageLoader _loader) {
		// Make sure the loader is not already added
		Remove(_loader);

		// Check state, add to corresponding collection
		switch(_loader.state) {
			// Queue
			case RemoteImageLoader.State.QUEUE: {
				instance.m_queue.Add(_loader);
			} break;

			// Loading
			case RemoteImageLoader.State.LOADING:
			case RemoteImageLoader.State.POST_LOADING: {
				instance.m_loading.Add(_loader);
			} break;

			// Ignore
			default: {
				// Do nothing
			} break;
		}

		// Mark as dirty
		instance.m_dirty = true;
	}

	/// <summary>
	/// Unregister a loader from the manager.
	/// </summary>
	/// <param name="_loader">Loader to be unregistered.</param>
	public static void Remove(RemoteImageLoader _loader) {
		// Check both collections
		instance.m_queue.Remove(_loader);
		instance.m_loading.Remove(_loader);

		// Mark as dirty
		instance.m_dirty = true;
	}

	/// <summary>
	/// Get a string representation of the current state of the manager.
	/// </summary>
	/// <returns>A <see cref="System.String"/> that represents the current state of the manager.</returns>
	public static string ToString() {
		return "<color=orange>(" + instance.m_queue.Count + ", " + instance.m_queue.Count + ")</color>";
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether we can start loading the first element in the queue.
	/// </summary>
	private void ProcessQueue() {
		// Nothing to do if queue is empty
		if(m_queue.Count == 0) return;

		// Nothing to do either if all loading slots are full
		if(m_loading.Count >= MAX_SIMULTANEOUS_LOADS) return;

		// Pop the first element in the queue into the loading collection
		RemoteImageLoader loader = m_queue[0];
		m_loading.Add(loader);
		m_queue.RemoveAt(0);

		// Start the loading process!
		loader.ChangeState(RemoteImageLoader.State.LOADING);

		// Check whether we can load next element in the queue
		ProcessQueue();
	}
}