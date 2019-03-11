﻿using UnityEngine;
using System.Collections;

public class FireTypeAutoSelector : MonoBehaviour {

    [SerializeField] FireColorSetupManager.FireColorVariants m_fireColorVariant = FireColorSetupManager.FireColorVariants.DEFAULT;

	[SeparatorAttribute("Fire Rush")]
	[SerializeField] private GameObject[] m_fireOnly;

	[SeparatorAttribute("Mega Fire Rush")]
	[SerializeField] private GameObject[] m_megaOnly;

	[SeparatorAttribute("Game Objects Refereces")]
	[SerializeField] private ParticleSystem[] m_psChangeStartColors;
	[SerializeField] private ParticleSystem[] m_psChangeGradientColors;
	[SerializeField] private Renderer[] m_rChangeMaterials;


	private Material m_fireMaterialInstance;
	private Material m_megaMaterialInstance;

	void OnEnable() {
		if (InstanceManager.player != null) {
			bool isMega = InstanceManager.player.IsMegaFuryOn();

			for (int i = 0; i < m_fireOnly.Length; ++i) {
				m_fireOnly[i].SetActive(isMega == false);
			}

			for (int i = 0; i < m_megaOnly.Length; ++i) {
				m_megaOnly[i].SetActive(isMega == true);
			}

            FireColorConfig fireColorConfig = FireColorSetupManager.instance.GetColorConfig(InstanceManager.player.breathBehaviour.currentColor, m_fireColorVariant);
            if ( fireColorConfig != null )
            {
    			for (int i = 0; i < m_psChangeStartColors.Length; ++i) {
    				ParticleSystem.MainModule main = m_psChangeStartColors[i].main;
    				ParticleSystem.MinMaxGradient color = main.startColor;
    
                    color.colorMin = fireColorConfig.m_fireStartColorA;
                    color.colorMax = fireColorConfig.m_fireStartColorB;
    				main.startColor = color;
    			}
    
    			for (int i = 0; i < m_psChangeGradientColors.Length; ++i) {
    				ParticleSystem.ColorOverLifetimeModule colorOverLifetime = m_psChangeGradientColors[i].colorOverLifetime;
    				ParticleSystem.MinMaxGradient color = colorOverLifetime.color;
                    
                    color.gradient = fireColorConfig.m_fireGradient;
    				colorOverLifetime.color = color;
    			}
                
                if (fireColorConfig.m_fireMaterial != null)
                {
                    if ( isMega )
                    {
                        if ( m_megaMaterialInstance == null) m_megaMaterialInstance = new Material(fireColorConfig.m_fireMaterial);
                        SetMaterial(m_megaMaterialInstance);
                    }
                    else
                    {
                        if (m_fireMaterialInstance == null) m_fireMaterialInstance = new Material(fireColorConfig.m_fireMaterial);
                        SetMaterial(m_fireMaterialInstance);
                    }
                }
                
            }
		}
	}
    
    
    private void SetMaterial( Material _materialInstance )
    {
        _materialInstance.SetFloat("_Seed", Random.value);
        for (int i = 0; i < m_rChangeMaterials.Length; ++i) {
            Material[] materials = m_rChangeMaterials[i].materials;
            for (int m = 0; m < materials.Length; ++m) {
                materials[m] = _materialInstance;
            }
            m_rChangeMaterials[i].materials = materials;
        }
    }
}
