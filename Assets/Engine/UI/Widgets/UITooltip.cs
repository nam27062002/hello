// UITooltip.cs
// 
// Created by Alger Ortín Castellví on 29/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Base class for a tooltip.
/// To fill/inherit as needed
/// </summary>
[RequireComponent(typeof(ShowHideAnimator))]
public class UITooltip : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum ArrowDirection {
		HORIZONTAL,
		VERTICAL
	};

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Setup
	[SerializeField] protected bool m_startHidden = true;

	// Arrow
	[Space]
	[SerializeField] protected RectTransform m_arrow = null;
	[Tooltip("Arrow Dir determins in which axis the arrow moves (i.e. arrows pointing to left and right are moving in the VERTICAL axis, while arrows pointing up and down are moving in the HORIZONTAL axis.")]
	[SerializeField] protected ArrowDirection m_arrowDir = ArrowDirection.HORIZONTAL;
	public ArrowDirection arrowDir {
		get { return m_arrowDir; }
	}

	// Optional components
	[Separator("Optional")]
	[SerializeField] protected TMPro.TextMeshProUGUI m_titleText = null;
	[SerializeField] protected TMPro.TextMeshProUGUI m_messageText = null;
    [SerializeField] protected Image m_icon = null;

	// Other references
	protected ShowHideAnimator m_animator = null;
	public ShowHideAnimator animator {
		get {
			if(m_animator == null) m_animator = GetComponent<ShowHideAnimator>();
			return m_animator; 
		}
	}

	// Backup values
	private float m_arrowOffset = 0.5f;	// Arrow offset, being 0.5 the center of the side (default), 0 the beginning and 1 the end.
	private float m_arrowOffsetCorrection = 0f;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {
		// Backup some values
		if(m_arrow != null) {
			// Original arrow offset
			switch(m_arrowDir) {
				case ArrowDirection.HORIZONTAL: {
					m_arrowOffset = m_arrow.anchorMin.x;
				} break;

				case ArrowDirection.VERTICAL: {
					m_arrowOffset = m_arrow.anchorMin.y;
				} break;
			}
		}

		// Get animator ref
		m_animator = GetComponent<ShowHideAnimator>();

		// Start hidden
		if(animator != null && m_startHidden) animator.ForceHide(false);
	}

	/// <summary>
	/// Adjust the arrow position along the side of the tooltip.
	/// </summary>
	/// <param name="_offset">Arrow offset, being 0.5 the center of the side (default), 0 the beginning and 1 the end.</param>
	public void SetArrowOffset(float _offset) {
		// Skip if there is no arrow
		if(m_arrow == null) return;

		// Store new value
		m_arrowOffset = _offset;

		// Apply offset
		switch(m_arrowDir) {
			case ArrowDirection.HORIZONTAL: {
				m_arrow.anchorMin = new Vector2(_offset, m_arrow.anchorMin.y);
				m_arrow.anchorMax = new Vector2(_offset, m_arrow.anchorMax.y);
			} break;

			case ArrowDirection.VERTICAL: {
				m_arrow.anchorMin = new Vector2(m_arrow.anchorMin.x, _offset);
				m_arrow.anchorMax = new Vector2(m_arrow.anchorMax.x, _offset);
			} break;
		}

		// Re-apply offset correction
		CorrectArrowOffset(m_arrowOffsetCorrection);
	}

	/// <summary>
	/// Correct the arrow offset.
	/// </summary>
	/// <param name="_offset">Amount of units to be corrected.</param>
	public void CorrectArrowOffset(float _offset) {
		// Skip if there is no arrow
		if(m_arrow == null) return;

		// Store value
		m_arrowOffsetCorrection = _offset;

		// Apply offset
		switch(m_arrowDir) {
			case ArrowDirection.HORIZONTAL: {
				RectTransform parentRT = m_arrow.parent as RectTransform;
				float size = parentRT.rect.width;
				if(Mathf.Abs(size) < Mathf.Epsilon) break;  // Just in case, avoid division by 0

				float relativeOffset = _offset / size;
				relativeOffset += m_arrowOffset;	// Add to original offset
				m_arrow.anchorMin = new Vector2(relativeOffset, m_arrow.anchorMin.y);
				m_arrow.anchorMax = new Vector2(relativeOffset, m_arrow.anchorMax.y);
			} break;

			case ArrowDirection.VERTICAL: {
				RectTransform parentRT = m_arrow.parent as RectTransform;
				float size = parentRT.rect.height;
				if(Mathf.Abs(size) < Mathf.Epsilon) break;	// Just in case, avoid division by 0

				float relativeOffset = _offset / size;
				relativeOffset += m_arrowOffset;    // Add to original offset
				m_arrow.anchorMin = new Vector2(m_arrow.anchorMin.x, relativeOffset);
				m_arrow.anchorMax = new Vector2(m_arrow.anchorMax.x, relativeOffset);
			} break;
		}
	}

	/// <summary>
	/// Initialize the tooltip with the given texts.
	/// If the tooltip has no textfields assigned, will be ignored.
	/// If a text is left empty, its corresponding textfield will be disabled.
	/// </summary>
	/// <param name="_title">Title string.</param>
	/// <param name="_text">Text string.</param>
	public void InitWithText(string _title, string _text) {
        Init(_title, _text, "");
    }

    /// <summary>
    /// Initialize the tooltip with the given texts and icon.
    /// If the tooltip has no textfields or icon assigned, will be ignored.
    /// If a text or icon is left empty, its corresponding game object will be disabled.
    /// </summary>
    /// <param name="_title">Title string.</param>
    /// <param name="_text">Text string.</param>
    /// <param name="_icon">Icon name and full path from resources folder.</param>
    public void Init(string _title, string _text, string _icon) {
        Sprite icon = null;
        // Icon
        if (m_icon != null) {
            if (!string.IsNullOrEmpty(_icon)) {
                icon = Resources.Load<Sprite>(_icon);
            }
        }

        Init(_title, _text, icon);
    }

    /// <summary>
    /// Initialize the tooltip with the given texts and icon.
    /// If the tooltip has no textfields or icon assigned, will be ignored.
    /// If a text or icon is left empty, its corresponding game object will be disabled.
    /// </summary>
    /// <param name="_title">Title string.</param>
    /// <param name="_text">Text string.</param>
    /// <param name="_icon">Icon sprite.</param>
    public virtual void Init(string _title, string _text, Sprite _icon) {
		// Title
		if(m_titleText != null) {
			m_titleText.text = _title;
			m_titleText.gameObject.SetActive(!string.IsNullOrEmpty(_title));
		}

		// Message
		if(m_messageText != null) {
			m_messageText.text = _text;
			m_messageText.gameObject.SetActive(!string.IsNullOrEmpty(_text));
		}

        // Icon
        if(m_icon != null) {
            if (_icon != null) {
                m_icon.sprite = _icon;
                m_icon.color = Color.white;
            }
			m_icon.gameObject.SetActive(_icon != null);
        }
	}

	//------------------------------------------------------------------------//
	// STATIC UTILS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Do all the maths to place the given tooltip so it spawns at the given anchor.
	/// Fairly expensive method, do not use lightly.
	/// </summary>
	/// <param name="_tooltip">The tooltip to be placed.</param>
	/// <param name="_anchor">Anchor used as position reference to place the tooltip.</param>
	/// <param name="_offset">Optional offset from the anchor.</param>
	/// <param name="_renderOnTop">Move the tooltip as the last sibling in its container canvas.</param>
	/// <param name="_checkScreenBounds">Whether to check going out of bounds or not.</param>
	public static void PlaceAndShowTooltip(UITooltip _tooltip, RectTransform _anchor, Vector2 _offset, bool _renderOnTop, bool _checkScreenBounds) {
		// Check some params
		if(_tooltip == null) {
			Debug.LogError("Attempting to place a NULL tooltip");
			return;
		}

		if(_anchor == null) {
			Debug.LogError("Attempting to place tooltip " + _tooltip.name + " but given anchor is NULL");
			return;
		}

		// We're good to go!
		// Aux vars
		Canvas parentCanvas = _tooltip.GetComponentInParent<Canvas>();
		
		// Activate the tooltip to make sure all the layouts, textfields and dynamic sizes are updated
		_tooltip.gameObject.SetActive(true);

		// If the render on top flag is set, move the tooltip to the top of its parent canvas
		if(_renderOnTop) {
			_tooltip.transform.SetParent(parentCanvas.transform);
			_tooltip.transform.SetAsLastSibling();
		}

		// Put tooltip on anchor's position
		// Wait a frame so all the measurements are right and updated
		UbiBCN.CoroutineManager.DelayedCallByFrames(
			() => {
				// Instantly unfold it for a moment to get the right measurements
				float deltaBackup = _tooltip.animator.delta;
				_tooltip.animator.ForceShow(false);

				// Put it at the anchor's position
				_tooltip.transform.localPosition = _anchor.parent.TransformPoint(_anchor.localPosition, _tooltip.transform.parent);

				// Apply manual offset
				_tooltip.transform.localPosition = _tooltip.transform.localPosition + new Vector3(_offset.x, _offset.y, 0f);

				// Some more aux vars
				Vector3 finalOffset = GameConstants.Vector3.zero;

				// If required, make sure tooltip is not out of screen
				if(_checkScreenBounds) {
					// Aux vars
					Rect canvasRect = (parentCanvas.transform as RectTransform).rect; // Canvas in local coords
					Rect tooltipRect = (_tooltip.transform as RectTransform).rect; // Tooltip in local coords
					tooltipRect = _tooltip.transform.TransformRect(tooltipRect, parentCanvas.transform);

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
				Vector3 finalCanvasPos = _tooltip.transform.parent.TransformPoint(_tooltip.transform.localPosition, parentCanvas.transform) + finalOffset;
				_tooltip.transform.localPosition = parentCanvas.transform.TransformPoint(finalCanvasPos, _tooltip.transform.parent);

				// Apply reverse offset to arrow so it keeps pointing to the original position
				switch(_tooltip.arrowDir) {
					case UITooltip.ArrowDirection.HORIZONTAL: {
						_tooltip.CorrectArrowOffset(-finalOffset.x);
					} break;

					case UITooltip.ArrowDirection.VERTICAL: {
						_tooltip.CorrectArrowOffset(-finalOffset.y);
					} break;
				}

				// Restore previous animation delta
				_tooltip.animator.delta = deltaBackup;

				// Just launch the animation!
				_tooltip.animator.ForceShow();
			}, 1
		);
	}
}