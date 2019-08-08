using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuDragonBirdView : MonoBehaviour {

	private Renderer m_renderer;
	private Material m_originalMaterial;
	private Material m_ashMaterial;
	private const float DISINTEGRATE_TIME = 1.25f;
	public bool m_prepareForBurning = false;
	void Awake()
	{
		// Search renderer
		m_renderer = transform.GetComponentInChildren<Renderer>();
		m_originalMaterial = m_renderer.material;
		if ( m_prepareForBurning )
			m_ashMaterial = Resources.Load("Game/Materials/BurnToAshes") as Material;
	}

	void OnEnable()
	{
		m_renderer.enabled = true;
		if ( m_prepareForBurning )
			m_ashMaterial.SetFloat( GameConstants.Materials.Property.ASH_LEVEL , 0);
		m_renderer.material = m_originalMaterial;
	}

	void Burn()
	{
		m_renderer.enabled = true;
		m_ashMaterial.SetFloat( GameConstants.Materials.Property.ASH_LEVEL , 0);
		// Swap material
		m_renderer.material = m_ashMaterial;
		// Burn
		StartCoroutine(Burning());
	}

	IEnumerator Burning()
	{
		float m_timer = 0;
		while( m_timer < DISINTEGRATE_TIME )
		{
			m_timer += Time.deltaTime;
			m_ashMaterial.SetFloat( GameConstants.Materials.Property.ASH_LEVEL, m_timer / DISINTEGRATE_TIME);
			yield return null;
		}
		m_ashMaterial.SetFloat( GameConstants.Materials.Property.ASH_LEVEL , 1);
		m_renderer.enabled = false;
	}

	void Disappear()
	{
		m_renderer.enabled = false;
	}
}
