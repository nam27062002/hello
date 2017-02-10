using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class BirdLeaderData : FollowLeaderData {}


		[CreateAssetMenu(menuName = "Behaviour/Bird Leader")]
		public class BirdLeader : FollowLeader {

			public override StateComponentData CreateData() {
				return new BirdLeaderData();
			}

			public override System.Type GetDataType() {
				return typeof(BirdLeaderData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<BirdLeaderData>();
			}
					
			protected override void OnUpdate() {
				// Update guide function
				if (m_pilot.guideFunction != null) {
					Vector3 target = m_pilot.guideFunction.NextPositionAtSpeed(m_pilot.speed);
					m_pilot.GoTo(target);
				} else {
					m_pilot.GoTo(m_pilot.homePosition);
				}

				base.OnUpdate();
			}
		}
	}
}