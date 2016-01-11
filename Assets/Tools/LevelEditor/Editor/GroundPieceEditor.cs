// GroundPieceEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// Custom editor for the GroundPiece class.
	/// Everything will be done here actually.
	/// </summary>
	[CustomEditor(typeof(GroundPiece))]
	public class GroundPieceEditor : Editor {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		// Auxiliar struct to track changes
		struct Change {
			public Vector3 oldValue;
			public Vector3 newValue;
			public bool changed { get { return oldValue != newValue; }}
		};

		//------------------------------------------------------------------//
		// PROPERTIES														//
		//------------------------------------------------------------------//
		private GroundPiece targetPiece { get { return target as GroundPiece; }}

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		private Vector3[] m_sides = new Vector3[4];
		private Mesh m_mesh = null;
		
		//------------------------------------------------------------------//
		// METHODS															//
		//------------------------------------------------------------------//
		/// <summary>
		/// The editor has been enabled - target object selected.
		/// </summary>
		private void OnEnable() {
			// Initial edges
			m_mesh = targetPiece.GetComponent<MeshFilter>().sharedMesh;
			if(m_mesh != null) {
				// 0,0,0 is the center
				m_sides = new Vector3[4] {
					new Vector3(m_mesh.bounds.min.x, 0f, 0f),	// left
					new Vector3(m_mesh.bounds.max.x, 0f, 0f),	// right
					new Vector3(0f, m_mesh.bounds.min.y, 0f),	// bottom
					new Vector3(0f, m_mesh.bounds.max.y, 0f)	// top
				};
			}
		}		

		/// <summary>
		/// The editor has been disabled - target object unselected.
		/// </summary>
		private void OnDisable() {

		}

		/// <summary>
		/// The scene is being refreshed.
		/// </summary>
		public void OnSceneGUI() {
			// We're using an scaled cube, so we know exactly where the edges of the ground piece are
			if(m_mesh == null) return;

			// Store rotation
			Vector3 oldRotation = targetPiece.transform.eulerAngles;

			// Check changes in all edges
			int changedIdx = -1;
			Change[] changes = new Change[m_sides.Length];
			for(int i = 0; i < m_sides.Length; i++) {
				// Store current value
				changes[i].oldValue = targetPiece.transform.TransformPoint(m_sides[i]);

				// Draw handler and store new value
				// Different color for horizontal and vertical handles
				if(i < 2) {
					Handles.color = Colors.coral;
				} else {
					Handles.color = Colors.orange;
				}
				//changes[i].newValue = Handles.FreeMoveHandle(changes[i].oldValue, Quaternion.identity, HandleUtility.GetHandleSize(changes[i].oldValue) * 0.25f, Vector3.zero, Handles.SphereCap);
				changes[i].newValue = Handles.FreeMoveHandle(changes[i].oldValue, Quaternion.identity, LevelEditor.settings.handlersSize, Vector3.zero, Handles.SphereCap);

				// Restrict to XY plane
				changes[i].newValue.z = changes[i].oldValue.z;

				// Snap to round values, skip Z - only if value has changed
				if(changes[i].newValue != changes[i].oldValue) {
					changedIdx = i;
					changes[i].newValue.x = MathUtils.Snap(changes[i].newValue.x, LevelEditor.settings.snapSize);
					changes[i].newValue.y = MathUtils.Snap(changes[i].newValue.y, LevelEditor.settings.snapSize);
				}
			}

			// Compute and apply new transformations
			// Only if actually there are changes
			if(changedIdx >= 0) {
				// Different for horizontal and vertical axis
				//Vector3 oldAxis;
				Vector3 newAxis;
				Vector3 newScale = targetPiece.transform.localScale;
				Vector3 newPos = targetPiece.transform.position;
				float newRotationZ = oldRotation.z;
				if(changedIdx < 2) {
					// Allow undoing
					Undo.RecordObject(targetPiece.transform, "GroundPiece Editing");

					// Horizontal axis
					//oldAxis = changes[1].oldValue - changes[0].oldValue;
					newAxis = changes[1].newValue - changes[0].newValue;
					
					// Position
					newPos = changes[0].newValue + newAxis/2f;
					targetPiece.transform.position = newPos;

					// Scale
					newScale.x = newAxis.magnitude;
					targetPiece.transform.localScale = newScale;

					// Rotation
					newRotationZ = Vector3.right.Angle360(newAxis, Vector3.forward);
					targetPiece.transform.eulerAngles = new Vector3(oldRotation.x, oldRotation.y, newRotationZ);
				} else {
					// Allow undoing
					Undo.RecordObject(targetPiece.transform, "GroundPiece Editing");

					// Vertical axis
					// [AOC] As requested by design, vertical handlers only affect scale, and only in the dragged edge
					//oldAxis = changes[3].oldValue - changes[2].oldValue;
					newAxis = changes[3].newValue - changes[2].newValue;

					// Scale
					newScale.y = newAxis.magnitude;
					targetPiece.transform.localScale = newScale;

					// We must reposition anyway, but only in the dragging direction
					// To do so, compute new position of the edges after the scaling has been applied, and apply de difference to the piece's position
					Vector3[] verticalEdges = new Vector3[2] {
						targetPiece.transform.TransformPoint(m_sides[2]),
						targetPiece.transform.TransformPoint(m_sides[3])
					};
					Vector3 offset = verticalEdges[changedIdx-2] - changes[changedIdx].oldValue;
					targetPiece.transform.position = newPos + offset;
				}
			}
		}

		/// <summary>
		/// Draw the inspector.
		/// </summary>
		public override void OnInspectorGUI() {
			// Nothing to do actually
			DrawDefaultInspector();
		}
	}
}