// AddDummyWindow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.IO;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// Auxiliar window to add a dummy to a given group from the level editor.
	/// </summary>
	public class AddDummyWindow : EditorWindow {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		private static readonly string ASSETS_DIR = "Tools/LevelEditor/LDAssets/";
		private static readonly float THUMB_SIZE = 100f;	// pixels
		private static readonly Vector2 THUMB_GRID = new Vector2(6, 4);

		private static readonly string PREFIX = "DM_";

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		private Group m_targetGroup = null;
		private GameObject[] m_prefabs = null;

		private int m_prefabIdx = -1;
		private Vector2 m_scrollPos = Vector2.zero;

		//------------------------------------------------------------------//
		// STATIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Show the window.
		/// </summary>
		/// <param name="_targetGroup">The group where to add the new spawner</param>
		public static void Show(Group _targetGroup) {
			// Nothing to do if given level is not valid
			if(_targetGroup == null) return;

			// Create a new window instance
			AddDummyWindow window = new AddDummyWindow();
			
			// Setup window
			Vector2 initialSize = new Vector2(THUMB_SIZE * THUMB_GRID.x + 40f, THUMB_SIZE * THUMB_GRID.y + 160f);	// XxY thumbs plus some room for extra controls (approx)
			window.minSize = initialSize;
			window.maxSize = initialSize;
			window.m_targetGroup = _targetGroup;

			// Open at cursor's position
			// The window expects the position in screen coords
			Rect pos = new Rect();
			pos.x = Event.current.mousePosition.x - window.maxSize.x/2f;
			pos.y = Event.current.mousePosition.y + 7f;	// A little bit lower
			pos.position = EditorGUIUtility.GUIToScreenPoint(pos.position);
			
			// Show it as a dropdown list so window is automatically closed upon losing focus
			// http://docs.unity3d.com/ScriptReference/EditorWindow.ShowAsDropDown.html
			window.ShowAsDropDown(pos, initialSize);	// Adjust to parent window initially
		}
		
		//------------------------------------------------------------------//
		// GENERIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Default constructor.
		/// </summary>
		public AddDummyWindow() {

		}

		/// <summary>
		/// Pseudo-constructor.
		/// </summary>
		private void OnEnable() {
			// Load all the dummy prefabs
			// Can't be done in the constructor -_-
			m_prefabs = EditorUtils.LoadAllAssetsAtPath<GameObject>(ASSETS_DIR, "prefab", true);
		}
		
		/// <summary>
		/// Called every frame.
		/// </summary>
		private void Update() {

		}

		private void OnDestroy() {
			// Clear references
			m_prefabs = null;
		}
		
		//------------------------------------------------------------------//
		// WINDOW METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Update the inspector window.
		/// </summary>
		public void OnGUI() {
			// Reset indentation and set custom label width
			int indentLevelBackup = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			EditorGUIUtility.labelWidth = 50f;
			
			// Show all options in a list
			EditorGUILayout.BeginVertical(); {
				// Spacing
				GUILayout.Space(10);

				// Assets List
				EditorGUILayout.BeginVertical(EditorStyles.helpBox); {
					// Some spacing
					GUILayout.Space(5);

					// Selector
					m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, false, false); {
						// Create a custom content for each prefab, containing the asset preview and the prefab name
						GUIContent[] contents = new GUIContent[m_prefabs.Length];
						for(int i = 0; i < contents.Length; i++) {
							contents[i] = new GUIContent(m_prefabs[i].name, AssetPreview.GetAssetPreview(m_prefabs[i]));
						}

						// Use custom button styles
						GUIStyle style = new GUIStyle();
						style.fixedWidth = THUMB_SIZE;
						style.fixedHeight = THUMB_SIZE;
						style.imagePosition = ImagePosition.ImageAbove;
						style.alignment = TextAnchor.MiddleCenter;
						style.padding = new RectOffset(5, 5, 5, 5);
						style.onActive.background = Texture2DExt.Create(2, 2, Colors.skyBlue);
						style.onNormal.background = Texture2DExt.Create(2, 2, Colors.skyBlue);

						// The selection grid will do the job
						m_prefabIdx = GUILayout.SelectionGrid(m_prefabIdx, contents, (int)THUMB_GRID.x, style);
					} EditorGUILayout.EndScrollView();
				} EditorUtils.EndVerticalSafe();

				// Spacing
				GUILayout.Space(10);

				// Do it button
				EditorGUILayout.BeginHorizontal(); {
					// We don't want the button to be huge, so add flexible spaces before and after
					GUILayout.FlexibleSpace();

					// Button
					GUI.enabled = m_prefabIdx >= 0;
					if(GUILayout.Button("ADD DUMMY", GUILayout.Width(200), GUILayout.Height(50))) {
						// Just do it!
						AddNewDummy();
					}
					GUI.enabled = true;

					// We don't want the button to be huge, so add flexible spaces before and after
					GUILayout.FlexibleSpace();
				}EditorUtils.EndHorizontalSafe();

				// Spacing
				GUILayout.Space(10);
			} EditorUtils.EndVerticalSafe();

			// Restore indentation and label width
			EditorGUI.indentLevel = indentLevelBackup;
			EditorGUIUtility.labelWidth = 0f;	// According to Unity's documentation, this should restore the default value
		}

		/// <summary>
		/// Creates and adds a new spawner to the current level, using the selected parameters.
		/// </summary>
		private void AddNewDummy() {
			// Check all required parameters
			if(m_targetGroup == null) { ShowNotification(new GUIContent("Target level is not valid")); return; }
			if(m_targetGroup.spawnersObj == null) { ShowNotification(new GUIContent("Target level doesn't have a container for new spawners")); return; }
			if(m_prefabIdx < 0 || m_prefabIdx >= m_prefabs.Length) { ShowNotification(new GUIContent("Please select an entity prefab from the list")); return; }

			// Get dummy prefab
			GameObject dummyPrefab = m_prefabs[m_prefabIdx];

			// Create a new instance of the prefab and add it to the scene
			GameObject newDummyObj = PrefabUtility.InstantiatePrefab(dummyPrefab) as GameObject;
			newDummyObj.transform.SetParent(m_targetGroup.editorObj.transform, true);
			newDummyObj.SetLayerRecursively("LevelEditor");

			// Add a name based on the entity prefab
			string dummyName = dummyPrefab.name.Replace("PF_", "");	// Entity name without the preffix (if any)
			newDummyObj.SetUniqueName(PREFIX + dummyName);	// Add a prefix of our own and generate unique name

			// Add and initialize the transform lock component
			// Arbitrary default values fitted to the most common usage when level editing
			TransformLock newLock = newDummyObj.AddComponent<TransformLock>();
			newLock.SetPositionLock(false, false, true);
			newLock.SetRotationLock(true, true, false);
			newLock.SetScaleLock(false, false, true);

			// Make operation undoable
			Undo.RegisterCreatedObjectUndo(newDummyObj, "LevelEditor AddDummy");

			// Set position more or less to where the camera is pointing, forcing Z-0
			// Select new object in the hierarchy and center camera to it
			LevelEditor.PlaceInFrontOfCameraAtZPlane(newDummyObj, true);
			
			// Focus new object
			EditorUtils.FocusObject(newDummyObj);

			// Close window
			Close();
		}
	}
}