// HUDHealthBar.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller for a health bar in the debug hud.
/// </summary>
public class HUDStatBar : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum Type {
		Health,
		Energy,
		Fury,
		SuperFury
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private Type m_type = Type.Health;
	[SerializeField] private float m_maxScreenSize = 1300f;

	private Slider m_extraBar;
	private Slider m_baseBar;
	private TextMeshProUGUI m_valueTxt;
	private GameObject m_icon;
	private GameObject m_iconAnimated = null;
	private List<GameObject> m_extraIcons = null;
	private CanvasGroup m_canvasGroup;
	private float m_timer = 0;
	private float m_timerDuration = 0;
	private bool m_instantSet;
	private ParticleSystem m_particles;

	private bool m_ready = false; // patch for particles!

    private float m_extraBarLastValue = -1f;
    private float m_extraBarLastMaxValue = -1f;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external references
		// m_bar = GetComponentInChildren<Slider>();
		// m_baseBar;
		m_canvasGroup = GetComponent<CanvasGroup>();
		Transform child;
		child = transform.FindChild("ExtraSlider");
		if ( child != null )
			m_extraBar = child.GetComponent<Slider>();
		
		child = transform.FindChild("BaseSlider");
		if ( child != null )
			m_baseBar = child.GetComponent<Slider>();

		m_valueTxt = gameObject.FindComponentRecursive<TextMeshProUGUI>("TextValue");

		m_particles = gameObject.FindComponentRecursive<ParticleSystem>();
		if (m_particles != null)
			m_particles.Stop();


		child = transform.FindChild("Icon");
		if ( child )
		{
			m_icon = child.gameObject;
			m_extraIcons = new List<GameObject>();
		}

		m_instantSet = true;
		m_ready = false;        
    }

	IEnumerator Start()
	{
		while( !InstanceManager.gameSceneControllerBase.IsLevelLoaded())
		{
			yield return null;
		}

		ResizeBars();

		if ( m_type == Type.Health )
		{
			// Check remaining lives to show more health Icons!
			Messenger.AddListener<DamageType>(GameEvents.PLAYER_KO, OnPlayerKo);
			Messenger.AddListener(GameEvents.PLAYER_FREE_REVIVE, OnFreeRevive);
			RefreshIcons();
		}

		Messenger.AddListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);
		m_timer = 10;
		m_timerDuration = 10;
		if ( m_type == Type.SuperFury )
		{
			Messenger.AddListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);
		}

		if (m_type == Type.Energy)
		{
			Messenger.AddListener<bool>(GameEvents.BOOST_TOGGLED, OnBoostToggled);
		}

		m_ready = true;
	}

	void OnDestroy()
	{
		if ( m_type == Type.Health )
		{
			Messenger.RemoveListener<DamageType>(GameEvents.PLAYER_KO, OnPlayerKo);
			Messenger.RemoveListener(GameEvents.PLAYER_FREE_REVIVE, OnFreeRevive);
		}
		else if (m_type == Type.Energy)
		{
			Messenger.RemoveListener<bool>(GameEvents.BOOST_TOGGLED, OnBoostToggled);
		}
		else if ( m_type == Type.SuperFury )
		{
			Messenger.RemoveListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);
		}
		Messenger.RemoveListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);
	}

	/// <summary>
	/// Keep values updated
	/// </summary>
	private void Update() {
		if (m_ready) {
			// Only if player is alive
			if (InstanceManager.player != null) 
			{
				// Aux vars
				float targetBaseValue = GetBaseValue();
				float targetExtraValue = GetExtraValue();
				float targetValue = GetValue();
				float targetValueStep = 0f;
                
				if (m_baseBar != null) {
                    if (m_baseBar.minValue != 0f)
                        m_baseBar.minValue = 0f;

                    if (m_baseBar.maxValue != targetExtraValue)
                        m_baseBar.maxValue = targetExtraValue;

                    targetValueStep = Mathf.Lerp(m_baseBar.value, targetValue, Time.deltaTime);
                }
                
				if (m_extraBar != null) {
                    if (m_extraBar.minValue != 0f)
                        m_extraBar.minValue = 0f;

                    if (m_extraBar.maxValue != targetExtraValue)
                        m_extraBar.maxValue = targetExtraValue; // this is the max value with all the bonus

					targetValueStep = Mathf.Lerp(m_extraBar.value, targetValue, Time.deltaTime);
				}

				//Extra bar                
				if (m_extraBar != null) {
					if (m_instantSet) {
                        if (m_extraBar.value != targetValue)
                            m_extraBar.value = targetValue;

						m_instantSet = false;
					} else {
                        // If going up, animate, otherwise instant set
                        float value = (targetValue > m_extraBar.value) ? targetValueStep : targetValue;                        
                        if (m_extraBar.value != value)
                            m_extraBar.value = value;						
					}
				}

				//Base bar
				if (m_baseBar != null) {
					targetValue = Mathf.Min(targetValue, targetBaseValue);
					targetValueStep = Mathf.Min(targetValueStep, targetBaseValue);

					if (m_instantSet) {
                        if (m_baseBar.value != targetValue)
                            m_baseBar.value = targetValue;

						m_instantSet = false;
					} else {
                        // If going up, animate, otherwise instant set
                        float value = (targetValue > m_baseBar.value) ? targetValueStep : targetValue;
                        if (m_baseBar.value != value)
                            m_baseBar.value = value;                      
					}
				}
                
				if (m_type == Type.SuperFury || m_type == Type.Fury) {
					if (m_particles != null) {
						if (Math.Abs(targetValue - targetValueStep) > 0.001f) {
							m_particles.Play();
						} else  {
							m_particles.Stop();
						}
					}
				}

				// Text
				if (m_valueTxt != null && 
                    (m_extraBarLastValue != m_extraBar.value || m_extraBarLastMaxValue != m_extraBar.maxValue)) {
                    m_extraBarLastValue = m_extraBar.value;
                    m_extraBarLastMaxValue = m_extraBar.maxValue;

                    m_valueTxt.text = String.Format("{0}/{1}",
					                                StringUtils.FormatNumber(m_extraBarLastValue, 0),
					                                StringUtils.FormatNumber(m_extraBarLastMaxValue, 0));                    
				}
			}

			if ( m_type == Type.SuperFury)
			{
				m_timer -= Time.deltaTime;
				if ( m_timer <= 0 )
				{
					m_canvasGroup.alpha = 0;
				}
				else if ( m_timer <= 1 ) 
				{
					m_canvasGroup.alpha = m_timer;
				}
				else if ( m_timer >= (m_timerDuration -1 ))
				{
					m_canvasGroup.alpha = 1.0f - (m_timer - ( m_timerDuration - 1));
				}
				else
				{
					m_canvasGroup.alpha = 1;
				}
			}
		}
	}

	private float GetExtraValue() {
		if ( InstanceManager.player )
		switch (m_type) {
			case Type.Health: 	return InstanceManager.player.healthMax;
			case Type.Energy:	return InstanceManager.player.energyMax;
			case Type.Fury:		return 1;	// [AOC] Furt powerup not yet implemented
			case Type.SuperFury:return 1;	// [AOC] Furt powerup not yet implemented
		}
		return 1;
	}

	private float GetBaseValue() {
		if ( InstanceManager.player )
		switch( m_type ) 
		{
			case Type.Health: 	return InstanceManager.player.healthBase;
			case Type.Energy:	return InstanceManager.player.energyBase;
			case Type.Fury:		return 1;	// [AOC] Furt powerup not yet implemented
			case Type.SuperFury:return 1;	// [AOC] Furt powerup not yet implemented
		}
		return 1;
	}

	private float GetValue() {
		if ( InstanceManager.player )
		switch (m_type) {
			case Type.Health: 	return InstanceManager.player.health;
			case Type.Energy:	return InstanceManager.player.energy;
			case Type.Fury:		return InstanceManager.player.furyProgression;
			case Type.SuperFury:return InstanceManager.player.superFuryProgression;
		}		
		return 0;
	}

	public float GetSizePerUnit()
	{
		if ( InstanceManager.player )
		switch (m_type) {
			case Type.Health: 	return InstanceManager.player.data.def.GetAsFloat("statsBarRatio");
			case Type.Energy:	return InstanceManager.player.data.def.GetAsFloat("statsBarRatio");
		}
		return 0.01f;
	}

	void OnPlayerKo(DamageType _type)
	{
		RefreshIcons();
	}

	void OnFreeRevive()
	{
		RefreshIcons( true );
	}

	private void RefreshIcons( bool showAnimations = false )
	{
		switch( m_type )
		{
			case Type.Health:
			{
				int remainingLives = InstanceManager.player.GetReminingLives();
				int max = Mathf.Max( m_extraIcons.Count, remainingLives);
				for( int i = 0; i<max; i++ )
				{
					while ( i >= m_extraIcons.Count )
					{
						// Add icon
						// Set active false
						GameObject extraIcon = Instantiate( m_icon );
						extraIcon.transform.parent = m_icon.transform.parent;

						RectTransform extraRt = extraIcon.GetComponent<RectTransform>();
						RectTransform rt = m_icon.GetComponent<RectTransform>();

						extraRt.sizeDelta = rt.sizeDelta;
						extraRt.localScale = rt.localScale;
						extraRt.localPosition = rt.localPosition + Vector3.right * (rt.rect.width * 0.2f * (m_extraIcons.Count + 1));
						// extraRt.offsetMax.y = 0;

						Animator anim = extraIcon.GetComponent<Animator>();
						if (anim) {
							anim.enabled = false;
						}

						m_extraIcons.Add( extraIcon );


					}
				}
				if (showAnimations && remainingLives < m_extraIcons.Count) {
					Animator anim = m_extraIcons[remainingLives].GetComponent<Animator>();
					if (anim) {
						anim.enabled = true;
					}
				}
			}break;
		}
	}

	void ResizeBars()
	{
		switch( m_type )
		{
			case Type.Energy:
			case Type.Health:
			{
				RectTransform rectTransform = this.transform as RectTransform;
				Vector2 size = rectTransform.sizeDelta;
				float fraction = Mathf.Clamp01(GetBaseValue() * GetSizePerUnit());
				size.x = fraction * m_maxScreenSize;
				rectTransform.sizeDelta = size;

				// [AOC] Both bars are now anchored to parent and automatically adopt the new size
				/*
				if ( m_baseBar != null )
				{
					rectTransform = m_baseBar.GetComponent<RectTransform>();
					size = rectTransform.sizeDelta;
					fraction = Mathf.Clamp01(GetBaseValue() * GetSizePerUnit());
					size.x = fraction * m_maxScreenSize;
					rectTransform.sizeDelta = size;
				}
				*/
			}break;

			case Type.Fury:
			case Type.SuperFury:
			{
				// [AOC] Furt powerup not yet implemented
				RectTransform rectTransform = this.transform as RectTransform;
				Vector2 size = rectTransform.sizeDelta;
				float fraction = GetExtraValue();
				size.x = fraction * m_maxScreenSize;
				rectTransform.sizeDelta = size;

				// [AOC] Both bars are now anchored to parent and automatically adopt the new size
				/*
				if ( m_baseBar != null )
				{
					rectTransform = m_baseBar.GetComponent<RectTransform>();
					size = rectTransform.sizeDelta;
					fraction = GetBaseValue();
					size.x = fraction * m_maxScreenSize;
					rectTransform.sizeDelta = size;
				}
				*/

			}break;
		}
	}

	private void OnLevelUp(DragonData _data) 
	{
		ResizeBars();
	}

	void OnFuryToggled(bool _active, DragonBreathBehaviour.Type type )
	{
		if ( type == DragonBreathBehaviour.Type.Standard && _active == false)
		{
			m_timer = m_timerDuration = 10;
		}
		else if ( type == DragonBreathBehaviour.Type.Super )
		{
			if ( _active )
			{
				// Fade IN
				m_timer = m_timerDuration = 3600;
			}
			else
			{
				// Fade Out
				m_timer = 1;
			}
		}
		 
		/*if (m_particles != null) {
			if (_active) m_particles.Play();
			else 		 m_particles.Stop();
		}*/
	}

	void OnBoostToggled(bool _active) {
		if (m_particles != null) {
			if (_active) m_particles.Play();
			else 		 m_particles.Stop();
		}
	}

}
