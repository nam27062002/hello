// MenuCameraGraph.cs
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
	[CreateAssetMenu]
	public class MenuCameraGraph : NodeGraph {
		//------------------------------------------------------------------------//
		// CONSTANTS															  //
		//------------------------------------------------------------------------//
		
		//------------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES												  //
		//------------------------------------------------------------------------//
		
		//------------------------------------------------------------------------//
		// PARENT OVERRIDES														  //
		//------------------------------------------------------------------------//

		//------------------------------------------------------------------------//
		// OTHER METHODS														  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Get the node corresponding to a specific screen.
		/// </summary>
		/// <returns>The node.</returns>
		/// <param name="_screenId">Screen identifier.</param>
		public MenuCameraGraphNode GetNode(Screen _screenId) {
			Node match = nodes.Find(
				(Node _n) => {
					return ((_n as MenuCameraGraphNode).screenId == _screenId);
				}
			);

			return match as MenuCameraGraphNode;
		}

		//------------------------------------------------------------------------//
		// CALLBACKS															  //
		//------------------------------------------------------------------------//
	}
}