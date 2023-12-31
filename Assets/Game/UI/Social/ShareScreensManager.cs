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
	public static readonly Vector2Int CAPTURE_SIZE = new Vector2Int(512, 512);

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Re-use the same textures for all captures (since we're not gonna be doing more than one simultaneous capture)
	private Texture2D m_captureTex = null;
	public static Texture2D captureTex {
		get {
			// If texture is not created, do it now
			if(m_instance.m_captureTex == null) {
				instance.m_captureTex = new Texture2D(CAPTURE_SIZE.x, CAPTURE_SIZE.y, TextureFormat.RGB24, false);   // We don't need alpha :)
			}
			return m_instance.m_captureTex;
		}
	}

	private RenderTexture m_renderTex = null;
	public static RenderTexture renderTex {
		get {
			// If texture is not created, do it now
			if(m_instance.m_renderTex == null) {
				m_instance.m_renderTex = new RenderTexture(CAPTURE_SIZE.x, CAPTURE_SIZE.y, 32, RenderTextureFormat.ARGB32);
			}
			return m_instance.m_renderTex;
		}
	}

	// Internal references
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
		List<IShareScreen> toDestroy = new List<IShareScreen>(instance.m_pool.Count);
		foreach(KeyValuePair<string, IShareScreen> kvp in instance.m_pool) {
			toDestroy.Add(kvp.Value);
		}
		instance.m_pool.Clear();

		for(int i = 0; i < toDestroy.Count; ++i) {
			Destroy(toDestroy[i]);
		}

		// Clear cached textures
		if(instance.m_captureTex != null) {
			DestroyImmediate(instance.m_captureTex);
			instance.m_captureTex = null;
		}

		if(instance.m_renderTex != null) {
			DestroyImmediate(instance.m_renderTex);
			instance.m_renderTex = null;
		}
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
		GameObject newInstance = Instantiate<GameObject>(prefab, GameConstants.Vector3.zero, Quaternion.identity);

		// We will be using the name as key for the pool, so make sure it's consistent with the definition
		newInstance.name = prefab.name;

		// Grab the share screen setup component and return it!
		IShareScreen screen = newInstance.GetComponent<IShareScreen>();
		Debug.Assert(screen != null, "Share Scren Setup component couldn't be found in the prefab " + prefab.name);
		return screen;
	}
}