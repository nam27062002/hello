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

		//------------------------------------------------------------------//
		// PROPERTIES														//
		//------------------------------------------------------------------//
		private GroundPiece targetPiece { get { return target as GroundPiece; }}

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		private Vector3 m_leftSide = Vector3.zero;
		private Vector3 m_rightSide = Vector3.zero;
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
				m_leftSide = new Vector3(m_mesh.bounds.min.x, 0f, 0f);		// 0,0,0 is the center
				m_rightSide = new Vector3(m_mesh.bounds.max.x, 0f, 0f);		// 0,0,0 is the center
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
			// We're only interested in modifying left and right edges for now
			if(m_mesh == null) return;

			// Store current values
			Vector3 oldLeftPos = targetPiece.transform.TransformPoint(m_leftSide);
			Vector3 oldRightPos = targetPiece.transform.TransformPoint(m_rightSide);
			Vector3 oldRotation = targetPiece.transform.eulerAngles;

			// Draw handlers and store new values
			Handles.color = Colors.coral;
			Vector3 newLeftPos = Handles.FreeMoveHandle(oldLeftPos, Quaternion.identity, HandleUtility.GetHandleSize(oldLeftPos) * 0.25f, Vector3.zero, Handles.SphereCap);

			Handles.color = Colors.coral;
			Vector3 newRightPos = Handles.FreeMoveHandle(oldRightPos, Quaternion.identity, HandleUtility.GetHandleSize(oldRightPos) * 0.25f, Vector3.zero, Handles.SphereCap);

			// Restrict to XY plane
			newLeftPos.z = oldLeftPos.z;
			newRightPos.z = oldRightPos.z;

			// Detect changes
			bool leftChanged = (oldLeftPos != newLeftPos);
			bool rightChanged = (oldRightPos != newRightPos);

			// Snap to round values - only if value has changed
			// Skip Z as well
			for(int i = 0; i < 2; i++) {
				if(leftChanged) newLeftPos[i] = MathUtils.Snap(newLeftPos[i], LevelEditor.snapSize);
				if(rightChanged) newRightPos[i] = MathUtils.Snap(newRightPos[i], LevelEditor.snapSize);
			}

			// Compute new transformations
			Vector3 oldAxis = (oldRightPos - oldLeftPos);
			Vector3 newAxis = (newRightPos - newLeftPos);

			float newScaleX = newAxis.magnitude;
			Vector3 newPos = newLeftPos + newAxis/2f;
			float newRotationZ = Vector3.right.Angle360(newAxis, Vector3.forward);

			// Apply transformations (only if there were actually changes)
			if(leftChanged || rightChanged) {
				Undo.RecordObject(targetPiece.transform, "GroundPiece Editing");
				targetPiece.transform.position = newPos;
				targetPiece.transform.localScale = new Vector3(newScaleX, targetPiece.transform.localScale.y, targetPiece.transform.localScale.z);
				targetPiece.transform.eulerAngles = new Vector3(oldRotation.x, oldRotation.y, newRotationZ);
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