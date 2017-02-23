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
				public float speedMultiplier = 1.5f;
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
			string[] possibleEntities;
			float m_speed;
			float m_frontDistance;
			EatBehaviour m_eatBehaviour;
			PetDogSpawner m_petDogSpawner;
			Entity m_spawnedEntity;
			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<PetTakePreyToPlayerData>();

				possibleEntities = m_data.m_possiblePreyList.Split(new string[] { "," }, StringSplitOptions.None);
				m_speed = InstanceManager.player.dragonMotion.absoluteMaxSpeed * m_data.speedMultiplier;
				m_frontDistance = InstanceManager.player.data.GetScaleAtLevel( InstanceManager.player.data.progression.maxLevel) * 6;
				m_eatBehaviour = m_machine.GetComponent<EatBehaviour>();
				m_petDogSpawner = m_machine.GetComponent<PetDogSpawner>();

				// m_petDogSpawner.AddPossibleSpawner();
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				base.OnEnter(_oldState, _param);

				m_eatBehaviour.AdvanceHold(false);
				m_petDogSpawner.Respawn();
				Pilot p = m_petDogSpawner.operatorPilot;
				m_spawnedEntity = p.GetComponent<Entity>();
				m_spawnedEntity.dieOutsideFrustum = false;
				m_pilot.SlowDown(false);
				m_eatBehaviour.StartHold( m_spawnedEntity.GetComponent<AI.IMachine>(), true);




			}

			protected override void OnUpdate() {
				Vector3 targetPos = InstanceManager.player.transform.position;
				targetPos += InstanceManager.player.dragonMotion.direction * m_frontDistance;
				m_pilot.GoTo(targetPos);

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
					Transition(OnPreyReleased);
				}
			}

			protected override void OnExit(State _newState){
				m_eatBehaviour.AdvanceHold(true);
			}

		}
	}
}