// AOCOutline.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on //2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

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
/// 
/// </summary>
public class AOCOutline : BaseMeshEffect {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private static readonly int VERTICES_PER_CHARACTER = 6;	// Each letter is formed by 2 triangles -> 6 vertices
	
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed
	public float m_size = 1f;
	public Color m_color = Colors.black;

	// Internal
	private List<UIVertex> m_vertexList;
	private List<UIVertex> m_newVertexList = new List<UIVertex>();	// Twice as the original vertex list
	private List<Vector3> m_centerPoints = new List<Vector3>();		// One per character
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	protected AOCOutline() { 
		
	}

	/// <summary>
	/// Modifies the mesh.
	/// </summary>
	/// <param name="_vh">The UI vertexs for this textfield.</param>
	public override void ModifyMesh(VertexHelper _vh) {
		if(!IsActive())
			return;

		// Get vertices list
		m_vertexList = new List<UIVertex>();
		_vh.GetUIVertexStream(m_vertexList);

		// Do your magic stuff
		// The idea is to duplicate the mesh behind expanding vertices individually for each character, so it goes bolder
		m_centerPoints.Clear();
		for(int i = 0; i < m_vertexList.Count; i++) {
			// Every 6 vertices represents a letter
			// Each letter is formed by 2 triangles -> 6 vertices
			// Vertex pairs 0-5 and 2-3 are the same
			// Find out the center of each character

			// New character - add a center point
			if(i % VERTICES_PER_CHARACTER == 0) {
				m_centerPoints.Add(Vector3.zero);
			}

			// Add the weighted position of this vertex to compute the center. Same as computing the average between all the vertices on that character.
			m_centerPoints[m_centerPoints.Count - 1] += m_vertexList[i].position/(float)VERTICES_PER_CHARACTER;
		}

		// Expand each vertex from the center of the character they belong to
		m_newVertexList.Clear();
		for(int i = 0; i < m_vertexList.Count; i++) {
			// Create a copy of the vertex (structs, assign makes a copy)
			UIVertex v = m_vertexList[i];

			// Expand it from the center of its corresponding character
			int characterIdx = Mathf.FloorToInt(i/(float)VERTICES_PER_CHARACTER);
			Vector3 expandDir = (v.position - m_centerPoints[characterIdx]);
			expandDir.Normalize();
			v.position += expandDir * m_size;

			// Apply color - respect source alpha
			Color newColor = m_color;
			newColor.a = newColor.a * v.color.ToColor().a;
			v.color = newColor.ToColor32();

			// Add new vertex to the new list
			m_newVertexList.Add(v);
		}

		// Finally, add the original vertices on top of the outline mesh
		m_newVertexList.AddRange(m_vertexList);

		// Update helper with the processed vertices
		_vh.Clear();
		_vh.AddUIVertexTriangleStream(m_newVertexList);
	}
}