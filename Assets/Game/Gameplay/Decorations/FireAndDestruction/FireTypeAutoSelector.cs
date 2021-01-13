using UnityEngine;
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


    private FireColorConfig m_fireColorConfig;
	private Material m_fireMaterialInstance;
	private Material m_megaMaterialInstance;
    [HideInInspector] public FireColorSetupManager.FireColorType m_fireType = FireColorSetupManager.FireColorType.RED;
    
    

	void OnEnable() {
		if (InstanceManager.player != null) {
            
			bool isMega = InstanceManager.player.IsMegaFuryOn();

			for (int i = 0; i < m_fireOnly.Length; ++i) {
				m_fireOnly[i].SetActive(isMega == false);
			}

			for (int i = 0; i < m_megaOnly.Length; ++i) {
				m_megaOnly[i].SetActive(isMega == true);
			}
            
            StartCoroutine(DelayedOnEnable(isMega));
		}
	}

    IEnumerator DelayedOnEnable( bool isMega )
    {
        yield return null;
        m_fireColorConfig = FireColorSetupManager.instance.GetColorConfig(m_fireType, m_fireColorVariant);
        if ( m_fireColorConfig != null )
        {
            for (int i = 0; i < m_psChangeStartColors.Length; ++i) {
                ParticleSystem.MainModule main = m_psChangeStartColors[i].main;
                ParticleSystem.MinMaxGradient color = main.startColor;

                color.colorMin = m_fireColorConfig.m_fireStartColorA;
                color.colorMax = m_fireColorConfig.m_fireStartColorB;
                main.startColor = color;
            }

            for (int i = 0; i < m_psChangeGradientColors.Length; ++i) {
                ParticleSystem.ColorOverLifetimeModule colorOverLifetime = m_psChangeGradientColors[i].colorOverLifetime;
                ParticleSystem.MinMaxGradient color = colorOverLifetime.color;
                
                color.gradient = m_fireColorConfig.m_fireGradient;
                colorOverLifetime.color = color;
            }
            
            if (m_fireColorConfig.m_fireMaterial != null)
            {
                if ( isMega )
                {
                    if (m_megaMaterialInstance == null)
                        m_megaMaterialInstance = FireColorSetupManager.instance.GetConfigMaterial(m_fireColorConfig);
                    SetMaterial(m_megaMaterialInstance);
                }
                else
                {
                    if (m_fireMaterialInstance == null) 
                        m_fireMaterialInstance = FireColorSetupManager.instance.GetConfigMaterial(m_fireColorConfig);
                    SetMaterial(m_fireMaterialInstance);
                }
            }
            
        }
    }

    private void OnDisable()
    {
        // Return materials
        ReturnMaterials();
    }
    
    private void ReturnMaterials()
    {
        if (FireColorSetupManager.instance != null)
        {
            if (m_megaMaterialInstance != null)
            {
                FireColorSetupManager.instance.ReturnConfigMaterial(m_fireColorConfig, m_megaMaterialInstance);
                m_megaMaterialInstance = null;
            }
            
            if (m_fireMaterialInstance != null)
            {
                FireColorSetupManager.instance.ReturnConfigMaterial(m_fireColorConfig, m_fireMaterialInstance);
                m_fireMaterialInstance = null;
            }
        }
    }

    private void SetMaterial( Material _materialInstance )
    {
        for (int i = 0; i < m_rChangeMaterials.Length; ++i) {
            Material[] materials = m_rChangeMaterials[i].materials;
            for (int m = 0; m < materials.Length; ++m) {
                materials[m] = _materialInstance;
            }
            m_rChangeMaterials[i].materials = materials;
        }
    }
}
