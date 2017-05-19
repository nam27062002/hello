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


	private ParticleSystem.MinMaxGradient m_fireStartColorGradient;
	private ParticleSystem.MinMaxGradient m_megaStartColorGradient;

	private ParticleSystem.MinMaxGradient m_fireColorOverTimeGradient;
	private ParticleSystem.MinMaxGradient m_megaColorOverTimeGradient;


	void Awake() {
		m_fireStartColorGradient = new ParticleSystem.MinMaxGradient(m_fireStartColorA, m_fireStartColorB);
		m_megaStartColorGradient = new ParticleSystem.MinMaxGradient(m_fireGradient);

		m_fireColorOverTimeGradient = new ParticleSystem.MinMaxGradient(m_megaStartColorA, m_megaStartColorB);
		m_megaColorOverTimeGradient = new ParticleSystem.MinMaxGradient(m_megaGradient);
	}


	void OnEnable() {
		if (InstanceManager.player != null) {
			bool isMega = InstanceManager.player.IsSuperFuryOn();

			for (int i = 0; i < m_fireOnly.Length; ++i) {
				m_fireOnly[i].SetActive(isMega == false);
			}

			for (int i = 0; i < m_megaOnly.Length; ++i) {
				m_megaOnly[i].SetActive(isMega == true);
			}

			for (int i = 0; i < m_psChangeStartColors.Length; ++i) {
				ParticleSystem.MainModule main = m_psChangeStartColors[i].main;

				if (isMega) main.startColor = m_megaStartColorGradient;
				else 		main.startColor = m_fireStartColorGradient;
			}

			for (int i = 0; i < m_psChangeGradientColors.Length; ++i) {
				ParticleSystem.ColorOverLifetimeModule colorOverLifetime = m_psChangeGradientColors[i].colorOverLifetime;

				if (isMega) colorOverLifetime.color = m_megaColorOverTimeGradient;
				else 		colorOverLifetime.color = m_fireColorOverTimeGradient;
			}

			for (int i = 0; i < m_rChangeMaterials.Length; ++i) {
				Material[] materials = m_rChangeMaterials[i].materials;
				for (int m = 0; m < materials.Length; ++m) {
					if (isMega) materials[m] = m_fireMaterial;
					else 		materials[m] = m_megaMaterial;
				}
			}
		}
	}
}
