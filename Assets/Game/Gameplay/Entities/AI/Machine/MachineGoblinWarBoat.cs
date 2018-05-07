using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachineGoblinWarBoat : MachineAir, IArmored {
		[SeparatorAttribute("Targeting system")]
		[SerializeField] private Transform m_targetDummy;
		[SerializeField] private Transform m_cannonEye;

		[SeparatorAttribute("Armor")]
		[SerializeField] private HitsPerDragonTier m_armorDurabilityPerTier;

		[SeparatorAttribute("Ground")]
		[SerializeField] private Collider[] m_ground;


		public Transform targetDummy { get { return m_targetDummy; } }
		public Transform cannonEye { get { return m_cannonEye; } }


		private Hit m_armorDurability = new Hit();
		private DragonTier m_minTierToBreak;

		private Quaternion m_rotationCannon;



		protected override void Awake() {
			m_minTierToBreak = m_armorDurabilityPerTier.GetMinTier();

			base.Awake();
		}

		public override void Spawn(ISpawner _spawner) {
			DragonPlayer player = InstanceManager.player;
			DragonTier tier = player.GetTierWhenBreaking();

			Hit originalHits = m_armorDurabilityPerTier.Get(tier);
			m_armorDurability.count = originalHits.count;
			m_armorDurability.needBoost = originalHits.needBoost;

			for (int i = 0; i < m_ground.Length; ++i) {
				m_ground[i].isTrigger = false;
			}

			m_rotationCannon = Quaternion.LookRotation(Vector3.left, Vector3.forward);
			m_targetDummy.position = m_rotationCannon * (Vector3.forward * 5f);

			base.Spawn(_spawner);
		}

		public override void CustomUpdate() {
			base.CustomUpdate();

			if (!GetSignal(Signals.Type.Ranged)) {
				Quaternion lookRotation = Quaternion.identity;

				if (GetSignal(Signals.Type.Warning)) {
					Vector3 enemyDir = enemy.position - m_cannonEye.position;
					enemyDir.z = 0;
					enemyDir.Normalize();
					lookRotation = Quaternion.LookRotation(enemyDir, Vector3.forward);
				} else {
					if (direction.x <= 0) 	lookRotation = Quaternion.LookRotation(Vector3.left + Vector3.up * 0.01f, Vector3.forward);
					else					lookRotation = Quaternion.LookRotation(Vector3.right + Vector3.up * 0.01f, Vector3.forward);
				}

				m_cannonEye.rotation = Quaternion.RotateTowards(m_cannonEye.rotation, lookRotation, 60f * Time.smoothDeltaTime);
				m_targetDummy.position = m_cannonEye.position + (m_cannonEye.forward * 5f);
			}
		}

		public bool ReduceDurability(bool _boost, IEntity.Type _source) {
			if (m_armorDurability.count > 0 && !GetSignal(Signals.Type.Burning)) {
				if (m_armorDurability.count > 0) {
					if (!m_armorDurability.needBoost || _boost) {
						ReceiveHit();
						m_armorDurability.count--;
						if (m_armorDurability.count <= 0) {
							for (int i = 0; i < m_ground.Length; ++i) {
								m_ground[i].isTrigger = true;
							}
							m_viewControl.PlayExplosion();
							Smash(_source);
							/*
							SetSignal(Signals.Type.Destroyed, true);

							// Get the reward to be given from the entity
							Reward reward = m_entity.GetOnKillReward(false);
							InstanceManager.player.AddLife(InstanceManager.player.dragonHealthBehaviour.GetBoostedHp(reward.origin, reward.health), DamageType.NONE, m_transform);

							// Initialize some death info
							m_entity.onDieStatus.source = _source;
							// Dispatch global event
							Messenger.Broadcast<Transform, Reward>(MessengerEvents.ENTITY_DESTROYED, m_transform, reward);
							*/
						}

						return true;
					} else {
						// Message : You need boost!
						Messenger.Broadcast(MessengerEvents.BREAK_OBJECT_NEED_TURBO);
					}
				} else {
					Messenger.Broadcast<DragonTier, string>(MessengerEvents.BIGGER_DRAGON_NEEDED, m_minTierToBreak, m_entity.sku);
				}
			}

			return false;
		}

		public override bool CanBeBitten() {
			return false;
		}
	}
}