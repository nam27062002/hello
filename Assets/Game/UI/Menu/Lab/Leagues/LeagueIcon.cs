// LeagueIcon.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/10/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple UI Widget to standalone control and initialize league icon.
/// </summary>
public class LeagueIcon : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[Tooltip("Optional")] [SerializeField] private Image m_leagueIcon = null;
	[Tooltip("Optional")] [SerializeField] private Localizer m_nameText = null;
	
	// Data
	private DefinitionNode m_leagueDef = null;
	public DefinitionNode leagueDef {
		get { return m_leagueDef; }
	}

	// Internal references (shortcuts)
	private ShowHideAnimator m_anim = null;
	public ShowHideAnimator anim {
		get {
			if(m_anim == null) m_anim = GetComponent<ShowHideAnimator>();
			return m_anim;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this button with the data from the given definition.
	/// </summary>
	/// <param name="_leagueDef">Power definition.</param>
	/// <param name="_animate">Optional, whether to show animations or not.</param>
	public void InitFromDefinition(DefinitionNode _leagueDef, bool _animate = true) {
		// Save definition
		m_leagueDef = _leagueDef;
		bool show = (_leagueDef != null);


		// Don't show if given definition is not valid
		// Use animator if available
		if(anim != null) {
			anim.Set(show, _animate);
		} else {
			this.gameObject.SetActive(show);
		}

		// If showing, initialize all visible items
		if(show) {
			Init(_leagueDef.Get("tidName"), _leagueDef.GetAsString("icon"));
		}
	}

	/// <summary>
	/// Initialize with custom data.
	/// </summary>
	/// <param name="_tidName">TID of the league name.</param>
	/// <param name="_icon">Name of the icon of the league.</param>
	public void Init(string _tidName, string _icon) {
		// League icon
		if(m_leagueIcon != null) {
			// Load from resources
			m_leagueIcon.sprite = Resources.Load<Sprite>(UIConstants.LEAGUE_ICONS_PATH + _icon);
		}

		// Name
		if(m_nameText != null) {
			m_nameText.Localize(_tidName);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}