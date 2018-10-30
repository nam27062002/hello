using System;
using UnityEngine;

public class PetParcaeWeapon : PetMeleeWeapon, IBroadcastListener {
    //--------------------------------------------------------------------------
    [Serializable]
    public class ModData {
        public float hpPercentage;
        public float xpPercentage;
        public float coinsPercentage;
    }

    [Serializable]
    public class ParcaeModifierDictionary : SerializableDictionary<string, ModData> { }

    [SerializeField] private ParcaeModifierDictionary m_modifiers;
    [SerializeField] private float m_modTime = 5f;

    [SerializeField] private PetParcaeViewControl m_view;


    //--------------------------------------------------------------------------
    private ModData m_currentMod = null;
    private ModDragonLifeGain m_modHP = null;
    private ModEntityXP m_modXP = null;
    private ModEntitySC m_modSC = null;

    private float m_modsTimer;


    //--------------------------------------------------------------------------
    protected override void OnAwake() {
        base.OnAwake();

        m_currentMod = null;
        m_modsTimer = 0f;

        Broadcaster.AddListener(BroadcastEventType.GAME_AREA_EXIT, this);
        Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
    }

    private void OnDestroy() {
        Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_EXIT, this);
        Broadcaster.RemoveListener(BroadcastEventType.GAME_ENDED, this);
    }
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch(eventType)
        {
            case BroadcastEventType.GAME_AREA_EXIT:
            case BroadcastEventType.GAME_ENDED:
            {
                RemoveMods();
            }break;
        }
    }

    protected override void Update() {
        if (m_modsTimer > 0f) {
            m_modsTimer -= Time.deltaTime;
            if (m_modsTimer <= 0f) {
                RemoveMods();
            }
        }
    }

    protected override void OnEntityKilled(Entity _e) {
        ModData nextMod = m_modifiers.Get(_e.sku);

        if (nextMod != null) {
            if (nextMod != m_currentMod) {
                RemoveMods();
                ApplyMod(nextMod);

                m_view.SetColor(_e.sku);
            }
            m_modsTimer = m_modTime;
        }
    }

    private void ApplyMod(ModData _data) {
        m_modHP = new ModDragonLifeGain(_data.hpPercentage);
        m_modXP = new ModEntityXP(_data.xpPercentage);
        m_modSC = new ModEntitySC(_data.coinsPercentage);

        m_modHP.Apply();
        m_modXP.Apply();
        m_modSC.Apply();

        m_currentMod = _data;

        Messenger.Broadcast(MessengerEvents.APPLY_ENTITY_POWERUPS);
    }

    private void RemoveMods() {
        if (m_currentMod != null) {
            m_modHP.Remove();
            m_modXP.Remove();
            m_modSC.Remove();

            m_currentMod = null;
            m_modsTimer = 0f;

            m_view.SetIdleColor();

            Messenger.Broadcast(MessengerEvents.APPLY_ENTITY_POWERUPS);
        }
    }
}
