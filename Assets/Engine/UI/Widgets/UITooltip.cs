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
	// Exposed References
	[SerializeField] private RectTransform m_arrow = null;
	[SerializeField] private ArrowDirection m_arrowDir = ArrowDirection.HORIZONTAL;
	[Separator("Optional")]
	[SerializeField] private TMPro.TextMeshProUGUI m_titleText = null;
	[SerializeField] private TMPro.TextMeshProUGUI m_messageText = null;

	// Other references
	private ShowHideAnimator m_animator = null;
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
	private void Awake() {
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
	/// Initialize the tooltip with the given texts.
	/// If the tooltip has no textfields assigned, will be ignored.
	/// If a text is left empty, its corresponding textfield will be disabled.
	/// </summary>
	/// <param name="_title">Title string.</param>
	/// <param name="_text">Text string.</param>
	public void InitWithText(string _title, string _text) {
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
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}