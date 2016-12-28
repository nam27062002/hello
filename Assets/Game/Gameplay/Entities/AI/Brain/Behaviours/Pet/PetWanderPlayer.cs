using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		
		[System.Serializable]
		public class WanderPlayerData : StateComponentData {
			public float speed;
		}

		[CreateAssetMenu(menuName = "Behaviour/Pet/Wander Player")]
		public class PetWanderPlayer : StateComponent {

			private static int m_groundMask;

			private SphereCollider m_collider;
			private WanderPlayerData m_data;
			private Transform m_target;
			private Vector3 m_targetOffset;

			public override StateComponentData CreateData() {
				return new WanderPlayerData();
			}

			public override System.Type GetDataType() {
				return typeof(WanderPlayerData);
			}

			protected override void OnInitialise() {
				m_groundMask = LayerMask.GetMask("Ground", "GroundVisible", "PreyOnlyCollisions");
				m_data = m_pilot.GetComponentData<WanderPlayerData>();
				m_collider = m_pilot.GetComponent<SphereCollider>();
				m_target = m_machine.transform;
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				SelectTarget();
				m_pilot.SlowDown(true); // this wander state doesn't have an idle check
				m_pilot.SetMoveSpeed(m_data.speed); //TODO
			}

			protected override void OnUpdate() {
				Vector3 targetPos = m_target.position + m_targetOffset;

				if ((targetPos - m_pilot.transform.position).sqrMagnitude > 2)
				{
					m_pilot.SetMoveSpeed(m_data.speed); //TODO
					m_pilot.GoTo(targetPos);

					Debug.DrawLine(m_machine.position, targetPos, Colors.gold);
				}
				else
				{
					// Move target?
					m_pilot.SetMoveSpeed(0);
					if ( m_pilot.speed <= 0.1f )
					{
						m_pilot.SetDirection( m_target.forward );
					}
				}
			}

			private void SelectTarget() {
				m_target = m_pilot.homeTransform;	//  Get Pet position??
				// check collision
				RaycastHit groundHit;
				if (Physics.Linecast(m_machine.position, m_target.position, out groundHit, m_groundMask)) {
					m_targetOffset = groundHit.point - m_target.position;
					m_targetOffset = m_targetOffset.normalized * (m_targetOffset.magnitude + m_collider.radius);
				} else {
					m_targetOffset = Vector3.zero;
				}
			}
		}
	}
}