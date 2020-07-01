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
	public enum TooltipInstantiationMode {
		PREFAB,
		INSTANCE
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] protected TooltipInstantiationMode m_instantiationMode = TooltipInstantiationMode.INSTANCE;
	
	[SerializeField] protected UITooltip m_tooltip = null;
	public UITooltip tooltip {
		get { return m_tooltip; }
        set { m_tooltip = value; }
	}

	[FileList("Resources/UI", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
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
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Force-open the tooltip by code.
	/// </summary>
	public void OpenTooltip() {
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
		}

		// Keeping original position
		else {
			// Render on top?
			if(m_renderOnTop && m_parentCanvas != null) {
				m_tooltip.transform.SetParent(m_parentCanvas.transform);
				m_tooltip.transform.SetAsLastSibling();
			}

			// Just launch the animation!
			m_tooltip.animator.ForceShow();
		}
	}

	/// <summary>
	/// Force-hide the tooltip by code.
	/// </summary>
	public void CloseTooltip() {
		// Just do it regardless of the state
		if(m_tooltip != null) m_tooltip.animator.ForceHide();
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
			CloseTooltip();
			return;
		}

		// Open tooltip!
		OpenTooltip();
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