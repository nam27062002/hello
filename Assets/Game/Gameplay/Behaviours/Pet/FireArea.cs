using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleArea2D))]
public class FireArea : MonoBehaviour {
    private const float ENTITY_CHECK_TIME = 0.3f;
    private const float FIRE_NODE_CHECK_TIME = 0.3f;


    [SerializeField] private DragonTier m_tier = DragonTier.TIER_4;
    [SerializeField] private bool m_megaFireIgnoresTier = false;
    [SerializeField] [EnumMask] private IEntity.Tag m_entityTags = 0;
    [SerializeField] [EnumMask] protected IEntity.Tag m_ignoreEntityTags = 0;
    [SerializeField] private bool m_burnDecorations = false;
    [SerializeField] private IEntity.Type m_type = IEntity.Type.PET;
    [SerializeField] private string m_onBurnAudio;


    private CircleArea2D m_circle;
	private Rect m_rect;

	private Entity[] m_checkEntities = new Entity[50];
	private int m_numCheckEntities = 0;

    private float m_entityCheckTimer = 0;
    private float m_fireNodeCheckTimer = 0;

    private bool m_burnAudioAvailable;


	// Use this for initialization
	private void Start () {
		m_circle = GetComponent<CircleArea2D>();
		m_rect = new Rect();

        m_burnAudioAvailable = !string.IsNullOrEmpty(m_onBurnAudio);
    }
	
	// Update is called once per frame
	private void Update () {
        m_entityCheckTimer -= Time.deltaTime;
        if (m_entityCheckTimer <= 0) {
            m_entityCheckTimer = ENTITY_CHECK_TIME;

            // Search for entities
            bool playBurnSound = false;
            m_numCheckEntities = EntityManager.instance.GetOverlapingEntities((Vector2)m_circle.center, m_circle.radius, m_checkEntities);
            for (int i = 0; i < m_numCheckEntities; i++) {
                Entity prey = m_checkEntities[i];

                if (prey.IsBurnable()) {
                    bool tagMatch = true;

                    if (m_entityTags > 0) { 
                        tagMatch = tagMatch && prey.HasTag(m_entityTags);
                    }
                    if (m_ignoreEntityTags > 0) {
                        tagMatch = tagMatch && !prey.HasTag(m_ignoreEntityTags);
                    }

                    if (tagMatch) {
                        bool burnableByTier = false;

                        if (m_megaFireIgnoresTier && InstanceManager.player.breathBehaviour.type == DragonBreathBehaviour.Type.Mega) {
                            burnableByTier = true;
                        } else {
                            burnableByTier = prey.IsBurnable(m_tier);
                        }

                        if (burnableByTier) {
                            AI.IMachine machine = prey.machine;
                            if (machine != null) {
                                machine.Burn(transform, m_type);
                                playBurnSound = true;
                            }
                        }
                    }
                }
            }

            if (playBurnSound && m_burnAudioAvailable) {
                AudioController.Play(m_onBurnAudio, m_circle.center);
            }
        }

        if (m_burnDecorations) {
            m_fireNodeCheckTimer -= Time.deltaTime;
            if (m_fireNodeCheckTimer <= 0) {
                m_fireNodeCheckTimer = FIRE_NODE_CHECK_TIME;

                // Update rect
                m_rect.center = m_circle.center;
                m_rect.height = m_rect.width = m_circle.radius;

                //
                FirePropagationManager.instance.FireUpNodes(m_rect, Overlaps, m_tier, DragonBreathBehaviour.Type.None, Vector3.zero, m_type);
            }
        }
	}

	private bool Overlaps(CircleAreaBounds _fireNodeBounds) {
		return m_circle.Overlaps( _fireNodeBounds.center, _fireNodeBounds.radius);
	}
}
