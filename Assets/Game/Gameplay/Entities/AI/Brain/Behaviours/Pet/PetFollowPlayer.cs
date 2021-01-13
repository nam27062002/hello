using UnityEngine;

namespace AI {
	namespace Behaviour {
		
		[System.Serializable]
		public class FollowPlayerData : StateComponentData {			
			public string petWanderSku = "common";
		}

		[CreateAssetMenu(menuName = "Behaviour/Pet/Follow Player")]
		public class PetFollowPlayer : StateComponent {

            private Transform dragonTransform;

			private SphereCollider m_collider;
			private Transform m_target;			
			private float m_speed;

			private float m_maxFarDistance;
			

			public override StateComponentData CreateData() {
                return new FollowPlayerData();
			}

			public override System.Type GetDataType() {
                return typeof(FollowPlayerData);
			}

			protected override void OnInitialise() {
                Equipable equipable = m_pilot.GetComponent<Equipable>();

                DragonPlayer dragon = InstanceManager.player;
                DragonEquip equip = dragon.GetComponent<DragonEquip>();

                m_target = equip.GetAttachPoint(equipable.attachPoint).transform;
                dragonTransform = dragon.transform;

                FollowPlayerData data = m_pilot.GetComponentData<FollowPlayerData>();
				m_collider = m_pilot.GetComponent<SphereCollider>();
				
				DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PET_MOVEMENT, data.petWanderSku);
                m_speed = dragon.dragonMotion.absoluteMaxSpeed * def.GetAsFloat("wanderSpeedMultiplier");

                m_maxFarDistance = dragon.data.maxScale;
			}

			protected override void OnEnter(State _oldState, object[] _param) {				
				m_pilot.SlowDown(false); // this wander state doesn't have an idle check
				m_pilot.SetMoveSpeed(m_speed); //TODO
			}

			protected override void OnUpdate() {
                Vector3 dir = m_target.position - dragonTransform.position;
                dir.Normalize();

                Vector3 targetPos = m_target.position + dir * m_maxFarDistance;				
				m_pilot.GoTo(targetPos);

                float magnitude = (targetPos - m_machine.position).sqrMagnitude;
				m_pilot.SetMoveSpeed(Mathf.Min( m_speed, magnitude));					
				if (m_pilot.speed <= 0.1f) {
					m_pilot.SlowDown(true);
					m_pilot.SetDirection( m_target.forward );
				}
				else
				{
					m_pilot.SlowDown(false);
				}

				Debug.DrawLine(m_machine.position, targetPos, Colors.gold);
			}
		}
	}
}