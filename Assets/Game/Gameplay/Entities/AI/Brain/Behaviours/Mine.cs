using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class MineData : StateComponentData {
			public float m_amplitude;
			public float m_frequency;
		}

		[CreateAssetMenu(menuName = "Behaviour/Mine")]
		public class Mine : StateComponent {
			[SerializeField] private float m_amplitude = 0f;
			[SerializeField] private float m_frequency = 0f;

			private float m_time;
			private Vector3 m_originalPostion;



			public override StateComponentData CreateData() {
				return new MineData();
			}

			protected override void OnInitialise() {
				m_originalPostion = m_machine.position;
			}

			protected override void OnUpdate() {				
				//
				if (m_frequency > 0) {
					m_time += Time.deltaTime;

					Vector3 pos = m_originalPostion;
					pos.y += (Mathf.Cos(m_time / m_frequency) * m_amplitude);
					m_pilot.GoTo(pos);
				}
			}
		}
	}
}