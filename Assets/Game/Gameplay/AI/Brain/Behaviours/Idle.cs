﻿using UnityEngine;
using System.Collections;
using AISM;

namespace AI {
	namespace Behaviour {		
		[CreateAssetMenu(menuName = "Behaviour/Idle")]
		public class Idle : StateComponent {

			[StateTransitionTrigger]
			private static string OnMove = "onMove";

			private float m_timer;

			private Pilot m_pilot;
			private Machine m_machine;

			protected override void OnInitialise(GameObject _go) {
				m_pilot 	= _go.GetComponent<Pilot>();
				m_machine	= _go.GetComponent<Machine>();
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_timer = Random.Range(2f, 4f);
				m_pilot.SetSpeed(0);
			}

			protected override void OnUpdate() {
				m_timer -= Time.deltaTime;
				if (m_timer <= 0f) {
					Transition(OnMove);
				}
			}
		}
	}
}