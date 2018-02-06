// MenuCameraGraphNode.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using XNode;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
namespace MenuCameraTest {
	/// <summary>
	/// 
	/// </summary>
	public class MenuCameraGraphNode : Node {
		//------------------------------------------------------------------------//
		// CONSTANTS															  //
		//------------------------------------------------------------------------//
		
		//------------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES												  //
		//------------------------------------------------------------------------//
		[SerializeField] private Screen m_screenId = Screen.NONE;
		public Screen screenId {
			get { return m_screenId; }
		}

		[Output] public Screen screenOutput = Screen.NONE;
		[Input] public Screen screenFrom = Screen.NONE;
		
		//------------------------------------------------------------------------//
		// GENERIC METHODS														  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// 
		/// </summary>
		public override object GetValue(NodePort port) {
			// Check which OUTPUT is being requested
			switch(port.fieldName) {
				case "screenOutput": return m_screenId; break;
			}
			return null;
		}

		//------------------------------------------------------------------------//
		// OTHER METHODS														  //
		//------------------------------------------------------------------------//

		//------------------------------------------------------------------------//
		// CALLBACKS															  //
		//------------------------------------------------------------------------//
	}
}