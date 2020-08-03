// WASDController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class WASDController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public float m_moveSpeed = 0.5f;
	public float m_rotationSpeed = 0.5f;

	[Space]
	public KeyCode m_forwardKey = KeyCode.Q;
	public KeyCode m_backwardsKey = KeyCode.E;
	[Space]
	public KeyCode m_leftKey = KeyCode.A;
	public KeyCode m_rightKey = KeyCode.D;
	[Space]
	public KeyCode m_upKey = KeyCode.W;
	public KeyCode m_downKey = KeyCode.S;

	[Space]
	[Tooltip("Hold to rotate rather than move")]
	public KeyCode m_rotationModifierKey = KeyCode.LeftShift;
	public bool m_rotateByDefault = false;

	private Vector3 m_originalPos = Vector3.one;
	private Quaternion m_originalRot = Quaternion.identity;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_originalPos = this.transform.localPosition;
		m_originalRot = this.transform.localRotation;
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		if(!this.isActiveAndEnabled) return;

		Vector3 offset = Vector3.zero;

		if(Input.GetKey(m_forwardKey)) {
			offset.z = +1f;
		}
		if(Input.GetKey(m_backwardsKey)) {
			offset.z = -1f;
		}

		if(Input.GetKey(m_rightKey)) {
			offset.x = +1f;
		}
		if(Input.GetKey(m_leftKey)) {
			offset.x = -1f;
		}

		if(Input.GetKey(m_upKey)) {
			offset.y = +1f;
		}
		if(Input.GetKey(m_downKey)) {
			offset.y = -1f;
		}

		// Apply transform
		bool rotateKey = Input.GetKey(m_rotationModifierKey);
		if((m_rotateByDefault && !rotateKey) || (!m_rotateByDefault && rotateKey)) {
			// To make rotation more intuitive, switch axis around
			offset = offset * m_rotationSpeed;
			this.transform.Rotate(
				offset.y * m_rotationSpeed * -1f,
				offset.x * m_rotationSpeed,
				offset.z * m_rotationSpeed,
				Space.Self
			);
		} else {
			offset = offset * m_moveSpeed;
			this.transform.Translate(offset, Space.Self);
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Reset position and rotation to original ones.
	/// </summary>
	public void Reset() {
		this.transform.localRotation = m_originalRot;
		this.transform.localPosition = m_originalPos;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}