// TextFX.cs
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
using UnityEngine.Serialization;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Set of text effects based on Unity's default outline effect.
/// Can be actually applied to any 2D graphic.
/// </summary>
public class TextFX : BaseMeshEffect {
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
	private bool highQuality { get { return m_outlineQuality == Quality.X8; }}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed
	public bool m_bold = false;
	[Range(0.01f, 1f)] public float m_boldSize = 0.5f;

	public bool m_outline = false;
	[InfoBox("Usually a X4 quality is enough, use X8 for huge outlines.\nTake in account that performance cost is doubled as well!")]
	[FormerlySerializedAs("m_quality")] public Quality m_outlineQuality = Quality.X4;
	[Range(0.01f, 10f)] [FormerlySerializedAs("m_size")]    public float m_outlineSize = 3f;
	[FormerlySerializedAs("m_color")]   public Color m_outlineColor = Colors.black;

	public bool m_shadow = false;
	[Range(0f, 360f)] public float m_shadowAngle = 315f;
	[Range(0.01f, 30f)] public float m_shadowSize = 5f;
	public Color m_shadowColor = Colors.black;

	// Internal
	private List<UIVertex> m_vertexList;
	List<UIVertex> m_shadowVertices;
	List<UIVertex>[] m_boldVertices = new List<UIVertex>[OFFSETS.Length];
	List<UIVertex>[] m_outlineVertices = new List<UIVertex>[OFFSETS.Length];

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	protected TextFX() { 
		
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
		// Create lists if not done, otherwise ensure capacity is enough
		if(m_shadowVertices == null) {
			m_shadowVertices = new List<UIVertex>(m_vertexList.Capacity);
		} else {
			m_shadowVertices.Clear();
			m_shadowVertices.Capacity = m_vertexList.Capacity;
		}

		for(int i = 0; i < OFFSETS.Length; i++) {
			if(m_outlineVertices[i] == null) {
				m_outlineVertices[i] = new List<UIVertex>(m_vertexList.Capacity);
			} else {
				m_outlineVertices[i].Clear();
				m_outlineVertices[i].Capacity = m_vertexList.Capacity;
			}

			if(m_boldVertices[i] == null) {
				m_boldVertices[i] = new List<UIVertex>(m_vertexList.Capacity);
			} else {
				m_boldVertices[i].Clear();
				m_boldVertices[i].Capacity = m_vertexList.Capacity;
			}
		}

		// Precompute shadow angle
		Vector2 shadowDir2D = Vector2.right.RotateDegrees(m_shadowAngle);
		Vector3 shadowDir = new Vector3(shadowDir2D.x, shadowDir2D.y, 0f);

		// Start treating each vertex
		Color sourceColor = Color.white;
		Color outlineColor = m_outlineColor;
		Color shadowColor = m_shadowColor;
		float accumulatedSize = 0f;
		for(int i = 0; i < m_vertexList.Count; i++) {
			// Compute colors for this vertex
			sourceColor = m_vertexList[i].color.ToColor();
			outlineColor.a = m_outlineColor.a * Mathf.Pow(sourceColor.a, 4);	// Exponientally decay to correct overlapping of the several layers
			shadowColor.a = m_shadowColor.a * outlineColor.a;					// Re-use alpha smoothing of the outline
			accumulatedSize = 0f;

			// BOLD
			// Bold is like doing an outline of the same color as the text
			if(m_bold) {
				// Increase size
				accumulatedSize += m_boldSize;

				// Duplicate each vertex for each of the outline offsets
				for(int j = 0; j < OFFSETS.Length; j++) {
					// Bold always in low quality, skip some offsets
					if(j % 2 == 1) continue;

					// Create a copy of the vertex (structs, assign makes a copy)
					UIVertex v = m_vertexList[i];
					v.position += OFFSETS[j] * accumulatedSize;	// Apply offset
					v.color = sourceColor.ToColor32();			// Apply color
					m_boldVertices[j].Add(v);					// Put new vertex into its corresponding list
				}
			}

			// OUTLINE
			if(m_outline) {
				// Increase size
				accumulatedSize += m_outlineSize;

				// Duplicate each vertex for each of the outline offsets
				for(int j = 0; j < OFFSETS.Length; j++) {
					// Skip some offsets if doing low quality
					if(!highQuality && (j % 2 == 1)) continue;

					// Create a copy of the vertex (structs, assign makes a copy)
					UIVertex v = m_vertexList[i];
					v.position += OFFSETS[j] * accumulatedSize;	// Apply offset
					v.color = outlineColor.ToColor32();			// Apply color
					m_outlineVertices[j].Add(v);				// Put new vertex into its corresponding list
				}
			}

			// SHADOW
			// If shadow is enabled, compute it now, otherwise skip it
			if(m_shadow && m_shadowSize != 0f) {
				// Increase size
				accumulatedSize += m_shadowSize;

				// Create a copy of the vertex (structs, assign makes a copy)
				UIVertex v = m_vertexList[i];
				v.position += shadowDir * accumulatedSize;	// Apply offset
				v.color = shadowColor.ToColor32();			// Apply color
				m_shadowVertices.Add(v);					// Put new vertex into its corresponding list
			}
		}

		// Join all lists into a single one - order is relevant!
		int capacity = m_vertexList.Capacity;	// Source mesh
		if(m_bold) capacity += m_vertexList.Capacity * 4;	// Bold - always low quality
		if(m_outline) capacity += m_vertexList.Capacity * (highQuality ? 8 : 4);	// Outline
		if(m_shadow) capacity += m_vertexList.Capacity;	// Shadow
		List<UIVertex> newVertexList = new List<UIVertex>(capacity);

		// 1) Shadow
		if(m_shadow) {
			newVertexList.AddRange(m_shadowVertices);
		}

		// 2) Outline
		if(m_outline) {
			for(int i = 0; i < m_outlineVertices.Length; i++) {
				newVertexList.AddRange(m_outlineVertices[i]);
			}
		}

		// 3) Bold
		if(m_bold) {
			for(int i = 0; i < m_boldVertices.Length; i++) {
				newVertexList.AddRange(m_boldVertices[i]);
			}
		}

		// 4) Text
		newVertexList.AddRange(m_vertexList);

		// Update helper with the processed vertices
		_vh.Clear();
		_vh.AddUIVertexTriangleStream(newVertexList);
	}
}