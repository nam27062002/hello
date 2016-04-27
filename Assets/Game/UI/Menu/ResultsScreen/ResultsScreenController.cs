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

		// Initialize some of the popup's members
		// Set initial values
		m_scoreAnimator.SetValue(0, 0);
		m_coinsAnimator.SetValue(0, 0);
		m_bonusCoinsAnimator.SetValue(0, 0);
		m_highScoreLabel = "0";

		// Set time - format to MM:SS
		GameSceneController game = InstanceManager.GetSceneController<GameSceneController>();
		m_timeLabel.text = TimeUtils.FormatTime(game.elapsedSeconds, TimeUtils.EFormat.ABBREVIATIONS, 2, TimeUtils.EPrecision.MINUTES);

		// Hide high score decoration
		m_newHighScoreDeco.SetActive(false);
	}

	void OnEnable() {
		// set values from score manager

	}


	//------------------------------------------------------------------//
	// DELEGATES														//
	//------------------------------------------------------------------//

	public void OnGoToMenu() {
		FlowManager.GoToMenu();
	}
}
