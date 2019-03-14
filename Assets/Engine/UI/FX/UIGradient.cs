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

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Gradient4 m_gradient = new Gradient4(Color.red, Color.red, Color.white, Color.white);
	public Gradient4 gradient {
		get { return m_gradient; }
		set { 
			m_gradient = value;
			Refresh();
		}
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
	/// Modifies gradient values with the ones from the given gradient.
	/// </summary>
	/// <param name="_gradient">Reference gradient.</param>
	public void SetValues(Gradient4 _gradient) {
		if(_gradient == null) return;
		gradient.Set(_gradient);
		Refresh();
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

		// Compute delta each vertex and use it to figure out its final color
		Vector2 delta = GameConstants.Vector2.zero;
		for(int i = 0; i < m_vertexList.Count; ++i) {
			// Create a duplicate of the vertex
			UIVertex v = m_vertexList[i];	// Since it's a struct, it will create a copy

			// Compute delta of this vertex relative to min/max coords
			delta.x = Mathf.InverseLerp(limits.xMin, limits.xMax, v.position.x);
			delta.y = Mathf.InverseLerp(limits.yMin, limits.yMax, v.position.y);

			// Compute final color for this vertex
			// Combine it with original color
			v.color = (v.color.ToColor() * m_gradient.Evaluate(delta)).ToColor32();
			m_newVertexList.Add(v);
		}

		// Update helper with the processed vertices
		_vh.Clear();
		_vh.AddUIVertexTriangleStream(m_newVertexList);
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// For some weird Unity thing, the mesh is not properly refreshed when changing 
	/// the gradient value. Forcing an enable/disable seems to do the trick.
	/// </summary>
	private void Refresh() {
		bool wasEnabled = this.enabled;
		this.enabled = false;
		this.enabled = wasEnabled;
	}
}