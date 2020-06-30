﻿// UITooltipTrigger.cs
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
	[Comment("If a tooltip instance is defined in the inspector, it will be used directly.\nIf not defined, a new instance of the defined prefab will be created as needed.")]
	[SerializeField] protected UITooltip m_tooltip = null;
	public UITooltip tooltip {
		get { return m_tooltip; }
        set { m_tooltip = value; }
	}

	[SerializeField] private string m_prefabPath = "";

	[Comment("\nThe tooltip will be spawned from this anchor point.\nIf not defined, it will be autopositioned using the trigger's transform as anchor.\nActivating the \"Keep Original Position\" flag will override the anchor.")]
	[SerializeField] protected RectTransform m_anchor = null;
	public RectTransform anchor {
		get { return m_anchor; }
		set { m_anchor = value; }
	}

	[SerializeField] protected Vector2 m_offset = Vector2.zero;
	public Vector2 offset {
		get { return m_offset; }
		set { m_offset = value; }
	}

	[SerializeField] protected bool m_autoHide = true;
	public bool autoHide {
		get { return m_autoHide; }
		set { m_autoHide = value; }
	}

	[SerializeField] protected bool m_keepOriginalPosition = false;
	public bool keepOriginalPosition {
		get { return m_keepOriginalPosition; }
		set { m_keepOriginalPosition = value; }
	}

	[SerializeField] protected bool m_checkScreenBounds = true;
	public bool checkScreenBounds {
		get { return m_checkScreenBounds; }
		set { m_checkScreenBounds = value; }
	}

	[SerializeField] protected bool m_renderOnTop = true;
	public bool renderOnTop {
		get { return m_renderOnTop; }
		set { m_renderOnTop = value; }
	}

	// Events, subscribe as needed via inspector or code
	[Serializable] public class TooltipEvent : UnityEvent<UITooltip, UITooltipTrigger> { }
	[Space]
	public TooltipEvent OnTooltipOpen = new TooltipEvent();

	// Internal references
	protected Canvas m_parentCanvas = null;
	protected Transform m_originalTooltipParent = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {
		
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected virtual void OnDestroy() {
		if(m_tooltip != null) m_tooltip.animator.OnHidePostAnimation.RemoveListener(OnTooltipClosed);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	protected virtual void OnDisable() {
		// Hide the tooltip!
		if(m_tooltip != null) m_tooltip.animator.Hide();
	}

	//------------------------------------------------------------------------//
	// OVERRIDE CANDIDATES													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Instantiate or choose the right instance of the tooltip to be displayed.
	/// Will override current m_tooltip.
	/// </summary>
	/// <param name="_spawnTransform">The root where to instantiate the new tooltip.</param>
	/// <returns>The new tooltip instance. <c>null</c> if tooltip instance couldn't be created.</returns>
	protected virtual UITooltip InitTooltipInstance(RectTransform _spawnTransform) {
		// If we already have the tooltip instance, just use it
		if(m_tooltip != null) return m_tooltip;

		// Create the tooltip instance
		UITooltip newTooltip = InstantiateTooltipPrefab(m_prefabPath, _spawnTransform);

		// Done!
		return newTooltip;
	}

	/// <summary>
	/// Create an instance of a UITooltip prefab at the given transform.
	/// </summary>
	/// <param name="_prefabPath">The path of the tooltip prefab to be instantiated.</param>
	/// <param name="_spawnTransform">The root where to instantiate the new tooltip.</param>
	/// <returns>The new tooltip instance. <c>null</c> if tooltip instance couldn't be created.</returns>
	protected virtual UITooltip InstantiateTooltipPrefab(string _prefabPath, RectTransform _spawnTransform) {
		// Load prefab
		GameObject prefabObj = Resources.Load<GameObject>(_prefabPath);
		if(prefabObj == null) {
			Debug.LogError("Tooltip prefab " + _prefabPath + " not found, skipping tooltip");
			return null;
		}

		// Create new instance as sibling of this object and default pos
		GameObject newObj = GameObject.Instantiate<GameObject>(prefabObj);
		newObj.transform.SetParent(_spawnTransform.parent, false);

		// Behind the spawn point!
		newObj.transform.SetSiblingIndex(_spawnTransform.GetSiblingIndex());

		// Store reference for future usage
		UITooltip newTooltip = newObj.GetComponent<UITooltip>();

		// If the given prefab doesn't have a show/hide animator, add one
		if(newTooltip == null) {
			newTooltip = newObj.AddComponent<UITooltip>();   // Default params are ok (no anim)
		}

		// Done!
		return newTooltip;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The pointer has gone down over this object.
	/// </summary>
	/// <param name="_eventData">Event data.</param>
	public virtual void OnPointerDown(PointerEventData _eventData) {
		// Only if active
		if(!isActiveAndEnabled) return;

		// If autohide is disabled and tooltip is on, hide it and do nothing else
		if(!m_autoHide && m_tooltip != null && m_tooltip.animator.visible) {
			m_tooltip.animator.ForceHide();
			return;
		}

		// Use a predefined anchor or use our transform as spawn point?
		RectTransform spawnTransform = null;
		if(m_anchor != null) {
			spawnTransform = m_anchor;
		} else {
			spawnTransform = (RectTransform)this.transform;
		}

		// Initialize tooltip instance. If invalid, don't do anything else.
		m_tooltip = InitTooltipInstance(spawnTransform);
		if(m_tooltip == null) {
			Debug.LogError("Couldn't instantiate tooltip for trigger " + this.name);
			return;
		}

		// Invoke event (before animation, in case anything needs to be initialized)
		OnTooltipOpen.Invoke(m_tooltip, this);

		// If the render on top flag is set, store original parent to restore it after animation
		if(m_renderOnTop) {
			// Make sure we only do this once
			if(m_parentCanvas == null) {
				m_parentCanvas = m_tooltip.GetComponentInParent<Canvas>();
				m_originalTooltipParent = m_tooltip.transform.parent;
				m_tooltip.animator.OnHidePostAnimation.AddListener(OnTooltipClosed);
			}
			//TOREMOVEm_tooltip.transform.SetParent(m_parentCanvas.transform);
			//TOREMOVEm_tooltip.transform.SetAsLastSibling();
		}

		// Unless explicitely denied, put tooltip on anchor's position
		if(!m_keepOriginalPosition) {
			// UITooltip does the hard math for us!
			UITooltip.PlaceAndShowTooltip(
				m_tooltip,
				spawnTransform,
				m_offset,
				m_renderOnTop,
				m_checkScreenBounds
			);
			// TOREMOVE
			/*// Activate the tooltip to make sure all the layouts, textfields and dynamic sizes are updated
			m_tooltip.gameObject.SetActive(true);

			// Wait a frame so all the measurements are right and updated
			UbiBCN.CoroutineManager.DelayedCallByFrames(
				() => {
					// Instantly unfold it for a moment to get the right measurements
					float deltaBackup = m_tooltip.animator.delta;
					m_tooltip.animator.ForceShow(false);

					// Put it at the anchor's position
					m_tooltip.transform.localPosition = spawnTransform.parent.TransformPoint(spawnTransform.localPosition, m_tooltip.transform.parent);

					// Apply manual offset
					m_tooltip.transform.localPosition = m_tooltip.transform.localPosition + new Vector3(m_offset.x, m_offset.y, 0f);

					// Get some aux vars
					Canvas canvas = GetComponentInParent<Canvas>();
					Vector3 finalOffset = GameConstants.Vector3.zero;

					// If required, make sure tooltip is not out of screen
					if(m_checkScreenBounds) {
						// Aux vars
						Rect canvasRect = (canvas.transform as RectTransform).rect; // Canvas in local coords
						Rect tooltipRect = (m_tooltip.transform as RectTransform).rect; // Tooltip in local coords
						tooltipRect = m_tooltip.transform.TransformRect(tooltipRect, canvas.transform);

						// Take safe area in account
						UISafeArea safeArea = UIConstants.safeArea;
						canvasRect.xMin += safeArea.left;
						canvasRect.xMax -= safeArea.right;
						canvasRect.yMin += safeArea.bottom;
						canvasRect.yMax -= safeArea.top;

						// Check horizontal edges
						if(tooltipRect.xMin < canvasRect.xMin) {
							finalOffset.x = canvasRect.xMin - tooltipRect.xMin;
						} else if(tooltipRect.xMax > canvasRect.xMax) {
							finalOffset.x = canvasRect.xMax - tooltipRect.xMax;
						}

						// Check vertical edges
						if(tooltipRect.yMin < canvasRect.yMin) {
							finalOffset.y = canvasRect.yMin - tooltipRect.yMin;
						} else if(tooltipRect.yMax > canvasRect.yMax) {
							finalOffset.y = canvasRect.yMax - tooltipRect.yMax;
						}
					}

					// Compute final position in tooltip's local coords and apply
					Vector3 finalCanvasPos = tooltip.transform.parent.TransformPoint(tooltip.transform.localPosition, canvas.transform) + finalOffset;
					m_tooltip.transform.localPosition = canvas.transform.TransformPoint(finalCanvasPos, tooltip.transform.parent);

					// Apply reverse offset to arrow so it keeps pointing to the original position
					switch(m_tooltip.arrowDir) {
						case UITooltip.ArrowDirection.HORIZONTAL: {
							m_tooltip.CorrectArrowOffset(-finalOffset.x);
						} break;

						case UITooltip.ArrowDirection.VERTICAL: {
							m_tooltip.CorrectArrowOffset(-finalOffset.y);
						} break;
					}

					// Restore previous animation delta
					m_tooltip.animator.delta = deltaBackup;

					// Just launch the animation!
					m_tooltip.animator.ForceShow();
				}, 1
			);*/
		}

		// Keeping original position
		else {
			// Just launch the animation!
			m_tooltip.animator.ForceShow();
		}
	}

	/// <summary>
	/// The pointer has gone up over this object.
	/// </summary>
	/// <param name="_eventData">Event data.</param>
	public virtual void OnPointerUp(PointerEventData _eventData) {
		// Nothing to do if auto hide not enabled
		if(!m_autoHide) return;

		// Just hide the tooltip, if created
		if(m_tooltip != null) m_tooltip.animator.Hide();
	}

	/// <summary>
	/// The pointer has gone off over this object.
	/// </summary>
	/// <param name="_eventData">Event data.</param>
	public virtual void OnPointerExit(PointerEventData _eventData) {
		// Nothing to do if auto hide not enabled
		if(!m_autoHide) return;

		// Just hide the tooltip, if created
		if(m_tooltip != null) m_tooltip.animator.Hide();
	}

	/// <summary>
	/// The tooltip has been closed.
	/// </summary>
	/// <param name="_anim">The animator that triggered the event.</param>
	public virtual void OnTooltipClosed(ShowHideAnimator _anim) {
		// Return tooltip to its original parent
		m_tooltip.transform.SetParent(m_originalTooltipParent);
	}
}