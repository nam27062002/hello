using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
        [System.Serializable]
        public class PetBubbyPowerData : StateComponentData {
            public float powerDelay = 0.5f;
            public float bubbleStunTime = 2f;
            public IEntity.Tag ignoreTag = 0;
            public string onBubbleExplodeAudio;
        }


        [CreateAssetMenu(menuName = "Behaviour/Pet/Bubby Power")]
		public class PetBubbyPower : StateComponent, IBroadcastListener {

			[StateTransitionTrigger]
			public static readonly int onBubbyPowerEnd = UnityEngine.Animator.StringToHash("onBubbyPowerEnd");

            protected PetBubbyPowerData m_data;
            private Entity[] m_entities;
            private float m_timer;

            private bool m_audioAvailable;

            private ParticleHandler m_effectHandler;

            DragonTier m_tierCheck;

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

                m_audioAvailable = !string.IsNullOrEmpty(m_data.onBubbleExplodeAudio);

                CreatePool();

                // create a projectile from resources (by name) and save it into pool
                Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
                Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);

                m_tierCheck = InstanceManager.player.data.tier;
            }

            void CreatePool() {
                m_effectHandler = ParticleManager.CreatePool("FX_SeaHorsePower");
            }

            protected override void OnRemove() {
                base.OnRemove();
                Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
                Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);
            }

            public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo) {
                switch (eventType) {
                    case BroadcastEventType.GAME_LEVEL_LOADED:
                    case BroadcastEventType.GAME_AREA_ENTER: {
                            CreatePool();
                        }
                        break;
                }
            }

            protected override void OnEnter(State _oldState, object[] _param) {
                m_pilot.PressAction(Pilot.Action.Button_A);
                m_timer = m_data.powerDelay;

                m_effectHandler.Spawn(null, m_machine.transform);   
            }

            protected override void OnExit(State _newState) {
                m_pilot.ReleaseAction(Pilot.Action.Button_A);
            }

            protected override void OnUpdate() {
                m_pilot.SlowDown(true);
                m_pilot.SetDirection(Vector3.forward, true);

                m_timer -= Time.deltaTime;
                if (m_timer <= 0f) {
                    bool playSound = false;
                    int entityCount = EntityManager.instance.GetOnScreenEntities(m_entities);

                    for (int i = 0; i < entityCount; ++i) {
                            // Check if it can be eaten by the player
                        if (!m_entities[i].HasTag(m_data.ignoreTag) && ( m_entities[i].IsEdible(m_tierCheck) || m_entities[i].CanBeHolded(m_tierCheck))) {
                            playSound = true; 
                            BubbledEntitySystem.AddEntity(m_entities[i], m_data.bubbleStunTime);
                        }
                        m_entities[i] = null; // we don't want to keep this reference
                    }

                    if (playSound && m_audioAvailable) {
                        AudioController.Play(m_data.onBubbleExplodeAudio, m_machine.position);
                    }

                    Transition(onBubbyPowerEnd);
                }
			}

		}
	}
}