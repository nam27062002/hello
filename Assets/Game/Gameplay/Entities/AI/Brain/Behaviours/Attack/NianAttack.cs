using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class NianAttackData : StateComponentData {
			public float jumpVelocity = 10;
			public float damage = 5f;
			// public int consecutiveAttacks = 3;
			// public float dizzyTime = 0f;
			// public float retreatTime = 0f;
			public float maxFacingAngle = 40;
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Nian Attack")]
		public class NianAttack : StateComponent {
			[StateTransitionTrigger]
			private static readonly int onDizzyRecover = UnityEngine.Animator.StringToHash("onDizzyRecover");

            [StateTransitionTrigger]
			private static readonly int onOutOfRange = UnityEngine.Animator.StringToHash("onOutOfRange");

            //-----------------------------------------------------
            private enum AttackState {
				Idle = 0,
				Attack,
				Dizzy	
			}

			private SpartakusAnimationEvents m_animEvents;

			private NianAttackData m_data;
			private MeleeWeapon m_meleeWeapon;

			// private int m_attacksLeft;
			private float m_timer;

			private AttackState m_attackState;
			private MC_MotionGround mC_MotionGround;
			private bool m_impulsed = false;
			private ViewControl m_nianViewControl;


			//-----------------------------------------------------
			public override StateComponentData CreateData() {
				return new NianAttackData();
			}

			public override System.Type GetDataType() {
				return typeof(NianAttackData);
			}

			protected override void OnInitialise() {
				m_animEvents = m_pilot.FindComponentRecursive<SpartakusAnimationEvents>();

				m_data = m_pilot.GetComponentData<NianAttackData>();
				m_meleeWeapon = m_pilot.FindComponentRecursive<MeleeWeapon>();
				m_meleeWeapon.damage = m_data.damage;
				m_meleeWeapon.entity = m_pilot.GetComponent<Entity>();
				m_meleeWeapon.DisableWeapon();

				mC_MotionGround = (m_machine as MachineGround).groundMotion;
				m_nianViewControl = (m_viewControl as ViewControl);
				
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				m_impulsed = false;
				m_animEvents.onJumpImpulse   += new SpartakusAnimationEvents.OnJumpImpulseDelegate(Jump);
                m_animEvents.onJumpFallDown  += new SpartakusAnimationEvents.OnJumpFallDownDelegate(FallDown);
                m_animEvents.onJumpReception += new SpartakusAnimationEvents.OnJumpReceptionDelegate(EndAttack);

				m_pilot.PressAction(Pilot.Action.Attack);

				m_attackState = AttackState.Idle;
			}

			protected override void OnExit(State _newState) {
				m_meleeWeapon.DisableWeapon();

				m_animEvents.onJumpImpulse   -= new SpartakusAnimationEvents.OnJumpImpulseDelegate(Jump);
                m_animEvents.onJumpFallDown  -= new SpartakusAnimationEvents.OnJumpFallDownDelegate(FallDown);
				m_animEvents.onJumpReception -= new SpartakusAnimationEvents.OnJumpReceptionDelegate(EndAttack);

				m_machine.SetSignal(Signals.Type.InvulnerableBite, false);
				m_pilot.ReleaseAction(Pilot.Action.Attack);
				m_pilot.ReleaseAction(Pilot.Action.Jump);
				m_pilot.SetDirection(m_pilot.direction, false);
			}

			protected override void OnUpdate() {

				switch (m_attackState) {
					case AttackState.Idle:
						// Check if out of range
						if ( m_machine.enemy == null || mC_MotionGround.IsInFreeFall() )
						{
							Transition(onOutOfRange);
						}
						else
						{
							// Check face to shoot
							Vector3 lookDir = m_machine.enemy.position - m_machine.position;
							Vector3 pilotDir = m_pilot.transform.forward;
							lookDir.z = pilotDir.z = 0;
							float angle = Vector2.Angle( lookDir, pilotDir);
							m_timer -= Time.deltaTime;
							if (Mathf.Abs(angle) <= m_data.maxFacingAngle && m_timer <= 0 && Vector3.Dot( lookDir, mC_MotionGround.groundNormal ) > 0) {
								StartAttack();
							}
							else
							{
								// Mix with ground direction!
								if ( Vector3.Dot( lookDir, mC_MotionGround.groundDirection )  >= 0) 
								{
									m_pilot.SetDirection( mC_MotionGround.groundDirection, true);
								}
								else
								{
									m_pilot.SetDirection( -mC_MotionGround.groundDirection, true);
								}
								
							}
						}
						break;

					case AttackState.Attack:
						if (m_machine.velocity.y <= 0 )
						{
							if (m_impulsed )
							{
								FallDown();
								m_impulsed = false;
							}
							
							if ( mC_MotionGround.heightFromGround < 2 )
							{
								Vector3 pilotDir = m_pilot.transform.forward;
								if ( Vector3.Dot(pilotDir, mC_MotionGround.groundDirection) > 0 )
								{
									m_pilot.SetDirection(mC_MotionGround.groundDirection, true);	
								}
								else
								{
									m_pilot.SetDirection(-mC_MotionGround.groundDirection, true);	
								}
							}
							else
							{
								Vector3 direction = GameConstants.Vector3.right;
								if ( m_machine.direction.x < 0 )
									direction = GameConstants.Vector3.left;
								m_pilot.SetDirection(direction, true);
							}
							
						}
						break;
				}

				if ( mC_MotionGround.state == MC_Motion.State.StandUp )
				{
					mC_MotionGround.OnStandUp();
				}
				if (m_machine.GetSignal(Signals.Type.InWater)) {
					m_machine.Drown();
				}
			}

			private void StartAttack() {
				m_pilot.PressAction(Pilot.Action.Jump);
				m_machine.SetSignal(Signals.Type.InvulnerableBite, true);
				m_attackState = AttackState.Attack;
			}

			private void Jump() {
				m_impulsed = true;
				m_meleeWeapon.EnableWeapon();

				Vector3 direction = Vector3.right;
				if (m_machine.enemy != null) {
					direction = m_machine.enemy.position - m_machine.position;
					direction.z = 0;
					direction.Normalize();
				}

				if ( Vector3.Dot( mC_MotionGround.groundNormal, direction ) < 0 )
				{
					// Reflect
					Vector3.Reflect(direction, mC_MotionGround.groundNormal);
				}
				
				m_pilot.SetDirection(direction, true);
				Vector3 jump = direction * m_data.jumpVelocity;
				if (jump.y < 1)
					jump.y = 1;
				m_machine.SetVelocity(jump);
			}

            private void FallDown() {
				
                m_machine.SetSignal(Signals.Type.InvulnerableBite, false);
				
            }

			private void EndAttack() {
				Debug.Log("END ATTACK");
				m_pilot.ReleaseAction(Pilot.Action.Jump);
				m_meleeWeapon.DisableWeapon();
				m_pilot.Stop();

				Vector3 direction = GameConstants.Vector3.right;
				if ( m_machine.direction.x < 0 )
					direction = GameConstants.Vector3.left;
				m_pilot.SetDirection(direction, true);

				m_timer = 2.0f;
				if (m_machine.GetSignal(Signals.Type.Danger)) {
					m_attackState = AttackState.Idle;
				} else {
					Transition(onOutOfRange);
				}

				if ( mC_MotionGround.state == MC_Motion.State.FreeFall )
				{
					// Force go to idle!
				}
				
			}
			/*
			private void DizzyRecover() {

				m_machine.DisableSensor(m_data.retreatTime);
				Transition(onDizzyRecover);
			}
			*/
		}
	}
}