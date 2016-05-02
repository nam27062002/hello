using UnityEngine;
using UnityEngine.UI;

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

	public void Initialize() {
		// set values from score manager
		// Launch number animators
		int survivalBonus = RewardManager.instance.CalculateSurvivalBonus();

		m_scoreAnimator.SetValue(0, (int)RewardManager.score);
		m_coinsAnimator.SetValue(0, (int)(RewardManager.coins + survivalBonus));
		m_bonusCoinsAnimator.SetValue(0, RewardManager.instance.CalculateSurvivalBonus()); //TODO: get bouns coins from Reward Manager

		m_highScoreLabel.text = "High Score: " + UserProfile.highScore;

		m_newHighScoreDeco.SetActive(RewardManager.isHighScore);

		// Set time - format to MM:SS
		GameSceneController game = InstanceManager.GetSceneController<GameSceneController>();
		m_timeLabel.text = TimeUtils.FormatTime(game.elapsedSeconds, TimeUtils.EFormat.ABBREVIATIONS, 2, TimeUtils.EPrecision.MINUTES);

		// Set Dragon Level
		// Get new dragon's data from the dragon manager
		DragonData data = DragonManager.GetDragonData(UserProfile.currentDragon);

		// Bar value
		m_levelBar.minValue = 0;
		m_levelBar.maxValue = 1;
		m_levelBar.value = data.progression.progressCurrentLevel;

		// Text
		m_levelText.Localize("TID_LEVEL_ABBR", StringUtils.FormatNumber( (float)data.progression.level+1, 0));
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
