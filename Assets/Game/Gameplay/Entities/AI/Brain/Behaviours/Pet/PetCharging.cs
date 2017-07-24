﻿using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {

		[CreateAssetMenu(menuName = "Behaviour/Pet/Charging")]
		public class PetCharging : StateComponent {

			Pet m_entity;

			protected override void OnInitialise() {
				m_entity = m_pilot.GetComponent<Pet>();
			}

			protected override void OnEnter(State oldState, object[] param) {
				base.OnEnter(oldState, param);
				m_entity.Charging = true;
			}

			protected override void OnExit(State _newState){
				base.OnExit(_newState);
				m_entity.Charging = false;
			}
		}
	}
}