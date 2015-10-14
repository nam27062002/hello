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

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		// We need to store it to prefs because Unity clears all static variables when entering play mode - and we don't want that
		public static DragonId testDragon {
			get { return (DragonId)PrefsExt.Get("LevelEditor.testDragon", 0); }
			set { PrefsExt.Set("LevelEditor.testDragon", (int)value); }
		}

		public static float snapSize {
			get { return PrefsExt.Get("LevelEditor.snapSize", 5f); }
			set { PrefsExt.Set("LevelEditor.snapSize", value); }
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
	}
}

