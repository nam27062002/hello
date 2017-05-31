using UnityEngine;
using System.Collections;

public class FireTypeAutoSelector : MonoBehaviour {
	[SeparatorAttribute("Fire Rush")]
	[SerializeField] private Color m_fireStartColorA;
	[SerializeField] private Color m_fireStartColorB;
	[SerializeField] private Gradient m_fireGradient;
	[SerializeField] private Material m_fireMaterial;
	[SerializeField] private GameObject[] m_fireOnly;

	[SeparatorAttribute("Mega Fire Rush")]
	[SerializeField] private Color m_megaStartColorA;
	[SerializeField] private Color m_megaStartColorB;
	[SerializeField] private Gradient m_megaGradient;
	[SerializeField] private Material m_megaMaterial;
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

			for (int i = 0; i < m_psChangeStartColors.Length; ++i) {
				ParticleSystem.MainModule main = m_psChangeStartColors[i].main;
				ParticleSystem.MinMaxGradient color = main.startColor;

				if (isMega) {
					color.colorMin = m_megaStartColorA;
					color.colorMax = m_megaStartColorB;
				} else {
					color.colorMin = m_fireStartColorA;
					color.colorMax = m_fireStartColorB;
				}

				main.startColor = color;
			}

			for (int i = 0; i < m_psChangeGradientColors.Length; ++i) {
				ParticleSystem.ColorOverLifetimeModule colorOverLifetime = m_psChangeGradientColors[i].colorOverLifetime;
				ParticleSystem.MinMaxGradient color = colorOverLifetime.color;

				if (isMega) color.gradient = m_megaGradient;
				else 		color.gradient = m_fireGradient;

				colorOverLifetime.color = color;
			}

			if (m_fireMaterial != null && m_megaMaterial != null) {

				if (m_fireMaterialInstance == null) m_fireMaterialInstance = new Material(m_fireMaterial);
				if (m_megaMaterialInstance == null) m_megaMaterialInstance = new Material(m_megaMaterial);

				m_fireMaterialInstance.SetFloat("_Seed", Random.value);
				m_megaMaterialInstance.SetFloat("_Seed", Random.value);

				for (int i = 0; i < m_rChangeMaterials.Length; ++i) {
					Material[] materials = m_rChangeMaterials[i].materials;
					for (int m = 0; m < materials.Length; ++m) {
						if (isMega) materials[m] = m_megaMaterialInstance;
						else 		materials[m] = m_fireMaterialInstance;
					}
					m_rChangeMaterials[i].materials = materials;
				}
			}
		}
	}
}
