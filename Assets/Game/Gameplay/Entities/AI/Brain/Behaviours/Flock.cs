using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[System.Serializable]
		public class FlockData : StateComponentData {			
			public float separation;
			public float frequency = 1f;
			public float amplitude = 0f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Flock")]
		public class Flock : StateComponent {

			private FlockData m_data;

			private float m_updateOffsetTimer;
			private Vector3 m_offset;
			private float m_phase;

			private bool m_changeFormationOrientation;

			public override StateComponentData CreateData() {
				return new FlockData();
			}

			public override System.Type GetDataType() {
				return typeof(FlockData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<FlockData>();
				m_changeFormationOrientation = false;

				Group group = m_machine.GetGroup();

				if (group != null && group.HasOffsets()) {
					m_offset = group.GetOffset(m_machine, m_data.separation);
				} else {
					m_offset = UnityEngine.Random.insideUnitSphere * m_data.separation;
				}

				m_machine.position = m_pilot.homePosition + m_offset;
			}

			protected override void OnEnter(State oldState, object[] param) {				
				m_updateOffsetTimer = 0f;

				Group group = m_machine.GetGroup();

				if (group != null && group.HasOffsets()) {
					m_offset = group.GetOffset(m_machine, m_data.separation);
					float phaseOffset = Mathf.PI * 2f / group.count;
					m_phase = phaseOffset * group.GetIndex(m_machine);
				} else {
					m_offset = UnityEngine.Random.insideUnitSphere * m_data.separation;
					m_phase = 0f;
				}


				m_changeFormationOrientation = group != null && (group.formation == Group.Formation.Triangle);
			}

			protected override void OnUpdate() {
                Group group = m_machine.GetGroup();

				// Every few seconds we change the leader of this flock
				if (group != null) {					
					m_updateOffsetTimer -= Time.deltaTime;
					if (m_updateOffsetTimer <= 0f) {
						m_offset = group.GetOffset(m_pilot.m_machine, m_data.separation);
						m_updateOffsetTimer = 2.5f;
					}
				}

				if (m_changeFormationOrientation) {
					if (m_machine.GetSignal(Signals.Type.Leader)) {
						group.UpdateRotation(m_pilot.direction);
					}
					m_offset = group.GetOffset(m_pilot.m_machine, m_data.separation);
				}

				// add variation to movement
				Vector3 offset = m_offset;
				if (/*!m_machine.GetSignal(Signals.Type.Leader) &&*/ m_data.amplitude > 0) {					
					offset += m_data.amplitude * Mathf.Cos(m_data.frequency * Time.timeSinceLevelLoad + m_phase) * m_machine.upVector;
				}

				m_pilot.GoTo(m_pilot.target + offset);
            }
		}
	}
}