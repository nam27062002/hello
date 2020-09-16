using System.Collections.Generic;
using UnityEngine;
using GameConstants.Materials;

public class DragonDissolveEffect : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] DragonTint m_dragonTint;

    [Header("Effect settings")]
    [SerializeField] float m_dissolveTime = 4f;

    [Header("Shader properties")]
    [SerializeField] Texture2D m_dissolveNoiseTexture;
    [SerializeField] float m_dissolveUpperLimit = 18.0f;
    [SerializeField] float m_dissolveLowerLimit = -18.0f;
    [SerializeField] float m_dissolveMargin = 0.1f;
    [SerializeField] Vector4 m_dissolveDirection;

    List<Material> m_material = new List<Material>();
    float m_delay;
    float m_time;
    
    void Awake()
    {
        Reset();
    }

    public void Reset()
    {
        m_delay = 0.0f;
        m_time = m_dissolveTime;

        enabled = false;
    }

    public void Execute()
    {
        if (m_material.Count == 0)
            m_material = m_dragonTint.GetDragonMaterials;
        
        for (int i = 0; i < m_material.Count; i++)
        {
            m_material[i].DisableKeyword(Keyword.FX_REFLECTION);
            m_material[i].EnableKeyword(Keyword.FX_DISSOLVE);
            m_material[i].SetTexture(Property.FIRE_MAP, m_dissolveNoiseTexture);
            m_material[i].SetFloat(Property.DISSOLVE_UPPER_LIMIT, m_dissolveUpperLimit);
            m_material[i].SetFloat(Property.DISSOLVE_LOWER_LIMIT, m_dissolveLowerLimit);
            m_material[i].SetFloat(Property.DISSOLVE_MARGIN, m_dissolveMargin);
            m_material[i].SetVector(Property.DISSOLVE_DIRECTION, m_dissolveDirection);
        }

        enabled = true;
    }

    void Update()
    {
        m_delay -= Time.unscaledDeltaTime;
        if (m_delay <= 0)
        {
            float a = m_time / m_dissolveTime;
            SetDissolve(a);
            m_time -= Time.unscaledDeltaTime;
            if (m_time <= 0) m_time = 0f;
        }
    }

    void SetDissolve(float alpha)
    {
        for (int i = 0; i < m_material.Count; i++)
        {
            m_material[i].SetFloat(Property.DISSOLVE_AMOUNT, 1.0f - alpha);
        }
    }
}
