// GroundPiece.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

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
		/// First update.
		/// </summary>
		protected void Start() {
			// Must be included if we want to be able to enable/disable the component
		}

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

		/// <summary>
		/// Called every frame.
		/// </summary>
		void Update() {

		}

	#if UNITY_EDITOR
		/// <summary>
		/// Draw stuff on the scene.
		/// </summary>
		private void OnDrawGizmos() {
			if(m_mesh != null) {
				Vector3 left = new Vector3(m_mesh.bounds.min.x, 0f, 0f);		// 0,0,0 is the center
				left = transform.TransformPoint(left);
				Vector3 right = new Vector3(m_mesh.bounds.max.x, 0f, 0f);		// 0,0,0 is the center
				right = transform.TransformPoint(right);
				
				// Draw handlers and store new values
				Handles.color = Colors.WithAlpha(Colors.skyBlue, 0.25f);
				Handles.SphereCap(0, left, Quaternion.identity, HandleUtility.GetHandleSize(left) * 0.25f);
				Handles.SphereCap(0, right, Quaternion.identity, HandleUtility.GetHandleSize(right) * 0.25f);
			}
		}
	#endif
	}
}

