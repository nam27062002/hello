using UnityEngine;
using System.Collections;

abstract public class IEntity :  MonoBehaviour, ISpawnable {

	// Health
	protected float m_maxHealth;
	protected float m_health;
	public float health { get { return m_health; } set { m_health = value; } }

	public abstract void Spawn(ISpawner _spawner);

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
