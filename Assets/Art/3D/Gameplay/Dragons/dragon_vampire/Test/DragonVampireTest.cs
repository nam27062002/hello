using System.Collections.Generic;
using UnityEngine;

public class DragonVampireTest : MonoBehaviour
{
    public GameObject effect;
    public Material bodyMaterial;
    public Material wingsMaterial;
    [SerializeField] private float m_fadeDelay = 0.5f;
    [SerializeField] private float m_fadeTime = 1f;
    float m_delay;
    float m_time;
    bool isDeathAnimationEnabled = false;
    bool isPauseEffectEnabled = false;

    int animTriggerDeath = Animator.StringToHash("DEATH");
    Animator anim;
    bool simulation = false;
    List<Material> m_fadeMaterials = new List<Material>();

    private void Awake()
    {
        anim = GetComponent<Animator>();
        m_delay = m_fadeDelay;
        m_time = m_fadeTime;
        m_fadeMaterials.Add(bodyMaterial);
        m_fadeMaterials.Add(wingsMaterial);
        SetAlpha(1.0f);
    }

    void Update()
    {
        if (simulation)
        {
            m_delay -= Time.deltaTime;
            if (m_delay <= 0)
            {
                float a = m_time / m_fadeTime;
                SetAlpha(a);
                m_time -= Time.deltaTime;
                if (m_time <= 0) m_time = 0f;
            }
        }
    }

    public void PlayEffect()
    {
        if (simulation)
        {
            ResetEffect();
        }

        simulation = true;
        if (isDeathAnimationEnabled)
            anim.SetTrigger(animTriggerDeath);
        SetDeathMaterials();
        effect.SetActive(true);

        if (isPauseEffectEnabled)
            Debug.Break();
    }

    public void ResetEffect()
    {
        simulation = false;
        SetAlpha(1.0f);
        m_delay = m_fadeDelay;
        m_time = m_fadeTime;
        anim.Play("Flying");
        effect.SetActive(false);  
    }

    public void EnableDeathAnimation(bool deathAnimationEnabled)
    {
        isDeathAnimationEnabled = deathAnimationEnabled;
    }

    public void EnablePauseEffect(bool pauseEffectEnabled)
    {
        isPauseEffectEnabled = pauseEffectEnabled;
    }

    void WingsFlyingSound()
    {

    }

    void SetDeathMaterials()
    {
        DragonCorpse.setDeathMode(bodyMaterial);
        DragonCorpse.setDeathMode(wingsMaterial);
    }

    void SetAlpha(float alpha)
    {
        int count = m_fadeMaterials.Count;
        for (int i = 0; i < count; ++i)
        {
            Color tint = m_fadeMaterials[i].GetColor(GameConstants.Materials.Property.TINT);
            tint.a = alpha;
            m_fadeMaterials[i].SetColor(GameConstants.Materials.Property.TINT, tint);
        }
    }
}
