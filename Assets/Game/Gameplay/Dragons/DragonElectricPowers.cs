using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DragonElectricPowers : MonoBehaviour {

    public List<CircleArea2D> m_circleAreas = new List<CircleArea2D>();
    private List<float> m_originalRadius = new List<float>();
    public DragonTier m_tier = DragonTier.TIER_4;
    public IEntity.Type m_type = IEntity.Type.PLAYER;
    public float m_waterMultiplier = 2;
    public float m_chainRadiusCheck = 2;
    public float m_blastRadius = 2;
    public string m_blastParticle = "PS_ElectricExplosion";
    public ParticleSystem m_blastParticleSystem;

    private Entity[] m_checkEntities = new Entity[50];
    private int m_numCheckEntities = 0;
    float m_extraRadius;
    DragonBoostBehaviour m_boost;
    DragonBreathBehaviour m_breath;
    DragonMotion m_motion;

    private bool m_active = false;
    private int m_powerLevel = 0;

    public float m_minTimerBetweenKills = 0.5f;
    protected float m_timer;

    public GameObject m_tailUp;
    public GameObject m_tailDown;

    public GameObject m_lightningPrefab;
    public string m_lightningSound;

    private const int NUM_LIGHTNINGS = 3;
    GameObject[] m_lightningInstances = new GameObject[NUM_LIGHTNINGS];
    ParticleSystem[] m_lightningPS = new ParticleSystem[NUM_LIGHTNINGS];

    public Renderer m_boostRenderer;
    protected Material m_boostMaterial;
    private float m_boostDelta = 0;

    private float m_superSizeMultiplier = 1;
    ToggleParam m_toggleParam = new ToggleParam();

    private void Awake() {
        Material[] mats = m_boostRenderer.materials;
        m_boostMaterial = mats[0];
        int max = mats.Length;
        for (int i = 0; i < max; i++) {
            mats[i] = m_boostMaterial;
        }
        m_boostRenderer.materials = mats;
        m_boostDelta = 0;
    }

    // Use this for initialization
    void Start() {

        for (int i = 0; i < NUM_LIGHTNINGS; i++) {
            m_lightningInstances[i] = Instantiate<GameObject>(m_lightningPrefab);
            SceneManager.MoveGameObjectToScene(m_lightningInstances[i], gameObject.scene);
            m_lightningPS[i] = m_lightningInstances[i].GetComponent<ParticleSystem>();
        }


        m_originalRadius.Clear();
        float scale = InstanceManager.player.data.scale;
        for (int i = 0; i < m_circleAreas.Count; i++) {
            m_circleAreas[i].radius = m_circleAreas[i].radius * scale;
            m_originalRadius.Add(m_circleAreas[i].radius);
        }
        m_blastRadius *= scale;
        m_chainRadiusCheck *= scale;

        m_boost = InstanceManager.player.dragonBoostBehaviour;
        m_breath = InstanceManager.player.breathBehaviour;
        m_motion = InstanceManager.player.dragonMotion;
        m_extraRadius = 1;
        m_tier = InstanceManager.player.data.tier;
        m_timer = m_minTimerBetweenKills;

        DragonDataSpecial dataSpecial = InstanceManager.player.data as DragonDataSpecial;
        m_powerLevel = dataSpecial.m_powerLevel;

        m_tailUp.SetActive(m_powerLevel > 0);
        m_tailDown.SetActive(m_powerLevel > 1);

        if (m_powerLevel >= 2) {
            m_blastParticleSystem = ParticleManager.InitLeveledParticle(m_blastParticle, null);
            SceneManager.MoveGameObjectToScene(m_blastParticleSystem.gameObject, gameObject.scene);
            m_blastParticleSystem.gameObject.SetActive(true);
        }
        Messenger.AddListener<bool, DragonSuperSize.Source>(MessengerEvents.SUPER_SIZE_TOGGLE, OnSuperSizeToggle);
    }

    private void OnDestroy() {
        Messenger.RemoveListener<bool, DragonSuperSize.Source>(MessengerEvents.SUPER_SIZE_TOGGLE, OnSuperSizeToggle);
    }

    // Update is called once per frame
    void Update() {

        int circleAreaCount = m_circleAreas.Count;

        if (m_boost.IsBoostActive() || m_breath.IsFuryOn()) {
            m_boostDelta = Mathf.Lerp(m_boostDelta, 0.37f, Time.deltaTime * 10);
            m_timer -= Time.deltaTime;
            if (m_timer <= 0) {
                m_timer = m_minTimerBetweenKills;

                if (!m_active) {
                    m_active = true;
                    m_toggleParam.value = m_active;
                    Broadcaster.Broadcast(BroadcastEventType.SPECIAL_POWER_TOGGLED, m_toggleParam);
                }

                bool done = false;

                for (int i = 0; i < circleAreaCount && !done; i++) {
                    Entity entity = GetFirstBurnableEntity((Vector2)m_circleAreas[i].center, m_circleAreas[i].radius);
                    if (entity != null) {
                        AI.IMachine startingMachine = entity.machine;
                        if (startingMachine != null) {
                            startingMachine.Burn(transform, m_type);
                            Vector3 startingPos = startingMachine.position;
                            if (entity.circleArea != null)
                                startingPos = entity.circleArea.center;


                            SpawnLightning(0, m_circleAreas[i].center, startingPos);
                            // BLAST!!
                            if (m_powerLevel >= 2) {
                                // Spawn particle!
                                m_blastParticleSystem.transform.position = startingPos;
                                m_blastParticleSystem.Play();
                                m_numCheckEntities = EntityManager.instance.GetOverlapingEntities((Vector2)startingPos, m_blastRadius * m_extraRadius * m_superSizeMultiplier, m_checkEntities);
                                for (int j = 0; j < m_numCheckEntities; j++) {
                                    Entity blastEntity = m_checkEntities[j];
                                    if (blastEntity.circleArea != null && blastEntity.IsBurnable() && (blastEntity.IsBurnable(m_tier) || (m_breath.IsFuryOn() && m_breath.type == DragonBreathBehaviour.Type.Mega))) {
                                        AI.IMachine blastMachine = blastEntity.machine;
                                        if (blastMachine != null) {
                                            // Launch Lightning!
                                            blastMachine.Burn(transform, m_type);
                                        }
                                    }
                                }
                            }

                            // Chain
                            if (m_powerLevel >= 1) {
                                float chainRadiusCheck = m_chainRadiusCheck * m_superSizeMultiplier;
                                Entity chain1_entity = GetFirstBurnableEntity((Vector2)startingPos, chainRadiusCheck);
                                if (chain1_entity != null) {
                                    // Burn chain 1 entity
                                    AI.IMachine chain1_machine = chain1_entity.machine;
                                    chain1_machine.Burn(transform, IEntity.Type.PLAYER);
                                    Vector3 entity1Pos = chain1_machine.position;
                                    if (chain1_entity.circleArea != null)
                                        entity1Pos = chain1_entity.circleArea.center;

                                    SpawnLightning(1, startingPos, entity1Pos);

                                    // Chain Upgrade
                                    if (m_powerLevel >= 3) {
                                        Entity chain2_entity = GetFirstBurnableEntity((Vector2)entity1Pos, chainRadiusCheck);
                                        if (chain2_entity != null) {
                                            AI.IMachine chain2_machine = chain2_entity.machine;
                                            chain2_machine.Burn(transform, IEntity.Type.PLAYER);
                                            Vector3 entity2Pos = chain1_machine.position;
                                            if (chain2_entity.circleArea != null)
                                                entity2Pos = chain2_entity.circleArea.center;
                                            SpawnLightning(2, entity1Pos, entity2Pos);
                                        }
                                    }
                                }
                            }

                            done = true;
                        }
                    }
                }
            }
        } else {
            m_boostDelta = Mathf.Lerp(m_boostDelta, 0, Time.deltaTime * 10);

            if (m_active) {
                m_active = false;
                m_toggleParam.value = m_active;
                Broadcaster.Broadcast(BroadcastEventType.SPECIAL_POWER_TOGGLED, m_toggleParam);
            }
        }

        m_boostMaterial.SetFloat(GameConstants.Materials.Property.OPACITY_SATURATION, m_boostDelta);

        if (m_motion.IsInsideWater()) {
            m_extraRadius += Time.deltaTime;
        } else {
            m_extraRadius -= Time.deltaTime;
        }
        m_extraRadius = Mathf.Clamp(m_extraRadius, 1, m_waterMultiplier);


        for (int i = 0; i < circleAreaCount; i++) {
            m_circleAreas[i].radius = m_originalRadius[i] * m_extraRadius * m_superSizeMultiplier;
        }



    }

    public Entity GetFirstBurnableEntity(Vector2 _center, float _radius) {
        Entity ret = null;
        m_numCheckEntities = EntityManager.instance.GetOverlapingEntities(_center, _radius, m_checkEntities);
        for (int i = 0; i < m_numCheckEntities && ret == null; i++) {
            Entity prey = m_checkEntities[i];
            if (prey.circleArea != null && prey.IsBurnable() && (prey.IsBurnable(m_tier) || (m_breath.IsFuryOn() && m_breath.type == DragonBreathBehaviour.Type.Mega))) {
                if (!prey.machine.IsDead() && !prey.machine.IsDying())
                    ret = prey;
            }
        }
        return ret;
    }

    private void SpawnLightning(int index, Vector3 start, Vector3 end) {
        m_lightningInstances[index].transform.position = start;
        Vector3 dir = start - end;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90.0f;
        m_lightningInstances[index].transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        m_lightningInstances[index].transform.localScale = GameConstants.Vector3.one * dir.magnitude * 0.5f;
        m_lightningPS[index].Play();

        if (!string.IsNullOrEmpty(m_lightningSound))
            AudioController.Play(m_lightningSound, transform);
    }

    private void OnSuperSizeToggle(bool _active, DragonSuperSize.Source _source) {
        DragonSuperSize superSize = InstanceManager.player.GetComponent<DragonSuperSize>();
        if (superSize != null) {
            m_superSizeMultiplier = _active ? superSize.sizeUpMultiplier : 1;
        }
    }
}
