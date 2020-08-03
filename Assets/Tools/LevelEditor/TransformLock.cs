// TransformLock.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// Simple script to lock transformations of an object in the editor.
	/// </summary>
	public class TransformLock : MonoBehaviour {
		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		public bool[] m_positionLock = new bool[3];
		public bool[] m_rotationLock = new bool[3];
		public bool[] m_scaleLock = new bool[3];

		//------------------------------------------------------------------//
		// PROPERTIES														//
		//------------------------------------------------------------------//
		// Set on a per-frame basis, transformation changes performed during that frame will be ignored if set to true
		public bool ignoreLock {get; set;}

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
		/// Sets the position lock.
		/// </summary>
		/// <param name="_x">X axis value.</param>
		/// <param name="_y">Y axis value.</param>
		/// <param name="_z">Z axis value.</param>
		public void SetPositionLock(bool _x, bool _y, bool _z) {
			m_positionLock[0] = _x;
			m_positionLock[1] = _y;
			m_positionLock[2] = _z;
		}

		/// <summary>
		/// Sets the rotation lock.
		/// </summary>
		/// <param name="_x">X axis value.</param>
		/// <param name="_y">Y axis value.</param>
		/// <param name="_z">Z axis value.</param>
		public void SetRotationLock(bool _x, bool _y, bool _z) {
			m_rotationLock[0] = _x;
			m_rotationLock[1] = _y;
			m_rotationLock[2] = _z;
		}

		/// <summary>
		/// Sets the scale lock.
		/// </summary>
		/// <param name="_x">X axis value.</param>
		/// <param name="_y">Y axis value.</param>
		/// <param name="_z">Z axis value.</param>
		public void SetScaleLock(bool _x, bool _y, bool _z) {
			m_scaleLock[0] = _x;
			m_scaleLock[1] = _y;
			m_scaleLock[2] = _z;
		}
	}
}

