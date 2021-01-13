// Level.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/10/2015.
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
	/// Default behaviour to be added to any editable level.
	/// </summary>
	[ExecuteInEditMode]
	public class Level : MonoBehaviour {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//

		//------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES											//
		//------------------------------------------------------------------//

		//------------------------------------------------------------------//
		// GENERIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Initialization.
		/// </summary>
		virtual protected void Awake() {
			// Make ouselves static, we don't want to accidentally move the parent object
			this.gameObject.isStatic = true;			
		}
	}
}

