// ShareScreensManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 10/04/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Singleton manager for handling Share Screens.
/// The screens will be cached for faster reuse.
/// </summary>
public class ShareScreensManager : UbiBCN.SingletonMonoBehaviour<ShareScreensManager> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private Dictionary<string, IShareScreen> m_pool = new Dictionary<string, IShareScreen>();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected override void OnDestroy() {
		// Clear all instances
		Clear();

		// Call parent
		base.OnDestroy();
	}

	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Obtain a reference to the instance of the share screen for a given location.
	/// The screen will try to be retrieved from the existing pool and, if not found,
	/// a new instance will be created.
	/// The screen will still need to be initialized.
	/// </summary>
	/// <returns>The share screen corresponding to the requested location.</returns>
	/// <param name="_shareLocationSku">Share location sku.</param>
	public static IShareScreen GetShareScreen(string _shareLocationSku) {
		// Get location definition
		DefinitionNode locationDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SHARE_LOCATIONS, _shareLocationSku);
		Debug.Assert(locationDef != null, "Share Location Definition for " + _shareLocationSku + " couldn't be found!");

		// Find out prefab's name
		string prefabName = locationDef.GetAsString("prefab");

		// Is the screen already in the pool?
		IShareScreen screen = null;
		if(!instance.m_pool.TryGetValue(prefabName, out screen)) {
			// No! Create a new instance
			screen = instance.InstantiateShareScreen(prefabName);

			// Add it to the pool
			instance.m_pool.Add(screen.name, screen);	// Use prefab's name as key, since different locations might reuse the same prefab
		}

		// If instance is null, something went really, really wrong
		Debug.Assert(screen != null, "Couldn't retrieve screen for share location " + _shareLocationSku);

		// Activate screen object
		screen.gameObject.SetActive(true);

		// Done!
		return screen;
	}

	/// <summary>
	/// Destroy all cached share screen instances.
	/// </summary>
	public static void Clear() {
		// Iterate all pooled instances and destroy them
		foreach(KeyValuePair<string, IShareScreen> kvp in instance.m_pool) {
			Destroy(kvp.Value.gameObject);
			instance.m_pool[kvp.Key] = null;
		}

		// Clear the collection
		instance.m_pool.Clear();
	}

	/// <summary>
	/// Removes a specific screen from the pool.
	/// </summary>
	/// <param name="_screen">The screen to be removed.</param>
	public static void RemoveScreen(IShareScreen _screen) {
		// No better way to remove by value in a dictionary than iterating all the entries
		List<string> keysToRemove = new List<string>();
		foreach(KeyValuePair<string, IShareScreen> kvp in instance.m_pool) {
			if(kvp.Value == _screen) {
				keysToRemove.Add(kvp.Key);
			}
		}

		// Remove all target entries
		for(int i = 0; i < keysToRemove.Count; ++i) {
			instance.m_pool.Remove(keysToRemove[i]);
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Create a new instance of the share screen setup for the given location.
	/// </summary>
	/// <returns>The newly created instance.</returns>
	/// <param name="_prefabName">The prefab to be loaded (as defined in shareLocationDefinitions).</param>
	private IShareScreen InstantiateShareScreen(string _prefabName) {
		// Load prefab
		GameObject prefab = Resources.Load<GameObject>(UIConstants.SHARE_SCREENS_PATH + _prefabName);
		Debug.Assert(prefab != null, "Share Setup Prefab " +_prefabName + " couldn't be found!");

		// Create a new instance
		// Put it on the root of the main scene, since the setups have their own camera and will be positioned later during the setup
		GameObject newInstance = Instantiate<GameObject>(prefab);

		// We will be using the name as key for the pool, so make sure it's consistent with the definition
		newInstance.name = prefab.name;

		// Grab the share screen setup component and return it!
		IShareScreen screen = newInstance.GetComponent<IShareScreen>();
		Debug.Assert(screen != null, "Share Scren Setup component couldn't be found in the prefab " + prefab.name);
		return screen;
	}
}