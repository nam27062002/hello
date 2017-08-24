// PopupCredits.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/08/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using DG.Tweening;

using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Credits screen.
/// [AOC] We might have an issue if final text is too long. In that case we might need to use different prefabs for each style.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupCredits : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Menu/PF_PopupCredits";
	private const string SOURCE_PATH = "UI/Popups/Menu/credits";

	[Serializable]
	public class TextStyle {
		public string name = "";
		public float size = 50f;
		public Color color = Color.white;
		public bool titleFont = false;
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private ScrollRect m_scroll = null;
	[SerializeField] private TextMeshProUGUI m_text = null;
	[Space]
	[SerializeField] private float m_scrollSpeed = 150f;	// Units per second
	[SerializeField] private float m_inertiaThreshold = 100f;
	[Space]
	[SerializeField] private List<TextStyle> m_styles = new List<TextStyle>();

	// Internal
	private Tween m_tween = null;
	private List<PopupController> m_popupsToRestore = new List<PopupController>();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to popup events
		PopupController popup = GetComponent<PopupController>();
		popup.OnOpenPreAnimation.AddListener(OnOpenPreAnimation);
		popup.OnOpenPostAnimation.AddListener(OnOpenPostAnimation);
		popup.OnClosePreAnimation.AddListener(OnClosePreAnimation);
		popup.OnClosePostAnimation.AddListener(OnClosePostAnimation);

		// Load text from file
		TextAsset creditsText = Resources.Load<TextAsset>(SOURCE_PATH);
		string text = creditsText.text;

		// Apply styles
		TextStyle style;
		string openTag;
		string closeTag;
		StringBuilder sb = new StringBuilder();
		for(int i = 0; i < m_styles.Count; ++i) {
			style = m_styles[i];

			// Generate tags to replace
			sb.Length = 0;
			sb.Append("<").Append(style.name).Append(">");
			openTag = sb.ToString();

			sb.Length = 0;
			sb.Append("</").Append(style.name).Append(">");
			closeTag = sb.ToString();

			// Compose opening formatting tag
			sb.Length = 0;
			sb.Append("<size=").Append(style.size.ToString(CultureInfo.InvariantCulture)).Append(">");
			sb.Append("<color=").Append(style.color.ToHexString("#")).Append(">");
			if(style.titleFont) {
				sb.Append("<font=FNT_Bold/FNT_Bold>");
			}

			// Replace opening tag
			text = text.Replace(openTag, sb.ToString());

			// Compose closing tag - reverse order of the opening tag
			sb.Length = 0;
			if(style.titleFont) {
				sb.Append("</font>");
			}
			sb.Append("</color>");
			sb.Append("</size>");

			// Replace closing tag
			text = text.Replace(closeTag, sb.ToString());
		}

		// Apply to textfield!
		m_text.text = text;
	}

	/// <summary>
	/// Something has changed on the inspector.
	/// </summary>
	private void OnValidate() {
		// If we have a tween running, update scroll speed
		if(m_tween != null) {
			// Unfortunately, we need to re-create the tween in order to change its duration
			// This is for development only purpose though, so shouldn't be a problem
			CreateTween();
		}
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Unfortunately, we have no easy way to detect drag events on the scroll list
		// To work around that, just pause the scroll animation whenever we have a finger touching the screen
		if(m_tween != null) {
			if(Input.GetMouseButtonDown(0)) {
				m_tween.Kill();
				m_tween = null;
			}
		} else if(!Input.GetMouseButton(0) && Mathf.Abs(m_scroll.velocity.y) <= m_inertiaThreshold) {
			CreateTween();	// Re-create to start from current pos
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Creates the scroll tween.
	/// If a tween already exists, it is killed.
	/// </summary>
	private void CreateTween() {
		// If a tween is already running, kill it!
		if(m_tween != null) {
			m_tween.Kill();
			m_tween = null;
		}

		// We want a constant speed regardless of the content size
		float normalizedSpeed = m_scrollSpeed / m_scroll.content.sizeDelta.y;
		m_tween = m_scroll.DOVerticalNormalizedPos(0f, normalizedSpeed)
			.SetSpeedBased()
			.SetRecyclable(true)
			.OnComplete(() => {
				// Auto-close popup once animation is completed!
				GetComponent<PopupController>().Close(true);
			})
			.Play();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The popup is about to open.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Hide the rest of UI elements so we get a clean background
		// HUD
		InstanceManager.menuSceneController.hud.animator.Hide();

		// Menu
		// Don't use animator, some things might break
		InstanceManager.menuSceneController.screensController.currentScreen.gameObject.SetActive(false);

		// Popups
		m_popupsToRestore = PopupManager.openedPopups;

		// Skip this one!
		m_popupsToRestore.Remove(this.GetComponent<PopupController>());

		// Reverse order, close older ones first
		for(int i = m_popupsToRestore.Count - 1; i >= 0; --i) {
			// Don't destroy! We want to restore them afterwards
			m_popupsToRestore[i].Close(false);
		}

		// Reset scroll position
		m_scroll.verticalNormalizedPosition = 1f;
	}

	/// <summary>
	/// The popup has opened.
	/// </summary>
	public void OnOpenPostAnimation() {
		// Start scrolling!
		CreateTween();
	}

	/// <summary>
	/// The popup is about to close.
	/// </summary>
	public void OnClosePreAnimation() {
		// Restore UI elements!
		// HUD
		InstanceManager.menuSceneController.hud.animator.Show();	// [AOC] We're assuming HUD was visible, store previous state if needed

		// Menu
		// Don't use animator, some things might break
		InstanceManager.menuSceneController.screensController.currentScreen.gameObject.SetActive(true);

		// Popups
		for(int i = 0; i < m_popupsToRestore.Count; ++i) {
			m_popupsToRestore[i].Open();
		}
	}

	/// <summary>
	/// The popup has been closed.
	/// </summary>
	public void OnClosePostAnimation() {

	}
}