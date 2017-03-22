using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AI {
	namespace Behaviour {
		[CreateAssetMenu(menuName = "Behaviour/Pet/Take Prey To Player")]
		public class PetTakePreyToPlayer : StateComponent {
			[System.Serializable]
			public class PetTakePreyToPlayerData : StateComponentData {
				[Comment("Comma Separated list", 5)]
				public string m_possiblePreyList;
				public float m_speedMultiplier = 1.5f;
				public Range m_eatPauseAfterPreyRelease;
			}

			public override StateComponentData CreateData() {
				return new PetTakePreyToPlayerData();
			}

			public override System.Type GetDataType() {
				return typeof(PetTakePreyToPlayerData);
			}

			[StateTransitionTrigger]
			private static string OnPreyReleased = "OnPreyReleased";

			PetTakePreyToPlayerData m_data;
			float m_speed;
			float m_frontDistance;
			EatBehaviour m_eatBehaviour;
			PetDogSpawner m_petDogSpawner;
			Entity m_spawnedEntity;
			object[] m_transitionParam;

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<PetTakePreyToPlayerData>();

				m_speed = InstanceManager.player.dragonMotion.absoluteMaxSpeed * m_data.m_speedMultiplier;
				m_frontDistance = InstanceManager.player.data.GetScaleAtLevel( InstanceManager.player.data.progression.maxLevel) * 6;
				m_eatBehaviour = m_machine.GetComponent<EatBehaviour>();
				m_petDogSpawner = m_machine.GetComponent<PetDogSpawner>();
				m_transitionParam = new object[1];
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				base.OnEnter(_oldState, _param);

				m_eatBehaviour.AdvanceHold(false);
				m_petDogSpawner.RamdomizeEntity();
				m_petDogSpawner.Respawn();
				Pilot p = m_petDogSpawner.operatorPilot;
				if ( p != null)
				{
					m_spawnedEntity = p.GetComponent<Entity>();
					m_spawnedEntity.dieOutsideFrustum = false;
					m_pilot.SlowDown(false);
					m_eatBehaviour.StartHold( m_spawnedEntity.GetComponent<AI.IMachine>(), true);
				}
				else
				{
					Debug.TaggedLogError( "PetDog", "No not get pilot on" + m_petDogSpawner.GetSelectedPrefabStr());
				}
			}

			protected override void OnUpdate() {
				Vector3 targetPos = InstanceManager.player.transform.position;
				targetPos += InstanceManager.player.dragonMotion.direction * m_frontDistance;
				m_pilot.GoTo(targetPos);

				if ( m_spawnedEntity != null )
				{
					float magnitude = (targetPos - m_pilot.transform.position).sqrMagnitude;
					if ( magnitude <= 1 )
					{
						// We are done
						// leave prey
						if ( m_spawnedEntity != null )
						{
							m_eatBehaviour.EndHold();
							m_spawnedEntity.dieOutsideFrustum = true;
							m_spawnedEntity = null;
						}

						m_transitionParam[0] = m_data.m_eatPauseAfterPreyRelease.GetRandom();
						Transition(OnPreyReleased, m_transitionParam);
					}
				}
				else
				{
					m_transitionParam[0] = m_data.m_eatPauseAfterPreyRelease.GetRandom();
					Transition(OnPreyReleased, m_transitionParam);
				}
			}

			protected override void OnExit(State _newState){
				m_eatBehaviour.AdvanceHold(true);
				m_eatBehaviour.enabled = false;
			}

		}
	}
}