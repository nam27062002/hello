// UIScene3DManager.cs
// 
// Created by Alger Ortín Castellví on 09/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Manager to generate, control and destroy 3d scenes within a UI canvas.
/// All scenes will be rendered in the "3dOverUI" layer (that should be added to your Unity project).
/// To try to prevent scenes seeing each other, all scenes will be distributed alongside 
/// the X axis at the manager's Z, which will be quite far away in the -Z axis.
/// In any case, scene's transformations can always be manually changed to fit necessities.
/// </summary>
public class UIScene3DManager : SingletonMonoBehaviour<UIScene3DManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly Vector3 DEFAULT_OFFSET = new Vector3(200f, 0f, 0f);	// 200 units sounds like more than enough for standard usage of 3D over UI scenes (which usually are single objects)
	private static readonly float DEFAULT_Z = -1000f;
	public static readonly string LAYER_NAME = "3dOverUI";

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private List<UIScene3D> m_scenes = new List<UIScene3D>();

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Put far, far away
		this.transform.SetPosZ(DEFAULT_Z);
	}

	/// <summary>
	/// The manager has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<string>(EngineEvents.SCENE_UNLOADED, OnSceneUnloaded);
	}

	/// <summary>
	/// The manager has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(EngineEvents.SCENE_UNLOADED, OnSceneUnloaded);
	}

	//------------------------------------------------------------------//
	// PRIVATE METHODS													//
	//------------------------------------------------------------------//


	//------------------------------------------------------------------//
	// SINGLETON STATIC METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Create, initialize and add to the manager a new UIScene3D.
	/// Additionally, attach a given GameObject to it if desired.
	/// </summary>
	/// <param name="_obj">An object to be attached to the new scene, optional.</param>
	public static UIScene3D Create(GameObject _obj = null) {
		// Create a new game object with the UIScene3D component
		GameObject sceneObj = new GameObject("UIScene3D");
		UIScene3D newScene = sceneObj.AddComponent<UIScene3D>();

		// If an object is given, add it to the new object's root
		if(_obj != null) {
			_obj.transform.SetParent(sceneObj.transform, false);
			_obj.transform.localPosition = Vector3.zero;
		}

		// Add the scene to the manager
		Add(newScene);
		return newScene;
	}

	/// <summary>
	/// Create, initialize and add to the manager a UIScene3D loaded from a prefab.
	/// </summary>
	/// <param name="_prefab">The prefab to be instantiated.</param>
	public static UIScene3D CreateFromPrefab(GameObject _prefab) {
		// Check params
		if(_prefab == null) return null;

		// Instantiate the prefab
		GameObject sceneObj = GameObject.Instantiate<GameObject>(_prefab);

		// If it doesn't have a UIScene3D component attached, add it now
		UIScene3D newScene = sceneObj.GetComponent<UIScene3D>();
		if(newScene == null) {
			newScene = sceneObj.AddComponent<UIScene3D>();
		}

		// Add the scene to the manager
		Add(newScene);
		return newScene;
	}

	/// <summary>
	/// Create, initialize and add to the manager a UIScene3D loaded from a prefab loaded from the Resources folder.
	/// </summary>
	/// <param name="_resourcesPath">The path of prefab to be instantiated.</param>
	public static UIScene3D CreateFromResources(string _resourcesPath) {
		// Load prefab from resources
		GameObject prefab = Resources.Load<GameObject>(_resourcesPath);

		// Use prefab creator
		return CreateFromPrefab(prefab);
	}

	/// <summary>
	/// Add the given scene to the manager.
	/// Will move it and place it into the manager's hierarchy.
	/// Layer will be set to default UIScene3DManager.LAYER_NAME.
	/// </summary>
	/// <param name="_scene">The scene to be added.</param>
	public static void Add(UIScene3D _scene) {
		// Check parameters
		if(_scene == null) return;

		// Skip if scene is already added
		if(instance.m_scenes.Contains(_scene)) return;

		// Set layer to the whole scene hierarchy
		_scene.gameObject.SetLayerRecursively(LAYER_NAME);
		_scene.camera.cullingMask = LayerMask.GetMask(new string[] { LAYER_NAME });

		// Find an available spot
		int spotIdx = -1;
		for(int i = 0; i < instance.m_scenes.Count; i++) {
			if(instance.m_scenes[i] == null) {
				spotIdx = i;
				instance.m_scenes[i] = _scene;
				break;
			}
		}

		// No spots available, add it to the end of the list
		if(spotIdx < 0) {
			instance.m_scenes.Add(_scene);
			spotIdx = instance.m_scenes.Count - 1;
		}

		// Add as child of the manager object
		_scene.transform.SetParent(instance.transform, false);

		// Set position based on spot index
		_scene.transform.localPosition = DEFAULT_OFFSET * spotIdx;
	}

	/// <summary>
	/// Remove and optionally destroy the given scene from the manager.
	/// </summary>
	/// <param name="_scene">The scene to be removed.</param>
	/// <param name="_destroy">Whether to destroy the object containing the scene or not.</param>
	public static void Remove(UIScene3D _scene, bool _destroy = true) {
		// Check parameters
		if(_scene == null) return;

		// Find the scene in the list
		int spotIdx = instance.m_scenes.IndexOf(_scene);
		if(spotIdx < 0) return;

		// Destroy scene
		if(_destroy) GameObject.Destroy(_scene.gameObject);

		// Remove from the list
		// Don't change list size so we can reuse spots (this could be improved with a smarter algorithm, although we shouldn't have that many objects)
		instance.m_scenes[spotIdx] = null;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A game scene has been unloaded.
	/// </summary>
	/// <param name="_sceneName">Name of the scene that has just been unloaded.</param>
	private void OnSceneUnloaded(string _sceneName) {
		// Delete non-persistent scenes
		List<UIScene3D> toDelete = new List<UIScene3D>(m_scenes.Count);
		for(int i = 0; i < m_scenes.Count; i++) {
			if(m_scenes[i] != null) {
				if(!m_scenes[i].persistent) toDelete.Add(m_scenes[i]);
			}
		}
		for(int i = 0; i < toDelete.Count; i++) {
			Remove(toDelete[i]);
		}
	}
}
