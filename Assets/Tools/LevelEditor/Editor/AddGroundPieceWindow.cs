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
using System.IO;
using System.Collections.Generic;

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
		private Material[] m_materials = null;

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
			window.minSize = new Vector2(500f, 270f);
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
		/// Pseudo-constructor.
		/// </summary>
		public void OnEnable() {
			// Load all the editor materials
			// Can't be done in the constructor -_-
			string[] materialFiles = Directory.GetFiles(Application.dataPath + "/Tools/LevelEditor/Materials/", "*.mat", SearchOption.AllDirectories);
			materialFiles.SortAlphanumeric();	// [AOC] Yeah, we don't want _0, _1, _10, _11...
			m_materials = new Material[materialFiles.Length];
			for(int i = 0; i < materialFiles.Length; i++) {
				string assetPath = "Assets" + materialFiles[i].Replace(Application.dataPath, "").Replace('\\', '/');
				m_materials[i] = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
			}

			// We want them displayed in columns of 3, so re-sort them
			Material[] tmpList = new Material[m_materials.Length];
			int rows = 3;
			int cols = Mathf.CeilToInt(m_materials.Length/(float)rows);
			int matIdx = 0;
			for(int col = 0; col < cols; col++) {
				for(int row = 0; row < rows; row++) {
					int i = col + row*cols;
					if(matIdx < m_materials.Length) {
						tmpList[i] = m_materials[matIdx];
						matIdx++;
					}
				}
			}
			m_materials = tmpList;
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
			// Store vars into editor preferences to save them between pieces
			// Reset indentation
			int indentLevelBackup = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			
			// Show all options in a list
			EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true)); {
				// Color Input
				// Unfortunately changing the color of a single object sharing material with others is not that simple in editor-time :-/
				// We will have a limited set of colors instead, each with its own material
				GUILayout.Label("Color");
				EditorGUILayout.BeginHorizontal(EditorStyles.helpBox); {
					// Center
					GUILayout.FlexibleSpace();
					
					// Selector
					// Create a custom content for each material, containing the asset preview and the material name
					GUIContent[] contents = new GUIContent[m_materials.Length];
					for(int i = 0; i < contents.Length; i++) {
						contents[i] = new GUIContent(m_materials[i].name, AssetPreview.GetAssetPreview(m_materials[i]));
					}
					
					// Use custom button styles
					GUIStyle style = new GUIStyle();
					style.fixedWidth = 48f;
					style.fixedHeight = style.fixedWidth;
					style.imagePosition = ImagePosition.ImageOnly;
					style.alignment = TextAnchor.MiddleCenter;
					style.padding = new RectOffset(2, 2, 2, 2);
					style.onActive.background = Texture2DExt.Create(2, 2, Colors.red);
					style.onNormal.background = Texture2DExt.Create(2, 2, Colors.red);
					style.onActive.textColor = Colors.red;
					style.onNormal.textColor = Colors.red;
					
					// The selection grid will do the job
					LevelEditor.settings.groundPieceColorIdx = GUILayout.SelectionGrid(LevelEditor.settings.groundPieceColorIdx, contents, Mathf.CeilToInt(m_materials.Length/(float)3), style);	// Group materials in columns of 3 (whiteTint, neutral, blackTint)

					// Center
					GUILayout.FlexibleSpace();
				} EditorUtils.EndHorizontalSafe();

				GUILayout.Space(5f);

				// Size input
				//LevelEditor.settings.groundPieceSize = EditorUtils.Vector3Field("Size", LevelEditor.settings.groundPieceSize);
				LevelEditor.settings.groundPieceSize = EditorGUILayout.Vector3Field("Size", LevelEditor.settings.groundPieceSize);

				GUILayout.Space(5f);
				
				// Confirm button
				if(GUILayout.Button("Add", GUILayout.Height(40))) {
					for(int i = 0; i < m_materials.Length; i++) {
					// Do it!!
					// We could have a prefab, specially if we need some custom scripts attached to it, but for now a simple cube is just fine
					// Create game object
					GameObject groundPieceObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

					// Apply color
					Renderer pieceRenderer = groundPieceObj.GetComponent<Renderer>();
					//pieceRenderer.sharedMaterial = m_materials[LevelEditor.settings.groundPieceColorIdx];
						pieceRenderer.sharedMaterial = m_materials[i];

					// Apply size: luckily scale is 1:1m
					groundPieceObj.transform.localScale = LevelEditor.settings.groundPieceSize;
					
					// Put it into the ground layer
					groundPieceObj.SetLayerRecursively("Ground");
					
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
					GroundPiece groundComp = groundPieceObj.AddComponent<GroundPiece>();

					// Make operation undoable
					Undo.RegisterCreatedObjectUndo(groundPieceObj, "LevelEditor AddGroundPiece");

					// Set position more or less to where the camera is pointing, forcing Z-0
					// Select new object in the hierarchy and center camera to it
					LevelEditor.PlaceInFrontOfCameraAtZPlane(groundPieceObj, true);

						int col = i%10;
						int row = i/10;
						Vector2 pos = new Vector2(
							LevelEditor.settings.groundPieceSize.x * col,
							LevelEditor.settings.groundPieceSize.y * 1.25f * row
							);
						groundPieceObj.transform.SetPosX(pos.x);
						groundPieceObj.transform.SetPosY(pos.y);
					}

					// Close window
					Close();
				}
			} EditorGUILayout.EndVertical();
			
			// Restore indentation
			EditorGUI.indentLevel = indentLevelBackup;
		}
	}
}