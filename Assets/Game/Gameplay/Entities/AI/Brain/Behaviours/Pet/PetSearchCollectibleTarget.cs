using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AI {
	namespace Behaviour {	
		public enum CollectibleType
		{
			EGG,
			CHEST,
			LETTERS,
		}

		[System.Serializable]
		public class PetSearchCollectibleTargetData : StateComponentData {
			public float m_dragonSizeRangeMultiplier = 10;
			public CollectibleType m_type = CollectibleType.EGG;
		}

		[CreateAssetMenu(menuName = "Behaviour/Pet/Search Collectible Target")]
		public class PetSearchCollectibleTarget : StateComponent {

			[StateTransitionTrigger]
			private static string OnCollectibleInRange = "onCollectibleInRange";

			private float m_shutdownSensorTime;
			private float m_timer;
			private object[] m_transitionParam;

			DragonPlayer m_owner;
			Transform m_ownerTransform;
			float m_range;
			PetSearchCollectibleTargetData m_data;


			public override StateComponentData CreateData() {
				return new PetSearchCollectibleTargetData();
			}

			public override System.Type GetDataType() {
				return typeof(PetSearchCollectibleTargetData);
			}

			protected override void OnInitialise() {
				m_timer = 0f;
				m_shutdownSensorTime = 0f;
				m_transitionParam = new object[1];
				base.OnInitialise();

				m_owner = InstanceManager.player;
				m_ownerTransform = m_owner.transform;
				m_data = m_pilot.GetComponentData<PetSearchCollectibleTargetData>();
				m_range = m_owner.data.maxScale * m_data.m_dragonSizeRangeMultiplier;
				m_range = m_range * m_range;
			}

			// The first element in _param must contain the amount of time without detecting an enemy
			protected override void OnEnter(State _oldState, object[] _param) {
				if (_param != null && _param.Length > 0 && _param[0] is float) {
					m_shutdownSensorTime = (float)_param[0];
				} else {
					m_shutdownSensorTime = 2.0f;
				}
                m_shutdownSensorTime =  Mathf.Max(2.0f, m_shutdownSensorTime);

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
					GameObject closestCollectible = GetClosestNotCollectedCollectible( m_data.m_type, centerPos);
					if ( closestCollectible != null )
					{
						Vector3 distance = m_ownerTransform.position - closestCollectible.transform.position;
						if (distance.sqrMagnitude < m_range)
						{
							m_transitionParam[0] = closestCollectible;
							Transition(OnCollectibleInRange, m_transitionParam);
						}
					}
				}
			}

			GameObject GetClosestNotCollectedCollectible( CollectibleType type, Vector3 centerPos )
			{
				GameObject closestObject = null;
				switch(type)
				{
					case CollectibleType.EGG:
					{
						if ( !CollectiblesManager.egg.collected )
							closestObject = CollectiblesManager.egg.gameObject;
					}break;
					case CollectibleType.CHEST:
					{
						CollectibleChest chest = CollectiblesManager.GetClosestActiveChest(centerPos);
						if (chest)
							closestObject = chest.gameObject;
					}break;
					case CollectibleType.LETTERS:
					{
						closestObject = InstanceManager.hungryLettersManager.GetClosestActiveLetter(centerPos);
					}break;
				}
				return closestObject;
			}

		}
	}
}