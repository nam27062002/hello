using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class AttackTurnAroundData : StateComponentData {
			public float speed = 1f;
			public float damage = 5f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Attack Turn Around")]
		public class AttackTurnAround : StateComponent {

			[StateTransitionTrigger]
			private static string OnTurnAroundEnd = "onTurnAroundEnd";


			private MeleeWeapon m_meleeWeapon;
			private AttackTurnAroundData m_data;

			private Vector3 m_targetDirection;
			private Quaternion m_targetRotation;


			public override StateComponentData CreateData() {
				return new AttackTurnAroundData();
			}

			public override System.Type GetDataType() {
				return typeof(AttackTurnAroundData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<AttackTurnAroundData>();
				m_meleeWeapon = m_pilot.FindComponentRecursive<MeleeWeapon>();

				m_meleeWeapon.enabled = false;
			}

			protected override void OnEnter(State _oldState, object[] _param) {				
				m_meleeWeapon.damage = m_data.damage;
				m_meleeWeapon.enabled = true;

				if (m_machine.direction.x > 0) {
					m_targetDirection = Vector3.left;
				} else {
					m_targetDirection = Vector3.right;
				}

				m_targetRotation = Quaternion.LookRotation(m_targetDirection + Vector3.back * 0.1f, m_machine.upVector);

				m_pilot.SetMoveSpeed(m_data.speed);
				m_pilot.GoTo(m_machine.position);
			}

			protected override void OnExit(State _newState) {				
				m_meleeWeapon.enabled = false;
				m_pilot.Stop();
			}

			protected override void OnUpdate() {
				m_pilot.SetDirection(m_targetDirection, true);

				float dot = Quaternion.Dot(m_machine.transform.rotation, m_targetRotation);
				if (dot > 0.75f) {
					Transition(OnTurnAroundEnd);
				}
			}
		}
	}
}