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

	public enum Bars {
		BASE,
		EXTRA,
		DAMAGE,

		COUNT
	}

	private const float DAMAGE_BAR_ANIMATION_THRESHOLD = 10f;	// Pixels

	private class BarData {
		public Slider slider = null;
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private Type m_type = Type.Health;
	[SerializeField] private float m_maxScreenSize = 1300f;
	[Space]
	[SerializeField] [Range(0f, 1f)] private float m_damageBarSpeed = 0.25f;	// %/sec

	private BarData[] m_bars = new BarData[(int)Bars.COUNT];
	private BarData baseBar { get { return m_bars[(int)Bars.BASE]; } }
	private BarData extraBar { get { return m_bars[(int)Bars.EXTRA]; } }
	private BarData damageBar { get { return m_bars[(int)Bars.DAMAGE]; } }

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

	private float m_damageAnimationThreshold = 0f;

	// Invulnerability FX
	private GameObject m_invulnerabilityGlow = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Aux vars
		Transform child;

		// Initialize bars
		string[] barNames = { "BaseSlider", "ExtraSlider", "DamageSlider" };
		for(int i = 0; i < (int)Bars.COUNT; i++) {
			m_bars[i] = new BarData();
			child = transform.FindChild(barNames[i]);
			if(child != null) {
				m_bars[i].slider = child.GetComponent<Slider>();
			}
		}

		// Other external references
		m_canvasGroup = GetComponent<CanvasGroup>();
		m_valueTxt = gameObject.FindComponentRecursive<TextMeshProUGUI>("TextValue");

		child = transform.FindChild("InvulnerabilityGlow");
		if(child != null) {
			m_invulnerabilityGlow = child.gameObject;
		}

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
			Messenger.AddListener<DamageType, Transform>(GameEvents.PLAYER_KO, OnPlayerKo);
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
			Messenger.RemoveListener<DamageType, Transform>(GameEvents.PLAYER_KO, OnPlayerKo);
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
			if(InstanceManager.player != null) {
				// Aux vars
				Slider targetSlider = null;
				float targetBaseValue = GetBaseValue();
				float targetExtraValue = GetExtraValue();
				float targetValue = GetValue();
				float targetValueStep = 0f;
                
				// Set base slider min/max
				targetSlider = baseBar.slider;
				if(targetSlider != null) {
					if(targetSlider.minValue != 0f) {
						targetSlider.minValue = 0f;
					}

					if(targetSlider.maxValue != targetExtraValue) {
						targetSlider.maxValue = targetExtraValue;
					}

					targetValueStep = Mathf.Lerp(targetSlider.value, targetValue, Time.deltaTime);
				}
                
				// Set extra slider min/max
				targetSlider = extraBar.slider;
				if(targetSlider != null) {
					if(targetSlider.minValue != 0f) {
						targetSlider.minValue = 0f;
					}

					if(targetSlider.maxValue != targetExtraValue) {
						targetSlider.maxValue = targetExtraValue; // this is the max value with all the bonus
					}

					targetValueStep = Mathf.Lerp(targetSlider.value, targetValue, Time.deltaTime);
				}

				// Set damage slider min/max
				targetSlider = damageBar.slider;
				if(targetSlider != null) {
					if(targetSlider.minValue != 0f) {
						targetSlider.minValue = 0f;
					}

					if(targetSlider.maxValue != targetExtraValue) {
						targetSlider.maxValue = targetExtraValue; // this is the max value with all the bonus
					}
				}

				// Extra bar value
				targetSlider = extraBar.slider;
				if(targetSlider != null) {
					if(m_instantSet) {
						if(targetSlider.value != targetValue) {
							targetSlider.value = targetValue;
						}
					} else {
						// If going up, animate, otherwise instant set
						float value = (targetValue > targetSlider.value) ? targetValueStep : targetValue;                        
						if(targetSlider.value != value) {
							targetSlider.value = value;	
						}
					}
				}

				// Base bar value
				targetSlider = baseBar.slider;
				if(targetSlider != null) {
					targetValue = Mathf.Min(targetValue, targetBaseValue);
					targetValueStep = Mathf.Min(targetValueStep, targetBaseValue);

					if(m_instantSet) {
						if(targetSlider.value != targetValue) {
							targetSlider.value = targetValue;
						}
					} else {
						// If going up, animate, otherwise instant set
						float value = (targetValue > targetSlider.value) ? targetValueStep : targetValue;
						if(targetSlider.value != value) {
							targetSlider.value = value;   
						}
					}
				}

				// Damage bar value
				targetSlider = damageBar.slider;
				if(targetSlider != null) {
					// Target value is the max between the extra bar value and the base var balue
					targetValue = Mathf.Max(extraBar.slider.value, baseBar.slider.value);

					if(m_instantSet) {
						if(targetSlider.value != targetValue) {
							targetSlider.value = targetValue;
						}
					} else {
						// Reverse case: animate when going down only and over a certain threshold
						if(targetSlider.value > targetValue + m_damageAnimationThreshold) {
							// Use normalized value so speed feels right for every dragon
							targetSlider.normalizedValue -= m_damageBarSpeed * Time.deltaTime;
						} else if(targetSlider.value < targetValue) {
							targetSlider.value = targetValue;
						}
					}
				}
                
				if(m_type == Type.SuperFury || m_type == Type.Fury) {
					if(m_particles != null) {
						if(Math.Abs(targetValue - targetValueStep) > 0.001f) {
							m_particles.Play();
						} else {
							m_particles.Stop();
						}
					}
				}

				// Text
				if(m_valueTxt != null &&
				    (m_extraBarLastValue != extraBar.slider.value || m_extraBarLastMaxValue != extraBar.slider.maxValue)) {
					m_extraBarLastValue = extraBar.slider.value;
					m_extraBarLastMaxValue = extraBar.slider.maxValue;

					m_valueTxt.text = String.Format("{0}/{1}",
						StringUtils.FormatNumber(m_extraBarLastValue, 0),
						StringUtils.FormatNumber(m_extraBarLastMaxValue, 0));                    
				}

				// Invulnerability FX
				if(m_invulnerabilityGlow != null) {
					// Only Health and Boost bars
					if(m_type == Type.Health || m_type == Type.Energy) {
						// Are we invulnerable?
						bool isInvulnerable = false;
						if(m_type == Type.Health) {
							isInvulnerable = InstanceManager.player.IsInvulnerable() || DebugSettings.invulnerable;
						} else if(m_type == Type.Energy) {
							isInvulnerable = !InstanceManager.player.dragonBoostBehaviour.IsDraining();
						}

						m_invulnerabilityGlow.SetActive(isInvulnerable);
					}
					/*// Update FX
					bool applyFX = m_instantSet;
					if(isInvulnerable) {
						// Update delta
						m_invulnerabilityColorDelta += m_invulnerabilityFXSpeed * m_invulnerabilityDirection * Time.deltaTime;

						// Reverse direction if needed
						if(m_invulnerabilityColorDelta >= 1f) {
							m_invulnerabilityDirection = -1f;
						} else if(m_invulnerabilityColorDelta <= 0f) {
							m_invulnerabilityDirection = 1f;
						}

						// Clamp
						m_invulnerabilityColorDelta = Mathf.Clamp01(m_invulnerabilityColorDelta);

						// Update flags
						m_wasInvulnerable = true;
						applyFX = true;
					} else if(m_wasInvulnerable) {
						// Just apply original color
						m_invulnerabilityColorDelta = 0f;
						applyFX = true;
					}

					// Apply color!
					if(applyFX) {
						if(baseBar.gradient != null) {
							baseBar.gradient.color1 = Color.Lerp(baseBar.originalColor1, m_invulnerabilityColor, m_invulnerabilityColorDelta);
							baseBar.gradient.color2 = Color.Lerp(baseBar.originalColor2, m_invulnerabilityColor, m_invulnerabilityColorDelta);
						} else if(baseBar.image != null) {
							baseBar.image.color = Color.Lerp(baseBar.originalColor, m_invulnerabilityColor, m_invulnerabilityColorDelta);
						}

						if(extraBar.gradient != null) {
							extraBar.gradient.color1 = Color.Lerp(extraBar.originalColor1, m_invulnerabilityColor, m_invulnerabilityColorDelta);
							extraBar.gradient.color2 = Color.Lerp(extraBar.originalColor2, m_invulnerabilityColor, m_invulnerabilityColorDelta);
						} else if(extraBar.image != null) {
							extraBar.image.color = Color.Lerp(extraBar.originalColor, m_invulnerabilityColor, m_invulnerabilityColorDelta);
						}
					}*/
				}

				// Reset instant set flag
				if(m_instantSet) {
					m_instantSet = false;
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
			case Type.Fury:		return 1;	// [AOC] Fury powerup not yet implemented
			case Type.SuperFury:return 1;	// [AOC] Fury powerup not yet implemented
		}
		return 1;
	}

	private float GetBaseValue() {
		if ( InstanceManager.player )
		switch( m_type ) 
		{
			case Type.Health: 	return InstanceManager.player.healthBase;
			case Type.Energy:	return InstanceManager.player.energyBase;
			case Type.Fury:		return 1;	// [AOC] Fury powerup not yet implemented
			case Type.SuperFury:return 1;	// [AOC] Fury powerup not yet implemented
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

	void OnPlayerKo(DamageType _type, Transform _source)
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
		RectTransform rectTransform = this.transform as RectTransform;
		Vector2 size = rectTransform.sizeDelta;
		switch( m_type )
		{
			case Type.Energy:
			case Type.Health:
			{
				float fraction = Mathf.Clamp01(GetBaseValue() * GetSizePerUnit());
				size.x = fraction * m_maxScreenSize;
				rectTransform.sizeDelta = size;
			}break;

			case Type.Fury:
			case Type.SuperFury:
			{
				// [AOC] Fury powerup not yet implemented
				float fraction = GetExtraValue();
				size.x = fraction * m_maxScreenSize;
				rectTransform.sizeDelta = size;
			}break;
		}

		// How many units correspond to the minimum threshold in pixels?
		m_damageAnimationThreshold = DAMAGE_BAR_ANIMATION_THRESHOLD/size.x;
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
		else if ( type == DragonBreathBehaviour.Type.Mega )
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
