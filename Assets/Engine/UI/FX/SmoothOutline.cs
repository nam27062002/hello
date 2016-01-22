// SmoothOutline.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 22/01/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Smoother version (and easier to setup) of the built-in unity outline.
/// </summary>
public class SmoothOutline : BaseMeshEffect {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private static readonly Vector3[] OFFSETS = {
		new Vector3(-1f, -1f,  0f),	// Bottom-left
		new Vector3(-1f,  0f,  0f),	// Left
		new Vector3(-1f,  1f,  0f),	// Top-left
		new Vector3( 0f,  1f,  0f),	// Top
		new Vector3( 1f,  1f,  0f),	// Top-right
		new Vector3( 1f,  0f,  0f),	// Right
		new Vector3( 1f, -1f,  0f),	// Bottom-right
		new Vector3( 0f, -1f,  0f) 	// Bottom
	};

	public enum Quality {
		X4,
		X8
	};

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private bool highQuality { get { return m_quality == Quality.X8; }}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed
	[InfoBox("Usually a X4 quality is enough, use X8 for huge outlines.\nTake in account that performance cost is doubled as well!")]
	public Quality m_quality = Quality.X4;
	public float m_size = 1f;
	public Color m_color = Colors.black;

	// Internal
	private List<UIVertex> m_vertexList;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	protected SmoothOutline() { 
		
	}

	/// <summary>
	/// Modifies the mesh.
	/// </summary>
	/// <param name="_vh">The UI vertexs for this textfield.</param>
	public override void ModifyMesh(VertexHelper _vh) {
		// Skip if component is not active
		if(!IsActive()) return;

		// Get vertices list
		m_vertexList = new List<UIVertex>();
		_vh.GetUIVertexStream(m_vertexList);

		// Do the magic
		// [AOC] The trick is to reply the mesh giving an offset in 4 directions: top-left, top-right, bottom-left, bottom-right
		// [AOC] To give color to the outline, just set the vertex color of the 4 new meshes
		// [AOC] Respect source color alpha though!
		// Store each mesh in a separate list
		List<UIVertex>[] newVertices = new List<UIVertex>[OFFSETS.Length + 1];
		for(int i = 0; i < newVertices.Length; i++) {
			newVertices[i] = new List<UIVertex>(m_vertexList.Capacity);
		}

		// Start treating each vertex
		for(int i = 0; i < m_vertexList.Count; i++) {
			// Store original vertex to the last mesh (so it's rendered on top)
			newVertices[newVertices.Length - 1].Add(m_vertexList[i]);
			Color sourceColor = m_vertexList[i].color.ToColor();

			// Duplicate each vertex for each of the outline offsets
			for(int j = 0; j < OFFSETS.Length; j++) {
				// Skip some offsets if doing low quality
				if(!highQuality && (j % 2 == 1)) continue;

				// Create a copy of the vertex (structs, assign makes a copy)
				UIVertex v = m_vertexList[i];

				// Apply offset
				v.position += OFFSETS[j] * m_size;

				// Apply color - respect source alpha
				Color newColor = m_color;
				newColor.a = newColor.a * Mathf.Pow(sourceColor.a, 4);	// Exponientally decay to correct overlapping of the several layers
				v.color = newColor.ToColor32();

				// Put new vertex into its corresponding list
				newVertices[j].Add(v);
			}
		}

		// Join all lists into a single one
		List<UIVertex> newVertexList = new List<UIVertex>(m_vertexList.Capacity * OFFSETS.Length + 1);
		for(int i = 0; i < newVertices.Length; i++) {
			newVertexList.AddRange(newVertices[i]);
		}

		// Update helper with the processed vertices
		_vh.Clear();
		_vh.AddUIVertexTriangleStream(newVertexList);
	}
}