﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

abstract public class IEntity :  MonoBehaviour, ISpawnable {

	public const string ENTITY_PREFABS_PATH = "Game/Entities/NewEntites/";
	public const string ENTITY_PREFABS_LOW_PATH = "Game/Entities/NewEntitesLow/";
    
    /// <summary>
    /// Returns the path where the prefabs for entities are stored. It depends on the quality settings
    /// </summary>
    public static string EntityPrefabsPath {
        get {
            // Entities LOD flag has been disabled because it's not really worth it
            //return (FeatureSettingsManager.instance.EntitiesLOD == FeatureSettings.ELevel2Values.low) ? ENTITY_PREFABS_LOW_PATH : ENTITY_PREFABS_PATH;
            return ENTITY_PREFABS_PATH;
        }
    }

	private int m_allowEdible;
	public bool allowEdible { get { return m_allowEdible == 0; } set { if (value) { m_allowEdible = Mathf.Max(0, m_allowEdible - 1); } else { m_allowEdible++; } } }

	private int m_allowBurnable;
	public bool allowBurnable { get { return m_allowBurnable == 0; } set { if (value) { m_allowBurnable = Mathf.Max(0, m_allowBurnable - 1); } else { m_allowBurnable++; } } }

	public virtual DragonTier edibleFromTier { get { return DragonTier.COUNT; } set { } }

	protected DefinitionNode m_def;
	public 	  DefinitionNode def { get { return m_def; } }
	public virtual string sku { get { return string.Empty; }}

	// Health
	protected float m_maxHealth;
	protected float m_health;
	public float health { get { return m_health; } set { m_health = value; } }

	public virtual int score { get { return 0; } }

	protected ISpawnable[] m_otherSpawnables;
	protected int m_otherSpawnablesCount;
	protected AI.IMachine m_machine;
	public AI.IMachine machine { get { return m_machine; } }

	protected IViewControl m_viewControl;

	protected virtual void Awake() {
		ISpawnable[] spawners = GetComponents<ISpawnable>();
		m_otherSpawnables = new ISpawnable[ spawners.Length - 1 ];
		m_otherSpawnablesCount = 0;
		ISpawnable thisSpawn = this as ISpawnable;
		for (int i = 0; i < spawners.Length; i++) {
			if (spawners[i] != thisSpawn)
			{
				m_otherSpawnables[m_otherSpawnablesCount] = spawners[i];
				m_otherSpawnablesCount++;
			}
		}
		m_machine = GetComponent<AI.IMachine>();
		m_viewControl = GetComponent<IViewControl>();
	}

	public virtual void Spawn(ISpawner _spawner) {
		m_health = m_maxHealth;

		m_allowEdible = 0;
		m_allowBurnable = 0;
	}

	protected bool m_isGolden = false;
	public bool isGolden { get { return m_isGolden; } }

	public virtual void SetGolden(Spawner.EntityGoldMode _mode) {
		m_isGolden = false;
	}

	protected bool m_isOnScreen = false;
	public bool isOnScreen { get { return m_isOnScreen; } }

	public int GetVertexCount() {
		if (m_viewControl != null && m_isOnScreen) {
			return m_viewControl.vertexCount;
		}
		return 0;
	}

	public int GetRendererCount() {
		if (m_viewControl != null && m_isOnScreen) {
			return m_viewControl.rendererCount;
		}
		return 0;
	}

	public void Damage(float damage) {
		m_health -= damage;
	}

	public virtual void Disable(bool _destroyed) {
		m_health = 0f;
		gameObject.SetActive(false);
	}

	public virtual Reward GetOnKillReward(bool _burnt) {
		return new Reward();
	}

    public virtual void CustomUpdate() {
		for (int i = 0; i < m_otherSpawnablesCount; i++) {
			m_otherSpawnables[i].CustomUpdate();
    	}
    }

	public virtual void CustomFixedUpdate() {
		if (m_machine != null) m_machine.CustomFixedUpdate();
	}

	public virtual bool CanBeSmashed()			{ return false; }
    public virtual bool CanDieOutsideFrustrum() { return true; }
	public virtual CircleArea2D circleArea 		{ get { return null; } }
}
