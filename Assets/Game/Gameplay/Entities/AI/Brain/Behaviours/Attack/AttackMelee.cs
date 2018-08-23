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
		
			private IMeleeWeapon m_meleeWeapon;
			private IEntity m_entity;


			public override StateComponentData CreateData() {
				return new AttackMeleeData();
			}

			public override System.Type GetDataType() {
				return typeof(AttackMeleeData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<AttackMeleeData>();
                m_entity = m_pilot.GetComponent<IEntity>(); 
				m_meleeWeapon = m_pilot.FindComponentRecursive<IMeleeWeapon>();

				m_meleeWeapon.enabled = false;

				base.OnInitialise();
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				base.OnEnter(_oldState, _param);
				m_meleeWeapon.damage = ((AttackMeleeData)m_data).damage;
				m_meleeWeapon.entity = m_entity;
				m_meleeWeapon.enabled = false;

				m_animEvents.onEnableWeapon += new PreyAnimationEvents.OnEnableWeaponDelegate(OnEnableWeapon);
				m_animEvents.onDisableWeapon += new PreyAnimationEvents.OnDisableWeaponDelegate(OnDisableWeapon);

				m_machine.SetSignal(Signals.Type.Melee, true);
			}

			protected override void OnExit(State _newState) {
				base.OnExit(_newState);
				m_meleeWeapon.enabled = false;

				m_animEvents.onEnableWeapon -= new PreyAnimationEvents.OnEnableWeaponDelegate(OnEnableWeapon);
				m_animEvents.onDisableWeapon -= new PreyAnimationEvents.OnDisableWeaponDelegate(OnDisableWeapon);

				m_machine.SetSignal(Signals.Type.Melee, false);
			}

			protected override void OnAnimDealDamageExtended() {
				OnEnableWeapon();
			}

			protected override void OnAnimEndExtended() {
				OnDisableWeapon();
			}

			private void OnEnableWeapon() {
				m_meleeWeapon.enabled = true;
			}

			private void OnDisableWeapon() {
				m_meleeWeapon.enabled = false;
			}
		}
	}
}