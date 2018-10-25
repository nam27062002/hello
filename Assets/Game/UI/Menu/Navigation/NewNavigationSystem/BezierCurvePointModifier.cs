// BezierCurvePointModifier.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple script to link curve points to a specific transform.
/// </summary>
[ExecuteInEditMode]
public class BezierCurvePointModifier : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum UpdateMode {
		ALWAYS,
		ONLY_EDIT_MODE,
		MANUAL
	}

	private enum ActionSource {
		ENABLE,
		UPDATE,
		OTHER
	}

	[System.Serializable]
	public class PointData {
		public BezierCurve curve = null;
		public string pointId = "";
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[Comment("This component will modify the selected curve points whenever the " +
	         "position of this object is changed. Use it to automatically update " +
	         "the camera paths when moving decorations and camera snap points around.")]

	[Tooltip("Be aware of performance!!")]
	[SerializeField] private UpdateMode m_updateMode = UpdateMode.ALWAYS;

	[SerializeField] private List<PointData> m_linkedPoints = new List<PointData>();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		AutoApply(ActionSource.ENABLE);
	}

	/// <summary>
	/// Called every frame during play mode, only called when something in the scene changed during edit mode.
	/// </summary>
	private void Update() {
		AutoApply(ActionSource.UPDATE);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Apply current transform to target points.
	/// </summary>
	public void Apply() {
		BezierPoint p = null;
		for(int i = 0; i < m_linkedPoints.Count; ++i) {
			// Valid point?
			if(m_linkedPoints[i].curve != null && !string.IsNullOrEmpty(m_linkedPoints[i].pointId)) {
				p = m_linkedPoints[i].curve.GetPoint(m_linkedPoints[i].pointId);
				if(p != null) {
					p.globalPosition = this.transform.position;
				}
			}
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether apply can be actually triggered, and do it.
	/// </summary>
	private void AutoApply(ActionSource _source = ActionSource.OTHER) {
		switch(m_updateMode) {
			case UpdateMode.ALWAYS: {
				Apply();
			} break;

			case UpdateMode.ONLY_EDIT_MODE: {
				// Always apply on OnEnable, even if not in edit mode
				if(_source == ActionSource.ENABLE) {
					Apply();
				} else if(!Application.isPlaying) {
					Apply();
				}
			} break;
		}
	}
}