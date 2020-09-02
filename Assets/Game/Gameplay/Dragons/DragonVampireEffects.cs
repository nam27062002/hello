using UnityEngine;
using UnityEngine.SceneManagement;

public class DragonVampireEffects : MonoBehaviour
{
    [Header("Damage effect")]
    [SerializeField] ParticleData onDamageParticleEffect;
    [SerializeField] int maxDamageParticlesAmount = 30;
    [SerializeField] float secondsBetweenSpawnDamageParticle = 10.0f;

    float m_lastTimeDamageParticleSpawned;
    GameObject m_damageParticleInstance;
    ParticleSystem.MainModule m_damageParticleMainModule;

    void Awake()
    {
        CreateInstance();
    }

    void OnEnable()
	{
        Messenger.AddListener<float, DamageType, Transform>(MessengerEvents.PLAYER_DAMAGE_RECEIVED, OnPlayerDamage);
    }

    void OnDisable()
    {
        Messenger.RemoveListener<float, DamageType, Transform>(MessengerEvents.PLAYER_DAMAGE_RECEIVED, OnPlayerDamage);
    }

    void OnPlayerDamage(float _damage, DamageType _damageType, Transform _transform)
    {
        // Ignore this type of damage
        if (_damage < 1.0f || _damageType == DamageType.POISON || _damageType == DamageType.CURSE)
            return;

        // Check if have passed enough time to spawn the particle
        float elapsedTime = Time.time - m_lastTimeDamageParticleSpawned;
        if (elapsedTime < secondsBetweenSpawnDamageParticle)
            return;

        // Calculate the amount of particles to spawn
        int particlesAmount = Mathf.RoundToInt((int)_damage * maxDamageParticlesAmount / 60); // BIG_DAMAGE = 60
        int maxParticles = Mathf.Clamp(particlesAmount, 1, maxDamageParticlesAmount);
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
        m_damageParticleInstance = onDamageParticleEffect.CreateInstance();
        ParticleSystem particleInstance = m_damageParticleInstance.GetComponent<ParticleSystem>();
        m_damageParticleMainModule = particleInstance.main;
        m_damageParticleInstance.SetActive(false);
        SceneManager.MoveGameObjectToScene(particleInstance.gameObject, gameObject.scene);
        m_damageParticleInstance.transform.SetParentAndReset(InstanceManager.player.transform);
    }
}
