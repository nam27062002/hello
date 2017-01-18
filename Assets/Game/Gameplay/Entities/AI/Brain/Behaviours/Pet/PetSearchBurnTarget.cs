﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AI {
	namespace Behaviour {	

		[CreateAssetMenu(menuName = "Behaviour/Pet/Search Burn Target")]
		public class PetSearchBurnTarget : StateComponent {

			[StateTransitionTrigger]
			private static string OnEnemyInBurnRange = "onEnemyInBurnRange";

			private float m_shutdownSensorTime;
			private float m_timer;
			private object[] m_transitionParam;

			private Entity[] m_checkEntities = new Entity[20];
			private int m_numCheckEntities = 0;

			private int m_collidersMask;

			DragonPlayer m_owner;
			float m_range;
			DragonTier m_tier;


			protected override void OnInitialise() {
				m_timer = 0f;
				m_shutdownSensorTime = 0f;

				m_transitionParam = new object[1];

				m_collidersMask = 1<<LayerMask.NameToLayer("Ground") | 1<<LayerMask.NameToLayer("Obstacle");

				base.OnInitialise();

				m_owner = InstanceManager.player;
				m_tier = InstanceManager.player.data.tier;
				m_range = m_owner.data.GetScaleAtLevel(m_owner.data.progression.maxLevel) * 10f;

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

						if ( entity.IsBurnable() && ( entity.IsBurnable(m_tier) || InstanceManager.player.breathBehaviour.type == DragonBreathBehaviour.Type.Super ) )
						{
							// Check if physics reachable
							Machine machine = entity.GetComponent<Machine>();
							RaycastHit hit;
							Vector3 dir = entity.circleArea.center - m_machine.position;
							bool hasHit = Physics.Raycast(m_machine.position, dir.normalized, out hit, dir.magnitude, m_collidersMask);
							if ( !hasHit )
							{
								// Check if closed? Not for the moment
								m_transitionParam[0] = entity.transform;
								Transition( OnEnemyInBurnRange, m_transitionParam);
								break;
							}
						}
					}

				}
			}
		}
	}
}