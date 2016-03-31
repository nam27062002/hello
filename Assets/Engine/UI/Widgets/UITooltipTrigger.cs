// UITooltipTrigger.cs
// 
// Created by Alger Ortín Castellví on 29/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom UI Widget to spawn tooltip-like elements.
/// TODO!! Auto adjust tooltip to trigger's rect
/// TODO!! Auto animation
/// </summary>
public class UITooltipTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private string m_prefabPath = "";

	// Events, subscribe as needed via inspector or code
	[Serializable] public class TooltipEvent : UnityEvent<UITooltip> { }
	public TooltipEvent OnTooltipOpen = new TooltipEvent();

	// Internal
	private UITooltip m_tooltip = null;
	public UITooltip tooltip {
		get { return m_tooltip; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The pointer has gone down over this object.
	/// </summary>
	/// <param name="_eventData">Event data.</param>
	public void OnPointerDown(PointerEventData _eventData) {
		// If prefab hasn't been instantiated, do it now
		if(m_tooltip == null) {
			// Load prefab
			GameObject prefabObj = Resources.Load<GameObject>(m_prefabPath);
			if(prefabObj == null) {
				Debug.LogError("Tooltip prefab " + m_prefabPath + " not found, skipping tooltip");
				return;
			}

			// Create new instance as sibling of this object and default pos
			GameObject newObj = GameObject.Instantiate<GameObject>(prefabObj);
			newObj.transform.SetParent(this.transform.parent, false);

			// Behind this object!
			newObj.transform.SetSiblingIndex(this.transform.GetSiblingIndex());

			// Store reference for future usage
			m_tooltip = newObj.GetComponent<UITooltip>();

			// If the given prefab doesn't have a show/hide animator, add one
			if(m_tooltip == null) {
				m_tooltip = newObj.AddComponent<UITooltip>();	// Default params are ok (no anim)
			}
		}

		// Make sure tooltip is not out of screen
		// Instantly unfold it for a moment to get the right measurements
		float deltaBackup = m_tooltip.delta;
		m_tooltip.ForceShow(false);

		// Put it at the trigger's position
		m_tooltip.transform.localPosition = this.transform.localPosition;
		Canvas canvas = GetComponentInParent<Canvas>();

		Rect tooltipRect = (m_tooltip.transform as RectTransform).rect;	// Tooltip in local coords
		Rect canvasRect = (canvas.transform as RectTransform).rect;	// Canvas in local coords
		tooltipRect = m_tooltip.transform.TransformRect(tooltipRect, canvas.transform);
		Vector3 offset = Vector3.zero;

		// Check horizontal edges
		if(tooltipRect.xMin < canvasRect.xMin) {
			offset.x = canvasRect.xMin - tooltipRect.xMin;
		} else if(tooltipRect.xMax > canvasRect.xMax) {
			offset.x = canvasRect.xMax - tooltipRect.xMax;
		}

		// Check vertical edges
		if(tooltipRect.yMin < canvasRect.yMin) {
			offset.y = canvasRect.yMin - tooltipRect.yMin;
		} else if(tooltipRect.yMax > canvasRect.yMax) {
			offset.y = canvasRect.yMax - tooltipRect.yMax;
		}

		// Convert offset to tooltip's local coords and apply
		offset = canvas.transform.TransformPoint(offset, this.transform.parent);
		m_tooltip.transform.localPosition = this.transform.localPosition + offset;

		// Restore previous animation delta and trigger show animation
		m_tooltip.delta = deltaBackup;
		m_tooltip.ForceShow();

		// Invoke event
		OnTooltipOpen.Invoke(m_tooltip);
	}

	/// <summary>
	/// The pointer has gone up over this object.
	/// </summary>
	/// <param name="_eventData">Event data.</param>
	public void OnPointerUp(PointerEventData _eventData) {
		// Just hide the tooltip, if created
		if(m_tooltip != null) m_tooltip.Hide();
	}

	/// <summary>
	/// The pointer has gone off over this object.
	/// </summary>
	/// <param name="_eventData">Event data.</param>
	public void OnPointerExit(PointerEventData _eventData) {
		// Just hide the tooltip, if created
		if(m_tooltip != null) m_tooltip.Hide();
	}
}