using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		
		[System.Serializable]
		public class WanderPlayerData : StateComponentData {
			// public float speedMultiplier = 1.5f;
			public string petWanderSku = "common";
		}

		[CreateAssetMenu(menuName = "Behaviour/Pet/Wander Player")]
		public class PetWanderPlayer : StateComponent {

            private DragonMotion m_dragonMotion;

			private SphereCollider m_collider;
			private Transform m_target;
			private Vector3 m_targetOffset;

            private float m_baseSpeedMultiplier;
            private float m_speed;

			private float m_maxFarDistance;
			private float m_startRandom;
			private float m_minorRandom;
			private float m_circleTimer = 0;

			public override StateComponentData CreateData() {
				return new WanderPlayerData();
			}

			public override System.Type GetDataType() {
				return typeof(WanderPlayerData);
			}

			protected override void OnInitialise() {				
				WanderPlayerData data = m_pilot.GetComponentData<WanderPlayerData>();

                m_dragonMotion = InstanceManager.player.dragonMotion;

                m_collider = m_pilot.GetComponent<SphereCollider>();
				m_target = m_machine.transform;
				
                DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PET_MOVEMENT, data.petWanderSku);
                m_baseSpeedMultiplier = def.GetAsFloat("wanderSpeedMultiplier");
                m_speed = m_dragonMotion.absoluteMaxSpeed * m_baseSpeedMultiplier;

				m_maxFarDistance = InstanceManager.player.data.maxScale * def.GetAsFloat("wanderDistanceMultiplier");
				m_startRandom = Random.Range(0, 2 * Mathf.PI);
				m_minorRandom = Random.Range( 2, 7);
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				SelectTarget();
				m_pilot.SlowDown(false);
				m_pilot.SetMoveSpeed(m_speed); 
			}

			protected override void OnUpdate() {
                Vector3 targetPos = InstanceManager.player.dragonMotion.position;
				Vector2 circleMove;

                circleMove.x = Mathf.Cos(m_circleTimer + m_startRandom) * m_maxFarDistance * 1.5f - m_maxFarDistance * 0.5f; // Trying an ellipsis
				circleMove.y = Mathf.Sin(m_circleTimer + m_startRandom) * m_maxFarDistance;

				targetPos.x += circleMove.x;
				targetPos.y += circleMove.y;
				m_pilot.GoTo(targetPos);

				float magnitude = (targetPos - m_pilot.transform.position).sqrMagnitude;
                if (magnitude < 5f) {
                    float dt = Time.deltaTime * (1f + m_dragonMotion.speed * 0.5f);
                    m_circleTimer += dt;
                }

                m_pilot.SetMoveSpeed(m_speed * (1f + m_dragonMotion.speed), false);

				if (m_pilot.speed <= 0.1f ) {
					m_pilot.SlowDown(true);
					m_pilot.SetDirection( m_target.forward );
				} else {
					m_pilot.SlowDown(true);
				}				

				Debug.DrawLine(m_machine.position, targetPos, Colors.gold);
			}

			private void SelectTarget() {
				m_target = m_pilot.homeTransform;	//  Get Pet position??			
				m_targetOffset = Vector3.zero;
			}
		}
	}
}