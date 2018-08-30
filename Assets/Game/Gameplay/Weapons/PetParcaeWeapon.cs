using System;
using UnityEngine;

public class PetParcaeWeapon : PetMeleeWeapon {
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

        Messenger.AddListener(MessengerEvents.GAME_AREA_EXIT, RemoveMods);
        Messenger.AddListener(MessengerEvents.GAME_ENDED, RemoveMods);
    }

    private void OnDestroy() {
        Messenger.RemoveListener(MessengerEvents.GAME_AREA_EXIT, RemoveMods);
        Messenger.RemoveListener(MessengerEvents.GAME_ENDED, RemoveMods);
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

            Messenger.Broadcast(MessengerEvents.APPLY_ENTITY_POWERUPS);
        }
    }
}
