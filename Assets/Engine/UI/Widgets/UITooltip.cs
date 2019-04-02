﻿// UITooltip.cs
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
	// Exposed References
	[SerializeField] protected RectTransform m_arrow = null;
	[SerializeField] protected ArrowDirection m_arrowDir = ArrowDirection.HORIZONTAL;
	public ArrowDirection arrowDir {
		get { return m_arrowDir; }
	}

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

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected void Awake() {
		// Get animator ref
		m_animator = GetComponent<ShowHideAnimator>();

		// Start hidden
		animator.ForceHide(false);
	}

	/// <summary>
	/// Adjust the arrow position along the side of the tooltip.
	/// </summary>
	/// <param name="_offset">Arrow offset, being 0.5 the center of the side (default), 0 the beginning and 1 the end.</param>
	public void SetArrowOffset(float _offset) {
		// Skip if there is no arrow
		if(m_arrow == null) return;

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
	}

	/// <summary>
	/// Correct the arrow offset.
	/// </summary>
	/// <param name="_offset">Amount of units to be corrected.</param>
	public void CorrectArrowOffset(float _offset) {
		// Skip if there is no arrow
		if(m_arrow == null) return;

		// Apply offset
		switch(m_arrowDir) {
			case ArrowDirection.HORIZONTAL: {
				RectTransform parentRT = m_arrow.parent as RectTransform;
				float size = parentRT.rect.width;
				float relativeOffset = _offset / size;
				m_arrow.anchorMin = new Vector2(m_arrow.anchorMin.x + relativeOffset, m_arrow.anchorMin.y);
				m_arrow.anchorMax = new Vector2(m_arrow.anchorMax.x + relativeOffset, m_arrow.anchorMax.y);
			} break;

			case ArrowDirection.VERTICAL: {
				RectTransform parentRT = m_arrow.parent as RectTransform;
				float size = parentRT.rect.height;
				float relativeOffset = _offset / size;
				m_arrow.anchorMin = new Vector2(m_arrow.anchorMin.x, m_arrow.anchorMin.y + relativeOffset);
				m_arrow.anchorMax = new Vector2(m_arrow.anchorMax.x, m_arrow.anchorMax.y + relativeOffset);
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
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}