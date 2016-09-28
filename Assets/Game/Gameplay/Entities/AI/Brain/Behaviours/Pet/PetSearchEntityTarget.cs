using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {	

		[CreateAssetMenu(menuName = "Behaviour/Pet/Search Entity Target")]
		public class PetSearchEntityTarget : StateComponent {

			[StateTransitionTrigger]
			private static string OnEnemyInRange = "onEnemyInRange";

			private float m_shutdownSensorTime;
			private float m_timer;
			private DragonTier m_eaterTier;
			private object[] m_transitionParam;

			private Entity[] m_checkEntities = new Entity[20];
			private int m_numCheckEntities = 0;

			private int m_collidersMask;

			DragonPlayer m_owner;
			float m_range;

			protected override void OnInitialise() {
				m_timer = 0f;
				m_shutdownSensorTime = 0f;

				// Temp
				MachineEatBehaviour machineEat = m_pilot.GetComponent<MachineEatBehaviour>();
				if ( machineEat )
					m_eaterTier = machineEat.eaterTier;

				m_transitionParam = new object[1];

				m_collidersMask = 1<<LayerMask.NameToLayer("Ground") | 1<<LayerMask.NameToLayer("Obstacle");

				base.OnInitialise();

				m_owner = InstanceManager.player;
				m_range = m_owner.data.GetScaleAtLevel( m_owner.data.progression.lastLevel) * 10f;

			}

			// The first element in _param must contain the amount of time without detecting an enemy
			protected override void OnEnter(State _oldState, object[] _param) {
				if (_param != null && _param.Length > 0) {
					m_shutdownSensorTime = (float)_param[0];
				} else {
					m_shutdownSensorTime = 0f;
				}

				if (m_shutdownSensorTime > 0f) {
					m_timer = m_shutdownSensorTime;
				} else {
					m_timer = 0f;
				}
			}

			protected override void OnUpdate() {
				if (m_timer > 0f) {
					m_timer -= Time.deltaTime;
				} else {
					Vector3 centerPos = m_owner.transform.position;
					m_numCheckEntities = EntityManager.instance.GetOverlapingEntities( centerPos , m_range, m_checkEntities);
					for (int e = 0; e < m_numCheckEntities; e++) 
					{
						Entity entity = m_checkEntities[e];
						Machine machine = entity.GetComponent<Machine>();
						if (
							entity.IsEdible() && entity.IsEdible( m_eaterTier ) && machine != null && machine.CanBeBitten() && !machine.isPetTarget
						)
						{
							// Check if physics reachable
							RaycastHit hit;
							Vector3 dir = entity.circleArea.center - m_machine.position;
							bool hasHit = Physics.Raycast(m_machine.position, dir.normalized, out hit, dir.magnitude, m_collidersMask);
							if ( !hasHit )
							{
								// Check if closed? Not for the moment
								m_transitionParam[0] = entity.transform;
								Transition( OnEnemyInRange, m_transitionParam);
								break;
							}
						}
					}

				}
			}
		}
	}
}