// LevelEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// Extra behaviour to the level editor scene.
	/// </summary>
	[ExecuteInEditMode]
	public class LevelEditor : MonoBehaviour {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		public static readonly string TAG = "LevelEditor";

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		// Store all settings in a scriptable object so they can be persisted between sessions and edit/play modes
		private static LevelEditorSettings m_settings = null;
		public static LevelEditorSettings settings {
			get {
				// Settings not yet initialized
				if(m_settings == null) {
#if UNITY_EDITOR
					// Load stored settings object
					string path = "Assets/Tools/LevelEditor/LevelEditorSettings.asset";
					m_settings = AssetDatabase.LoadAssetAtPath<LevelEditorSettings>(path);

					// No object stored, create it and save it
					if(m_settings == null) {
						m_settings = ScriptableObject.CreateInstance<LevelEditorSettings>();
						AssetDatabase.CreateAsset(m_settings, path);
					}
#else
					// Shouldn't happen, since level editor is not meant to be run outside the editor
					// Just create a new instance of the scriptable object
					m_settings = ScriptableObject.CreateInstance<LevelEditorSettings>();
#endif
				}
				return m_settings;
			}
		}

		//------------------------------------------------------------------//
		// GENERIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Initialization.
		/// </summary>
		protected void Awake() {

		}

		/// <summary>
		/// First update.
		/// </summary>
		void Start() {
		
		}
		
		/// <summary>
		/// Called every frame.
		/// </summary>
		void Update() {

		}

		/// <summary>
		/// Destructor.
		/// </summary>
		protected void OnDestroy() {

		}

	#if UNITY_EDITOR
		/// <summary>
		/// Draw stuff on the scene.
		/// </summary>
		private void OnDrawGizmos() {
			// Draw axis at scene's origin
			float axisLength = 100000f;
			Handles.matrix = Matrix4x4.identity;

			// X-axis
			Handles.color = Handles.xAxisColor;
			Handles.DrawLine(Vector3.zero, Vector3.right * axisLength);
			Handles.color = Handles.color * 0.5f;
			Handles.DrawLine(Vector3.zero, Vector3.left * axisLength);

			// Y-axis
			Handles.color = Handles.yAxisColor;
			Handles.DrawLine(Vector3.zero, Vector3.up * axisLength);
			Handles.color = Handles.color * 0.5f;
			Handles.DrawLine(Vector3.zero, Vector3.down * axisLength);

			// Z-axis
			Handles.color = Handles.zAxisColor;
			Handles.DrawLine(Vector3.zero, Vector3.forward * axisLength);
			Handles.color = Handles.color * 0.5f;
			Handles.DrawLine(Vector3.zero, Vector3.back * axisLength);

			// Size reference planes
			// X-0
			Handles.color = Colors.white;
			Vector3[] verts = new Vector3[] {
				new Vector3(0, 0, 0),
				new Vector3(0, 0, 1),
				new Vector3(0, 1, 1),
				new Vector3(0, 1, 0)
			};
			Handles.DrawSolidRectangleWithOutline(verts, Colors.WithAlpha(Handles.xAxisColor, 0.25f), Handles.xAxisColor);

			// Y-0
			verts = new Vector3[] {
				new Vector3(0, 0, 0),
				new Vector3(1, 0, 0),
				new Vector3(1, 0, 1),
				new Vector3(0, 0, 1)
			};
			Handles.DrawSolidRectangleWithOutline(verts, Colors.WithAlpha(Handles.yAxisColor, 0.25f), Handles.yAxisColor);

			// Z-0
			verts = new Vector3[] {
				new Vector3(0, 0, 0),
				new Vector3(1, 0, 0),
				new Vector3(1, 1, 0),
				new Vector3(0, 1, 0)
			};
			Handles.DrawSolidRectangleWithOutline(verts, Colors.WithAlpha(Handles.zAxisColor, 0.25f), Handles.zAxisColor);

			// In-scene GUI
			Handles.BeginGUI(); {
				// Aux vars
				GUIStyle style = new GUIStyle(GUI.skin.label);
				style.normal.textColor = Colors.silver;
				Rect pos = new Rect(5, 5, Screen.width, style.lineHeight + style.margin.vertical);

				// Selected object data
				GameObject selectedObj = Selection.activeGameObject;
				if(selectedObj != null) {
					// Name
					GUI.Label(pos, selectedObj.name, style);

					// ?? TODO
					pos.y += pos.height;
					GUI.Label(pos, "", style);
				}
			} Handles.EndGUI();
		}
	#endif

		//------------------------------------------------------------------//
		// STATIC UTILS														//
		//------------------------------------------------------------------//
	#if UNITY_EDITOR
		/// <summary>
		/// Places the given game object at in front of the current scene camera, 
		/// snapping it to Z0 plane.
		/// </summary>
		/// <param name="_obj">The object to be moved.</param>
		/// <param name="_selectAndFocus">Whether to make target object the selected one and focus the scene camera to it.</param>
		public static void PlaceInFrontOfCameraAtZPlane(GameObject _obj, bool _selectAndFocus) {
			// If there is a transform lock, make it ignore the following transform changes
			TransformLock lockComponent = _obj.GetComponent<TransformLock>();
			if(lockComponent != null) {
				lockComponent.ignoreLock = true;	// This will ignore changes for a single frame
			}

			// Put the object where the camera is looking, at z-0 plane
			Camera sceneCamera = SceneView.lastActiveSceneView.camera;
			Ray cameraRay = sceneCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));	// Z is ignored
			Plane zPlane = new Plane(Vector3.forward, Vector3.zero);
			float dist = 0;
			if(zPlane.Raycast(cameraRay, out dist)) {
				// Looking at z-0
				_obj.transform.position = cameraRay.GetPoint(dist);
			} else {
				// Not looking at z-0, put object at an arbitrary distance from the camera and force z-0
				_obj.transform.position = cameraRay.GetPoint(100f);
				_obj.transform.SetPosZ(0f);
			}

			// If required, select the object in the hierarchy and move camera towards it
			if(_selectAndFocus) {
				Selection.activeGameObject = _obj;
				EditorGUIUtility.PingObject(_obj);
				SceneView.FrameLastActiveSceneView();
			}
		}
	#endif
	}
}

