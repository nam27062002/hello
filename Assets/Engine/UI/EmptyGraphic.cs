// EmptyGraphic.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/02/2016.
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
/// Completely empty graphic, with 0 render cost. Useful to define hit areas
/// on UI buttons.
/// From http://answers.unity3d.com/questions/844524/ugui-how-to-increase-hitzone-click-area-button-rec.html
/// </summary>
public class EmptyGraphic : Graphic {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Function you can override to generate specific vertex generation in your application.
	/// Used by Text, Image, and RawImage for example to generate vertices specific to their use case.
	/// For Unity 5.2 and lower.
	/// </summary>
	/// <param name="_vbo">List to fill with vertices.</param>
	protected override void OnFillVBO(List<UIVertex> _vbo) {
		// Just don't fill the list :D
		_vbo.Clear();
	}

	/// <summary>
	/// Callback function when a UI element needs to generate vertices.
	/// For Unity 5.3 and higher.
	/// </summary>
	/// <param name="_vh">VertexHelper utility.</param>
	protected override void OnPopulateMesh(VertexHelper _vh) {
		_vh.Clear();
	}

	/// <summary>
	/// Draw gizmos on scene.
	/// </summary>
	void OnDrawGizmos() {
		// Color and matrix
		Gizmos.color = color;
		Gizmos.matrix = transform.localToWorldMatrix;

		// Correct pivot (DrawCube's 0,0 is the center whereas graphic's 0,0 is bot-left)
		Vector2 pivotCorrection = new Vector2(rectTransform.pivot.x - 0.5f, rectTransform.pivot.y - 0.5f);	// Pivot from [0..1] to [-0.5..0.5]
		Vector2 cubePos = new Vector2(rectTransform.rect.width * (-pivotCorrection.x), rectTransform.rect.height * (-pivotCorrection.y));
		Gizmos.DrawCube(new Vector3(cubePos.x, cubePos.y, 0f), new Vector3(rectTransform.rect.width, rectTransform.rect.height, 1f));

		// Restore matrix and color
		Gizmos.matrix = Matrix4x4.identity;
		Gizmos.color = Colors.white;
	}
}