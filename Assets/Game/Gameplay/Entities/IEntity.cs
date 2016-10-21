using UnityEngine;
using System.Collections;

abstract public class IEntity :  MonoBehaviour, ISpawnable {

	public const string ENTITY_PREFABS_PATH = "Game/Entities/NewEntites/";
	public const string ENTITY_PREFABS_LOW_PATH = "Game/Entities/NewEntitesLow/";

	private int m_allowEdible;
	public bool allowEdible { get { return m_allowEdible == 0; } set { if (value) { m_allowEdible = Mathf.Max(0, m_allowEdible - 1); } else { m_allowEdible++; } } }

	private int m_allowBurnable;
	public bool allowBurnable { get { return m_allowBurnable == 0; } set { if (value) { m_allowBurnable = Mathf.Max(0, m_allowBurnable - 1); } else { m_allowBurnable++; } } }

	// Health
	protected float m_maxHealth;
	protected float m_health;
	public float health { get { return m_health; } set { m_health = value; } }

	public virtual void Spawn(ISpawner _spawner) {
		m_allowEdible = 0;
		m_allowBurnable = 0;
	}

	protected bool m_isOnScreen = false;
	public bool isOnScreen { get { return m_isOnScreen; } }

	public void Damage(float damage)  {
		m_health -= damage;
	}

	public virtual void Disable(bool _destroyed) {
		m_health = 0f;
		gameObject.SetActive(false);
	}
}
