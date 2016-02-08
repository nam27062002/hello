// SectionGround.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// 
	/// </summary>
	public class SectionGround : ILevelEditorSection {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		private static readonly string PREFIX = "COLL_";
		
		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		private Material[] m_materials = null;
		private Vector2 m_scrollPos = Vector2.zero;
		
		//------------------------------------------------------------------//
		// INTERFACE IMPLEMENTATION											//
		//------------------------------------------------------------------//
		/// <summary>
		/// Initialize this section.
		/// </summary>
		public void Init() {
			// Load all the editor materials
			// Can't be done in the constructor -_-
			m_materials = EditorUtils.LoadAllAssetsAtPath<Material>("Tools/LevelEditor/Materials/", "mat", true);
			
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
		/// Draw the section.
		/// </summary>
		public void OnGUI() {
			// Store vars into editor preferences to save them between pieces
			// Show all options in a list
			EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true)); {
				// Shape selection
				EditorGUIUtility.labelWidth = 50f;
				LevelEditor.settings.groundPieceShape = (CollisionShape)EditorGUILayout.EnumPopup("Shape:", LevelEditor.settings.groundPieceShape);
				EditorGUIUtility.labelWidth = 0f;

				// Color Input
				// Unfortunately changing the color of a single object sharing material with others is not that simple in editor-time :-/
				// We will have a limited set of colors instead, each with its own material
				GUILayout.Label("Color");
				m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, EditorStyles.helpBox); {
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
				} EditorGUILayoutExt.EndScrollViewSafe();
				
				GUILayout.Space(5f);
				
				// Size input
				LevelEditor.settings.groundPieceSize = EditorGUILayout.Vector3Field("Size", LevelEditor.settings.groundPieceSize);
				
				GUILayout.Space(5f);
				
				// Confirm button
				if(GUILayout.Button("Add", GUILayout.Height(40))) {
					// Do it!!
					OnAddGroundPiece();
				}

				GUILayout.FlexibleSpace();
			} EditorGUILayout.EndVertical();
		}

		//------------------------------------------------------------------//
		// INTERNAL UTILS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Create a new collision game object based on settings shape value.
		/// </summary>
		/// <returns>The new object.</returns>
		private GameObject CreateObject() {
			switch(LevelEditor.settings.groundPieceShape) {
				case CollisionShape.RECTANGLE: {
					// Easy
					return GameObject.CreatePrimitive(PrimitiveType.Cube);
				} break;

				case CollisionShape.CIRCLE: {
					// Create an empty game object and add to it the required components
					GameObject newGO = new GameObject();
					newGO.AddComponent<MeshRenderer>();
					newGO.AddComponent<MeshFilter>();
					newGO.AddComponent<MeshCollider>();

					ProceduralMeshGenerator meshGenerator = newGO.AddComponent<ProceduralMeshGenerator>();
					meshGenerator.GenerateMesh(21, 360f);	// 5 triangles per quarter (as in Unity's default cylinder primitive)

					// Done!
					return newGO;
				} break;

				case CollisionShape.TRIANGLE: {
					// Create an empty game object and add to it the required components
					GameObject newGO = new GameObject();
					newGO.AddComponent<MeshRenderer>();
					newGO.AddComponent<MeshFilter>();
					newGO.AddComponent<MeshCollider>();

					ProceduralMeshGenerator meshGenerator = newGO.AddComponent<ProceduralMeshGenerator>();
					meshGenerator.GenerateMesh(3, 360f);	// Generate a height 1 isosceles triangle

					// Done!
					return newGO;
				} break;
			}
			return null;
		}

		//------------------------------------------------------------------//
		// CALLBACKS														//
		//------------------------------------------------------------------//
		/// <summary>
		/// Add a new ground piece to the current group.
		/// </summary>
		private void OnAddGroundPiece() {
			// First of all check that we have a selected group to add the preview to
			Group targetGroup = LevelEditorWindow.instance.sectionGroups.selectedGroup;
			if(targetGroup == null) {
				LevelEditorWindow.instance.ShowNotification(new GUIContent("A group must be selected first!"));
				return;
			}

			// We could have a prefab, specially if we need some custom scripts attached to it, but for now a simple cube is just fine
			// Create game object
			GameObject groundPieceObj = CreateObject();

			// Apply color
			Renderer pieceRenderer = groundPieceObj.GetComponent<Renderer>();
			pieceRenderer.sharedMaterial = m_materials[LevelEditor.settings.groundPieceColorIdx];
			
			// Apply size: luckily scale is 1:1m
			groundPieceObj.transform.localScale = LevelEditor.settings.groundPieceSize;
			
			// Put it into the ground layer
			groundPieceObj.SetLayerRecursively("Ground");
			
			// Add it to the editor group in the level's hierarchy and generate unique name
			groundPieceObj.transform.SetParent(targetGroup.groundObj.transform, true);
			groundPieceObj.SetUniqueName(PREFIX);	// GR_0, GR_1...
			
			// Add and initialize the transform lock component
			// Arbitrary default values fitted to the most common usage when level editing
			TransformLock newLock = groundPieceObj.AddComponent<TransformLock>();
			newLock.SetPositionLock(false, false, true);
			newLock.SetRotationLock(true, true, false);
			newLock.SetScaleLock(false, false, true);
			
			// Add a Ground Piece component as well to facilitate edition
			// [AOC] Only for rectangles
			if(LevelEditor.settings.groundPieceShape == CollisionShape.RECTANGLE) {
				groundPieceObj.AddComponent<GroundPiece>();
			}
			
			// Make operation undoable
			Undo.RegisterCreatedObjectUndo(groundPieceObj, "LevelEditor AddGroundPiece");
			
			// Set position more or less to where the camera is pointing, forcing Z-0
			// Select new object in the hierarchy and center camera to it
			LevelEditor.PlaceInFrontOfCameraAtZPlane(groundPieceObj, true);
		}
	}
}