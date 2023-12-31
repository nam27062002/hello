﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class QuickDragonSettings : MonoBehaviour {

	public Slider m_sliderAcceleration;
	public Slider m_sliderMass;
	public Slider m_sliderFricction;
	public Slider m_sliderGravity;
	public TMP_Dropdown m_eatTypeDropDown;
    public TMP_Dropdown m_spawnAreas;
    public TMP_Dropdown m_spawnPoints;
    private DragonMotion m_motion;

    private List<string> m_areas;
    private List<List<string>> m_points;

	void OnEnable()
	{
		if ( InstanceManager.player != null )
		{
			m_motion = InstanceManager.player.GetComponent<DragonMotion>();

			m_sliderAcceleration.value = m_motion.m_dragonForce;
			m_sliderMass.value = m_motion.m_dragonMass;
			m_sliderFricction.value = m_motion.m_dragonFricction;
			switch( EntityManager.instance.overlapingMethod )
			{
				case EntityManager.OverlapingMethod.EntitiesManager: m_eatTypeDropDown.value = 0;break;
				case EntityManager.OverlapingMethod.Capsule: m_eatTypeDropDown.value = 1;break;
				case EntityManager.OverlapingMethod.Box: m_eatTypeDropDown.value = 2;break;
			}
			m_sliderGravity.value = m_motion.m_dragonGravityModifier;

		}

        m_areas = new List<string>();
        m_points = new List<List<string>>();
        List<DefinitionNode> spawnPoints = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.LEVEL_SPAWN_POINTS);
        foreach (DefinitionNode spawnPoint in spawnPoints) {
            string area = spawnPoint.Get("area");
            System.Predicate<string> match = s => s.Equals(area);
            int index = m_areas.FindIndex(match);
            if (index < 0) {
                index = m_areas.Count;
                m_areas.Add(area);
                m_points.Add(new List<string>());
            }

            m_points[index].Add(spawnPoint.sku);
        }

        m_spawnAreas.ClearOptions();
        m_spawnAreas.AddOptions(m_areas);
        m_spawnAreas.value = 0;
        m_spawnPoints.ClearOptions();
        m_spawnPoints.AddOptions(m_points[0]);
        m_spawnPoints.value = 0;
    }
	
	public void SetDragonAcceleration(float _size) 
	{
		if ( m_motion != null )
			m_motion.m_dragonForce = _size;
	}

	public void SetDragonMass(float _size) 
	{
		if ( m_motion != null )
			m_motion.m_dragonMass = _size;
	}

	public void SetDragonFricction(float _size) 
	{
		if ( m_motion != null )
			m_motion.m_dragonFricction = _size;
	}

	public void SetGravityModifier(float _size) 
	{
		if ( m_motion != null )
			m_motion.m_dragonGravityModifier = _size;
	}

	public void SetEatType(int type)
	{
		if (EntityManager.instance != null)	
		{
			switch( type )
			{
				case 0:EntityManager.instance.overlapingMethod = EntityManager.OverlapingMethod.EntitiesManager;break;
				case 1:EntityManager.instance.overlapingMethod = EntityManager.OverlapingMethod.Capsule;break;
				case 2:EntityManager.instance.overlapingMethod = EntityManager.OverlapingMethod.Box;break;
			}
		}
	}	

	public void SetRespawnTo0( bool _value )
	{
		DebugSettings.ignoreSpawnTime = _value;
	}

	public void SetSpawnChance0( bool _value )
	{
		DebugSettings.spawnChance0 = _value;
	}

	public void SetSpawnChance100( bool _value )
	{
		DebugSettings.spawnChance100 = _value;
	}


    public void SetOverridePlayerSpawn(bool _value) {
        DebugSettings.overrideSpawnPoint = _value;
    }

    public void OnAreaChange(int _value) {
        m_spawnPoints.value = 0;
        m_spawnPoints.ClearOptions();
        m_spawnPoints.AddOptions(m_points[_value]);

        DebugSettings.spawnArea = m_areas[_value];
        DebugSettings.spawnPoint = m_points[_value][0];
    }

    public void OnPointChange(int _value) {
        DebugSettings.spawnPoint = m_points[m_spawnAreas.value][_value];
    }

	public void Toggle60FPS( bool _value )
	{
		if ( _value )
		{
			Application.targetFrameRate = 60;
			Time.captureFramerate = 60;
		}
		else
		{
			Application.targetFrameRate = 30;
			Time.captureFramerate = 30;
		}
	}

	public void ToggleHitStop( bool _value )
	{
		DebugSettings.hitStopEnabled = _value;
	}


    public void ToggleNight(bool _value)
    {
        if (_value)
        {
            Shader.EnableKeyword("NIGHT");
        }
        else
        {
            Shader.DisableKeyword("NIGHT");
        }
    }


}
