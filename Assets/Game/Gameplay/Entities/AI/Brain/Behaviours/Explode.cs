﻿using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[System.Serializable]
		public class ExplodeData : StateComponentData {
			public float damage = 5f;
			public float cameraShakeDuration = 1.0f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Explode")]
		public class Explode : StateComponent {

			private ExplodeData m_data;


			public override StateComponentData CreateData() {
				return new ExplodeData();
			}

			public override System.Type GetDataType() {
				return typeof(ExplodeData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<ExplodeData>();
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				// explode
				if (_param != null && _param.Length > 0) {

					GameObject collider = (GameObject)_param[0];

					if (collider.CompareTag("Player")) {
						DragonPlayer dragon = InstanceManager.player;

						if ( !m_machine.IsDying() )
						{
							if (dragon.HasMineShield()) {
								dragon.LoseMineShield();
							} else {
								DragonHealthBehaviour health = dragon.GetComponent<DragonHealthBehaviour>();
								if ( health != null)
								{
									health.ReceiveDamage(m_data.damage, DamageType.NORMAL, m_machine.transform);
									if ( health.IsAlive() )
										Messenger.Broadcast<float, float>(GameEvents.CAMERA_SHAKE, m_data.cameraShakeDuration, 0);		
								}
							}

							DragonMotion dragonMotion = dragon.GetComponent<DragonMotion>();

							Vector3 knockBack = dragonMotion.transform.position - m_machine.position;
							knockBack.Normalize();

							knockBack *= Mathf.Log(Mathf.Max(dragonMotion.velocity.magnitude * m_data.damage, 2f));

							dragonMotion.AddForce(knockBack);
						}
					} else if ( collider.CompareTag("Pet") ){
						// is armored pet we should push it
					} else if (collider.layer == LayerMask.NameToLayer("GroundPreys")) {
						IMachine machine = collider.GetComponent<IMachine>();
						if (machine != null) {
							machine.Burn(m_machine.transform);
						}
					}
				}

				m_machine.SetSignal(Signals.Type.Destroyed, true);
			}
		}
	}
}