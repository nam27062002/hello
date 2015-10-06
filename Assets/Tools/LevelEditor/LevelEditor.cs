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
		get { 
			#if UNITY_EDITOR
			return (DragonId)EditorPrefs.GetInt(testDragonPrefKey, 0);
			#else
			return (DragonId)PlayerPrefs.GetInt(testDragonPrefKey, 0);
			#endif
		}
		set {
			#if UNITY_EDITOR
			EditorPrefs.SetInt(testDragonPrefKey, (int)value);
			#else
			PlayerPrefs.SetInt(testDragonPrefKey, (int)value);
			#endif
		}
	}

	private static string testDragonPrefKey {
		get { return typeof(LevelEditor).Name + ".testDragon"; }
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
			Rect pos = new Rect(5, 5, 200, GUI.skin.label.lineHeight + GUI.skin.label.margin.vertical);

			// Selected object data
			GameObject selectedObj = Selection.activeGameObject;
			if(selectedObj != null) {
				// Name
				GUI.Label(pos, selectedObj.name);

				// ?? TODO
				pos.y += pos.height;
				GUI.Label(pos, "");
			}
		} Handles.EndGUI();
	}
#endif
}

