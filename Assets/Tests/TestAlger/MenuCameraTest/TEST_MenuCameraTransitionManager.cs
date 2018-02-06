// TEST_MenuCameraTransitionManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/01/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
namespace MenuCameraTest {
	/// <summary>
	/// 
	/// </summary>
	public class TEST_MenuCameraTransitionManager : MonoBehaviour {
		//------------------------------------------------------------------------//
		// CONSTANTS															  //
		//------------------------------------------------------------------------//
		[Serializable]
		public class SnapPointSetup {
			public Screen id = Screen.NONE;
			public int pathPointIdx = 0;
		}

		[Serializable]
		public class Transition {
			public BezierCurve path = null;
			public SnapPointSetup screen1 = new SnapPointSetup();
			public SnapPointSetup screen2 = new SnapPointSetup();
		}

		private class ScreenPair {
			public Screen screen1;
			public Screen screen2;

			public ScreenPair(Screen _scr1, Screen _scr2) {
				screen1 = _scr1;
				screen2 = _scr2;
			}

			public override bool Equals(System.Object _obj) {
				// Check for null values and compare run-time types.
				if(_obj == null || GetType() != _obj.GetType()) return false;

				ScreenPair k = (ScreenPair)_obj;
				return (this.screen1 == k.screen1 && this.screen2 == k.screen2)
					|| (this.screen2 == k.screen1 && this.screen1 == k.screen2);
			}

			public override int GetHashCode() {
				return screen1.GetHashCode() ^ screen2.GetHashCode();
			}
		}
		
		//------------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES												  //
		//------------------------------------------------------------------------//
		// Exposed
		[SerializeField] private Transition[] m_transitions = new Transition[0];

		// For faster transition lookup
		private Dictionary<ScreenPair, Transition> m_transitionsPerScreen = null;
		private Dictionary<ScreenPair, Transition> transitionsPerScreen {
			get {
				if(m_transitionsPerScreen == null) {
					m_transitionsPerScreen = new Dictionary<ScreenPair, Transition>();
					for(int i = 0; i < m_transitions.Length; ++i) {
						Transition t = m_transitions[i];
						m_transitionsPerScreen[new ScreenPair(t.screen1.id, t.screen2.id)] = t;
					}
				}
				return m_transitionsPerScreen;
			}
		}
		
		//------------------------------------------------------------------------//
		// GENERIC METHODS														  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Initialization.
		/// </summary>
		private void Awake() {
			// Initialize dictionary
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
		/// Called every frame.
		/// </summary>
		private void Update() {

		}

		/// <summary>
		/// Destructor.
		/// </summary>
		private void OnDestroy() {

		}

		//------------------------------------------------------------------------//
		// OTHER METHODS														  //
		//------------------------------------------------------------------------//

		//------------------------------------------------------------------------//
		// CALLBACKS															  //
		//------------------------------------------------------------------------//
	}
}