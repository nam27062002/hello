using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VegetationMovement : MonoBehaviour 
{

	private List<Material> m_materials;

	private DragonPlayer m_player;
	private DragonBoostBehaviour m_boost;
	private DragonMotion m_playerMotion;

	[Range(0,10)]
	public float m_maxDisplacement = 1;
	private float m_currentDisplacement = 0;

	[Range(0,1000)]
	public float m_distanceCheck = 1;


	// Use this for initialization
	void Start () 
	{
		m_materials = new List<Material>();
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		for( int i = 0; i<renderers.Length; i++ )
		{
			Renderer r = renderers[i];
			for( int j = 0; j < r.materials.Length; j++ )
			{
				if ( r.materials[j].shader.name.EndsWith("Vegetation") )
				{	
					r.materials[j].SetFloat("_Height", r.bounds.size.y);
					m_materials.Add( r.materials[j] );
				}
			}
		}

		m_player = InstanceManager.player;
		m_boost = m_player.GetComponent<DragonBoostBehaviour>();
		m_playerMotion = m_player.GetComponent<DragonMotion>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		bool noDisplacement = true;
		// Check if player is doind a turbo
		if ( m_boost.IsBoostActive() )
		{
			if ( m_playerMotion.GetDirection().x > 0 )
			{
				if ( transform.position.x < m_player.transform.position.x )
				{
					if ( (transform.position - m_player.transform.position).sqrMagnitude < m_distanceCheck )
					{
						noDisplacement = false;
						m_currentDisplacement += Time.deltaTime * 10;
					}
				}
			}
			else
			{
				if ( transform.position.x > m_player.transform.position.x )
				{
					if ( (transform.position - m_player.transform.position).sqrMagnitude < m_distanceCheck )
					{
						noDisplacement = false;
						m_currentDisplacement -= Time.deltaTime * 10;
					}
				}
			}

			// clamp
			m_currentDisplacement = Mathf.Clamp(m_currentDisplacement, -m_maxDisplacement, m_maxDisplacement);
		}


		if (noDisplacement)
		{
			m_currentDisplacement = m_currentDisplacement * 0.95f;
		}

		// Check if value is different enough?

		//
		for( int i = 0; i<m_materials.Count; i++ )
		{
			m_materials[i].SetFloat("_WindDiplacement", m_currentDisplacement);
		}

	}
}
