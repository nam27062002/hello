using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AI {
	namespace Behaviour {	

		[System.Serializable]
		public class PetSearchCageTargetData : StateComponentData {
			public float m_dragonSizeRangeMultiplier = 10;
		}

		[CreateAssetMenu(menuName = "Behaviour/Pet/Search Cage Target")]
		public class PetSearchCageTarget : StateComponent {

			[StateTransitionTrigger]
			private static readonly int onCageInRange = UnityEngine.Animator.StringToHash("onCageInRange");

            private float m_shutdownSensorTime;
			private float m_timer;
			private object[] m_transitionParam;

			private Cage[] m_checkCages = new Cage[5];
			private int m_numCheckCages = 0;

			private int m_collidersMask;

			DragonPlayer m_owner;
			float m_range;

			public override StateComponentData CreateData() {
				return new PetSearchCageTargetData();
			}

			public override System.Type GetDataType() {
				return typeof(PetSearchCageTargetData);
			}

			protected override void OnInitialise() {
				m_timer = 0f;
				m_shutdownSensorTime = 0f;

				m_transitionParam = new object[1];

				m_collidersMask = 1<<LayerMask.NameToLayer("Ground") | 1<<LayerMask.NameToLayer("Obstacle");

				base.OnInitialise();

				m_owner = InstanceManager.player;
				PetSearchCageTargetData data = m_pilot.GetComponentData<PetSearchCageTargetData>();
				m_range = m_owner.data.maxScale * data.m_dragonSizeRangeMultiplier;
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

			private bool IsReachable( Transform targetTransform )
			{
				// Check if physics reachable
				RaycastHit hit;
				Vector3 dir = targetTransform.position - m_machine.position;
				bool hasHit = Physics.Raycast(m_machine.position, dir.normalized, out hit, dir.magnitude, m_collidersMask);
				return !hasHit;
			}

			protected override void OnUpdate() {
				if (m_timer > 0f) {
					m_timer -= Time.deltaTime;
				} else {
					Vector3 centerPos = m_owner.transform.position;
					m_numCheckCages = EntityManager.instance.GetOverlapingCages( centerPos , m_range, m_checkCages);
					for (int e = 0; e < m_numCheckCages; e++) 
					{
						Cage cage = m_checkCages[e];
						if (!cage.behaviour.broken && IsReachable( cage.behaviour.centerTarget ))
						{
							m_transitionParam[0] = cage.behaviour.centerTarget;
							Transition(onCageInRange, m_transitionParam);
						}
					}
				}
			}
		}
	}
}