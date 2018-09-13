using System;
using UnityEngine;


public class PetParcaeViewControl : ViewControl {
    //--------------------------------------------------------------------------
    [Serializable]
    public class PetParcaeColorsDictionary : SerializableDictionary<string, ColorRange> { }
    //--------------------------------------------------------------------------


    //--------------------------------------------------------------------------
    [Separator("Pet Parcae")]
    [SerializeField] private PetParcaeColorsDictionary m_colors;
    [SerializeField] private string m_idleKey;
    [SerializeField] private ParticleSystem m_smoke;
    //--------------------------------------------------------------------------


    //--------------------------------------------------------------------------
    private ColorRange m_currentColor;
    private bool m_applyColor;
    //--------------------------------------------------------------------------


    protected override void Awake() {
        base.Awake();

        m_currentColor = m_colors.Get(m_idleKey);
        ApplyColor();
    }

    public override void CustomUpdate() {
        base.CustomUpdate();

        if (m_applyColor) {
            ApplyColor();
        }
    }


    public void SetIdleColor() {
        SetColor(m_idleKey);
    }

    public void SetColor(string _key) {
        ColorRange color = m_colors.Get(_key);
        if (m_currentColor != color) {
            m_currentColor = color;
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

        m_applyColor = false;
    }
}
