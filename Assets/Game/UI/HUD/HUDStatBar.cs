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
	// MEMBERS															//
	//------------------------------------------------------------------//
	[SerializeField] private Type m_type = Type.Health;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private Slider m_bar;
	private Slider m_baseBar;
	private Text m_valueTxt;
	private GameObject m_icon;
	private List<GameObject> m_extraIcons = null;
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
		Transform child;
		child = transform.FindChild("Slider");
		if ( child != null )
			m_bar = child.GetComponent<Slider>();
		
		child = transform.FindChild("Slider/BaseSlider");
		if ( child != null )
			m_baseBar = child.GetComponent<Slider>();

		m_valueTxt = gameObject.FindComponentRecursive<Text>("TextValue");

		child = transform.FindChild("Icon");
		if ( child )
		{
			m_icon = child.gameObject;
			m_extraIcons = new List<GameObject>();

		}
	}

	IEnumerator Start()
	{
		while( !InstanceManager.GetSceneController<GameSceneControllerBase>().IsLevelLoaded())
		{
			yield return null;
		}
		RectTransform rectTransform;
		Vector2 size;

		rectTransform = m_bar.GetComponent<RectTransform>();
		size = rectTransform.sizeDelta;
		size.x = GetMaxValue() * GetSizePerUnit();
		rectTransform.sizeDelta = size;

		if ( m_baseBar != null )
		{
			rectTransform = m_baseBar.GetComponent<RectTransform>();
			size = rectTransform.sizeDelta;
			size.x = (GetMaxValue() - GetPowerUpsAddition()) * GetSizePerUnit();
			rectTransform.sizeDelta = size;
		}

		if ( m_type == Type.Health )
		{
			// Check remaining lives to show more health Icons!
			Messenger.AddListener(GameEvents.PLAYER_KO, OnPlayerKo);
			Messenger.AddListener(GameEvents.PLAYER_FREE_REVIVE, OnFreeRevive);
			RefreshIcons();
		}
	}

	void OnDestroy()
	{
		if ( m_type == Type.Health )
		{
			Messenger.RemoveListener(GameEvents.PLAYER_KO, OnPlayerKo);
			Messenger.RemoveListener(GameEvents.PLAYER_FREE_REVIVE, OnFreeRevive);
		}
	}

	/// <summary>
	/// Keep values updated
	/// </summary>
	private void Update() {
		// Only if player is alive
		if (InstanceManager.player != null) {
			// Bar value
			m_bar.minValue = 0f;
			m_bar.maxValue = GetMaxValue();
			m_bar.value = GetValue();

			if ( m_baseBar != null )
			{
				m_baseBar.minValue = 0f;
				m_baseBar.maxValue = GetMaxValue() - GetPowerUpsAddition();
				m_baseBar.value = GetValue();
			}
			// Text
			if (m_valueTxt != null) {
				m_valueTxt.text = String.Format("{0}/{1}",
				                                StringUtils.FormatNumber(m_bar.value, 0),
				                                StringUtils.FormatNumber(m_bar.maxValue, 0));
			}
		}
	}

	private float GetMaxValue() {
		switch (m_type) {
			case Type.Health: 	return InstanceManager.player.healthMax;
			case Type.Energy:	return InstanceManager.player.energyMax;
			case Type.Fury:		return InstanceManager.player.furyMax;
			case Type.SuperFury:return InstanceManager.player.furyMax;
		}
		return 0;
	}

	private float GetPowerUpsAddition() {
		switch( m_type ) 
		{
			case Type.Health: 	return InstanceManager.player.healthModifier;
			case Type.Energy:	return InstanceManager.player.energyModifier;
			case Type.Fury:		return InstanceManager.player.furyModifier;
			case Type.SuperFury:return InstanceManager.player.furyModifier;
		}
		return 0;
	}

	private float GetValue() {
		switch (m_type) {
			case Type.Health: 	return InstanceManager.player.health;
			case Type.Energy:	return InstanceManager.player.energy;
			case Type.Fury:		return InstanceManager.player.fury;
			case Type.SuperFury:return InstanceManager.player.superFury;
		}		
		return 0;
	}

	public float GetSizePerUnit()
	{
		switch (m_type) {
			case Type.Health: 	return InstanceManager.player.data.def.GetAsFloat("statsBarRatio");
			case Type.Energy:	return InstanceManager.player.data.def.GetAsFloat("statsBarRatio");
			case Type.Fury:		return InstanceManager.player.data.def.GetAsFloat("fireBarRatio");
			case Type.SuperFury:return InstanceManager.player.data.def.GetAsFloat("fireBarRatio");
		}
		return 5;
	}

	void OnPlayerKo()
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
						extraRt.localPosition = rt.position + Vector3.right * (rt.rect.width * 0.2f * (m_extraIcons.Count + 1));
						extraRt.localScale = rt.localScale;
						// extraRt.offsetMax.y = 0;

						m_extraIcons.Add( extraIcon );


					}
					/*
					if ( showAnimations )
					{
						bool wasActive = m_extraIcons[ i ].activeSelf;
						bool shouldBeActive = i < remainingLives;
						if ( wasActive && !shouldBeActive )	// if was active and now it's not we lose a life -> show particle breaking the icon
						{
							
						}
						else if ( !wasActive && shouldBeActive ) // Wasn't active and now it is so we won a life
						{
								
						}
					}
					else
					*/
					{
						m_extraIcons[ i ].SetActive( i < remainingLives );
					}
					
				}
			}break;
		}
	}
}
