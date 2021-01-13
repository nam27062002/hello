using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbledEntitySystem : Singleton<BubbledEntitySystem>, IBroadcastListener {
    //--[Classes]------------------------------------------------------------------------------------------
    private class QueueSystem {
        private int m_count = 0;

        private List<IEntity> m_entities = new List<IEntity>();
        private List<float> m_timers = new List<float>();

        public void Add(IEntity _entity, float _time) {
            m_entities.Add(_entity);
            m_timers.Add(_time);

            m_count++;
        }

        public void Remove(IEntity _entity) {
            int index = m_entities.IndexOf(_entity);
            if (index >= 0) {
                m_entities.RemoveAt(index);
                m_timers.RemoveAt(index);

                m_count--;
            }
        }

        public void Process(ActiveSystem _activeSystem) {
            for (int i = 0; i < m_count; i++) {
                _activeSystem.Add(m_entities[i], m_timers[i]);
            }
            m_entities.Clear();
            m_timers.Clear();
            m_count = 0;
        }
    }

    private class ActiveSystem {
        private ParticleHandler m_bubbleHandler;
        private ParticleHandler m_splashHandler;

        private int m_count = 0;
        private List<IEntity> m_entities = new List<IEntity>();
        private List<float> m_timers = new List<float>();

        private List<Transform> m_bubbles = new List<Transform>();
        private List<float> m_bubblesScale = new List<float>();
        private List<float> m_bubblesSpeed = new List<float>();
        private List<float> m_bubblesTimer = new List<float>();


        public ActiveSystem() {
            m_bubbleHandler = ParticleManager.CreatePool("FX_BubbleNPC");
            m_splashHandler = ParticleManager.CreatePool("FX_BubbleSplashNPC");
        }

        public void Add(IEntity _entity, float _time) {
            AI.Machine machine = _entity.machine as AI.Machine;

            if (machine != null) {
                machine.Bubbled(true);

                float bubbleScale = 1f;
                Vector3 center = machine.position;
                if (_entity.circleArea != null) {
                    center = _entity.circleArea.center;
                    bubbleScale = _entity.circleArea.radius / 0.5f;
                }

                m_splashHandler.Spawn(null, center);

                GameObject bubbleGO = m_bubbleHandler.Spawn(null, machine.transform);//, center - machine.position);
                Transform bubble = null;
                if (bubbleGO != null) {
                    bubble = bubbleGO.transform;
                    bubble.localScale = GameConstants.Vector3.zero;
                }

                m_entities.Insert(0, _entity);
                m_timers.Insert(0, _time);

                m_bubbles.Insert(0, bubble);
                m_bubblesScale.Insert(0, bubbleScale);
                m_bubblesSpeed.Insert(0, bubbleScale);
                m_bubblesTimer.Insert(0, 0f);

                m_count++;
            }
        }

        public void Remove(IEntity _entity) {
            int index = m_entities.IndexOf(_entity);
            if (index >= 0) {
                m_timers[index] = 0f;
            }
        }

        public void Process(EndSystem _endSystem) {
            float dt = Time.deltaTime;
            for (int i = 0; i < m_count; i++) {
                m_timers[i] -= dt;
            }

            for (int i = m_count - 1; i >= 0; i--) {
                if (m_timers[i] <= 0) {
                    _endSystem.Add(m_entities[i], m_bubbles[i]);

                    m_entities.RemoveAt(i);
                    m_timers.RemoveAt(i);

                    m_bubbles.RemoveAt(i);
                    m_bubblesScale.RemoveAt(i);
                    m_bubblesSpeed.RemoveAt(i);
                    m_bubblesTimer.RemoveAt(i);

                    m_count--;
                }
            }



            for (int i = 0; i < m_count; i++) {
                m_bubblesTimer[i] += dt * 4f * m_bubblesSpeed[i]; //speed up things a little
            }

            for (int i = 0; i < m_count; i++) {
                if (m_bubbles[i] != null) {
                    m_bubbles[i].localScale = GameConstants.Vector3.one * Mathf.Lerp(0f, m_bubblesScale[i], m_bubblesTimer[i]);
                }
            }          
        }
    }

    private class EndSystem {
        private ParticleHandler m_bubbleHandler;
        private ParticleHandler m_splashHandler;

        private int m_count = 0;
        private List<IEntity> m_entities = new List<IEntity>();

        private List<Transform> m_bubbles = new List<Transform>();
        private List<float> m_bubblesScale = new List<float>();
        private List<float> m_bubblesSpeed = new List<float>();
        private List<float> m_bubblesTimer = new List<float>();

        public EndSystem() {
            m_bubbleHandler = ParticleManager.CreatePool("FX_BubbleNPC");
            m_splashHandler = ParticleManager.CreatePool("FX_BubbleSplashNPC");
        }

        public void Add(IEntity _entity, Transform _bubble) {
            Vector3 center = _entity.machine.position;
            if (_entity.circleArea != null) center = _entity.circleArea.center;

            m_splashHandler.Spawn(null, center);

            m_entities.Insert(0, _entity);


            float bubbleScale = 1f;
            if (_bubble != null) {
                bubbleScale = _bubble.localScale.x;
            }
            m_bubbles.Insert(0, _bubble);
            m_bubblesScale.Insert(0, bubbleScale);
            m_bubblesSpeed.Insert(0, bubbleScale);
            m_bubblesTimer.Insert(0, 0f);

            m_count++;
        }

        public void Process() {
            for (int i = m_count - 1; i >= 0; i--) {
                if (m_bubblesTimer[i] >= 1f) {
                    AI.Machine machine = m_entities[i].machine as AI.Machine;
                    machine.Bubbled(false);

                    if (m_bubbles[i] != null) {
                        m_bubbleHandler.ReturnInstance(m_bubbles[i].gameObject);
                    }

                    m_entities.RemoveAt(i);

                    m_bubbles.RemoveAt(i);
                    m_bubblesScale.RemoveAt(i);
                    m_bubblesSpeed.RemoveAt(i);
                    m_bubblesTimer.RemoveAt(i);

                    m_count--;
                }
            }

            float dt = Time.deltaTime;

            for (int i = 0; i < m_count; i++) {
                m_bubblesTimer[i] += dt * 6f * m_bubblesSpeed[i]; //speed up things a little bit more
            }

            for (int i = 0; i < m_count; i++) {
                if (m_bubbles[i] != null) {
                    m_bubbles[i].localScale = GameConstants.Vector3.one * Mathf.Lerp(m_bubblesScale[i], 0f , m_bubblesTimer[i]);
                }
            }
        }
    }



    //--[Attributes]---------------------------------------------------------------------------------------

    private List<IEntity> m_deadEntities = new List<IEntity>();

    private QueueSystem m_queueSystem;
    private ActiveSystem m_activeSystem;
    private EndSystem m_endSystem;

    private bool m_loaded;


    //--[Static methods]-----------------------------------------------------------------------------------
    public static void AddEntity(IEntity _entity, float _time) {
        if (_entity.machine != null) {
            if (!_entity.machine.IsBubbled() && !_entity.machine.IsDying() && !FreezingObjectsRegistry.instance.IsFreezing(_entity)) {
                m_instance.EnqueueEntity(_entity, _time);
            }
        }
    }

    public static void RemoveEntity(IEntity _entity) {
        m_instance.m_deadEntities.Add(_entity);
    }


    //--[Instance methods]---------------------------------------------------------------------------------
    protected override void OnCreateInstance() {
        m_loaded = false;
    
        // Subscribe to external events
        Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
        Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);
        Broadcaster.AddListener(BroadcastEventType.GAME_AREA_EXIT, this);
        Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
    }

    protected override void OnDestroyInstance() {
        // Unsubscribe from external events
        Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
        Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);
        Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_EXIT, this);
        Broadcaster.RemoveListener(BroadcastEventType.GAME_ENDED, this);
    }


    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo) {
        switch (eventType) {
            case BroadcastEventType.GAME_LEVEL_LOADED:
            case BroadcastEventType.GAME_AREA_ENTER: {
                    m_queueSystem = new QueueSystem();
                    m_activeSystem = new ActiveSystem();
                    m_endSystem = new EndSystem();

                    m_loaded = true;
                }
                break;

            case BroadcastEventType.GAME_ENDED:
            case BroadcastEventType.GAME_AREA_EXIT: {
                    m_loaded = false;

                    m_queueSystem = null;
                    m_activeSystem = null;
                    m_endSystem = null;
                }
                break;
        }
    }


    private void EnqueueEntity(IEntity _entity, float _time) {
        m_queueSystem.Add(_entity, _time);
    }

    public void Update() {
        if (m_loaded) {
            for (int i = 0; i < m_deadEntities.Count; ++i) {
                m_queueSystem.Remove(m_deadEntities[i]);
                m_activeSystem.Remove(m_deadEntities[i]);
            }
            m_deadEntities.Clear();

            m_queueSystem.Process(m_activeSystem);
            m_activeSystem.Process(m_endSystem);
            m_endSystem.Process();
        }
    }
}
