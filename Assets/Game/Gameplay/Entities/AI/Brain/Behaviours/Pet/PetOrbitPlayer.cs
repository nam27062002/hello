using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		public enum SpeedUpConditions {
            None = 0,
            Boost,
            FireRush
        };

		[System.Serializable]
		public class PetOrbitPlayerData : StateComponentData {			
			public string petWanderSku = "common";
            public float baseSpeed = 0;

            public SpeedUpConditions speedWhen = SpeedUpConditions.None;
            public float speedMultiplier = 1f;
        }

        [CreateAssetMenu(menuName = "Behaviour/Pet/Orbit Player")]
		public class PetOrbitPlayer : StateComponent {

            private DragonBreathBehaviour m_dragonBreath;
            private DragonBoostBehaviour m_dragonBoost;
            private DragonMotion m_dragonMotion;

			private SphereCollider m_collider;
			private Vector3 m_targetLocal;

            private SpeedUpConditions m_speedUpWhen;
            private float m_speedMultiplier;
            private float m_speed;

			private float m_maxFarDistance;
			private float m_startRandom;
			private float m_minorRandom;
			private float m_circleTimer = 0;
            private float m_selectTargetTimer = 0;




			public override StateComponentData CreateData() {
				return new PetOrbitPlayerData();
			}

			public override System.Type GetDataType() {
				return typeof(PetOrbitPlayerData);
			}

			protected override void OnInitialise() {
                PetOrbitPlayerData data = m_pilot.GetComponentData<PetOrbitPlayerData>();

                m_dragonBreath = InstanceManager.player.breathBehaviour;
                m_dragonBoost = InstanceManager.player.dragonBoostBehaviour;
                m_dragonMotion = InstanceManager.player.dragonMotion;

                m_collider = m_pilot.GetComponent<SphereCollider>();
                m_targetLocal = GameConstants.Vector3.zero;
				
                DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PET_MOVEMENT, data.petWanderSku);
                m_speedUpWhen = data.speedWhen;
                m_speedMultiplier = data.speedMultiplier;
                m_speed = data.baseSpeed * def.GetAsFloat("wanderSpeedMultiplier"); ;

				m_maxFarDistance = InstanceManager.player.data.maxScale * def.GetAsFloat("wanderDistanceMultiplier");
				m_startRandom = Random.Range(0, 2 * Mathf.PI);
				m_minorRandom = Random.Range( 2, 7);
			}

			protected override void OnEnter(State _oldState, object[] _param) {
                SelectTargetAtSpeed(m_speed);
				m_pilot.SlowDown(false);
				m_pilot.SetMoveSpeed(m_speed);
                m_pilot.GoTo(m_dragonMotion.position);
            }

			protected override void OnUpdate() {
                float speed = m_speed;
                float extraSpeed = 0;

                switch (m_speedUpWhen) {
                    case SpeedUpConditions.Boost:
                    if (m_dragonBoost.IsBoostActive()) {
                        speed *= m_speedMultiplier;
                    }
                    break;

                    case SpeedUpConditions.FireRush:
                    if (m_dragonBreath.IsFuryOn()) {
                        speed *= m_speedMultiplier;
                    }
                    break;
                }

                if (m_dragonMotion.speed > 1) {
                    float dot = Vector3.Dot(m_dragonMotion.direction, m_machine.direction);
                    if (dot >= 0) {
                        extraSpeed = m_dragonMotion.speed;
                    } else {
                        extraSpeed = speed * dot * 0.65f;
                    }
                }

                float dsqr = (m_pilot.target - m_machine.position).sqrMagnitude;
                float deltaDSqr = Mathf.Max(1f, m_pilot.speed * m_pilot.speed);

                m_selectTargetTimer -= Time.deltaTime;
                if (m_selectTargetTimer <= 0 || dsqr < 5f) {
                    SelectTargetAtSpeed(speed);
                }

                m_pilot.SlowDown(false);
                m_pilot.SetMoveSpeed(speed + m_dragonMotion.speed);
                m_pilot.GoTo(m_targetLocal + m_dragonMotion.position);

                if (m_pilot.speed <= 0.1f) {
                    m_pilot.SetDirection(m_dragonMotion.forward);
                }

				Debug.DrawLine(m_machine.position, m_pilot.target, Colors.gold);
			}

			private void SelectTargetAtSpeed(float _speed) {
                m_targetLocal = GameConstants.Vector3.zero;

                float dMultiplier = 0f;
                if (m_dragonMotion.speed < 1f) {
                    dMultiplier = 0.5f;
                } else {
                    m_targetLocal += m_dragonMotion.direction * m_maxFarDistance * 0.5f;
                }

                float speedFactor = 0.36f + ((0.25f - 0.36f) * ((m_maxFarDistance - 2f) / (8f - 2f)));
                float dt = (_speed) * Time.deltaTime * speedFactor * 4f;
                m_circleTimer += dt;


                m_targetLocal.x += (Mathf.Cos(m_circleTimer + m_startRandom) * m_maxFarDistance * (1f + dMultiplier)) - m_maxFarDistance * dMultiplier; // Trying an ellipsis    
                m_targetLocal.y += Mathf.Sin(m_circleTimer + m_startRandom) * m_maxFarDistance;

                m_selectTargetTimer = 0.25f;
            }
		}
	}
}