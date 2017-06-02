// UIGradient.cs
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
/// Simple mesh modifier to add a gradient effect to a 2D graphic (using vertex coloring).
/// Can be actually applied to any 2D graphic (image, text, etc.).
/// </summary>
public class UIGradient : BaseMeshEffect {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum Direction {
		HORIZONTAL,
		VERTICAL,
		DIAGONAL_1,		// Top-left to bottom-right
		DIAGONAL_2		// Bottom-left to top-right
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Color m_color1 = Colors.white;
	public Color color1 {
		get { return m_color1; }
		set { m_color1 = value; }
	}

	[SerializeField] private Color m_color2 = Colors.red;
	public Color color2 {
		get { return m_color2; }
		set { m_color2 = value; }
	}

	[SerializeField] private Direction m_direction = Direction.VERTICAL;
	public Direction direction {
		get { return m_direction; }
		set { m_direction = value; }
	}

	// Internal
	private List<UIVertex> m_vertexList;
	private List<UIVertex> m_newVertexList;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	protected UIGradient() { 

	}

	/// <summary>
	/// Modifies the mesh.
	/// </summary>
	/// <param name="_vh">The UI vertexs for this textfield.</param>
	public override void ModifyMesh(VertexHelper _vh) {
		// Skip if component is not active
		if(!IsActive()) return;

		// Get vertices list
		if(m_vertexList == null) m_vertexList = new List<UIVertex>();
		_vh.GetUIVertexStream(m_vertexList);

		// Init new vertices list
		if(m_newVertexList == null) {
			m_newVertexList = new List<UIVertex>(m_vertexList.Capacity);
		} else {
			m_newVertexList.Clear();
			m_newVertexList.Capacity = m_vertexList.Capacity;
		}

		// Do a first iteration to figure out min and max coords
		Rect limits = new Rect();
		for(int i = 0; i < m_vertexList.Count; i++) {
				limits.xMin = Mathf.Min(limits.xMin, m_vertexList[i].position.x);
				limits.xMax = Mathf.Max(limits.xMax, m_vertexList[i].position.x);
				limits.yMin = Mathf.Min(limits.yMin, m_vertexList[i].position.y);
				limits.yMax = Mathf.Max(limits.yMax, m_vertexList[i].position.y);
		}

		Color sourceColor;
		Color finalColor;
		float delta = 0f;
		for(int i = 0; i < m_vertexList.Count; i++) {
			// Create a duplicate of the vertex
			UIVertex v = m_vertexList[i];	// Since it's a struct, it will create a copy

			// Compute delta of this vertex considering target direction
			switch(m_direction) {
				case Direction.HORIZONTAL: {
					delta = Mathf.InverseLerp(limits.xMin, limits.xMax, v.position.x);
				} break;

				case Direction.VERTICAL: {
					// Invert delta since it makes more sense for color 1 to be on top (delta 1)
					delta = 1f - Mathf.InverseLerp(limits.yMin, limits.yMax, v.position.y);
				} break;

				case Direction.DIAGONAL_1: {
					// Average between the vertical and the horizontal deltas
					delta = (Mathf.InverseLerp(limits.yMin, limits.yMax, v.position.y) + Mathf.InverseLerp(limits.xMin, limits.xMax, v.position.x));
					delta /= 2f;
				} break;

				case Direction.DIAGONAL_2: {
					// Same as diagonal 1 but inverting the Y delta
					delta = (1f - Mathf.InverseLerp(limits.yMin, limits.yMax, v.position.y) + Mathf.InverseLerp(limits.xMin, limits.xMax, v.position.x));
					delta /= 2f;
				} break;
			}

			// Compute colors for this vertex
			sourceColor = v.color.ToColor();
			finalColor = Color.Lerp(m_color1, m_color2, delta);
			finalColor *= sourceColor;
			v.color = finalColor.ToColor32();

			m_newVertexList.Add(v);
		}

		// Update helper with the processed vertices
		_vh.Clear();
		_vh.AddUIVertexTriangleStream(m_newVertexList);
	}
}