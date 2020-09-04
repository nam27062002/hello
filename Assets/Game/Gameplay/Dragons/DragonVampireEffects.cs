using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(DragonDissolveEffect))]
public class DragonVampireEffects : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] DragonDissolveEffect m_dissolveEffect;

    [Header("Death effect")]
    [SerializeField] ParticleData m_deathParticle;
    [SerializeField] Transform m_deathAttachpoint;

    [Header("Damage effect")]
    [SerializeField] ParticleData m_damageParticle;
    [SerializeField] int m_maxDamageParticlesAmount = 30;
    [SerializeField] float m_secondsBetweenSpawnDamageParticle = 5.0f;

    float m_lastTimeDamageParticleSpawned;
    GameObject m_damageParticleInstance;
    ParticleSystem.MainModule m_damageParticleMainModule;

    GameObject m_deathParticleInstance;

    void Awake()
    {
        CreateInstance();
    }

    void OnEnable()
	{
        Messenger.AddListener<float, DamageType, Transform>(MessengerEvents.PLAYER_DAMAGE_RECEIVED, OnPlayerDamage);
        Messenger.AddListener<DragonPlayer.ReviveReason>(MessengerEvents.PLAYER_REVIVE, OnPlayerRevive);
        Messenger.AddListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnPlayerKo);
    }

    void OnDisable()
    {
        Messenger.RemoveListener<float, DamageType, Transform>(MessengerEvents.PLAYER_DAMAGE_RECEIVED, OnPlayerDamage);
        Messenger.RemoveListener<DragonPlayer.ReviveReason>(MessengerEvents.PLAYER_REVIVE, OnPlayerRevive);
        Messenger.RemoveListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnPlayerKo);
    }

    void OnPlayerRevive(DragonPlayer.ReviveReason _reviveReason)
    {
        // Reset dissolve effect
        m_dissolveEffect.Reset();
    }

    void OnPlayerKo(DamageType _damageType, Transform _transform)
    {
        // Spawn death particle effect
        m_deathParticleInstance.transform.SetPositionAndRotation(m_deathAttachpoint.position, transform.rotation);
        m_deathParticleInstance.SetActive(true);

        // Start dissolve effect
        m_dissolveEffect.Execute();
    }

    void OnPlayerDamage(float _damage, DamageType _damageType, Transform _transform)
    {
        // Ignore this type of damage
        if (_damage < 1.0f || _damageType == DamageType.POISON || _damageType == DamageType.CURSE)
            return;

        // Check if have passed enough time to spawn the particle
        float elapsedTime = Time.time - m_lastTimeDamageParticleSpawned;
        if (elapsedTime < m_secondsBetweenSpawnDamageParticle)
            return;

        // Calculate the amount of particles to spawn
        int particlesAmount = Mathf.RoundToInt((int)_damage * m_maxDamageParticlesAmount / 60); // BIG_DAMAGE = 60
        int maxParticles = Mathf.Clamp(particlesAmount, 1, m_maxDamageParticlesAmount);
        m_damageParticleMainModule.maxParticles = maxParticles;

        // Spawn damage particle
        m_damageParticleInstance.transform.SetPositionAndRotation(transform.position, transform.rotation);
        m_damageParticleInstance.SetActive(true);

        // Update last time particle was spawned
        m_lastTimeDamageParticleSpawned = Time.time;
    }

    void CreateInstance()
    {
        // Preload damage effect
        m_damageParticleInstance = m_damageParticle.CreateInstance();
        ParticleSystem particleInstance = m_damageParticleInstance.GetComponent<ParticleSystem>();
        m_damageParticleMainModule = particleInstance.main;
        m_damageParticleInstance.SetActive(false);
        SceneManager.MoveGameObjectToScene(particleInstance.gameObject, gameObject.scene);
        m_damageParticleInstance.transform.SetParentAndReset(InstanceManager.player.transform);

        // Preload death effect
        m_deathParticleInstance = m_deathParticle.CreateInstance();
        m_deathParticleInstance.SetActive(false);
        SceneManager.MoveGameObjectToScene(m_deathParticleInstance, gameObject.scene);
    }
}
