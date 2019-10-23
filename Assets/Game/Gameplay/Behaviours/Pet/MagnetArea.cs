using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetArea : MonoBehaviour {

	public float m_magnetForce = 10;
	public DragonTier m_magnetTier = DragonTier.TIER_3;
	private CircleArea2D m_circle;
	private Entity[] m_checkEntities = new Entity[50];
	private int m_numCheckEntities = 0;

	// Use this for initialization
	void Start () 
	{
		m_circle = GetComponent<CircleArea2D>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		Vector3 pos = transform.position;
		m_numCheckEntities =  EntityManager.instance.GetOverlapingEntities((Vector2)m_circle.center, m_circle.radius, m_checkEntities);
		for (int i = 0; i < m_numCheckEntities; i++) 
		{
			Entity prey = m_checkEntities[i];
			if (prey.IsEdible(m_magnetTier))
			{
				AI.IMachine machine = prey.machine;
				if (machine != null) {
					Vector3 dir = pos - machine.position;
					machine.AddExternalForce( dir.normalized * Mathf.Min(m_magnetForce, dir.sqrMagnitude) );
				}
			}
		}
	}
}
