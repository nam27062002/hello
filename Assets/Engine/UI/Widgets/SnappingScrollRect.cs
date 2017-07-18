// SnapScrollRect.cs
// 
// Created by Alger Ortín Castellví on 28/01/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Specialization of the native ScrollRect snapping to defined points.
/// </summary>
public class SnappingScrollRect : ScrollRect {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Selection changed event.
	/// </summary>
	[System.Serializable]
	public class SelectionChangedEvent : UnityEvent<ScrollRectSnapPoint> { }
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	[Separator("Snap Settings")]
	[Comment("Normalized within the viewport")]
	[SerializeField] protected Vector2 m_snapPos = new Vector2(0.5f, 0.5f);
	protected Vector2 snapPosViewport {
		get {
			// Compute actual position of the snap point in viewport coords
			Vector2 viewportPos = m_snapPos;
			viewportPos.Scale(viewport.rect.size);
			viewportPos.y *= -1;

			// Ignore locked scrolling directions and return
			if(!horizontal) viewportPos.x = 0;
			if(!vertical) viewportPos.y = 0;
			return viewportPos;
		}
	}

	[Comment("Seconds")]
	[SerializeField] protected float m_snapAnimDuration = 0.5f;
	public float snapAnimDuration {
		get { return m_snapAnimDuration; }
		set { m_snapAnimDuration = value; }
	}

	[SerializeField] protected bool m_snapAfterDragging = true;
	public bool snapAfterDragging {
		get { return m_snapAfterDragging; }
		set { m_snapAfterDragging = value; }
	}

	[SerializeField] protected Ease m_snapEase = Ease.OutExpo;
	public Ease snapEase {
		get { return m_snapEase; }
		set { m_snapEase = value; }
	}

	[Space]
	[SerializeField]
	protected ScrollRectSnapPoint m_selectedPoint = null;
	public ScrollRectSnapPoint selectedPoint {
		get { return m_selectedPoint; }
		set { SelectPoint(value); }
	}

	[SerializeField]
	protected SelectionChangedEvent m_onSelectionChanged = new SelectionChangedEvent();
	public SelectionChangedEvent onSelectionChanged { 
		get { return m_onSelectionChanged; } 
		set { m_onSelectionChanged = value; }
	}

	// Internal
	protected Tweener m_tweener = null;
	protected bool m_dirty = false;
	protected bool m_dragging = false;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Call parent
		base.Awake();
	}

	/// <summary>
	/// Component enabled
	/// </summary>
	override protected void OnEnable() {
		// Call parent
		base.OnEnable();

		// Make sure we start snapped
		// [AOC] For some reason, setting the content position in Enable, Start or Awake doesn't work, so delay it until first update.
		m_dirty = true;
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	override protected void LateUpdate() {
		// Call parent
		base.LateUpdate();

		// If dirty, scroll to selected point
		if(m_dirty) {
			// If a default selection is not defined, try to find one instead
			if(m_selectedPoint == null) {
				Snap();
			} else {
				ScrollToSelection();
			}
			m_dirty = false;
		} else {
			// While moving with inertia, prevent content from going too far
			if(!m_dragging && velocity.sqrMagnitude > 0) {
				CheckBounds();
			}
		}
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Snap to the nearest child.
	/// </summary>
	/// <param name="_animate">Whether to animate the snapping or not.</param>
	public void Snap(bool _animate = true) {
		// Skip if either viewport or content are not defined
		if(viewport == null || content == null) {
			Debug.LogError("SnapScrollRect: either viewport or content not defined - mandatory");
			m_selectedPoint = null;
			return;
		}

		// Pre-compute some values outside the loop
		Vector2 viewportSnapPos = snapPosViewport;

		// Find nearest child to the snap point
		float minDist = float.MaxValue;
		Vector2 minOffset = Vector2.zero;
		RectTransform nearestChild = null;
		foreach(RectTransform child in content) {
			// Skip if child doesn't have the "SnapPoint" component
			if(child.GetComponent<ScrollRectSnapPoint>() == null) continue;

			// Skip if child is not active
			if(!child.gameObject.activeSelf) continue;

			// Compute distance from the child (correcting with content's current position) to the snap point
			Vector2 offset = (new Vector2(child.localPosition.x, child.localPosition.y) + content.anchoredPosition) - viewportSnapPos;

			// Ignore locked scrolling directions
			if(!horizontal) offset.x = 0;
			if(!vertical) offset.y = 0;

			// Is it the nearest child?
			float dist = offset.magnitude;
			if(dist < minDist) {
				minDist = dist;
				minOffset = offset;
				nearestChild = child;
			}
		}

		// Store new nearest child as selected
		ScrollRectSnapPoint newPoint = null;
		if(nearestChild != null) newPoint = nearestChild.GetComponent<ScrollRectSnapPoint>();
		SelectPoint(newPoint);
	}

	/// <summary>
	/// Sets the selected point.
	/// </summary>
	/// <param name="_newSelection">New selected point - should be a child of the object.</param>
	/// <param name="_animate">Whether to animate the snapping or not, the snapping will be done anyway.</param>
	public void SelectPoint(ScrollRectSnapPoint _newSelection, bool _animate = true) {
		// Skip if either viewport or content are not defined
		if(viewport == null || content == null) {
			Debug.LogError("SnapScrollRect: either viewport or content not defined - mandatory");
			m_selectedPoint = null;
			return;
		}

		// Store new selection
		ScrollRectSnapPoint previousSelectedChild = m_selectedPoint;
		m_selectedPoint = _newSelection;

		// If selection has changed, propagate event
		if(m_selectedPoint != previousSelectedChild) {
			m_onSelectionChanged.Invoke(m_selectedPoint);
		}
		
		// Scroll to newly selected point
		if(m_selectedPoint != null) {
			ScrollToSelection(_animate);
		}
	}

	/// <summary>
	/// Scroll to currently selected point.
	/// </summary>
	/// <param name="_animate">Whether to animate the scroll or not.</param>
	public void ScrollToSelection(bool _animate = true) {
		// If new selection is null, do nothing
		if(m_selectedPoint == null) return;

		// Stop inertia
		velocity = Vector2.zero;

		// Compute target position of the content so the target child is aligned to the snap point
		// Compute distance from the child (correcting with content's current position) to the snap point
		Vector2 offset = (new Vector2(m_selectedPoint.transform.localPosition.x, m_selectedPoint.transform.localPosition.y) + content.anchoredPosition) - snapPosViewport;
		if(!horizontal) offset.x = 0;	// Ignore locked scrolling directions
		if(!vertical) offset.y = 0;		// Ignore locked scrolling directions
		Vector2 targetContentPos = content.anchoredPosition - offset;

		// Move content to the target position
		if(_animate) {
			// Animate
			m_tweener = DOTween.To(
				() => { 
					return content.anchoredPosition; 
				}, 
				_newValue => { 
					SetContentAnchoredPosition(_newValue); 
				}, 
				targetContentPos, 
				m_snapAnimDuration)
				.SetEase(m_snapEase)
				.SetAutoKill(true)
				.SetRecyclable(true);	// [AOC] By making it recyclable, we do a "pooling" of this tween type, avoiding creating memory garbage
		} else {
			// Don't animate
			SetContentAnchoredPosition(targetContentPos); 
		}
	}

	/// <summary>
	/// Check whether the content has gone outside the snap anchor, then force a snap to it.
	/// Use it to simulate scroll lists elastic behaviour (prevent content going to Cuenca).
	/// </summary>
	protected void CheckBounds() {
		// Check both axis
		Vector2 viewportSnapPos = snapPosViewport;
		bool outside = false;
		if(horizontal)	outside |= m_ContentBounds.min.x > viewportSnapPos.x || m_ContentBounds.max.x < viewportSnapPos.x;
		if(vertical)	outside |= m_ContentBounds.min.y > viewportSnapPos.y || m_ContentBounds.max.y < viewportSnapPos.y;

		// We're outside snap point! Snap to closest point
		if(outside) {
			Snap();
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Dragging starts now.
	/// </summary>
	/// <param name="_eventData">Event data.</param>
	override public void OnBeginDrag(PointerEventData _eventData) {
		// Update flag (before anything else)
		m_dragging = true;

		// Call parent and custom implementation
		base.OnBeginDrag(_eventData);

		// If tweener is active, stop it
		if(m_tweener != null && m_tweener.IsPlaying()) {
			m_tweener.Kill();
			m_tweener = null;
		}

		// Selected point is no more
		SelectPoint(null, false);
	}

	/// <summary>
	/// Dragging has ended.
	/// </summary>
	/// <param name="_eventData">Event data.</param>
	override public void OnEndDrag(PointerEventData _eventData) {
		// Update flag (before anything else)
		m_dragging = false;

		// Call parent and custom implementation
		base.OnEndDrag(_eventData);

		// Snap to nearest child?
		if(m_snapAfterDragging) {
			Snap();
		} else {
			// Force it if content is out of the snap point (elastic behaviour simulation)
			CheckBounds();
		}
	}

	/// <summary>
	/// Custom implementation of the OnDrag event.
	/// Not actually needed, we're using it for debug purposes.
	/// </summary>
	/// <param name="_eventData">Event data.</param>
	/*
	override public void OnDrag(PointerEventData _eventData) {
		base.OnDrag(_eventData);

		// Pre-compute some values outside the loop
		Vector2 viewportSnapPos = snapPosViewport;

		// Find nearest child to the snap point
		float minDist = float.MaxValue;
		Vector2 minOffset = Vector2.zero;
		RectTransform nearestChild = null;
		foreach(RectTransform child in content) {
			// Skip if child doesn't have the "SnapPoint" component
			if(child.GetComponent<ScrollRectSnapPoint>() == null) continue;

			// Skip if child is not active
			if(!child.gameObject.activeSelf) continue;

			// Compute distance from the child (correcting with content's current position) to the snap point
			Vector2 offset = (new Vector2(child.localPosition.x, child.localPosition.y) + content.anchoredPosition) - viewportSnapPos;

			// Ignore locked scrolling directions
			if(!horizontal) offset.x = 0;
			if(!vertical) offset.y = 0;

			// Is it the nearest child?
			float dist = offset.magnitude;
			if(dist < minDist) {
				minDist = dist;
				minOffset = offset;
				nearestChild = child;
			}
		}

		Debug.Log("------------------------------------------------------");
		Debug.Log(
			"<color=red>minOffset: " + minOffset.x + "</color>" 
			+ ", <color=red>minDist: " + minDist + "</color>\n" 
			+ "<color=lime>viewportSnapPos: " + viewportSnapPos.x + "</color>" 
			+ ", <color=magenta>content.anchoredPosition: " + content.anchoredPosition.x + "</color>"
		);

		Bounds viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);	// Same as m_ViewBounds (private var)
		Debug.Log(
			"<color=magenta>contentBounds: " + m_ContentBounds.min.x + ", " + m_ContentBounds.max.x + "</color>" 
			+ "\n<color=lime>viewBounds: " + viewBounds.min.x + ", " + viewBounds.max.x + "</color>" 
		);

		float diffMinX = viewBounds.min.x - m_ContentBounds.min.x;
		float diffMaxX = viewBounds.max.x - m_ContentBounds.max.x;
		Debug.Log(
			"<color=cyan>difMinX: " + diffMinX + ", diffMaxX" + diffMaxX + "</color>" 
		);

		Debug.Log(
			"<color=lime>snapPoint: " + viewportSnapPos.x + "</color>\n" +
			"<color=yellow>contentMin: " + m_ContentBounds.min.x + ", contentMax: " + m_ContentBounds.max.x + "</color>"
		);

		bool outside = m_ContentBounds.min.x > viewportSnapPos.x || m_ContentBounds.max.x < viewportSnapPos.x;
		Debug.Log((outside ? "<color=red>OUTSIDE!</color>" : "<color=lime>INSIDE!</color>"));
	}
	*/
}