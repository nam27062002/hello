using UnityEngine;
using System.Collections;

public class FrameColoring : MonoBehaviour, IBroadcastListener {

    public Color m_fireColor = Color.black;
    public Color m_superFireColor = Color.black;
    public Color m_iceFireColor = Color.black;
    public Color m_starvingColor = Color.black;

    private float m_value = 0.5f;
    private Color m_color;
    public Material m_material = null;

    private bool m_furyOn = false;
    DragonBreathBehaviour.Type m_furyType = DragonBreathBehaviour.Type.None;
    protected Color m_currentColor = Color.black;
    private bool m_starvingOn = false;
    private bool m_criticalOn = false;
    private bool m_ko = false;

    void Awake() {
        m_material = new Material(m_material);
        m_value = 0;
        m_color = Color.black;
        Broadcaster.AddListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);
        Messenger.AddListener<DragonHealthModifier, DragonHealthModifier>(MessengerEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);
        Messenger.AddListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnKo);
        Messenger.AddListener<DragonPlayer.ReviveReason>(MessengerEvents.PLAYER_REVIVE, OnRevive);
    }

    private void OnDestroy() {
        Broadcaster.RemoveListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);
        Messenger.RemoveListener<DragonHealthModifier, DragonHealthModifier>(MessengerEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);
        Messenger.RemoveListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnKo);
        Messenger.RemoveListener<DragonPlayer.ReviveReason>(MessengerEvents.PLAYER_REVIVE, OnRevive);
    }

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo) {
        switch (eventType) {
            case BroadcastEventType.FURY_RUSH_TOGGLED: {
                    FuryRushToggled furyRushToggled = (FuryRushToggled)broadcastEventInfo;
                    OnFury(furyRushToggled.activated, furyRushToggled.type, furyRushToggled.color);
                }
                break;
        }
    }


    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (m_furyOn) {
            switch (m_furyType) {
                case DragonBreathBehaviour.Type.Standard: {
                        m_value = Mathf.Lerp(m_value, 0.69f, Time.deltaTime * 10);
                        m_color = Color.Lerp(m_color, m_currentColor, Time.deltaTime * 10);
                    }
                    break;
                case DragonBreathBehaviour.Type.Mega: {
                        m_value = Mathf.Lerp(m_value, 0.69f, Time.deltaTime * 15);
                        m_color = Color.Lerp(m_color, m_currentColor, Time.deltaTime * 15);
                    }
                    break;
            }

        } else if (m_criticalOn) {
            m_value = Mathf.Lerp(m_value, 0.7f + Mathf.Sin(Time.time * 5) * 0.2f, Time.deltaTime * 10);
            m_color = Color.Lerp(m_color, m_starvingColor, Time.deltaTime * 10);
        } else if (m_starvingOn) {
            m_value = Mathf.Lerp(m_value, 0.15f + Mathf.Sin(Time.time * 2.5f) * 0.1f, Time.deltaTime * 10);
            m_color = Color.Lerp(m_color, m_starvingColor, Time.deltaTime * 5);
        } else if (m_ko) {
            m_value = Mathf.Lerp(m_value, 0.9f + Mathf.Sin(Time.time * 2.5f) * 0.05f, Time.deltaTime * 10);
            m_color = Color.Lerp(m_color, m_starvingColor, Time.deltaTime * 2.5f);
        } else {
            m_value = Mathf.Lerp(m_value, 0, Time.deltaTime);
            m_color = Color.Lerp(m_color, Color.black, Time.deltaTime);
        }
        if (m_value <= 0.04f) {
            Graphics.Blit(source, destination);
        } else {
            m_material.SetColor(GameConstants.Materials.Property.COLOR, m_color);
            m_material.SetFloat(GameConstants.Materials.Property.INTENSITY, m_value);
            Graphics.Blit(source, destination, m_material);
        }
    }

    private void OnFury(bool _enabled, DragonBreathBehaviour.Type _type, FireColorSetupManager.FireColorType _color) {
        m_furyOn = _enabled;
        m_furyType = _type;
        switch (_color) {
            case FireColorSetupManager.FireColorType.LAVA:
            case FireColorSetupManager.FireColorType.RED: {
                    m_currentColor = m_fireColor;
                }
                break;
            case FireColorSetupManager.FireColorType.BLUE: {
                    m_currentColor = m_superFireColor;
                }
                break;
            case FireColorSetupManager.FireColorType.ICE: {
                    m_currentColor = m_iceFireColor;
                }
                break;

        }
    }

    private void OnHealthModifierChanged(DragonHealthModifier _oldModifier, DragonHealthModifier _newModifier) {
        m_starvingOn = (_newModifier != null && _newModifier.IsStarving());
        m_criticalOn = (_newModifier != null && _newModifier.IsCritical());
    }

    private void OnKo(DamageType _type, Transform _source) {
        m_ko = true;
    }

    private void OnRevive(DragonPlayer.ReviveReason reason) {
        m_ko = false;
    }
}
