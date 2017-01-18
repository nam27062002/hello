using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[System.Serializable]
		public class ExplodeData : StateComponentData {
			public float damage = 5f;
			public float radius = 0f;
			public float cameraShakeDuration = 1.0f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Explode")]
		public class Explode : StateComponent {

			private ExplodeData m_data;
			private float m_playerRadius;


			public override StateComponentData CreateData() {
				return new ExplodeData();
			}

			public override System.Type GetDataType() {
				return typeof(ExplodeData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<ExplodeData>();

				SphereCollider sc = InstanceManager.player.GetComponentInChildren<SphereCollider>();
				m_playerRadius = sc.radius;
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				// explode
				bool hasPlayerReceivedDamage = false;
				DragonPlayer dragon = InstanceManager.player;

				if (_param != null && _param.Length > 0) {
					GameObject collider = (GameObject)_param[0];

					if (collider.CompareTag("Player")) {
						hasPlayerReceivedDamage = true;
					} else if ( collider.CompareTag("Pet") ){
						// is armored pet we should push it
					} else if (collider.layer == LayerMask.NameToLayer("GroundPreys")) {
						IMachine machine = collider.GetComponent<IMachine>();
						if (machine != null) {
							machine.Burn(m_machine.transform);
						}
					}
				}

				if (!hasPlayerReceivedDamage && m_data.radius > 0f) {
					float dSqr = (dragon.transform.position - m_machine.position).sqrMagnitude;
					float rSqr = (m_data.radius * m_data.radius) + (m_playerRadius * m_playerRadius);

					Debug.Log("EXPLODE: d: " + dSqr + " | " + rSqr);

					hasPlayerReceivedDamage = (dSqr <= rSqr);
				}

				if (hasPlayerReceivedDamage) {
					if (!m_machine.IsDying()) {
						if (dragon.HasShield( DamageType.MINE )) {
							dragon.LoseShield(DamageType.MINE);
						} else {
							DragonHealthBehaviour health = dragon.GetComponent<DragonHealthBehaviour>();
							if (health != null) {
								health.ReceiveDamage(m_data.damage, DamageType.MINE, m_machine.transform);
								if (health.IsAlive())
									Messenger.Broadcast<float, float>(GameEvents.CAMERA_SHAKE, m_data.cameraShakeDuration, 0);		

								DragonMotion dragonMotion = dragon.GetComponent<DragonMotion>();

								Vector3 knockBack = dragonMotion.transform.position - m_machine.position;
								knockBack.Normalize();

								knockBack *= Mathf.Log(Mathf.Max(dragonMotion.velocity.magnitude * m_data.damage, 2f));

								dragonMotion.AddForce(knockBack);
							}
						}
					}
				}

				m_machine.SetSignal(Signals.Type.Destroyed, true);
			}
		}
	}
}