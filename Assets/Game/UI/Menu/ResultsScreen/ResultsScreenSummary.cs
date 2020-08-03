// ResultsScreenSummary.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/10/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Small summary for the results screen
/// </summary>
public class ResultsScreenSummary : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[Separator("Textfields")]
	[SerializeField] private TextMeshProUGUI m_timeText = null;
	[SerializeField] private TextMeshProUGUI m_scoreText = null;
	[SerializeField] private TextMeshProUGUI m_coinsText = null;
	[SerializeField] private TextMeshProUGUI m_goldenFragmentsText = null;
	[SerializeField] private TextMeshProUGUI m_chestsText = null;
	[SerializeField] private TextMeshProUGUI m_eggsText = null;
	[SerializeField] private TextMeshProUGUI m_missionsText = null;

	[Separator("Other references")]
	[SerializeField] private Image m_chestsIcon = null;
	[SerializeField] private Image m_eggsIcon = null;

	[Separator("Animators")]
	[SerializeField] private ShowHideAnimator m_timeAnim = null;
	[SerializeField] private ShowHideAnimator m_scoreAnim = null;
	[SerializeField] private ShowHideAnimator m_coinsAnim = null;
	[SerializeField] private ShowHideAnimator m_goldenFragmentsAnim = null;
	[SerializeField] private ShowHideAnimator m_collectiblesAnim = null;
	[SerializeField] private ShowHideAnimator m_missionsAnim = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the summary.
	/// </summary>
	public void InitSummary() {
		// Disable all root objects
		m_timeAnim.transform.parent.gameObject.SetActive(false);
		m_scoreAnim.transform.parent.gameObject.SetActive(false);
		m_coinsAnim.transform.parent.gameObject.SetActive(false);
		m_goldenFragmentsAnim.transform.parent.gameObject.SetActive(false);
		m_collectiblesAnim.transform.parent.gameObject.SetActive(false);
		m_missionsAnim.transform.parent.gameObject.SetActive(false);

		// Reset the animations
		m_timeAnim.ForceHide(false);
		m_scoreAnim.ForceHide(false);
		m_coinsAnim.ForceHide(false);
		m_goldenFragmentsAnim.ForceHide(false);
		m_collectiblesAnim.ForceHide(false);
		m_missionsAnim.ForceHide(false);

		// Start hidden
		this.GetComponent<ShowHideAnimator>().ForceHide(false);
	}

	/// <summary>
	/// Show the time slot.
	/// </summary>
	/// <param name="_timeSeconds">Time to be displayed (seconds).</param>
	/// <param name="_animate">Animate slot?.</param>
	public void ShowTime(float _timeSeconds, bool _animate = true) {
		// Set text
		m_timeText.text = TimeUtils.FormatTime(
			_timeSeconds, 
			TimeUtils.EFormat.DIGITS, 2, 
			TimeUtils.EPrecision.MINUTES
		);
		
		// Show element
		ShowElement(m_timeAnim, _animate);
	}

	/// <summary>
	/// Show the score slot.
	/// </summary>
	/// <param name="_score">Score to be displayed.</param>
	public void ShowScore(long _score) {
		// Set text
		m_scoreText.text = StringUtils.FormatNumber(_score);

		// Show element
		ShowElement(m_scoreAnim, true);
	}

	/// <summary>
	/// Show the coins slot.
	/// </summary>
	/// <param name="_coins">Number of collected coins.</param>
	public void ShowCoins(long _coins) {
		// Set text
		m_coinsText.text = StringUtils.FormatNumber(_coins);

		// Show element
		ShowElement(m_coinsAnim, true);
	}

	/// <summary>
	/// Show the golden fragments slot.
	/// </summary>
	/// <param name="_goldenFragments">Number of collected golden fragments.</param>
	public void ShowGoldenFragments(long _goldenFragments) {
		// Set text
		m_goldenFragmentsText.text = StringUtils.FormatNumber(_goldenFragments);

		// Show element
		ShowElement(m_goldenFragmentsAnim, true);
	}

	/// <summary>
	/// Show the collectibles slot.
	/// </summary>
	/// <param name="_chests">Number of collected chests.</param>
	/// <param name="_eggs">Number of collected eggs.</param>
	public void ShowCollectibles(int _chests, int _eggs) {
		// Set texts
		SetChests(_chests);
		SetEgg(_eggs);

		// Show element!
		ShowElement(m_collectiblesAnim, true);
	}

	/// <summary>
	/// Show the missions slot.
	/// </summary>
	/// <param name="_missions">Number of completed missions.</param>
	public void ShowMissions(int _missions) {
		// Set text
		m_missionsText.text = StringUtils.FormatNumber(_missions);

		// Show element
		ShowElement(m_missionsAnim, true);
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Set the amount of chests to the counter with some flashy FX.
	/// </summary>
	/// <param name="_collectedChests">Total collected chests to be displayed. Negative value to hide the chests counter.</param>
	private void SetChests(int _collectedChests) {
		// Don't show if negative value.
		bool show = _collectedChests >= 0;
		m_chestsIcon.gameObject.SetActive(show);
		m_chestsText.gameObject.SetActive(show);

		// Set text
		if(show) {
			m_chestsText.text = LocalizationManager.SharedInstance.Localize(
				"TID_FRACTION",
				StringUtils.FormatNumber(_collectedChests),
				StringUtils.FormatNumber(ChestManager.NUM_DAILY_CHESTS)
			);
		}
	}

	/// <summary>
	/// Set the amount of eggs to the counter with some flashy FX.
	/// </summary>
	/// <param name="_collectedChests">Total collected eggs to be displayed. Negative value to hide the eggs counter.</param>
	private void SetEgg(int _collectedEggs) {
		// Don't show if negative value.
		bool show = _collectedEggs >= 0;
		m_eggsIcon.gameObject.SetActive(show);
		m_eggsText.gameObject.SetActive(show);

		// Set text
		if(show) {
			m_eggsText.text = LocalizationManager.SharedInstance.Localize(
				"TID_FRACTION",
				StringUtils.FormatNumber(_collectedEggs),
				StringUtils.FormatNumber(1)
			);
		}
	}

	/// <summary>
	/// Show a single element of the summary.
	/// Will put the element at the end of the summary layout.
	/// </summary>
	/// <param name="_element">The element to be displayed.</param>
	/// <param name="_animate">Trigger animation?</param>
	private void ShowElement(ShowHideAnimator _element, bool _animate) {
		// Unless element is already visible, put it at the end of the summary layout
		if(!_element.transform.parent.gameObject.activeInHierarchy) {
			_element.transform.parent.SetAsLastSibling();
		}

		// Activate root object
		_element.transform.parent.gameObject.SetActive(true);

		// Trigger animation!
		_element.ForceShow(_animate);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}