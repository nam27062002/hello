using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[System.Serializable]
		public class ExplodeData : StateComponentData {
			public bool isMine = true;
			public float damage = 5f;
			public float radius = 0f;
			public float cameraShakeDuration = 1.0f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Explode")]
		public class Explode : StateComponent {

			private ExplodeData m_data;
			private Explosive m_explosive;
			private Entity m_entity;

			public override StateComponentData CreateData() {
				return new ExplodeData();
			}

			public override System.Type GetDataType() {
				return typeof(ExplodeData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<ExplodeData>();
				m_explosive = new Explosive(m_data.isMine, m_data.damage, m_data.radius, m_data.cameraShakeDuration);
				m_entity = m_machine.GetComponent<Entity>();
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				// explode
				bool playerTriggeredExplosion = false;
				DragonPlayer dragon = InstanceManager.player;

				m_entity.onDieStatus.source = IEntity.Type.OTHER;

				if (m_data.damage > 0) {
					if (_param != null && _param.Length > 0) {
						if (_param[0] is GameObject) {
							GameObject collider = (GameObject)_param[0];

							if (collider.CompareTag("Player")) {
								playerTriggeredExplosion = true;
								m_entity.onDieStatus.source = IEntity.Type.PLAYER;
							} else if ( collider.CompareTag("Pet") ){
								// is armored pet we should push it
								Pet pet = collider.GetComponent<Pet>();
								if ( pet != null && !pet.CanExplodeMines)
									return;
								// Check powerup is explode_mine
								m_entity.onDieStatus.source = IEntity.Type.PET;
							} else if (collider.layer == LayerMask.NameToLayer("GroundPreys")) {
								IMachine machine = collider.GetComponent<IMachine>();
								if (machine != null) {
									machine.Burn(m_machine.transform, IEntity.Type.OTHER);
								}
							}
						}
					}

					if (!m_machine.IsDying()) {
						m_explosive.Explode(m_machine.transform, 2f, playerTriggeredExplosion);
					}
				}

				m_machine.SetSignal(Signals.Type.Destroyed, true);

				// [AOC] Notify game!
				Reward reward = m_entity.reward;
				reward.SetNoReward();

                Messenger.Broadcast<Transform, IEntity, Reward, KillType>(MessengerEvents.ENTITY_KILLED, m_machine.transform, m_entity, reward, KillType.HIT);
                
                if ( playerTriggeredExplosion && !m_machine.IsDying())
                {
                    InstanceManager.timeScaleController.HitStop();
                }

			}
		}
	}
}