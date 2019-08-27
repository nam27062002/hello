using UnityEngine;
using System.Collections.Generic;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class AttackTurnAroundData : StateComponentData {			
			public float damage = 5f;
			public string weaponA;
			public string weaponB;
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Attack Turn Around")]
		public class AttackTurnAround : StateComponent {

			[StateTransitionTrigger]
			private static int onTurnAroundEnd = UnityEngine.Animator.StringToHash("onTurnAroundEnd");


			private Entity m_entity;
			private List<IMeleeWeapon> m_meleeWeapons;
			private AttackTurnAroundData m_data;
			private PreyAnimationEvents m_animEvents;



			public override StateComponentData CreateData() {
				return new AttackTurnAroundData();
			}

			public override System.Type GetDataType() {
				return typeof(AttackTurnAroundData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<AttackTurnAroundData>();

				m_meleeWeapons = new List<IMeleeWeapon>();

				if (!string.IsNullOrEmpty(m_data.weaponA)) {
					m_meleeWeapons.Add(m_pilot.FindComponentRecursive<IMeleeWeapon>(m_data.weaponA));
				}

				if (!string.IsNullOrEmpty(m_data.weaponB)) {
					m_meleeWeapons.Add(m_pilot.FindComponentRecursive<IMeleeWeapon>(m_data.weaponB));
				}

				m_entity = m_pilot.GetComponent<Entity>();
                for (int i = 0; i < m_meleeWeapons.Count; ++i) {                 
                    m_meleeWeapons[i].entity = m_entity;
                }

                m_animEvents = m_pilot.FindComponentRecursive<PreyAnimationEvents>();

				OnDisableWeapon();
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				m_pilot.SetMoveSpeed(0);

				m_animEvents.onAttackDealDamage += new PreyAnimationEvents.OnAttackDealDamageDelegate(OnAnimDealDamage);
				m_animEvents.onEnableWeapon 	+= new PreyAnimationEvents.OnEnableWeaponDelegate(OnEnableWeapon);
				m_animEvents.onDisableWeapon 	+= new PreyAnimationEvents.OnDisableWeaponDelegate(OnDisableWeapon);
				m_animEvents.onAttackEnd 		+= new PreyAnimationEvents.OnAttackEndDelegate(OnAnimEnd);
				m_animEvents.onInterrupt 		+= new PreyAnimationEvents.OnInterruptDelegate(OnAnimEnd);
			}

			protected override void OnExit(State _newState) {
				m_animEvents.onAttackDealDamage -= new PreyAnimationEvents.OnAttackDealDamageDelegate(OnAnimDealDamage);
				m_animEvents.onEnableWeapon 	-= new PreyAnimationEvents.OnEnableWeaponDelegate(OnEnableWeapon);
				m_animEvents.onDisableWeapon 	-= new PreyAnimationEvents.OnDisableWeaponDelegate(OnDisableWeapon);
				m_animEvents.onAttackEnd 		-= new PreyAnimationEvents.OnAttackEndDelegate(OnAnimEnd);
				m_animEvents.onInterrupt 		-= new PreyAnimationEvents.OnInterruptDelegate(OnAnimEnd);

				m_pilot.Stop();
			}

			private void OnAnimDealDamage() {
				for (int i = 0; i < m_meleeWeapons.Count; ++i) {
					m_meleeWeapons[i].damage = m_data.damage;
					m_meleeWeapons[i].entity = m_entity;
				}
				OnEnableWeapon();
			}

			private void OnEnableWeapon() {
				for (int i = 0; i < m_meleeWeapons.Count; ++i) {
					m_meleeWeapons[i].enabled = true;
				}
			}

			private void OnDisableWeapon() {
				for (int i = 0; i < m_meleeWeapons.Count; ++i) {
					m_meleeWeapons[i].enabled = false;
				}
			}

			private void OnAnimEnd() {
				OnDisableWeapon();
				m_machine.orientation = m_machine.orientation * Quaternion.AngleAxis(180f, Vector3.up);
				m_pilot.SetDirection(m_machine.orientation * Vector3.forward, true);
				Transition(onTurnAroundEnd);
			}
		}
	}
}