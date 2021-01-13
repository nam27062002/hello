using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalSetup : MonoBehaviour {
	[SeparatorAttribute("References")]
	[SerializeField] private GameObject m_arrow;
	[SerializeField] private GameObject m_arrowBurned;
	[SerializeField] private Renderer m_sticker;
	[SerializeField] private Material m_sharedStickerMaterial;

	[SeparatorAttribute("Setup")]
	[SerializeField] private bool m_arrowVisible = true;
	[SerializeField] private Vector3 m_arrowRotation = GameConstants.Vector3.zero;
	[SerializeField] private int m_stickerIndex = 0;

	private Material m_customMaterial;

	void Start() {
		UpdateSticker();
		UpdateArrowRotation();
		UpdateArrowVisibility();
	}

	public void UpdateSticker() {
		if (m_customMaterial == null) {
			m_customMaterial = new Material(m_sharedStickerMaterial);
			m_sticker.material = m_customMaterial;
		}

		m_customMaterial.SetInt("_IDSignal", m_stickerIndex);
	}

	public void UpdateArrowRotation() {
		m_arrow.transform.localRotation = Quaternion.Euler(m_arrowRotation);
		m_arrowBurned.transform.localRotation = Quaternion.Euler(m_arrowRotation);
	}

	public void UpdateArrowVisibility() {
		m_arrow.SetActive(m_arrowVisible);
		m_arrowBurned.SetActive(m_arrowVisible);
	}
}
