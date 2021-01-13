using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AI {
	namespace Behaviour {

		[System.Serializable]
		public class PetSearchShootAreaTargetData : StateComponentData {
			public Range m_shutdownRange = new Range(10,20);
            public float m_frontDistance = 4;
            public float m_frontAreaSize = 4;
		}

		[CreateAssetMenu(menuName = "Behaviour/Pet/Search Shoot Area Target")]
		public class PetSearchShootAreaTarget : StateComponent {

			[StateTransitionTrigger]
			private static readonly int onEnemyTargeted = UnityEngine.Animator.StringToHash("onEnemyTargeted");

			private float m_shutdownSensorTime;
			private float m_timer;
			private object[] m_transitionParam;
            MachineSensor m_sensor;

			DragonPlayer m_owner;
            Transform m_target;

			private PetSearchShootAreaTargetData m_data;

			public override StateComponentData CreateData() {
				return new PetSearchShootAreaTargetData();
			}

			public override System.Type GetDataType() {
				return typeof(PetSearchShootAreaTargetData);
			}

			protected override void OnInitialise() {
				m_timer = 0f;
				m_shutdownSensorTime = 0f;
				m_transitionParam = new object[1];
				base.OnInitialise();
				m_owner = InstanceManager.player;
				m_data = m_pilot.GetComponentData<PetSearchShootAreaTargetData>();
				m_sensor = (m_machine as Machine).sensor;
                m_target = m_machine.transform.Find("target");
                m_target.transform.parent = m_machine.transform.parent;
                m_transitionParam[0] = m_target;
                m_sensor.SetupEnemy(m_target, 0, null);
			}

			// The first element in _param must contain the amount of time without detecting an enemy
			protected override void OnEnter(State _oldState, object[] _param) {
				m_shutdownSensorTime = m_data.m_shutdownRange.GetRandom();
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
                    centerPos +=  m_owner.dragonMotion.direction * m_data.m_frontDistance * m_owner.data.maxScale;
                    Vector3 target = centerPos + (Vector3)(UnityEngine.Random.insideUnitCircle * m_data.m_frontAreaSize * m_owner.data.maxScale);
                    target.z = 0;
                    m_transitionParam[0] = target;
                    m_target.transform.position = target;
                    m_machine.SetSignal(Signals.Type.Warning, true);
                    Transition( onEnemyTargeted, m_transitionParam);
				}
			}
		}
	}
}