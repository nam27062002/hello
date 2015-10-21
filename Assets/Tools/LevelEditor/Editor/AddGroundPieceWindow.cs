// AddGroundPieceWindow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/10/2015.
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
	/// Auxiliar window to add a ground piece from the editor.
	/// </summary>
	public class AddGroundPieceWindow : EditorWindow {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		private static readonly string PREFIX = "GR_";

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		private Group m_targetGroup = null;

		//------------------------------------------------------------------//
		// STATIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Show the window.
		/// </summary>
		/// <param name="_targetGroup">The group where to add the new ground piece</param>
		public static void Show(Group _targetGroup) {
			// Nothing to do if given level is not valid
			if(_targetGroup == null) return;

			// Create a new window instance
			AddGroundPieceWindow window = new AddGroundPieceWindow();
			
			// Setup window
			window.minSize = new Vector2(300f, 70f);
			window.maxSize = window.minSize;
			window.m_targetGroup = _targetGroup;

			// Open at cursor's position
			// The window expects the position in screen coords
			Rect pos = new Rect();
			pos.x = Event.current.mousePosition.x - window.maxSize.x/2f;
			pos.y = Event.current.mousePosition.y + 7f;	// A little bit lower
			pos.position = EditorGUIUtility.GUIToScreenPoint(pos.position);
			
			// Show it as a dropdown list so window is automatically closed upon losing focus
			// http://docs.unity3d.com/ScriptReference/EditorWindow.ShowAsDropDown.html
			window.ShowAsDropDown(pos, window.maxSize);
		}
		
		//------------------------------------------------------------------//
		// GENERIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Default constructor.
		/// </summary>
		public AddGroundPieceWindow() {
			// Nothing to do
		}
		
		/// <summary>
		/// Called every frame.
		/// </summary>
		private void Update() {

		}
		
		//------------------------------------------------------------------//
		// WINDOW METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Update the inspector window.
		/// </summary>
		public void OnGUI() {
			// Reset indentation
			int indentLevelBackup = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			
			// Show all options in a list
			EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true)); {
				// Store vars into editor preferences to save them between pieces
				// Size input
				//LevelEditor.settings.groundPieceSize = EditorGUILayout.Vector3Field("Size", LevelEditor.settings.groundPieceSize);
				LevelEditor.settings.groundPieceSize = EditorUtils.Vector3Field("Size", LevelEditor.settings.groundPieceSize);

				// Color Input
				EditorGUIUtility.labelWidth = 55f;
				LevelEditor.settings.groundPieceColor = EditorGUILayout.ColorField("Color", LevelEditor.settings.groundPieceColor);
				EditorGUIUtility.labelWidth = 0f;

				GUILayout.Space(5f);
				
				// Confirm button
				if(GUILayout.Button("Add")) {
					// Do it!!
					// We could have a prefab, specially if we need some custom scripts attached to it, but for now a simple cube is just fine
					// Create game object
					GameObject groundPieceObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

					// Apply color
					groundPieceObj.GetComponent<Renderer>().sharedMaterial.color = LevelEditor.settings.groundPieceColor;

					// Apply size: luckily scale is 1:1m
					groundPieceObj.transform.localScale = LevelEditor.settings.groundPieceSize;
					
					// Put it into the ground layer
					groundPieceObj.layer = LayerMask.NameToLayer("Ground");
					
					// Add it to the editor group in the level's hierarchy and generate unique name
					groundPieceObj.transform.SetParent(m_targetGroup.groundObj.transform, true);
					groundPieceObj.SetUniqueName(PREFIX);	// GR_0, GR_1...

					// Add and initialize the transform lock component
					// Arbitrary default values fitted to the most common usage when level editing
					TransformLock newLock = groundPieceObj.AddComponent<TransformLock>();
					newLock.SetPositionLock(false, false, true);
					newLock.SetRotationLock(true, true, false);
					newLock.SetScaleLock(false, false, true);

					// Add a Ground Piece component as well to facilitate edition
					groundPieceObj.AddComponent<GroundPiece>();

					// Make operation undoable
					Undo.RegisterCreatedObjectUndo(groundPieceObj, "LevelEditor AddGroundPiece");

					// Set position more or less to where the camera is pointing, forcing Z-0
					// Select new object in the hierarchy and center camera to it
					LevelEditor.PlaceInFrontOfCameraAtZPlane(groundPieceObj, true);

					// Close window
					Close();
				}
			} EditorGUILayout.EndVertical();
			
			// Restore indentation
			EditorGUI.indentLevel = indentLevelBackup;
		}
	}
}