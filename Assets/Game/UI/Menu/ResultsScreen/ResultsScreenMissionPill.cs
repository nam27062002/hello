// ResultsScreenMissionPill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Progression to unlock next dragon.
/// </summary>
public class ResultsScreenMissionPill : ResultsScreenCarouselPill {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private static readonly float ANIM_SPEED_MULT = 1f;	// To easily adjust timings
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private Image m_missionIcon = null;
	[SerializeField] private Text m_missionText = null;
	[SerializeField] private Text m_rewardText = null;
	[SerializeField] private Image m_completedFXIcon = null;
	[SerializeField] private NumberTextAnimator m_coinsTotalAnimator = null;

	// Setup
	public Mission.Difficulty difficulty = Mission.Difficulty.EASY;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_missionIcon != null, "Required field not initialized!");
		Debug.Assert(m_missionText != null, "Required field not initialized!");
		Debug.Assert(m_rewardText != null, "Required field not initialized!");
		Debug.Assert(m_completedFXIcon != null, "Required field not initialized!");
		Debug.Assert(m_coinsTotalAnimator != null, "Required field not initialized!");
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------------//
	// ResultsScreenCarouselPill IMPLEMENTATION								  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this pill must be displayed on the carousel or not.
	/// </summary>
	/// <returns><c>true</c> if the pill must be displayed on the carousel, <c>false</c> otherwise.</returns>
	public override bool MustBeDisplayed() {
		// Must be displayed if mission objective was completed!
		Mission targetMission = MissionManager.GetMission(difficulty);
		if(targetMission == null) return false;

		return targetMission.objective.isCompleted;
	}

	/// <summary>
	/// Initializes, shows and animates the pill.
	/// The <c>OnFinished</c> event will be invoked once the animation has finished.
	/// </summary>
	protected override void StartInternal() {
		// Aux vars
		Mission targetMission = MissionManager.GetMission(difficulty);
		if(targetMission == null) return;

		// Mission description
		m_missionText.text = targetMission.objective.GetDescription();

		// Reward
		m_rewardText.text = StringUtils.FormatNumber(targetMission.rewardCoins);

		// Change Icon
		m_missionIcon.sprite = Resources.Load<Sprite>(targetMission.def.GetAsString("icon"));

		// Trigger "completed" animation
		m_completedFXIcon.color = Colors.WithAlpha(Colors.white, 0f);
		DOTween.Sequence()
			// Initial delay, give some time for the pill to show
			.SetId(this)
			.AppendInterval(0.5f * ANIM_SPEED_MULT)

			// Add reward to the summary's total
			.AppendCallback(() => {
				m_coinsTotalAnimator.SetValue(m_coinsTotalAnimator.finalValue + targetMission.rewardCoins);
			})

			// Show check mark
			.Append(m_completedFXIcon.DOFade(1f, 0.5f * ANIM_SPEED_MULT))
			.Join(m_completedFXIcon.transform.DOScale(3f, 1f * ANIM_SPEED_MULT).From().SetEase(Ease.OutBounce))

			// Final delay before finishing
			.AppendInterval(1f * ANIM_SPEED_MULT)
			.AppendCallback(() => {
				OnFinished.Invoke();
			})

			.Play();

		// Show ourselves!
		gameObject.SetActive(true);
		animator.Show();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}