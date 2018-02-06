// MenuCameraGraphTester.cs
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
	public class MenuCameraGraphTester : MonoBehaviour {
		//------------------------------------------------------------------------//
		// CONSTANTS															  //
		//------------------------------------------------------------------------//
		
		//------------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES												  //
		//------------------------------------------------------------------------//
		[SerializeField] private MenuCameraGraph m_graph = null;
		public MenuCameraGraph graph {
			get { return m_graph; }
		}

		[Space]
		[SerializeField] private Screen m_testScreen = Screen.NONE;
		
		//------------------------------------------------------------------------//
		// GENERIC METHODS														  //
		//------------------------------------------------------------------------//

		//------------------------------------------------------------------------//
		// OTHER METHODS														  //
		//------------------------------------------------------------------------//

		//------------------------------------------------------------------------//
		// CALLBACKS															  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Tester button.
		/// </summary>
		public void Test() {
			MenuCameraGraphNode n = m_graph.GetNode(m_testScreen);
			if(n == null) {
				Debug.Log("<color=red>No node found for " + m_testScreen + "</color>");
			} else {
				Screen[] inputs = n.GetInputValues<Screen>("screenFrom");
				string str = inputs.Length + " connections found for node " + m_testScreen + ": \n";
				for(int i = 0; i < inputs.Length; ++i) {
					str += "\t" + inputs[i] + "\n";
				}
				Debug.Log(str);
			}
		}
	}
}