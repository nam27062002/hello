// UIGradient_OLDEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/04/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the UIGradient_OLD class.
/// </summary>
[CustomEditor(typeof(UIGradient_OLD), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class UIGradient_OLDEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Default inspector
		DrawDefaultInspector();

		// Button to replace it by the new UIGradient
		GUI.color = Color.yellow;
		if(GUILayout.Button("REPLACE WITH NEW UIGradient\n(All selected targets)", GUILayout.Height(40f))) {
			// Copy array to allow destroying within the loop
			Object[] targetsCopy = new Object[targets.Length];
			targets.CopyTo(targetsCopy, 0);

			// Apply to all selected targets
			for(int i = 0; i < targetsCopy.Length; ++i) {
				// Create a new UIGradient
				UIGradient_OLD oldGradient = targetsCopy[i] as UIGradient_OLD;
				UIGradient newGradient = oldGradient.gameObject.GetComponent<UIGradient>();		// Reuse existing one if possible
				if(newGradient == null) {
					newGradient = Undo.AddComponent<UIGradient>(oldGradient.gameObject);
				}

				// Clone values to it
				switch(oldGradient.direction) {
					case UIGradient_OLD.Direction.HORIZONTAL: {
						newGradient.gradient.Set(
							oldGradient.color1,
							oldGradient.color2,
							oldGradient.color1,
							oldGradient.color2
						);
					} break;

					case UIGradient_OLD.Direction.VERTICAL: {
						newGradient.gradient.Set(
							oldGradient.color1,
							oldGradient.color1,
							oldGradient.color2,
							oldGradient.color2
						);
					} break;

					case UIGradient_OLD.Direction.DIAGONAL_1: {
						newGradient.gradient.Set(
							Color.Lerp(oldGradient.color1, oldGradient.color2, 0.5f),
							oldGradient.color2,
							oldGradient.color1,
							Color.Lerp(oldGradient.color1, oldGradient.color2, 0.5f)
						);
					} break;

					case UIGradient_OLD.Direction.DIAGONAL_2: {
						newGradient.gradient.Set(
							oldGradient.color1,
							Color.Lerp(oldGradient.color1, oldGradient.color2, 0.5f),
							Color.Lerp(oldGradient.color1, oldGradient.color2, 0.5f),
							oldGradient.color2
						);
					} break;
				}

				// Destroy old component
				Undo.DestroyObjectImmediate(targetsCopy[i]);
				targetsCopy[i] = null;
			}

			// Feedback
			Debug.Log(Color.green.Tag("DONE! ") + targetsCopy.Length + " successfully replaced");
		}
		GUI.color = Color.white;
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}
}