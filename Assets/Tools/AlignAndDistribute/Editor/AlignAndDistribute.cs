// AlignAndDistribute.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 31/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple editor window to align and distribute objects.
/// TODO!!
/// - Custom axis (based on selected objects)
/// </summary>
public class AlignAndDistributeEditorWindow : EditorWindow {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private enum Axis {
		X = 0,
		Y,
		Z
	}

	private enum Mode {
		MIN,
		MID,
		MAX
	}

	private static readonly float BUTTON_SIZE = 35f;

	//------------------------------------------------------------------------//
	// STATICS																  //
	//------------------------------------------------------------------------//
	// Button icons
	private static Texture[][] s_alignButtonTextures = null;
	private static Texture[] s_distributeButtonTextures = null;

	// Windows instance
	private static AlignAndDistributeEditorWindow m_instance = null;
	public static AlignAndDistributeEditorWindow instance {
		get {
			if(m_instance == null) {
				m_instance = (AlignAndDistributeEditorWindow)EditorWindow.GetWindow(typeof(AlignAndDistributeEditorWindow), true, "Align And Distribute", true);
			}
			return m_instance;
		}
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Shows the window.
	/// </summary>
	[MenuItem("Tools/Align And Distribute", false, 1)]
	public static void ShowWindow() {
		// Setup window
		instance.titleContent = new GUIContent("Align And Distribute");
		instance.minSize = new Vector2(140, 205);	// Arbitrary
		instance.maxSize = instance.minSize;	// Fixed size

		// Show it
		instance.ShowUtility();
	}

	/// <summary>
	/// Creates custom GUI styles if not already done.
	/// Must only be called from the OnGUI() method.
	/// </summary>
	private void InitStyles() {
		// Align buttons
		if(s_alignButtonTextures == null) {
			// Create array
			s_alignButtonTextures = new Texture[3][];	// X-Y-Z, MIN-MID-MAX

			// Icon names
			string[] axisNames = new string[] {"x", "y", "z"};
			string[] modeNames = new string[] {"min", "mid", "max"};

			// Init array
			for(int i = 0; i < 3; i++) {
				s_alignButtonTextures[i] = new Texture[3];
				for(int j = 0; j < 3; j++) {
					string file = "Assets/Tools/AlignAndDistribute/Editor/Assets/" + "align_" + axisNames[i] + "_" + modeNames[j] + ".png";
					s_alignButtonTextures[i][j] = AssetDatabase.LoadAssetAtPath<Texture>(file);
				}
			}
		}

		// Distribute buttons
		if(s_distributeButtonTextures == null) {
			// Create array
			s_distributeButtonTextures = new Texture[3];	// X-Y-Z

			// Icon names
			string[] axisNames = new string[] {"x", "y", "z"};

			// Init array
			for(int i = 0; i < 3; i++) {
				string file = "Assets/Tools/AlignAndDistribute/Editor/Assets/" + "distribute_" + axisNames[i] + ".png";
				s_distributeButtonTextures[i] = AssetDatabase.LoadAssetAtPath<Texture>(file);
			}
		}
	}

	/// <summary>
	/// Update the inspector window.
	/// </summary>
	public void OnGUI() {
		// Make sure styles are initialized - must be done in the OnGUI call
		InitStyles();

		// Initial space
		EditorGUILayout.Space();

		// Align section
		//EditorGUILayout.LabelField("Align");
		EditorGUILayoutExt.Separator(new SeparatorAttribute("Align", 2));
		for(int i = 0; i < 3; i++) {
			// One horizontal layout per axis
			EditorGUILayout.BeginHorizontal(); {
				// Aux
				Axis axis = (Axis)i;

				// Label - vertically centered
				EditorGUILayout.BeginVertical(GUILayout.Height(BUTTON_SIZE)); {
					GUILayout.FlexibleSpace();
					GUILayout.Label(axis.ToString());
					GUILayout.FlexibleSpace();
				} EditorGUILayout.EndVertical();

				// Buttons
				for(int j = 0; j < 3; j++) {
					// One button per mode
					Mode mode = (Mode)j;
					if(GUILayout.Button(s_alignButtonTextures[i][j], GUILayout.Width(BUTTON_SIZE), GUILayout.Height(BUTTON_SIZE))) {
						Align(axis, mode);
					}
				}
			} EditorGUILayoutExt.EndHorizontalSafe();
		}

		// Distribute section
		EditorGUILayout.Space();
		//EditorGUILayout.LabelField("Distribute");
		EditorGUILayoutExt.Separator(new SeparatorAttribute("Distribute", 2));
		EditorGUILayout.BeginHorizontal(); {
			// Try to align with "align" buttons
			GUILayout.Space(23f);

			// One button per axis
			for(int i = 0; i < 3; i++) {
				// Button
				Axis axis = (Axis)i;
				if(GUILayout.Button(s_distributeButtonTextures[i], GUILayout.Height(BUTTON_SIZE), GUILayout.Width(BUTTON_SIZE))) {
					Distribute(axis);
				}
			}
		} EditorGUILayoutExt.EndHorizontalSafe();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_axis">Axis.</param>
	/// <param name="_mode">Mode.</param>
	private void Align(Axis _axis, Mode _mode) {
		// Get current selection
		Transform[] transforms = Selection.transforms;

		// Nothing to do if there are not enough objects selected
		if(transforms.Length < 2) return;

		// Undo support!
		Undo.RecordObjects(transforms, "Align");

		// Initialize ref pos to the first selected object
		Vector3 minPos = transforms[0].position;
		Vector3 maxPos = minPos;

		// Find out min and max positions in the target axis
		foreach(Transform t in transforms) {
			minPos.x = Mathf.Min(minPos.x, t.position.x);
			minPos.y = Mathf.Min(minPos.y, t.position.y);
			minPos.z = Mathf.Min(minPos.z, t.position.z);

			maxPos.x = Mathf.Max(maxPos.x, t.position.x);
			maxPos.y = Mathf.Max(maxPos.y, t.position.y);
			maxPos.z = Mathf.Max(maxPos.z, t.position.z);
		}

		// Compute the anchor point based on selected mode
		Vector3 anchorPos = Vector3.zero;
		switch(_mode) {
			case Mode.MIN: anchorPos = minPos; break;
			case Mode.MID: anchorPos = Vector3.Lerp(minPos, maxPos, 0.5f); break;
			case Mode.MAX: anchorPos = maxPos; break;
		}

		// Apply new position to selected objects
		foreach(Transform t in transforms) {
			switch(_axis) {
				case Axis.X: t.SetPosX(anchorPos.x);	break;
				case Axis.Y: t.SetPosY(anchorPos.y);	break;
				case Axis.Z: t.SetPosZ(anchorPos.z);	break;
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_axis">Axis.</param>
	private void Distribute(Axis _axis) {
		// Get current selection and sort by axis current position to respect the current order
		Transform[] transforms = Selection.transforms;
		SortByAxis(ref transforms, _axis);

		// Nothing to do if there are not enough objects selected
		if(transforms.Length < 2) return;

		// Undo support!
		Undo.RecordObjects(transforms, "Distribute");

		// Initialize ref pos to the first selected object
		Vector3 minPos = transforms[0].position;
		Vector3 maxPos = minPos;

		// Find out min and max overall positions
		foreach(Transform t in transforms) {
			minPos.x = Mathf.Min(minPos.x, t.position.x);
			minPos.y = Mathf.Min(minPos.y, t.position.y);
			minPos.z = Mathf.Min(minPos.z, t.position.z);

			maxPos.x = Mathf.Max(maxPos.x, t.position.x);
			maxPos.y = Mathf.Max(maxPos.y, t.position.y);
			maxPos.z = Mathf.Max(maxPos.z, t.position.z);
		}

		// Distribute all selected objects evenly among the target axis
		for(int i = 0; i < transforms.Length; i++) {
			float delta = (float)i/(float)(transforms.Length - 1);
			switch(_axis) {
				case Axis.X: transforms[i].SetPosX(Mathf.Lerp(minPos.x, maxPos.x, delta));	break;
				case Axis.Y: transforms[i].SetPosY(Mathf.Lerp(minPos.y, maxPos.y, delta));	break;
				case Axis.Z: transforms[i].SetPosZ(Mathf.Lerp(minPos.z, maxPos.z, delta));	break;
			}
		}
	}

	/// <summary>
	/// Sorts the given set of transforms by the given axis (lowest to highest value).
	/// </summary>
	/// <param name="_transforms">The transforms to be sorted.</param>
	/// <param name="_axis">The axis to sort by.</param>
	private void SortByAxis(ref Transform[] _transforms, Axis _axis) {
		// Use native C# sort algorithm with a custom comparer
		Array.Sort(_transforms,
			delegate(Transform _t1, Transform _t2) {
				// It should return less than 0 when _t1 < _t2, zero when _t1 = _t2 and greater than 0 when _t1 > _t2. 
				// Doing the subtraction will instantly produce the right return value ^_^
				// Luckily axis indexes correspond to x,y,z position values so we don't have to switch
				return (int)(_t1.position[(int)_axis] - _t2.position[(int)_axis]);
			}
		);
	}
}