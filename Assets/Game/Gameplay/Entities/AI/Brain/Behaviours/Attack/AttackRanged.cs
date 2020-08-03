using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {	
		[System.Serializable]
		public class AttackRangedData : AttackData {
			public string projectileName = "";
			public string projectileSpawnTransformName = "";

			public float damage = 5f;
			public bool canFollowTarget = false;
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Ranged")]
		public class AttackRanged: Attack, IBroadcastListener {
			
			private GameObject m_projectile;
			private Transform m_projectileSpawnPoint;
			

			private PoolHandler m_poolHandler;

			public override StateComponentData CreateData() {
				return new AttackRangedData();
			}

			public override System.Type GetDataType() {
				return typeof(AttackRangedData);
			}

			protected override void OnInitialise() {
                if ( m_data == null )
                    m_data = m_pilot.GetComponentData<AttackRangedData>();

				m_projectileSpawnPoint = m_pilot.FindTransformRecursive(((AttackRangedData)m_data).projectileSpawnTransformName);
			
				CreatePool();

                // create a projectile from resources (by name) and save it into pool
                Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
				Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);
                
				base.OnInitialise();
			}

			void CreatePool() {
				m_poolHandler = PoolManager.CreatePool(((AttackRangedData)m_data).projectileName, 4, true);
			}

			protected override void OnRemove() {
                base.OnRemove();
                Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
                Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);
			}

            public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
            {
                switch( eventType )
                {
                    case BroadcastEventType.GAME_LEVEL_LOADED:
                    case BroadcastEventType.GAME_AREA_ENTER:
                    {
                        CreatePool();
                    }break;
                }
            }
    
			protected override void StartAttack() 
			{
				base.StartAttack();
				if (m_data.forceFaceToShoot && m_viewControl != null) {
					// Tell view position to attack
					(m_viewControl as ViewControl).attackTargetPosition = m_facingTarget;
				}
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				base.OnEnter(_oldState, _param);
				m_pilot.PressAction(Pilot.Action.Aim);

				m_machine.SetSignal(Signals.Type.Ranged, true);
			}

			protected override void OnExit(State _newState) {
				base.OnExit(_newState);
				m_pilot.ReleaseAction(Pilot.Action.Aim);
				if (m_projectile != null) {
					m_projectile.SetActive(false);
					m_poolHandler.ReturnInstance(m_projectile);
					m_projectile = null;
				}

				m_machine.SetSignal(Signals.Type.Ranged, false);
			}

			protected override void OnAttachProjectileExtended() {	
				if (m_projectile == null) {
					m_projectile = m_poolHandler.GetInstance();

					if (m_projectile != null) {
						IProjectile projectile = m_projectile.GetComponent<IProjectile>();
						projectile.AttachTo(m_projectileSpawnPoint);
					} else {
						Debug.LogError("Projectile not available");
					}
				}
			}

			protected override void OnAnimDealDamageExtended() {
				if (m_projectile != null) {					
					IProjectile projectile = m_projectile.GetComponent<IProjectile>();
					if ((m_data.forceFaceToShoot && !((AttackRangedData)m_data).canFollowTarget) || (m_machine.enemy == null)) {
						projectile.ShootAtPosition(m_facingTarget, m_machine.transform.forward, ((AttackRangedData)m_data).damage, m_machine.transform);
					} else {    
                        projectile.Shoot(m_machine.enemy , m_machine.transform.forward, ((AttackRangedData)m_data).damage, m_machine.transform);
					}
					m_projectile = null;
				}
			}
		}
	}
}