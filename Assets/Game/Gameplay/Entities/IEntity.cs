﻿using UnityEngine;
using System.Collections;

abstract public class IEntity :  MonoBehaviour, ISpawnable {

	public const string ENTITY_PREFABS_PATH = "Game/Entities/NewEntites/";
	public const string ENTITY_PREFABS_LOW_PATH = "Game/Entities/NewEntitesLow/";
    
    /// <summary>
    /// Returns the path where the prefabs for entities are stored. It depends on the quality settings
    /// </summary>
    public static string EntityPrefabsPath
    {
        get
        {
            // Entities LOD flag has been disabled because it's not really worth it
            //return (FeatureSettingsManager.instance.EntitiesLOD == FeatureSettings.ELevel2Values.low) ? ENTITY_PREFABS_LOW_PATH : ENTITY_PREFABS_PATH;
            return ENTITY_PREFABS_PATH;
        }
    }

	private int m_allowEdible;
	public bool allowEdible { get { return m_allowEdible == 0; } set { if (value) { m_allowEdible = Mathf.Max(0, m_allowEdible - 1); } else { m_allowEdible++; } } }

	private int m_allowBurnable;
	public bool allowBurnable { get { return m_allowBurnable == 0; } set { if (value) { m_allowBurnable = Mathf.Max(0, m_allowBurnable - 1); } else { m_allowBurnable++; } } }


	protected DefinitionNode m_def;
	public 	  DefinitionNode def { get { return m_def; } }

	// Health
	protected float m_maxHealth;
	protected float m_health;
	public float health { get { return m_health; } set { m_health = value; } }

	public virtual void Spawn(ISpawner _spawner) {
		m_health = m_maxHealth;

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

    public virtual void LogicUpdate() {}
    public virtual bool CanDieOutsideFrustrum() { return true; }

	public virtual CircleArea2D circleArea { get{ return null; } }
}
