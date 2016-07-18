using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {	
		[System.Serializable]
		public class AttackRangedData : AttackData {
			public string projectileName = "";
			public string projectileSpawnTransformName = "";

			public float damage = 5f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack Ranged")]
		public class AttackRanged: Attack {
			
			private GameObject m_projectile;
			private Transform m_projectileSpawnPoint;


			public override StateComponentData CreateData() {
				return new AttackRangedData();
			}

			protected override void OnInitialise() {
				m_data = (AttackRangedData)m_pilot.GetComponentData<AttackRanged>();

				m_projectileSpawnPoint = m_machine.FindTransformRecursive(((AttackRangedData)m_data).projectileSpawnTransformName);
			
				// create a projectile from resources (by name) and save it into pool
				GameObject projectilePrefab = Resources.Load<GameObject>("Game/Projectiles/" + ((AttackRangedData)m_data).projectileName);
				PoolManager.CreatePool(projectilePrefab, 2, true);


				base.OnInitialise();
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				base.OnEnter(_oldState, _param);
				m_pilot.PressAction(Pilot.Action.Aim);
			}

			protected override void OnExit(State _newState) {
				base.OnExit(_newState);
				m_pilot.ReleaseAction(Pilot.Action.Aim);
				if (m_projectile != null) {
					m_projectile.SetActive(false);
					PoolManager.ReturnInstance(m_projectile);
					m_projectile = null;
				}
			}

			protected override void OnAttachProjectileExtended() {	
				if (m_projectile == null) {
					m_projectile = PoolManager.GetInstance(((AttackRangedData)m_data).projectileName);

					if (m_projectile != null) {
						ProjectileBehaviour projectile = m_projectile.GetComponent<ProjectileBehaviour>();
						projectile.AttachTo(m_projectileSpawnPoint);
					} else {
						Debug.LogError("Projectile not available");
					}
				}
			}

			protected override void OnAnimDealDamageExtended() {
				if (m_projectile != null) {					
					ProjectileBehaviour projectile = m_projectile.GetComponent<ProjectileBehaviour>();
					projectile.Shoot(m_projectileSpawnPoint, ((AttackRangedData)m_data).damage);
					m_projectile = null;
				}
			}
		}
	}
}