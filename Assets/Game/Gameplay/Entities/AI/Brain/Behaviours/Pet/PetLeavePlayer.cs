using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[CreateAssetMenu(menuName = "Behaviour/Pet/Leave Player")]
		public class PetLeavePlayer : StateComponent {

			[StateTransitionTrigger]
			public static readonly int onOutOfScreen = UnityEngine.Animator.StringToHash("onOutOfScreen");

			private SphereCollider m_collider;
			private float m_speed;
			private Vector3 m_direction;

			protected override void OnInitialise() {
				m_collider = m_pilot.GetComponent<SphereCollider>();
				m_speed = InstanceManager.player.dragonMotion.absoluteMaxSpeed;
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				m_pilot.SlowDown(true);
				m_pilot.SetMoveSpeed(m_speed);

				// Get direction
				m_direction = -InstanceManager.player.dragonMotion.direction;
				m_direction.z = 0;

				// Seach good direction to move
			}

			protected override void OnUpdate() 
			{
				// Check if I'm out of screen so I can dissapear
				if (InstanceManager.gameCamera.IsInsideDeactivationArea( m_machine.position))
				{
					// 
					Transition( onOutOfScreen );
				}
				else
				{
					/*
					if (Physics.Linecast(m_machine.position, m_machine.position + (m_direction * m_speed), m_groundMask))
					{
						m_direction.RotateXYDegrees( Time.deltaTime * 180 );
					}
					*/
					// keep moving far away form the player
					// m_pilot.SetDirection( m_direction );
					Vector3 target = m_machine.position + m_direction * m_speed;
					m_pilot.GoTo(target);
				}
			}

		}
	}
}