// DragControlRotation.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

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

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Convert from value units to rotation.
	/// </summary>
	/// <returns>The to rotation equivalent to the input value.</returns>
	/// <param name="_value">The value to be converted.</param>
	public Quaternion ValueToRotation(Vector2 _value) {
		return Quaternion.Euler(0f, -_value.x, _value.y);
	}

	/// <summary>
	/// Convert from rotation units to value.
	/// </summary>
	/// <returns>The value equivalent to the input rotation.</returns>
	/// <param name="_q">The rotation to be converted.</param>
	public Vector2 RotationToValue(Quaternion _q) {
		return new Vector2(-_q.eulerAngles.y, _q.eulerAngles.z);
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Do whatever needed to apply the value.
	/// </summary>
	override protected void ApplyValue() {
		// Ignore if target not valid
		if(m_target == null) return;
		m_target.localRotation = ValueToRotation(m_value);
	}

	/// <summary>
	/// Get the current value from target.
	/// </summary>
	/// <returns>The current value from target.</returns>
	override protected Vector2 GetValueFromTarget() {
		// Ignore if target not valid
		if(m_target == null) return Vector2.zero;
		return RotationToValue(m_target.localRotation);
	}

	/// <summary>
	/// Apply clamping to the current value.
	/// Override to keep the value looping through the range [0-360]
	/// </summary>
	override protected void ApplyClamp() {
		// Apply default clamping
		base.ApplyClamp();

		// Loop rotation values
		for(int i = 0; i < 2; i++) {
			// Keep it in the range [-180, 180] so we take the shortest way when restoring original value
			m_value[i] = m_value[i] % 360f;
			if(m_value[i] < -180f) {
				m_value[i] += 360f;
			} else if(m_value[i] > 180f) {
				m_value[i] -= 360f;
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}