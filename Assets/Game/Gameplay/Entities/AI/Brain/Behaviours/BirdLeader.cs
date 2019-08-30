using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class BirdLeaderData : FollowLeaderData {}


		[CreateAssetMenu(menuName = "Behaviour/Bird Leader")]
		public class BirdLeader : FollowLeader {

			private Vector3 m_target;
			private float m_timer; // change target when time is over

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

				m_pilot.SetMoveSpeed(m_data.speed);
				SelectTarget();
				m_pilot.GoTo(m_target);
			}
					
			protected override void OnUpdate() {
                // Update guide function
                if (m_pilot.guideFunction != null || m_pilot.area != null) {
					float dsqr = (m_pilot.target - m_machine.position).sqrMagnitude;
					float deltaDSqr = Mathf.Max(1f, m_pilot.speed * m_pilot.speed);

					m_timer -= Time.deltaTime;

					if (m_timer <= 0f || dsqr < deltaDSqr) {
						SelectTarget();
					}
					m_pilot.GoTo(m_target);
				} else {
					m_pilot.GoTo(m_pilot.homePosition);
				}

				base.OnUpdate();
            }

			protected override void CheckPromotion() { }

			private void SelectTarget() {
                if (m_pilot.guideFunction != null) {
					m_target = m_pilot.guideFunction.NextPositionAtSpeed(m_pilot.speed);					
				} else {
					m_target = m_pilot.area.RandomInside();
				}

				if (m_pilot.IsActionPressed(Pilot.Action.Avoid)) {
					m_timer = 0.25f;
				} else if (m_data.speed > 0f) {
					m_timer = (m_machine.position - m_target).magnitude / Mathf.Max(m_pilot.speed, m_data.speed);
				} else {
					m_timer = 1f;
				}
            }
		}
	}
}