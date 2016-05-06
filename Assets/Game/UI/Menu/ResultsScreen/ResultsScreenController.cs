using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ResultsScreenController : MonoBehaviour {

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private Text m_timeLabel = null;
	[SerializeField] private NumberTextAnimator m_scoreAnimator = null;
	[SerializeField] private NumberTextAnimator m_coinsAnimator = null;
	[SerializeField] private NumberTextAnimator m_bonusCoinsAnimator = null;

	[SerializeField] private Text m_highScoreLabel = null;
	[SerializeField] private GameObject m_newHighScoreDeco = null;

	[SerializeField] private Slider m_levelBar;
	[SerializeField] private Localizer m_levelText;

	// Internal
	private int m_levelAnimCount = 0;
	private Tween m_xpBarTween = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//

	void Awake() {
		// Check required fields
		DebugUtils.Assert(m_timeLabel != null, "Required field not initialized!");
		DebugUtils.Assert(m_scoreAnimator != null, "Required field not initialized!");
		DebugUtils.Assert(m_coinsAnimator != null, "Required field not initialized!");
		DebugUtils.Assert(m_bonusCoinsAnimator != null, "Required field not initialized!");
		DebugUtils.Assert(m_highScoreLabel != null, "Required field not initialized!");
		DebugUtils.Assert(m_newHighScoreDeco != null, "Required field not initialized!");
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<PopupController>(EngineEvents.POPUP_CLOSED, OnPopupClosed);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<PopupController>(EngineEvents.POPUP_CLOSED, OnPopupClosed);
	}

	private void OnDestroy() {
		// If we got a tween running, kill it immediately
		if(m_xpBarTween != null) {
			m_xpBarTween.Kill(false);
			m_xpBarTween = null;
		}
	}

	public void Initialize() {
		// set values from score manager
		// Launch number animators
		int survivalBonus = RewardManager.instance.CalculateSurvivalBonus();

		m_scoreAnimator.SetValue(0, (int)RewardManager.score);
		m_coinsAnimator.SetValue(0, (int)(RewardManager.coins + survivalBonus));
		m_bonusCoinsAnimator.SetValue(0, RewardManager.instance.CalculateSurvivalBonus()); //TODO: get bouns coins from Reward Manager

		m_highScoreLabel.text = "High Score: " + StringUtils.FormatNumber(UserProfile.highScore);	// [AOC] HARDCODED!!

		m_newHighScoreDeco.SetActive(RewardManager.isHighScore);

		// Set time - format to MM:SS
		GameSceneController game = InstanceManager.GetSceneController<GameSceneController>();
		m_timeLabel.text = TimeUtils.FormatTime(game.elapsedSeconds, TimeUtils.EFormat.DIGITS, 2, TimeUtils.EPrecision.MINUTES);

		// Set Dragon Level
		// Get new dragon's data from the dragon manager
		DragonData data = DragonManager.currentDragon;

		// Bar value - animate!
		// [AOC] As usual, animating the XP bar is not obvious (dragon may have leveled up several times during a single game)
		m_levelBar.minValue = 0;
		m_levelBar.maxValue = 1;
		m_levelBar.value = RewardManager.dragonInitialLevelProgress;
		m_levelAnimCount = RewardManager.dragonInitialLevel;
		m_levelText.Localize("TID_LEVEL_ABBR", StringUtils.FormatNumber(m_levelAnimCount + 1));
		LaunchXPBarAnim();
	}

	/// <summary>
	/// Launches the XP anim.
	/// </summary>
	private void LaunchXPBarAnim() {
		// Aux vars
		DragonData data = DragonManager.currentDragon;
		bool isTargetLevel = (m_levelAnimCount == DragonManager.currentDragon.progression.level);
		float targetDelta = isTargetLevel ? data.progression.progressCurrentLevel : 1f;	// Full bar if not target level

		// Create animation
		m_xpBarTween = DOTween.To(
			// Getter function
			() => { 
				return m_levelBar.value; 
			}, 

			// Setter function
			(_newValue) => {
				m_levelBar.value = _newValue;
			},

			// Value and speed
			targetDelta, 1f
		)

		// Other setup parameters
		.SetSpeedBased(true)
		.SetEase(Ease.InOutCubic)

		// What to do once the anim has finished?
		.OnComplete(
			() => {
				// Was it the target level? We're done!
				if(isTargetLevel) return;

				// Not the target level, increase level counter and restart animation!
				m_levelAnimCount++;

				// Set text and animate
				m_levelText.Localize("TID_LEVEL_ABBR", StringUtils.FormatNumber(m_levelAnimCount + 1));
				m_levelText.transform.DOScale(1.5f, 0.15f).SetLoops(2, LoopType.Yoyo);

				// Put bar to the start
				m_levelBar.value = 0f;

				// Lose tween reference (will be self-destroyed immediately) and create a new one
				m_xpBarTween = null;
				LaunchXPBarAnim();
			}
		);
	}

	private void GoToMenu() {
		// [AOC] TODO!! Update global stats

		// Apply rewards to user profile
		RewardManager.ApplyRewardsToProfile();

		// Process Missions: give rewards and generate new missions replacing those completed
		MissionManager.ProcessMissions();

		// Clear chest manager
		ChestManager.ClearSelectedChest();

		// Save persistence
		PersistenceManager.Save();

		// Go back to main menu
		FlowManager.GoToMenu();
	}

	//------------------------------------------------------------------//
	// DELEGATES														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A popup has been closed.
	/// </summary>
	/// <param name="_popup">The popup that has been closed</param>
	public void OnPopupClosed(PopupController _popup) {
		if(_popup.GetComponent<PopupChestReward>() != null) {
			// Go back to menu
			GoToMenu();
		}
	}

	/// <summary>
	/// Go back to the main menu, finalizing all the required stuff in the game scene.
	/// </summary>
	public void OnGoToMenu() {
		if(ChestManager.selectedChest != null && ChestManager.selectedChest.collected) {
			PopupManager.OpenPopupAsync(PopupChestReward.PATH);
		} else {
			GoToMenu();
		}
	}
}
