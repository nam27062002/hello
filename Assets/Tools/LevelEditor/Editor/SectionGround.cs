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
					// Generate a diameter 1 vertical cylinder mesh
					Mesh mesh = new Mesh();
					mesh.name = "circle_mesh";
					List<Vector3> vertices = new List<Vector3>();
					List<int> triangles = new List<int>();

					// Circle planes
					// Vertices
					float numRadius = 21;	// 5 triangles per quarter (as in Unity's default cylinder primitive)
					for(int i = 0; i < numRadius; i++) {
						// Compute position in the X-Y plane
						float angle = (float)i/(float)numRadius * 360f;
						Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
						Vector3 newVertex = q * (Vector3.right * 0.5f);

						// Front face
						newVertex.z = -0.5f;
						vertices.Add(newVertex);

						// Back face
						newVertex.z = 0.5f;
						vertices.Add(newVertex);
					}

					// Central points
					vertices.Add(new Vector3(0, 0, -0.5f));
					vertices.Add(new Vector3(0, 0,  0.5f));

					mesh.vertices = vertices.ToArray();

					// Triangles
					int frontCentralVertex = vertices.Count - 2;
					int backCentralVertex = vertices.Count - 1;
					int numVertices = vertices.Count - 2;	// Skip the central vertices
					for(int i = 0; i < numVertices; i++) {
						// Do all triangles linked to that vertex
						// even vertices -> front face, odd vertices -> back face
						if(i % 2 == 0) {
							// 1) Triangle to the central point
							triangles.Add(i);
							triangles.Add(frontCentralVertex);
							triangles.Add((i + 2) % numVertices);

							// 2) Lateral face
							triangles.Add(i);
							triangles.Add((i + 2) % numVertices);
							triangles.Add((i + 1) % numVertices);
						} else {
							// 1) Triangle to the central point
							triangles.Add(i);
							triangles.Add((i + 2) % numVertices);
							triangles.Add(backCentralVertex);

							// 2) Lateral face
							triangles.Add(i);
							triangles.Add((i + 1) % numVertices);
							triangles.Add((i + 2) % numVertices);
						}
					}
					mesh.triangles = triangles.ToArray();

					Vector3[] normals = new Vector3[vertices.Count];
					for(int i = 0; i < vertices.Count; i++) {
						normals[i] = vertices[i].normalized;
					}
					mesh.normals = normals;

					Vector2[] uvs = new Vector2[mesh.vertices.Length];
					for(int i = 0; i < mesh.vertices.Length; i++) {
						uvs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].y);
					}
					mesh.uv = uvs;

					// Create an empty game object and add to it the required components
					GameObject newGO = new GameObject();
					newGO.AddComponent<MeshRenderer>();

					MeshFilter newMeshFilter = newGO.AddComponent<MeshFilter>();
					newMeshFilter.sharedMesh = mesh;

					MeshCollider collider = newGO.AddComponent<MeshCollider>();
					collider.sharedMesh = mesh;
					collider.convex = true;

					// Done!
					return newGO;
				} break;

				case CollisionShape.TRIANGLE: {
					// Generate a height 1 isosceles triangle
					Mesh mesh = new Mesh();
					mesh.name = "triangle_mesh";

					mesh.vertices = new Vector3[] {
						// Front face
						new Vector3(-0.5f, -0.5f, -0.5f), // bot-left
						new Vector3( 0,     0.5f, -0.5f), // top
						new Vector3( 0.5f, -0.5f, -0.5f), // bot-right

						// Back face
						new Vector3(-0.5f, -0.5f,  0.5f), // bot-left
						new Vector3( 0,     0.5f,  0.5f), // top
						new Vector3( 0.5f, -0.5f,  0.5f)  // bot-right
					};

					mesh.triangles = new int[] {
						0, 1, 2,	// Front face
						5, 4, 3,	// Back face

						0, 3, 1,	// Left panel 1
						1, 3, 4,	// Left panel 2

						1, 4, 2,	// Right panel 1
						2, 4, 5,	// Right panel 2

						0, 5, 3,	// Bottom panel 1
						0, 2, 5 	// Bottom panel 2
					};

					Vector3[] normals = new Vector3[mesh.vertices.Length];
					for(int i = 0; i < mesh.vertices.Length; i++) {
						normals[i] = mesh.vertices[i].normalized;
					}
					mesh.normals = normals;

					Vector2[] uvs = new Vector2[mesh.vertices.Length];
					for(int i = 0; i < mesh.vertices.Length; i++) {
						uvs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].y);
					}
					mesh.uv = uvs;

					// Create an empty game object and add to it the required components
					GameObject newGO = new GameObject();
					newGO.AddComponent<MeshRenderer>();

					MeshFilter newMeshFilter = newGO.AddComponent<MeshFilter>();
					newMeshFilter.sharedMesh = mesh;

					MeshCollider collider = newGO.AddComponent<MeshCollider>();
					collider.sharedMesh = mesh;
					collider.convex = true;

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