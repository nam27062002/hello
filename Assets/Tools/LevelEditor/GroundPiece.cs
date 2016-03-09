// GroundPiece.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// Script defining a ground piece in the level.
	/// </summary>
	[ExecuteInEditMode]
	public class GroundPiece : MonoBehaviour {
		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		private Mesh m_mesh = null;

		//------------------------------------------------------------------//
		// GENERIC METHODS													//
		//------------------------------------------------------------------//

		/// <summary>
		/// The object has been selected.
		/// </summary>
		void OnEnable() {
			m_mesh = GetComponent<MeshFilter>().sharedMesh;
		}

		/// <summary>
		/// The object has been unselected.
		/// </summary>
		void OnDisable() {
			m_mesh = null;
		}


	#if UNITY_EDITOR
		/// <summary>
		/// Draw stuff on the scene.
		/// </summary>
		private void OnDrawGizmos() {
			if(m_mesh != null) {
				// 0,0,0 is the center
				Vector3[] edges = new Vector3[4] {
					new Vector3(m_mesh.bounds.min.x, 0f, 0f),	// left
					new Vector3(m_mesh.bounds.max.x, 0f, 0f),	// right
					new Vector3(0f, m_mesh.bounds.min.y, 0f),	// bottom
					new Vector3(0f, m_mesh.bounds.max.y, 0f)	// top
				};

				// Draw a handler for each edge
				Handles.color = Colors.WithAlpha(Colors.skyBlue, 0.25f);
				for(int i = 0; i < edges.Length; i++) {
					// Transform to world coords
					edges[i] = transform.TransformPoint(edges[i]);

					// Draw the handler
					//Handles.SphereCap(0, left, Quaternion.identity, HandleUtility.GetHandleSize(edges[i]) * 0.25f);
					Handles.SphereCap(0, edges[i], Quaternion.identity, LevelEditor.settings.handlersSize);
				}

			}
				
			Gizmos.color = Colors.WithAlpha(Color.cyan, 0.5f);
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}
	#endif

		//------------------------------------------------------------------//
		// TOOLS															//
		//------------------------------------------------------------------//
		/// <summary>
		/// Generates a prism mesh with an arbitrary amount of sides.
		/// </summary>
		/// <returns>The prism mesh.</returns>
		/// <param name="_sides">Number of sides, minimum 3 for a convex polygon.</param>
		/// <param name="_angle">The aperture angle in degrees, 360 for a full polygon.</param>
		public static Mesh GeneratePrismMesh(int _sides, float _angle) {
			// Convert angle to the [0-360] range
			_angle = Mathf.Abs(_angle);
			while(_angle > 360f) _angle -= 360f;
			if(_angle == 0) _angle = 1f;	// A 0 angle could cause problems

			// Some aux vars
			int sideVerts = _sides + 1;	// Number of external vertices of either one of the front/back faces
			int totalSideVerts = sideVerts * 2;	// Front and back faces
			int totalUniqueVerts = totalSideVerts + 2;	// Front and back faces central vertex
			int coverVerts = 4 * 2;	// 4 verts per cover, duplicated from original ones
			int totalVerts = (totalSideVerts * 2) + 2 + coverVerts;	// Side verts are duplicated since they have 2 normals (front-face and side-face) + front/back central vertices + duplicated verts for cover faces
			int frontCentralVertIdx = totalUniqueVerts - 2;
			int backCentralVertIdx = totalUniqueVerts - 1;
			int coverVertStartIdx = totalVerts - coverVerts;

			int frontTris = _sides;	// Number of triangles of the front/back face
			int totalFrontTris = frontTris * 2;	// Front and back faces
			int sideTris = _sides * 2;	// Number of triangles of the sides faces - 2 per side
			int coverTris = 4;	// Number of triangles of the cover faces - could be 0 if angle is 360f, but ignore the case
			int totalTris = totalFrontTris + sideTris + coverTris;

			// Vertices
			List<Vector3> vertices = new List<Vector3>(totalVerts);
			for(int i = 0; i < sideVerts; i++) {
				// Compute position in the X-Y plane
				float angle = ((float)i/(float)_sides * _angle);
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

			// Duplicate side vertices since they have 2 normals (front-face and side-face)
			for(int i = 0; i < totalSideVerts; i++) {
				vertices.Add(vertices[i]);
			}

			// In addition, vertex on the cover sides have a 3rd normal for the cover faces
			// First cover
			vertices.Add(vertices[0]);
			vertices.Add(vertices[1]);
			vertices.Add(vertices[frontCentralVertIdx]);
			vertices.Add(vertices[backCentralVertIdx]);

			// Second cover - order relevant!
			vertices.Add(vertices[backCentralVertIdx]);
			vertices.Add(vertices[totalSideVerts - 1]);
			vertices.Add(vertices[frontCentralVertIdx]);
			vertices.Add(vertices[totalSideVerts - 2]);

			// Triangles
			List<int> triangles = new List<int>(totalTris);
			for(int i = 0; i < totalSideVerts - 2; i++) {	// [AOC] Skip last pair of vertices since they will be linked to the cover sides
				// Do all triangles linked to that vertex
				// even vertices -> front face, odd vertices -> back face
				if(i % 2 == 0) {
					// 1) Triangle to the central point
					triangles.Add(i);
					triangles.Add(frontCentralVertIdx);
					triangles.Add((i + 2));

					// 2) Lateral face - use the quivalent duplicated vertex with normal pointing to the side face
					triangles.Add(i + totalUniqueVerts);
					triangles.Add((i + 2) + totalUniqueVerts);
					triangles.Add((i + 1) + totalUniqueVerts);
				} else {
					// 1) Triangle to the central point
					triangles.Add(i);
					triangles.Add((i + 2));
					triangles.Add(backCentralVertIdx);

					// 2) Lateral face - use the quivalent duplicated vertex with normal pointing to the side face
					triangles.Add(i + totalUniqueVerts);
					triangles.Add((i + 1) + totalUniqueVerts);
					triangles.Add((i + 2) + totalUniqueVerts);
				}
			}

			// Normals
			List<Vector3> normals = new List<Vector3>(new Vector3[vertices.Count]);	// Each edge vertex has 2 normals: one facing either top or back faces and another one in the direction from the center of the front/back face. Add the 2 central vertices as well which have one normal each.
			for(int i = 0; i < totalSideVerts; i++) {	// Skip the central vertices
				// 1) Face normal
				normals[i] = new Vector3(0f, 0f, vertices[i].z).normalized;

				// 2) Side normal
				normals[i + totalUniqueVerts] = new Vector3(vertices[i].x, vertices[i].y, 0f).normalized;
			}

			// Normals for the face central vertices
			normals[frontCentralVertIdx] = new Vector3(0f, 0f, vertices[frontCentralVertIdx].z).normalized;
			normals[backCentralVertIdx] = new Vector3(0f, 0f, vertices[backCentralVertIdx].z).normalized;

			// Add triangles and compute normals for the cover faces
			// Do triangles and normals together for simplicity
			for(int i = 0; i < 2; i++) {
				// 4 verts per face
				int j = coverVertStartIdx + 4 * i;

				// Cover triangle 1
				triangles.Add(j);
				triangles.Add(j + 1);
				triangles.Add(j + 3);

				// Cover triangle 2
				triangles.Add(j + 2);
				triangles.Add(j);
				triangles.Add(j + 3);

				// Cover normals
				Plane p = new Plane(vertices[j], vertices[j + 1], vertices[j + 2]);
				Vector3 normal = p.normal;
				for(int k = 0; k < 4; k++) {
					normals[j + k] = normal;
				}
			}

			// UVs
			Vector2[] uvs = new Vector2[vertices.Count];
			for(int i = 0; i < vertices.Count; i++) {
				// Same as normals, we have one UV coord for the front/back face and one for the side face
				// In the front face, map coords from [-0.5,0.5] range (vertex pos) to [0,1] (tex relative coord)
				// Check which type of vertex this is
				if(i < totalSideVerts) {
					// Side vertex, face normal
					uvs[i] = new Vector2(vertices[i].x + 0.5f, vertices[i].y + 0.5f);	// Normalize from [-0.5, 0.5] to [0, 1]
				} else if(i == frontCentralVertIdx || i == backCentralVertIdx) {
					// Front/back central vertex
					uvs[i] = new Vector2(0.5f, 0.5f);	// Just the middle of the texture :P
				} else if(i < coverVertStartIdx) {
					// Side vertex, side normal
					uvs[i] = new Vector2(vertices[i].y + 0.5f, vertices[i].z + 0.5f);	// Normalize from [-0.5, 0.5] to [0, 1]
					float sideVertexIdx = Mathf.Floor((i - totalUniqueVerts)/2f);
					float sideDelta = sideVertexIdx/(float)(_sides);
					uvs[i].x = sideDelta * 2;
					if(uvs[i].x > 1f) uvs[i].x -= 1f;
				} else {
					// Cover vertex, good luck!
					// Manually do it for each of the 8 vertices composing the covers
					int coverIdx = i - coverVertStartIdx;
					uvs[i] = new Vector2(0f, vertices[i].z + 0.5f);	// Normalize from [-0.5, 0.5] to [0, 1]
					if(coverIdx == 0 || coverIdx == 1) {
						uvs[i].x = 1f;	// First cover, outer vertices
					} else if(coverIdx == 5 || coverIdx == 7) {
						uvs[i].x = 0f;	// Second cover, outer vertices
					} else {
						uvs[i].x = 0.5f;	// Both covers, central vertices
					}
				}
			}

			// Colors
			Color[] colors = new Color[vertices.Count];
			for(int i = 0; i < vertices.Count; i++) {
				colors[i] = Colors.white;
			}

			// Generate and return mesh
			Mesh mesh = new Mesh();
			mesh.vertices = vertices.ToArray();
			mesh.triangles = triangles.ToArray();
			mesh.normals = normals.ToArray();
			mesh.uv = uvs;
			mesh.colors = colors;
			return mesh;
		}
	}
}

