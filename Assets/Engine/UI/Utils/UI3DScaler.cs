// UI3DScaler.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/02/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple script meant for 3D objects within a UI canvas.
/// Helper component to attach 3D objects to a rect transform and work as expected.
/// Usually makes sense that the attached rect transform is the parent of the 3D object.
/// </summary>
[ExecuteInEditMode]
public class UI3DScaler : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum FitType {
		FIT,		// The biggest side will fit within the parent, leaving empty spaces on the other side
		ENVELOPE	// The smallest side will fit the parent side, overflowing on the other side
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Tooltip("Reference rect transform which will determine the size and position of this object.")]
	[SerializeField] private RectTransform m_rectTransform = null;

	[SerializeField] private FitType m_fitType = FitType.FIT;
	public FitType fitType {
		get { return m_fitType; }
		set { m_fitType = value; }
	}

	[Tooltip("Activate only if the scaler's size is prompt to change during gameplay. No need for other basic transformations such as position, rotation or scale.")]
	[SerializeField] private bool m_constantUpdateRect = true;

	[Tooltip("Activate only if the 3D object's bounds will change oftenly. Quite expensive, don't toggle it unless really needed.")]
	[SerializeField] private bool m_constantUpdateBounds = false;

	// Debug
	[Space]
	[SerializeField] private bool m_showBounds = false;
	[SerializeField] private Color m_rectBoundsColor = new Color(1f, 0f, 0f, 0.5f);
	[SerializeField] private Color m_targetBoundsColor = new Color(0f, 0.0f, 1f, 0.5f);

	// Internal
	private Vector3 m_scaleFactor = Vector3.one;
	private Bounds m_thisOriginalBounds = default(Bounds);
	private Bounds m_thisScaledBounds = default(Bounds);
	private Vector3 m_offset = Vector3.zero;
	private Bounds m_rectBounds = default(Bounds);
	private Vector3[] m_rectCorners = new Vector3[4];
	private Quaternion m_oldRotation = Quaternion.identity;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		Refresh(true, true);
	}

	/// <summary>
	/// A change has been done in the inspector.
	/// </summary>
	private void OnValidate() {
		
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Update according to setup
		Refresh(m_constantUpdateBounds, m_constantUpdateRect);
	}

	/// <summary>
	/// Debug only.
	/// </summary>
	private void OnDrawGizmosSelected() {
		// Show reference size box
		if(!m_showBounds) return;
		if(m_rectTransform == null) return;

		// Update according to setup
		Refresh(m_constantUpdateBounds, m_constantUpdateRect);
		bool validBounds = (m_thisScaledBounds.size.x > 0f && m_thisScaledBounds.size.y > 0f);

		// Draw target's bounds
		if(validBounds) {
			Gizmos.matrix = Matrix4x4.identity;
			Gizmos.color = m_targetBoundsColor;
			Gizmos.DrawCube(m_thisScaledBounds.center, m_thisScaledBounds.size);
		}

		// Draw rect bounds
		Gizmos.matrix = Matrix4x4.identity;
		Gizmos.color = m_rectBoundsColor;
		Gizmos.DrawCube(m_rectBounds.center, m_rectBounds.size);

		if(validBounds) {
			Gizmos.color = Color.magenta;
			Gizmos.DrawLine(m_thisScaledBounds.center, m_rectBounds.center);

			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(m_thisScaledBounds.center, m_thisScaledBounds.center + m_offset);
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh object's scale to match current canvas and rectTransform.
	/// Call it for example if you know that object's bounds could've changed.
	/// </summary>
	/// <param name="_updateBounds">Whether to update the object's bounds. Be careful, it could be expensive!</param>
	/// <param name="_updateRect">Whether to update the reference rect transform.</param>
	public void Refresh(bool _updateBounds, bool _updateRect = true) {
		// Make sure we have all the required stuff
		if(m_rectTransform == null) return;

		// Reset transformation
		transform.localScale = Vector3.one;
		transform.localPosition = Vector3.zero;

		// Ignore rotation to compute bounds, otherwise the projection will depend on the rotation.
		m_oldRotation = transform.localRotation;
		transform.localRotation = Quaternion.identity;

		// Find out world space bounds for this object with the position and scale reset
		if(_updateBounds) {
			m_thisOriginalBounds = this.gameObject.ComputeRendererBounds(true);	// World
			m_offset = this.transform.position - m_thisOriginalBounds.center;
		}
		m_thisScaledBounds = m_thisOriginalBounds;

		// Find out world bounds of the reference rect transform
		if(_updateRect) {
			// Make sure array is valid
			if(m_rectCorners == null || m_rectCorners.Length != 4) {
				m_rectCorners = new Vector3[4];
			}

			// Get world corners and compute bounds
			m_rectTransform.GetWorldCorners(m_rectCorners);
			Vector3 min = m_rectCorners[0];
			Vector3 max = m_rectCorners[0];
			for(int i = 1; i < m_rectCorners.Length; i++) {
				min.x = Mathf.Min(min.x, m_rectCorners[i].x);
				min.y = Mathf.Min(min.y, m_rectCorners[i].y);
				min.z = Mathf.Min(min.z, m_rectCorners[i].z);

				max.x = Mathf.Max(max.x, m_rectCorners[i].x);
				max.y = Mathf.Max(max.y, m_rectCorners[i].y);
				max.z = Mathf.Max(max.z, m_rectCorners[i].z);
			}
			m_rectBounds.SetMinMax(min, max);
		}

		// Don't do anything if bounds are 0
		if(m_thisScaledBounds.size.x == 0f || m_thisScaledBounds.size.y == 0f) {
			this.transform.localRotation = m_oldRotation;
			return;	// Would cause a div/0 error
		}

		// Compute new scale factor!
		m_scaleFactor.x = m_rectBounds.size.x / m_thisScaledBounds.size.x;
		m_scaleFactor.y = m_rectBounds.size.y / m_thisScaledBounds.size.y;

		// Fitting
		float referenceAspectRatio = Mathf.Clamp(m_thisScaledBounds.size.x / m_thisScaledBounds.size.y, 0.001f, 1000f);
		switch(m_fitType) {
			case FitType.FIT: {
				if(m_rectBounds.size.y * referenceAspectRatio > m_rectBounds.size.x) {
					m_scaleFactor.y = m_scaleFactor.x;
					m_scaleFactor.z = m_scaleFactor.x;
				} else {
					m_scaleFactor.x = m_scaleFactor.y;
					m_scaleFactor.z = m_scaleFactor.y;
				}
			} break;

			case FitType.ENVELOPE: {
				if(m_rectBounds.size.y * referenceAspectRatio > m_rectBounds.size.x) {
					m_scaleFactor.x = m_scaleFactor.y;
					m_scaleFactor.z = m_scaleFactor.y;
				} else {
					m_scaleFactor.y = m_scaleFactor.x;
					m_scaleFactor.z = m_scaleFactor.x;
				}
			} break;
		}

		// Restore rotation
		this.transform.localRotation = m_oldRotation;

		// Apply new scale factor! (and update bounds)
		m_thisScaledBounds.size = Vector2.Scale(m_thisOriginalBounds.size, m_scaleFactor);
		this.transform.localScale = m_scaleFactor;

		// Fit target into rect transform!
		Vector3 newWorldPos = m_rectBounds.center + Vector3.Scale(m_offset, m_scaleFactor);
		newWorldPos.z = m_rectTransform.position.z;
		this.transform.position = newWorldPos;
	}

	/// <summary>
	/// Refresh object's scale to match current canvas and rectTransform.
	/// Call it for example if you know that object's bounds could've changed.
	/// Paramterless version to be connected via inspector.
	/// </summary>
	public void Refresh() {
		// Use default parameters
		Refresh(true, true);
	}

    /// <summary>
    /// Set if the bounds need to be updated in every frame.
    /// Sometimes we need to enable it during an animation and disable it 
    /// when its finished.
    /// </summary>
    public void SetConstantUpdateBounds (bool enable)
    {
        m_constantUpdateBounds = enable;
    }


    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
}