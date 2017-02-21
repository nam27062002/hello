﻿using UnityEngine;
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
			GameObject prey;
			float m_speed;
			float m_frontDistance;

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<PetTakePreyToPlayerData>();

				possibleEntities = m_data.m_possiblePreyList.Split(new string[] { "," }, StringSplitOptions.None);
				m_speed = InstanceManager.player.dragonMotion.absoluteMaxSpeed * m_data.speedMultiplier;
				m_frontDistance = InstanceManager.player.data.GetScaleAtLevel( InstanceManager.player.data.progression.maxLevel) * 6;

			}

			protected override void OnEnter(State _oldState, object[] _param) {
				base.OnEnter(_oldState, _param);

				// Select prey and put on mouth
				string prefabStr = possibleEntities[  UnityEngine.Random.Range(0, possibleEntities.Length) ];
				if ( !string.IsNullOrEmpty( prefabStr ) )
				{
					prey = PoolManager.GetInstance(prefabStr, true);
				}
				m_pilot.SlowDown(false);

				// Start going back to the player
			}

			protected override void OnUpdate() {
				Vector3 targetPos = InstanceManager.player.transform.position;
				targetPos += InstanceManager.player.dragonMotion.direction * m_frontDistance;
				m_pilot.GoTo(targetPos);

				float magnitude = (targetPos - m_pilot.transform.position).sqrMagnitude;
				m_pilot.SetMoveSpeed(Mathf.Min( m_speed, magnitude));
				if ( m_pilot.speed <= 0.1f )
				{
					// We are done
					// leave prey
					if ( prey != null )
					{
							
					}
					Transition(OnPreyReleased);
				}
			}

		}
	}
}