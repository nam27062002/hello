using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class AttackTurnAroundData : StateComponentData {			
			public float damage = 5f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Attack Turn Around")]
		public class AttackTurnAround : StateComponent {

			[StateTransitionTrigger]
			private static string OnTurnAroundEnd = "onTurnAroundEnd";


			private MeleeWeapon m_meleeWeapon;
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
				m_meleeWeapon = m_pilot.FindComponentRecursive<MeleeWeapon>();
				m_animEvents = m_pilot.FindComponentRecursive<PreyAnimationEvents>();

				m_meleeWeapon.enabled = false;
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
				m_meleeWeapon.damage = m_data.damage;
				OnEnableWeapon();
			}

			private void OnEnableWeapon() {
				m_meleeWeapon.enabled = true;
			}

			private void OnDisableWeapon() {
				m_meleeWeapon.enabled = false;
			}

			private void OnAnimEnd() {
				OnDisableWeapon();
				m_machine.orientation = m_machine.orientation * Quaternion.AngleAxis(180f, Vector3.up);
				m_pilot.SetDirection(m_machine.orientation * Vector3.forward, true);
				Transition(OnTurnAroundEnd);
			}
		}
	}
}