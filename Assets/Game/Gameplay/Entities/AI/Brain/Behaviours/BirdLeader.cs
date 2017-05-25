using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class BirdLeaderData : FollowLeaderData {}


		[CreateAssetMenu(menuName = "Behaviour/Bird Leader")]
		public class BirdLeader : FollowLeader {

			private Vector3 m_target;

			public override StateComponentData CreateData() {
				return new BirdLeaderData();
			}

			public override System.Type GetDataType() {
				return typeof(BirdLeaderData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<BirdLeaderData>();
			}

			protected override void OnEnter(State _oldState, object[] _param) {				                
				base.OnEnter(_oldState, _param);

				m_target = m_pilot.guideFunction.NextPositionAtSpeed(m_pilot.speed);
				m_pilot.GoTo(m_target);
			}
					
			protected override void OnUpdate() {
				// Update guide function
				if (m_pilot.guideFunction != null) {
					float dsqr = (m_pilot.target - m_machine.position).sqrMagnitude;
					float deltaDSqr = Mathf.Max(1f, m_pilot.speed * m_pilot.speed);
					if (dsqr < deltaDSqr) {
						m_target = m_pilot.guideFunction.NextPositionAtSpeed(m_pilot.speed);
					}
					m_pilot.GoTo(m_target);
				} else {
					m_pilot.GoTo(m_pilot.homePosition);
				}

				base.OnUpdate();
			}

			protected override void CheckPromotion() { }
		}
	}
}