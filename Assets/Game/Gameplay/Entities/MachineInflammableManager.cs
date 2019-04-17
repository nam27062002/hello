﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineInflammableManager : UbiBCN.SingletonMonoBehaviour<MachineInflammableManager>, IBroadcastListener { 
	private const float DISINTEGRATE_TIME = 1.25f;

    public struct AshesMaterials
    {
        public Material wait;
        public Material disintegrate;
        public Material end;
    }
    private Dictionary<FireColorSetupManager.FireColorType, AshesMaterials> m_ashesMats = new Dictionary<FireColorSetupManager.FireColorType, AshesMaterials>();
	private Dictionary<FireColorSetupManager.FireColorType, float> m_timers;
    private Dictionary<FireColorSetupManager.FireColorType, List<AI.MachineInflammable>> m_list_wait;
    private Dictionary<FireColorSetupManager.FireColorType, List<AI.MachineInflammable>> m_list_disintegrate;


	public static void Add(AI.MachineInflammable _machine, FireColorSetupManager.FireColorType _fireColor = FireColorSetupManager.FireColorType.RED ) {
		instance.__Add(_machine, _fireColor);
	}

	private void Awake() {
		m_list_wait = new Dictionary<FireColorSetupManager.FireColorType, List<AI.MachineInflammable>>();
		m_list_disintegrate = new Dictionary<FireColorSetupManager.FireColorType, List<AI.MachineInflammable>>();
        m_timers = new Dictionary<FireColorSetupManager.FireColorType, float>();
        int max = (int)FireColorSetupManager.FireColorType.COUNT;
        for (int i = 0; i < max; i++)
        {
            FireColorSetupManager.FireColorType fireColorType = (FireColorSetupManager.FireColorType)i;
            m_list_wait.Add(fireColorType, new List<AI.MachineInflammable>());
            m_list_disintegrate.Add(fireColorType, new List<AI.MachineInflammable>());
            m_timers.Add(fireColorType, 0 );
        }
    }

    public void RegisterColor(FireColorSetupManager.FireColorType _type)
    {
        if ( !m_ashesMats.ContainsKey( _type ) )
        {
            Material sharedAshesMaterial = null;
            switch( _type )
            {
                case FireColorSetupManager.FireColorType.LAVA:
                case FireColorSetupManager.FireColorType.RED:
                    {
                        sharedAshesMaterial = Resources.Load("Game/Materials/BurnToAshes") as Material;
                    }break;
                case FireColorSetupManager.FireColorType.BLUE:
                    {
                        sharedAshesMaterial = Resources.Load("Game/Materials/BurnToAshes") as Material;
                    }break;
                case FireColorSetupManager.FireColorType.ICE:
                    {
                        sharedAshesMaterial = Resources.Load("Game/Materials/BurnToAshesIce") as Material;
                    }break;
            }

            AshesMaterials ashesMaterials = new AshesMaterials();
            ashesMaterials.wait = new Material(sharedAshesMaterial);
            ashesMaterials.wait.SetFloat( GameConstants.Material.ASH_LEVEL , 0f);
            ashesMaterials.disintegrate = new Material(sharedAshesMaterial);
            ashesMaterials.disintegrate.SetFloat( GameConstants.Material.ASH_LEVEL , 0f);
            ashesMaterials.end = new Material(sharedAshesMaterial);
            ashesMaterials.end.SetFloat( GameConstants.Material.ASH_LEVEL , 1f);
            m_ashesMats.Add(_type, ashesMaterials);
        }
    }

    /// <summary>
    /// Component enabled.
    /// </summary>
    private void OnEnable() {
		// Subscribe to external events
		Broadcaster.AddListener(BroadcastEventType.GAME_AREA_EXIT, this);
		Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_EXIT, this);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_ENDED, this);
	}
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.GAME_AREA_EXIT:
            case BroadcastEventType.GAME_ENDED:
            {
                ClearQueues();
            }break;
        }
    }

	private void ClearQueues() {
	    int max = (int)FireColorSetupManager.FireColorType.COUNT;
        for (int i = 0; i < max; i++)
        {
            FireColorSetupManager.FireColorType fireColorType = (FireColorSetupManager.FireColorType)i;
            m_list_wait[fireColorType].Clear();
            m_list_disintegrate[fireColorType].Clear();
        }
	}

	private void __Add(AI.MachineInflammable _machine,  FireColorSetupManager.FireColorType _fireColorType = FireColorSetupManager.FireColorType.RED ) {
		if (m_timers[ _fireColorType ] <= 0.25f) {
			AddToDisintegrateList(_machine, _fireColorType);
		} else {
			AddToWaitQueue(_machine, _fireColorType);
		}
	}

	private void AddToWaitQueue(AI.MachineInflammable _machine, FireColorSetupManager.FireColorType _fireColorType) {
		ChangeMaterials(_machine, m_ashesMats[_fireColorType].wait);
		m_list_wait[_fireColorType].Add(_machine);
	}

	private void AddToDisintegrateList(AI.MachineInflammable _machine, FireColorSetupManager.FireColorType _fireColorType) {
		ChangeMaterials(_machine, m_ashesMats[_fireColorType].disintegrate);
		m_list_disintegrate[_fireColorType].Add(_machine);
	}

	private void ChangeMaterials(AI.MachineInflammable _machine, Material _material) {
		List<Renderer> renderers = _machine.GetBurnableRenderers();
		for (int i = 0; i < renderers.Count; ++i) {
			Material[] materials = renderers[i].materials;
			for (int m = 0; m < materials.Length; ++m) {
				materials[m] = _material;
			}
			renderers[i].materials = materials;
		}
	}

	//
	private void Update() {
		float dt = Time.deltaTime;


        int max = (int)FireColorSetupManager.FireColorType.COUNT;
        for (int c = 0; c < max; c++)
        {
            FireColorSetupManager.FireColorType colorType = (FireColorSetupManager.FireColorType)c;
            // manage the renderers starting to burn
            if (m_list_disintegrate[colorType].Count == 0) {
                if (m_list_wait[colorType].Count > 0) {
                    for (int i = 0; i < m_list_wait[colorType].Count; ++i) {
                        AddToDisintegrateList(m_list_wait[colorType][i], colorType);
                    }
                    m_list_wait[colorType].Clear();
                    m_timers[ colorType ] = 0f;
                }
            } else {
                m_timers[ colorType ] += dt;
                Mathf.Clamp(m_timers[ colorType ], 0f, DISINTEGRATE_TIME);
    
                if (m_timers[ colorType ] >= DISINTEGRATE_TIME) {
                    for (int i = 0; i < m_list_disintegrate[colorType].Count; ++i) {
                        m_list_disintegrate[colorType][i].Burned();
                        ChangeMaterials(m_list_disintegrate[colorType][i], m_ashesMats[colorType].end);
                    }
                    m_list_disintegrate[colorType].Clear();
                    m_timers[ colorType ] = 0f;
                }
    
                m_ashesMats[colorType].disintegrate.SetFloat( GameConstants.Material.ASH_LEVEL , m_timers[ colorType ] / DISINTEGRATE_TIME);
            }
        }

        
	}
}
