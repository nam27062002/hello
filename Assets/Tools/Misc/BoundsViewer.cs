// BoundsViewer.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/08/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Aux script to display the bounding box of an object, considering all the 
/// Renderer components within it.
/// </summary>
public class BoundsViewer : MonoBehaviour {
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private bool m_showAlways = false;
	[SerializeField] private Color m_color = Colors.pink;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Draw the gizmos.
	/// </summary>
	private void OnDrawGizmos() {
		if(!m_showAlways) return;
		DrawGizmos();
	}

	/// <summary>
	/// Draw the gizmos when selected.
	/// </summary>
	private void OnDrawGizmosSelected() {
		if(m_showAlways) return;
		DrawGizmos();
	}

	/// <summary>
	/// Draw the gizmos!
	/// </summary>
	private void DrawGizmos() {
		// Find all renderers within the object
		Renderer[] renderers = this.GetComponentsInChildren<Renderer>();
		if(renderers.Length == 0) return;	// If the object doesn't contain any mesh filter, no need to proceed

		// Compute accumulated meshes
		Bounds b = renderers[0].bounds;
		for(int i = 1; i < renderers.Length; i++) {
			b.Encapsulate(renderers[i].bounds);
		}

		// Draw bounds!
		Gizmos.color = m_color;
		Gizmos.DrawWireCube(b.center, b.size);
	}
}