using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragonVampireTest : MonoBehaviour
{
    [SerializeField] GameObject m_effect;
    [SerializeField] Material m_bodyMaterial;
    [SerializeField] Material m_wingsMaterial;
    [SerializeField] float m_fadeDelay = 0.5f;
    [SerializeField] float m_fadeTime = 1f;

    public bool m_isVampire = true;

    bool m_simulation = false;
    bool m_isDeathAnimationEnabled = false;
    bool m_isPauseEffectEnabled = false;

    float m_delay;
    float m_time;
    Animator m_anim;
    readonly List<Material> m_fadeMaterials = new List<Material>();
    readonly int m_animTriggerDeath = Animator.StringToHash("DEATH");

    void Awake()
    {
        m_anim = GetComponent<Animator>(); 
        m_fadeMaterials.Add(m_bodyMaterial);
        m_fadeMaterials.Add(m_wingsMaterial);

        ResetEffect(resetOrientation: true);
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            RotateDragon();
        }

        if (m_simulation)
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

    void RotateDragon()
    {
        transform.Rotate(new Vector3(100 * Time.deltaTime, 0, 0));
    }

    public void PlayEffect()
    {
        if (m_simulation)
            ResetEffect(resetOrientation: false);

        m_simulation = true;
        SetDeathMaterials();
        if (m_isDeathAnimationEnabled)
            m_anim.SetTrigger(m_animTriggerDeath);
        m_effect.SetActive(true);

        if (m_isPauseEffectEnabled)
            Debug.Break();
    }

    public void ResetEffect(bool resetOrientation)
    {
        m_simulation = false;
        SetAlpha(1.0f);
        m_delay = m_fadeDelay;
        m_time = m_fadeTime;
        m_anim.Play("Flying");
        m_effect.SetActive(false);

        if (resetOrientation)
            transform.rotation = Quaternion.Euler(0, 90, 0);
    }

    public void EnableDeathAnimation(bool deathAnimationEnabled)
    {
        m_isDeathAnimationEnabled = deathAnimationEnabled;
    }

    public void EnablePauseEffect(bool pauseEffectEnabled)
    {
        m_isPauseEffectEnabled = pauseEffectEnabled;
    }

    void SetDeathMaterials()
    {
        if (m_isVampire)
        {
            m_bodyMaterial.EnableKeyword("FXLAYER_DISSOLVE");
            m_wingsMaterial.EnableKeyword("FXLAYER_DISSOLVE");
        }
        else
        {
            DragonCorpse.setDeathMode(m_bodyMaterial);
            DragonCorpse.setDeathMode(m_wingsMaterial);

        }
    }

    void SetAlpha(float alpha)
    {
        int count = m_fadeMaterials.Count;
        for (int i = 0; i < count; ++i)
        {
            if (m_isVampire)
            {
                m_fadeMaterials[i].SetFloat("_DissolveAmount", 1.0f - alpha);
            }
            else
            {
                Color tint = m_fadeMaterials[i].GetColor(GameConstants.Materials.Property.TINT);
                tint.a = alpha;
                m_fadeMaterials[i].SetColor(GameConstants.Materials.Property.TINT, tint);
            }
        }
    }

    // Dummy flying animation event
    void WingsFlyingSound()
    {

    }
}
