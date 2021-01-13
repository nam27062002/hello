using System;
using System.Collections.Generic;
using UnityEngine;


public class PetParcaeViewControl : ViewControl {
    //--------------------------------------------------------------------------
    [Serializable]
    public class PetParcaeColorsDictionary : SerializableDictionary<string, ColorRange> { }
    [Serializable]
    public class PetParcaeRampDictionary : SerializableDictionary<string, int> { }
    [Serializable]
    public class PetParcaeFresnelDictionary : SerializableDictionary<string, Color> { }
    //--------------------------------------------------------------------------


    //--------------------------------------------------------------------------
    [Separator("Pet Parcae")]
    [SerializeField] private PetParcaeColorsDictionary m_colors;
    [SerializeField] private PetParcaeRampDictionary m_colorRampIndex;
    [SerializeField] private PetParcaeFresnelDictionary m_colorFresnel;
    [SerializeField] private string m_idleKey;
    [SerializeField] private ParticleSystem m_smoke;
    //--------------------------------------------------------------------------


    //--------------------------------------------------------------------------
    private ColorRange m_currentColor;
    private bool m_applyColor;

    private Color m_fresnelColor;
    private int m_rampIndexA;
    private int m_rampIndexB;
    private float m_rampT;
    //--------------------------------------------------------------------------


    protected override void Awake() {
        base.Awake();

        m_currentColor = m_colors.Get(m_idleKey);
        m_rampIndexA = m_rampIndexB = m_colorRampIndex.Get(m_idleKey);

        m_rampT = 1f;
        ApplyColor();
    }

    public override void CustomUpdate() {
        base.CustomUpdate();

        if (m_applyColor) {
            ApplyColor();
        }

        if (m_rampT < 1f) {
            m_rampT += Time.deltaTime * 0.5f;
            for (int i = 0; i < m_materialList.Count; ++i) {
                m_materialList[i].SetFloat(GameConstants.Materials.Property.COLOR_RAMP_AMOUNT, m_rampT);
            }
        }
    }


    public void SetIdleColor() {
        SetColor(m_idleKey);
    }

    public void SetColor(string _key) {
        ColorRange color = m_colors.Get(_key);
        if (m_currentColor != color) {
            m_currentColor = color;

            m_rampIndexA = m_rampIndexB;
            m_rampIndexB = m_colorRampIndex.Get(_key);
            m_rampT = 0f;

            m_fresnelColor = m_colorFresnel.Get(_key);

            m_applyColor = true;
        }
    }

    private void ApplyColor() {
        ParticleSystem.MainModule main = m_smoke.main;
        ParticleSystem.MinMaxGradient gradient = main.startColor;
        if (gradient.mode == ParticleSystemGradientMode.TwoColors) {
            gradient.colorMin = m_currentColor.GetA();
            gradient.colorMax = m_currentColor.GetB();
        } else {
            gradient.color = m_currentColor.GetRandom();
        }
        main.startColor = gradient;

        // update materials
        for (int i = 0; i < m_materialList.Count; ++i) {
            m_materialList[i].SetFloat(GameConstants.Materials.Property.COLOR_RAMP_ID_0, m_rampIndexA);
            m_materialList[i].SetFloat(GameConstants.Materials.Property.COLOR_RAMP_ID_1, m_rampIndexB);
            m_materialList[i].SetFloat(GameConstants.Materials.Property.COLOR_RAMP_AMOUNT, m_rampT);
            m_materialList[i].SetColor(GameConstants.Materials.Property.FRESNEL_COLOR, m_fresnelColor);
        }

        m_applyColor = false;
    }
}
