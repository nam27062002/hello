﻿using UnityEngine;
using System.Collections.Generic;

public class DecorationManager : Singleton<DecorationManager>, IBroadcastListener
{
    private List<Decoration> m_decorations;
    private List<AmbientHazard> m_ambientHazards;
    
    	
    public enum OverlapingMethod
    {
        EntitiesManager,
        Box,
        Capsule
    }
    private OverlapingMethod m_overlapingMethod = OverlapingMethod.EntitiesManager;
    public OverlapingMethod overlapingMethod
    {
        get { return m_overlapingMethod; }
        set { m_overlapingMethod = value; }
    }

    private bool m_updateEnabled;


    protected override void OnCreateInstance() {
        m_decorations = new List<Decoration>();
        m_ambientHazards = new List<AmbientHazard>();
        
        m_updateEnabled = false;

        Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
        Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);
        Broadcaster.AddListener(BroadcastEventType.GAME_AREA_EXIT, this);
        Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
    }

    protected override void OnDestroyInstance() {
        Broadcaster.RemoveListener(BroadcastEventType.GAME_ENDED, this);
        Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_EXIT, this);
        Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);
        Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
    }
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch(eventType) 
        {
            case BroadcastEventType.GAME_LEVEL_LOADED:  m_updateEnabled = true; break;
            case BroadcastEventType.GAME_AREA_ENTER:    m_updateEnabled = true; break;
            case BroadcastEventType.GAME_AREA_EXIT:
                {
                    FreezingObjectsRegistry.instance.ClearEntities();
                    FreezingObjectsRegistry.instance.ClearScalings();
                    m_updateEnabled = false;
                }
                break;
            case BroadcastEventType.GAME_ENDED:         m_updateEnabled = false; OnGameEnded(); break;
        }
    }



	public void RegisterDecoration(Decoration _deco)	{ 
        // TODO: Check with @Marc if we can avoid this check
        if ( !m_decorations.Contains(_deco) ){
            m_decorations.Add(_deco); 
        }
    }
	public void UnregisterDecoration(Decoration _deco)	{ m_decorations.Remove(_deco); }

    public void RegisterAmbientHazard(AmbientHazard _hazard)	{ m_ambientHazards.Add(_hazard); }
	public void UnregisterAmbientHazard(AmbientHazard _hazard)	{ m_ambientHazards.Remove(_hazard); }



    public void Update()
	{
        if (m_updateEnabled) {
            int i;
            int count;
            float delta = Time.deltaTime;
                      
            count = m_decorations.Count - 1;
            for (i = count; i >= 0; i--) {
                m_decorations[i].CustomUpdate();
            }

            count = m_ambientHazards.Count-1;
            for (i = count; i >= 0; i--)
            {
                m_ambientHazards[i].CustomUpdate( delta );
            }
        }
    }


	void OnGameEnded() {
		if (m_decorations != null)  { m_decorations.Clear(); }
        if (m_ambientHazards != null) { m_ambientHazards.Clear(); } 
	}

    #region debug
    private bool m_entitiesVisibility = true;
    public bool Debug_EntitiesVisibility {
        get {
            return m_entitiesVisibility;
        }

        set {
            m_entitiesVisibility = value;
        }
    }
    // Check if alive and not dying prior to force golden
    #endregion
}
