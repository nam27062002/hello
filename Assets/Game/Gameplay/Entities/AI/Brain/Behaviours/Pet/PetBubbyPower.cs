using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
        [System.Serializable]
        public class PetBubbyPowerData : StateComponentData {
            public float powerDelay = 0.5f;
            public float bubbleStunTime = 2f;
            public IEntity.Tag ignoreTag = 0;
        }


        [CreateAssetMenu(menuName = "Behaviour/Pet/Bubby Power")]
		public class PetBubbyPower : StateComponent {

			[StateTransitionTrigger]
			public const string onBubbyPowerEnd = "onBubbyPowerEnd";

            protected PetBubbyPowerData m_data;
            private Entity[] m_entities;
            private float m_timer;


            //------------------------------------------------------------------------

            public override StateComponentData CreateData() {
                return new PetBubbyPowerData();
            }

            public override System.Type GetDataType() {
                return typeof(PetBubbyPowerData);
            }

            protected override void OnInitialise() {
                m_data = m_pilot.GetComponentData<PetBubbyPowerData>();
                m_entities = new Entity[255];
            }

            protected override void OnEnter(State _oldState, object[] _param) {
                m_pilot.PressAction(Pilot.Action.Button_A);
                m_timer = m_data.powerDelay;
            }

            protected override void OnExit(State _newState) {
                m_pilot.ReleaseAction(Pilot.Action.Button_A);
            }

            protected override void OnUpdate() {
                m_pilot.SlowDown(true);
                m_pilot.SetDirection(Vector3.forward, true);

                m_timer -= Time.deltaTime;
                if (m_timer <= 0f) {
                    int entityCount = EntityManager.instance.GetOnScreenEntities(m_entities);

                    for (int i = 0; i < entityCount; ++i) {
                        if (!m_entities[i].HasTag(m_data.ignoreTag)) {
                            BubbledEntitySystem.AddEntity(m_entities[i], m_data.bubbleStunTime);
                        }
                        m_entities[i] = null; // we don't want to keep this reference
                    }

                    Transition(onBubbyPowerEnd);
                }
			}

		}
	}
}