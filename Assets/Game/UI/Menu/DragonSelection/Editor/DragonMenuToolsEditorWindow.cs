// DragonMenuToolsEditorWindow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/04/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom tools window for the Dragon menu.
/// </summary>
public class DragonMenuToolsEditorWindow : EditorWindow {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// STATICS																  //
	//------------------------------------------------------------------------//
	// Windows instance
	private static DragonMenuToolsEditorWindow m_instance = null;
	public static DragonMenuToolsEditorWindow instance {
		get {
			if(m_instance == null) {
				m_instance = (DragonMenuToolsEditorWindow)EditorWindow.GetWindow(typeof(DragonMenuToolsEditorWindow), false, "Dragon Menu Tools", true);
			}
			return m_instance;
		}
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private Vector2 m_scrollPos = Vector2.zero;
	private int m_cameraScrollIdx = 0;
	private float m_cameraScrollDelta = 0f;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Shows the window.
	/// </summary>
	public static void ShowWindow() {
		// Setup window
		instance.titleContent = new GUIContent("Dragon Menu Tools");
		instance.minSize = new Vector2(250, 100);	// Arbitrary
		instance.maxSize = new Vector2(float.PositiveInfinity, float.PositiveInfinity);	// No limit

		// Show it
		instance.ShowUtility();
	}

	/// <summary>
	/// Update the inspector window.
	/// </summary>
	public void OnGUI() {
		// Scrollable
		bool errorCheck = true;
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos); {
			// Initial space
			EditorGUILayout.Space();

			// Don't do anything if not on the menu scene
			MenuSceneController menuSceneController = FindObjectOfType<MenuSceneController>();
			if(menuSceneController == null) {
				EditorGUILayout.HelpBox("This tool can only be used at the SC_Menu scene!", MessageType.Error);
				errorCheck = false;
			}

			// Not use during runtime
			if(EditorApplication.isPlaying) {
				EditorGUILayout.HelpBox("Tool shouldn't be used in play mode.", MessageType.Warning);
				errorCheck = false;
			}

			// Camera control
			if(errorCheck) {
				// Get required components
				Camera mainCamera = GameObjectExt.FindComponent<Camera>(false, "Camera3D");
				PathFollower posPathFollower = GameObjectExt.FindComponent<PathFollower>(false, "PosPath");
				PathFollower lookAtPathFollower = GameObjectExt.FindComponent<PathFollower>(false, "LookAtPath");

				// Label
				GUILayout.Label("Camera Preview", CustomEditorStyles.commentLabelLeft);

				// Slot idx slider
				GUI.changed = false;
				m_cameraScrollIdx = EditorGUILayout.IntSlider("Dragon Slot Idx", m_cameraScrollIdx, 0, Mathf.Max(posPathFollower.path.pointCount, lookAtPathFollower.path.pointCount));
				if(GUI.changed) {
					// Apply to camera!
					posPathFollower.snapPoint = m_cameraScrollIdx;
					lookAtPathFollower.snapPoint = m_cameraScrollIdx;
					mainCamera.transform.position = posPathFollower.position;
					mainCamera.transform.LookAt(lookAtPathFollower.position);

					// Sync with delta
					m_cameraScrollDelta = posPathFollower.delta;
				}

				// Delta
				GUI.changed = false;
				m_cameraScrollDelta = EditorGUILayout.Slider("Dragon Scroll Delta", m_cameraScrollDelta, 0f, 1f);
				if(GUI.changed) {
					// Apply to camera!
					posPathFollower.delta = m_cameraScrollDelta;
					lookAtPathFollower.delta = m_cameraScrollDelta;
					mainCamera.transform.position = posPathFollower.position;
					mainCamera.transform.LookAt(lookAtPathFollower.position);

					// Sync with snap point
					m_cameraScrollIdx = posPathFollower.snapPoint;
				}
			}

			// Reload Dragon Prefabs
			if(errorCheck) {
				// Just the button to do it!
				EditorGUILayout.Space();
				if(GUILayout.Button("Reload Dragon Prefabs", GUILayout.Height(50f))) {
					// Find slots
					int slotIdx = 0;
					GameObject slotObj = GameObject.Find("DragonSlot" + slotIdx);
					List<GameObject> slots = new List<GameObject>();
					while(slotObj != null) {
						slots.Add(slotObj);
						slotIdx++;
						slotObj = GameObject.Find("DragonSlot" + slotIdx);
					}

					// Load dragon definitions and sort them by order
					List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.DRAGONS);
					DefinitionsManager.SharedInstance.SortByProperty(ref defs, "order", DefinitionsManager.SortType.NUMERIC);

					// Load dragon previews!
					for(int i = 0; i < defs.Count; i++) {
						// Check enough slots!
						if(i >= slots.Count) {
							Debug.LogError("Not enough slots for dragon " + defs[i].sku);
							continue;
						}

						// If possible, use the MenuDragonLoader objects in each slot to replace current dragon preview by the new one
						MenuDragonLoader dragonLoader = slots[i].GetComponentInChildren<MenuDragonLoader>();
						if(dragonLoader != null) {
							// Change target sku
							dragonLoader.dragonSku = defs[i].sku;

							// Reload!
							dragonLoader.mode = MenuDragonLoader.Mode.MANUAL;
							dragonLoader.RefreshDragon();
						} else {
							// Dragon loader not present, instantiate dragon directly
							// Delete existing previews first
							MenuDragonPreview[] toDelete = slots[i].GetComponentsInChildren<MenuDragonPreview>();
							for(int j = 0; j < toDelete.Length; j++) {
								GameObject.DestroyImmediate(toDelete[j].gameObject);
							}

							// Instantiate the prefab and add it as child of the slot
							GameObject dragonPrefab = Resources.Load<GameObject>(DragonData.MENU_PREFAB_PATH + defs[i].GetAsString("menuPrefab"));
							GameObject dragonObj = PrefabUtility.InstantiatePrefab(dragonPrefab) as GameObject;
							dragonObj.transform.SetParent(slots[i].transform, false);
							dragonObj.name = dragonPrefab.name;	// Remove the "(Clone)" text
						}
					}
				}
			}
		} EditorGUILayout.EndScrollView();
	}
}