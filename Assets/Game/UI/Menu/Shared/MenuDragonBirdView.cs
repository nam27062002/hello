using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuDragonBirdView : MonoBehaviour {

	private Renderer m_renderer;
	private Material m_originalMaterial;
	private Material m_ashMaterial;
	private const float DISINTEGRATE_TIME = 1.25f;
	public bool m_prepareForBurning = false;
	public bool m_prepareForFreeze = false;
	private AddressablesOp m_loadingRequest = null;
	private GameObject m_freezingParticle;
	public Transform m_anchor;

	void Awake()
	{
		// Search renderer
		m_renderer = transform.GetComponentInChildren<Renderer>();
		m_originalMaterial = m_renderer.material;
		if ( m_prepareForBurning )
			m_ashMaterial = Resources.Load("Game/Materials/BurnToAshes") as Material;
		if ( m_prepareForFreeze )
		{
			m_loadingRequest = HDAddressablesManager.Instance.LoadAssetAsync("FX_FrozenSmallNPC", "Master");
		}
	}

	void OnEnable()
	{
		m_renderer.enabled = true;
		if ( m_prepareForBurning )
			m_ashMaterial.SetFloat( GameConstants.Materials.Property.ASH_LEVEL , 0);
		m_renderer.material = m_originalMaterial;
	}

	void OnDisable()
	{
		if ( m_freezingParticle != null )
			m_freezingParticle.gameObject.SetActive(false);
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

	public void Freeze()
	{
		StartCoroutine(SpawnFreezing());
	}

	IEnumerator SpawnFreezing()
	{
		if ( m_loadingRequest != null )
		{
			if (m_freezingParticle == null)
			{
				while( !m_loadingRequest.isDone )
				{
					yield return null;
				}
				GameObject go = m_loadingRequest.GetAsset<GameObject>();
				m_freezingParticle = Instantiate(go,m_anchor);
			}
			m_freezingParticle.SetActive(true);
			Transform tr = m_freezingParticle.transform;
			tr.localPosition = GameConstants.Vector3.zero;
			tr.localRotation = GameConstants.Quaternion.identity;
			float time = 0;
			while( time < 0.5f )
			{
				tr.localScale = GameConstants.Vector3.one * time / 0.5f;
				time += Time.deltaTime;
				yield return null;
			}
		}
	}

	void Disappear()
	{
		m_renderer.enabled = false;
	}
}
