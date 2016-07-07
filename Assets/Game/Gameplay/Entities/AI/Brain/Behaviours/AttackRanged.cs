using UnityEngine;
using System.Collections;
using AISM;

namespace AI {
	namespace Behaviour {	
		[System.Serializable]
		public class AttackRangedData : AttackData {
			public string m_projectileName = "";
			public string m_projectileSpawnTransformName = "";

			public float m_damage = 5f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack Ranged")]
		public class AttackRanged: Attack {

			private string m_projectileName = "PF_Arrow";
			private string m_projectileSpawnTransformName = "Human_R_Weapon_01";

			private GameObject m_projectile;
			private Transform m_projectileSpawnPoint;



			public override StateComponentData CreateData() {
				return new AttackRangedData();
			}

			protected override void OnInitialise(GameObject _go) {
				base.OnInitialise(_go);

				m_projectileSpawnPoint = m_machine.FindTransformRecursive(m_projectileSpawnTransformName);
			
				// create a projectile from resources (by name) and save it into pool
				GameObject projectilePrefab = Resources.Load<GameObject>("Game/Projectiles/" + m_projectileName);
				PoolManager.CreatePool(projectilePrefab, 2, true);
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
					m_projectile = PoolManager.GetInstance(m_projectileName);

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
					projectile.Shoot(m_projectileSpawnPoint, 5f);
					m_projectile = null;
				}
			}
		}
	}
}