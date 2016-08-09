using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class AttackMeleeData : AttackData {
			public float damage = 5f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Melee")]
		public class AttackMelee : Attack {
		
			private MeleeWeapon m_meleeWeapon;


			public override StateComponentData CreateData() {
				return new AttackMeleeData();
			}

			public override System.Type GetDataType() {
				return typeof(AttackMeleeData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<AttackMeleeData>();
				m_meleeWeapon = m_pilot.FindComponentRecursive<MeleeWeapon>();

				m_meleeWeapon.enabled = false;

				base.OnInitialise();
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				base.OnEnter(_oldState, _param);
				m_meleeWeapon.damage = ((AttackMeleeData)m_data).damage;
				m_meleeWeapon.enabled = false;
			}

			protected override void OnExit(State _newState) {
				base.OnExit(_newState);
				m_meleeWeapon.enabled = false;
			}

			protected override void OnAnimDealDamageExtended() {
				m_meleeWeapon.enabled = true;
			}

			protected override void OnAnimEndExtended() {
				m_meleeWeapon.enabled = false;
			}
		}
	}
}