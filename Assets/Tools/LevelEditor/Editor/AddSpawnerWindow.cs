// AddSpawnerWindow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 06/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// Auxiliar window to add a spawner to the current level from the level editor.
	/// </summary>
	public class AddSpawnerWindow : EditorWindow {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		private enum Shape {
			POINT,
			RECTANGLE,
			CIRCLE
		};

		private enum Type {
			STANDARD,
			FLOCK
		};

		private static readonly string RESOURCES_DIR = "Game/Entities";
		private static readonly float THUMB_SIZE = 100f;	// pixels
		private static readonly Vector2 THUMB_GRID = new Vector2(6, 4);

		private static readonly string PREFIX = "SP_";

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		private Group m_targetGroup = null;
		private GameObject[] m_entityPrefabs = null;

		private Shape m_shape = Shape.POINT;
		private Type m_type = Type.STANDARD;
		private int m_entityPrefabIdx = -1;
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
			AddSpawnerWindow window = new AddSpawnerWindow();
			
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
		public AddSpawnerWindow() {

		}

		/// <summary>
		/// Pseudo-constructor.
		/// </summary>
		private void OnEnable() {
			// Find all spawnable prefabs
			// Can't be done in the constructor -_-
			m_entityPrefabs = Resources.LoadAll<GameObject>(RESOURCES_DIR);
		}
		
		/// <summary>
		/// Called every frame.
		/// </summary>
		private void Update() {

		}

		private void OnDestroy() {
			// Clear references and unload assets
			m_entityPrefabs = null;
			Resources.UnloadUnusedAssets();
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

				// Shape
				m_shape = (Shape)EditorGUILayout.EnumPopup("Shape:", m_shape, GUILayout.Height(20));

				// Type
				m_type = (Type)EditorGUILayout.EnumPopup("Type:", m_type, GUILayout.Height(20));

				// Label
				GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
				labelStyle.alignment = TextAnchor.MiddleCenter;
				GUILayout.Label("Entity:", labelStyle);

				// Entity
				EditorGUILayout.BeginVertical(EditorStyles.helpBox); {
					// Some spacing
					GUILayout.Space(5);

					// Selector
					m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, false, false); {
						// Create a custom content for each prefab, containing the asset preview and the prefab name
						GUIContent[] contents = new GUIContent[m_entityPrefabs.Length];
						for(int i = 0; i < contents.Length; i++) {
							contents[i] = new GUIContent(m_entityPrefabs[i].name, AssetPreview.GetAssetPreview(m_entityPrefabs[i]));
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
						m_entityPrefabIdx = GUILayout.SelectionGrid(m_entityPrefabIdx, contents, (int)THUMB_GRID.x, style);
					} EditorGUILayout.EndScrollView();
				} EditorUtils.EndVerticalSafe();

				// Spacing
				GUILayout.Space(10);

				// Do it button
				EditorGUILayout.BeginHorizontal(); {
					// We don't want the button to be huge, so add flexible spaces before and after
					GUILayout.FlexibleSpace();

					// Button
					GUI.enabled = m_entityPrefabIdx >= 0;
					if(GUILayout.Button("ADD SPAWNER", GUILayout.Width(200), GUILayout.Height(50))) {
						// Just do it!
						AddNewSpawner();
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
		private void AddNewSpawner() {
			// Check all required parameters
			if(m_targetGroup == null) { ShowNotification(new GUIContent("Target level is not valid")); return; }
			if(m_targetGroup.spawnersObj == null) { ShowNotification(new GUIContent("Target level doesn't have a container for new spawners")); return; }
			if(m_entityPrefabIdx < 0 || m_entityPrefabIdx >= m_entityPrefabs.Length) { ShowNotification(new GUIContent("Please select an entity prefab from the list")); return; }

			// Get target entity prefab
			GameObject entityPrefab = m_entityPrefabs[m_entityPrefabIdx];

			// Create a new object and add it to the scene
			GameObject newSpawnerObj = new GameObject();
			newSpawnerObj.transform.SetParent(m_targetGroup.spawnersObj.transform, true);

			// Add a name based on the entity prefab
			string entityName = entityPrefab.name.Replace("PF_", "");	// Entity name without the preffix (if any)
			newSpawnerObj.SetUniqueName(PREFIX + entityName);	// Add a prefix of our own and generate unique name

			// Add and initialize the transform lock component
			// Arbitrary default values fitted to the most common usage when level editing
			TransformLock newLock = newSpawnerObj.AddComponent<TransformLock>();
			newLock.SetPositionLock(false, false, true);
			newLock.SetRotationLock(true, true, true);
			newLock.SetScaleLock(true, true, true);

			// Add the spawner component - and optionally a suffix
			Spawner sp = null;
			switch(m_type) {
				case Type.STANDARD: {
					sp = newSpawnerObj.AddComponent<Spawner>();
				} break;

				case Type.FLOCK: {
					sp = newSpawnerObj.AddComponent<FlockSpawner>();
					newSpawnerObj.name = newSpawnerObj.name + "Flock";
				} break;
			}

			// Initialize spawner with the target prefab
			sp.m_entityPrefab = entityPrefab;

			// Add the shape component
			float size = 25f;	// Will be used to focus the camera. Start with the default size for a point.
			switch(m_shape) {
				case Shape.POINT: {
					// Nothing to do :)
				} break;

				case Shape.RECTANGLE: {
					RectArea2D area = newSpawnerObj.AddComponent<RectArea2D>();
					size = area.size.magnitude;
				} break;

				case Shape.CIRCLE: {
					CircleArea2D area = newSpawnerObj.AddComponent<CircleArea2D>();
					size = area.radius * 2f;
				} break;
			}

			// Make operation undoable
			Undo.RegisterCreatedObjectUndo(newSpawnerObj, "LevelEditor AddSpawner");

			// Set position more or less to where the camera is pointing, forcing Z-0
			// Select new object in the hierarchy and center camera to it
			LevelEditor.PlaceInFrontOfCameraAtZPlane(newSpawnerObj, true);
			
			// [AOC] Unfortunately FrameLastActiveSceneView fails to figure out the actual bounds of the spawner, so we 
			//       move the camera manually (based on the SceneView source code found in https://github.com/MattRix/UnityDecompiled/blob/cc432a3de42b53920d5d5dae85968ff993f4ec0e/UnityEditor/UnityEditor/SceneView.cs)
			SceneView scene = SceneView.lastActiveSceneView;
			scene.LookAt(newSpawnerObj.transform.position, scene.rotation, size, scene.orthographic, EditorApplication.isPlaying);
			
			// Close window
			Close();
		}
	}
}