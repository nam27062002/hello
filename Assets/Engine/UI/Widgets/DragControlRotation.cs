// DragControlRotation.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Connector of a drag control with a rotation transformation.
/// </summary>
public class DragControlRotation : DragControl {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private Transform m_target = null;
	public Transform target {
		get { return m_target; }
		set { InitFromTarget(value, false); }
	}

	[Space]
	[SerializeField] private bool m_restoreOnDisable = true;
	[SerializeField] private float m_restoreDuration = 0.25f;

	// Internal
	private Quaternion m_originalRotation = Quaternion.identity;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void OnEnable() {
		base.OnEnable();

		// Initial setup
		InitFromTarget(target, true);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	override protected void OnDisable() {
		// Restore original value?
		if(m_restoreOnDisable) {
			RestoreOriginalValue();
		}

		base.OnDisable();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Do whatever needed to apply the value.
	/// </summary>
	override protected void ApplyValue() {
		// Ignore if target not valid
		if(m_target == null) return;

		// Apply value as a rotation
		m_target.localRotation = Quaternion.Euler(0f, -m_value.x, m_value.y);
	}

	/// <summary>
	/// Intitialize the drag controller with the given target.
	/// </summary>
	/// <param name="_target">New target.</param>
	/// <param name="_force">Do all the initialization even if the target is the same as the current one.</param>
	private void InitFromTarget(Transform _target, bool _force) {
		// If we have a valid target already, reset it to its original position (if the flag says so)
		// Only if we're active!
		if(isActiveAndEnabled && m_restoreOnDisable) {
			RestoreOriginalValue();
		}

		// Store target
		Transform oldTarget = m_target;
		m_target = _target;

		// Nothing else to do if new target is not valid
		if(m_target == null) return;

		// Extra stuff only if the target is different than the current one (or forced)
		// Otherwise we would be overriding original values
		if(m_target != oldTarget || _force) {
			// Store target's original rotation
			m_originalRotation = m_target.localRotation;

			// Initialize drag control with current target value
			value = new Vector2(-m_originalRotation.eulerAngles.y, m_originalRotation.eulerAngles.z);
		}
	}

	/// <summary>
	/// Restore original value of the target.
	/// Doesn't check the flag nor the state of the component.
	/// </summary>
	private void RestoreOriginalValue() {
		// Target must be valid
		if(m_target == null) return;

		// Animated?
		if(m_restoreDuration > 0f) {
			m_target.DOLocalRotateQuaternion(m_originalRotation, m_restoreDuration)
				.SetUpdate(true)
				.SetEase(Ease.InOutQuad);
		} else {
			m_target.localRotation = m_originalRotation;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}