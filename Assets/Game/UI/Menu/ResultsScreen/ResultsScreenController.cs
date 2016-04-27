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

	void OnEnable() {
		// set values from score manager
		// Launch number animators
		m_scoreAnimator.SetValue(0, (int)RewardManager.score);
		m_coinsAnimator.SetValue(0, (int)RewardManager.coins);
		m_bonusCoinsAnimator.SetValue(0, 0); //TODO: get bouns coins from Reward Manager

		m_highScoreLabel.text = "High Score: " + RewardManager.score;

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


	//------------------------------------------------------------------//
	// DELEGATES														//
	//------------------------------------------------------------------//

	public void OnGoToMenu() {
		FlowManager.GoToMenu();
	}
}
