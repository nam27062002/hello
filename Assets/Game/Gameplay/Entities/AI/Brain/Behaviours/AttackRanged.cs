using UnityEngine;
using System.Collections;
using AISM;

namespace AI {
	namespace Behaviour {		
		[CreateAssetMenu(menuName = "Behaviour/Attack Ranged")]
		public class AttackRanged: Attack {

			private string m_projectileName = "PF_Arrow";
			private Transform m_projectileSpawnPoint;

			private GameObject m_projectile;


			protected override void OnInitialise(GameObject _go) {
				base.OnInitialise(_go);

				m_projectileSpawnPoint = m_machine.transform.FindChild("Human_R_Weapon_01"); //TODO

				// create a projectile from resources (by name) and save it into pool
				// PoolManager.CreatePool(m_projectilePrefab, 2, true);
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				base.OnEnter(_oldState, _param);
				m_pilot.PressAction(Pilot.Action.Aim);
			}

			protected override void OnExit(State _newState) {
				base.OnExit(_newState);
				m_pilot.ReleaseAction(Pilot.Action.Aim);
			}

			protected override void OnAttachProjectileExtended() {	
				/*if (m_projectile == null) {
					m_projectile = GameObject.Instantiate(Resources.Load<GameObject>("Game/Projectiles/"+m_projectileName));
					//m_projectile = PoolManager.GetInstance(m_projectilePrefab.name);

					if (m_projectile != null) {
						ProjectileBehaviour projectile = m_projectile.GetComponent<ProjectileBehaviour>();
						projectile.AttachTo(m_projectileSpawnPoint);
					} else {
						Debug.LogError("Projectile not available");
					}
				}*/
			}

			protected override void OnAnimDealDamageExtended() {
				/*if (m_projectile != null) {					
					ProjectileBehaviour projectile = m_projectile.GetComponent<ProjectileBehaviour>();
					projectile.Shoot(m_machine.transform, 5f);
					m_projectile = null;
				}*/
			}
		}
	}
}