using UnityEngine;
using System.Collections;
using AISM;

namespace AI {
	namespace Behaviour {		
		[CreateAssetMenu(menuName = "Behaviour/Attack Melee")]
		public class AttackMelee : Attack {
			private MeleeWeapon m_meleeWeapon;

			protected override void OnInitialise(GameObject _go) {
				base.OnInitialise(_go);
				m_meleeWeapon	= _go.FindComponentRecursive<MeleeWeapon>();
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				base.OnEnter(_oldState, _param);
				m_meleeWeapon.damage = 5f; // TODO
				m_meleeWeapon.enabled = false;
			}

			protected override void OnExit(State _newState) {
				base.OnExit(_newState);
				m_meleeWeapon.enabled = false;
			}

			private void OnAnimDealDamageExtended() {				
				m_meleeWeapon.enabled = true;
			}

			protected override void OnAnimEndExtended() {
				m_meleeWeapon.enabled = false;	
			}
		}
	}
}