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

	private static readonly Vector3 SHADOW_DIR = new Vector3(1f, -1f, 0f);	// Bottom-right

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

	[Space(10)]
	public float m_shadowSize = 0f;
	public Color m_shadowColor = Colors.black;

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
		// [AOC] The trick is to replicate the mesh giving an offset in 4/8 directions
		// [AOC] To give color to the outline, just set the vertex color of the new meshes
		// [AOC] Respect source color alpha though!
		// Store each mesh in a separate list
		List<UIVertex> shadowVertices = new List<UIVertex>(m_vertexList.Capacity);
		List<UIVertex>[] offsetVertices = new List<UIVertex>[OFFSETS.Length];
		for(int i = 0; i < offsetVertices.Length; i++) {
			offsetVertices[i] = new List<UIVertex>(m_vertexList.Capacity);
		}

		// Start treating each vertex
		for(int i = 0; i < m_vertexList.Count; i++) {
			// Compute new color for this vertex
			Color sourceColor = m_vertexList[i].color.ToColor();
			Color newColor = m_color;
			newColor.a = newColor.a * Mathf.Pow(sourceColor.a, 4);	// Exponientally decay to correct overlapping of the several layers

			// Duplicate each vertex for each of the outline offsets
			for(int j = 0; j < OFFSETS.Length; j++) {
				// Skip some offsets if doing low quality
				if(!highQuality && (j % 2 == 1)) continue;

				// Create a copy of the vertex (structs, assign makes a copy)
				UIVertex v = m_vertexList[i];
				v.position += OFFSETS[j] * m_size;	// Apply offset
				v.color = newColor.ToColor32();		// Apply color
				offsetVertices[j].Add(v);			// Put new vertex into its corresponding list
			}

			// If shadow is enabled, compute it now, otherwise skip it
			if(m_shadowSize != 0f) {
				// Create a copy of the vertex (structs, assign makes a copy)
				UIVertex v = m_vertexList[i];
				v.position += SHADOW_DIR * (m_size + m_shadowSize);	// Apply offset - take outline in account!
				v.color = Colors.WithAlpha(m_shadowColor, m_shadowColor.a * newColor.a).ToColor32();		// Apply color - re-use alpha smoothing of the outline
				shadowVertices.Add(v);				// Put new vertex into its corresponding list
			}
		}

		// Join all lists into a single one - order is relevant!
		int capacity = m_vertexList.Capacity;
		capacity += m_vertexList.Capacity * (highQuality ? 8 : 4);
		List<UIVertex> newVertexList = new List<UIVertex>(m_vertexList.Capacity * OFFSETS.Length + 1);

		// 1) Shadow
		newVertexList.AddRange(shadowVertices);

		// 2) Outline offsets
		for(int i = 0; i < offsetVertices.Length; i++) {
			newVertexList.AddRange(offsetVertices[i]);
		}

		// 3) Text
		newVertexList.AddRange(m_vertexList);

		// Update helper with the processed vertices
		_vh.Clear();
		_vh.AddUIVertexTriangleStream(newVertexList);
	}
}