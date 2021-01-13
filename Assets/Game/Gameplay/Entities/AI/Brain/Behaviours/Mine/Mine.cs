﻿using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class MineData : StateComponentData {
			public float amplitude;
			public float frequency;
		}

		[CreateAssetMenu(menuName = "Behaviour/Mine")]
		public class Mine : StateComponent {

			private MineData m_data;

			private float m_time;
			private Vector3 m_originalPostion;



			public override StateComponentData CreateData() {
				return new MineData();
			}

			public override System.Type GetDataType() {
				return typeof(MineData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<MineData>();
				m_originalPostion = m_machine.position;
				m_pilot.SetDirection(Vector3.forward, true);
			}

			protected override void OnUpdate() {				
				//
				if (m_data.frequency > 0) {
					m_time += Time.deltaTime;

					Vector3 pos = m_originalPostion;
					pos.y += (Mathf.Cos(m_time / m_data.frequency) * m_data.amplitude);
					m_pilot.GoTo(pos);
				}
			}
		}
	}
}