// LevelTypeCollision.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/12/2015.
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
	/// Specialization of a level.
	/// </summary>
	[ExecuteInEditMode]
	public class LevelTypeCollision : Level {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//

		//------------------------------------------------------------------//
		// GENERIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Initialization.
		/// </summary>
		override protected void Awake() {
			// Call parent
			base.Awake();
		}
	}
}

