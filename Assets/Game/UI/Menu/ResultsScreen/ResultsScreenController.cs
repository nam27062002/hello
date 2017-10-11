using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class ResultsScreenController : MonoBehaviour {
    //------------------------------------------------------------------//
    // CONSTANTS														//
    //------------------------------------------------------------------//
    public const string NAME = "SC_ResultsScreen";

	// Execution order!
	public enum Step {
		INIT = 0,

		INTRO,				// Always, dragon animation
		SCORE,				// Always, run score + high score feedback
		REWARDS,			// Always, sc earned during the run

		COLLECTIBLES,		// Always, collected Eggs, Chests, etc.
		MISSIONS,			// Optional, completed missions

		XP,					// Always, dragon xp progression

		TRACKING,			// Logic step, send end of game tracking - before applying the rewards!
		APPLY_REWARDS,		// Logic step, apply rewards to the profile

		SKIN_UNLOCKED,		// Optional, if a skin was unlocked. As many times as needed if more than one skin was unlocked in the same run
		DRAGON_UNLOCKED,	// Optional, if a new dragon was unlocked

		GLOBAL_EVENT_CONTRIBUTION,		// Optional, if there is an active event and the player has a score to add to it
		GLOBAL_EVENT_NO_CONTRIBUTION,	// Optional, if there is an active event but the player didn't score

		FINISHED,

		COUNT
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private ResultsScreenStep[] m_steps = new ResultsScreenStep[(int)Step.COUNT];

	// Other references
	private ResultsSceneSetup m_scene = null;
	public ResultsSceneSetup scene {
		get { return m_scene; }
	}

	// Internal
	private Step m_step = Step.INIT;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Make use of properties to easily add test code without getting it all dirty
	// They can be static since they're getting the data from static singletons as well
	public long score {
		get {
			if(CPResultsScreenTest.testEnabled) {
				return CPResultsScreenTest.scoreValue;
			} else {
				return RewardManager.score;
			}
		}
	}

	public long coins {
		get {
			if(CPResultsScreenTest.testEnabled) {
				return CPResultsScreenTest.coinsValue;
			} else {
				return RewardManager.coins;
			}
		}
	}

	public int survivalBonus {
		get {
			if(CPResultsScreenTest.testEnabled) {
				return (int)CPResultsScreenTest.survivalBonus;
			} else {
				return RewardManager.instance.CalculateSurvivalBonus();
			}
		}
	}

	public long highScore {
		get {
			if(CPResultsScreenTest.testEnabled) {
				return CPResultsScreenTest.highScoreValue;
			} else {
				return UsersManager.currentUser.highScore;
			}
		}
	}

	public bool isHighScore {
		get {
			if(CPResultsScreenTest.testEnabled) {
				return CPResultsScreenTest.newHighScore;
			} else {
				return RewardManager.isHighScore;
			}
		}
	}

	public float time {
		get {
			if(CPResultsScreenTest.testEnabled) {
				return CPResultsScreenTest.timeValue;
			} else {
				return InstanceManager.gameSceneControllerBase.elapsedSeconds;
			}
		}
	}

	private bool m_eggFound = false;
	public bool eggFound {
		get { return m_eggFound; }
	}

	// Accumulated rewards during the results flow
	private long m_totalCoins = 0;
	public long totalCoins {
		get { return m_totalCoins; }
		set { m_totalCoins = value; }
	}

	private long m_totalPc = 0;
	public long totalPc {
		get { return m_totalPc; }
		set { m_totalPc = value; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to the finished event on each step
		for(int i = 0; i < m_steps.Length; ++i) {
			if(m_steps[i] != null) {
				m_steps[i].OnFinished.AddListener(OnStepFinished);
			}
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from the finished event on each step
		for(int i = 0; i < m_steps.Length; ++i) {
			if(m_steps[i] != null) {
				m_steps[i].OnFinished.RemoveListener(OnStepFinished);
			}
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the screen with the given parameters and start the flow.
	/// </summary>
	/// <param name="_scene">Reference to the 3D scene we'll be using.</param>
	public void StartFlow(ResultsSceneSetup _scene) {
		// Store the scene reference for future use
		m_scene = _scene;

		// Setup dark screen
		ResultsDarkScreen.targetCamera = m_scene.camera;
		ResultsDarkScreen.Hide(false);

		// Initialize some internal vars
		m_totalCoins = UsersManager.currentUser.coins;
		m_totalPc = UsersManager.currentUser.pc;
		m_eggFound = false;
		switch(CPResultsScreenTest.eggMode) {
			case CPResultsScreenTest.EggTestMode.FOUND: {
				m_eggFound = true; 
			} break;

			case CPResultsScreenTest.EggTestMode.NOT_FOUND: {
				m_eggFound = false; 
			} break;

			case CPResultsScreenTest.EggTestMode.RANDOM: {
				m_eggFound = (Random.Range(0f, 1f) > 0.5f); 
			} break;

			case CPResultsScreenTest.EggTestMode.NONE: {
				m_eggFound = CollectiblesManager.egg != null && CollectiblesManager.egg.collected;
			} break;
		}

		// Initialize all steps
		for(int i = 0; i < m_steps.Length; ++i) {
			if(m_steps[i] != null) {
				// Tell them we're their parent
				m_steps[i].Init(this);

				// Start hidden!
				m_steps[i].gameObject.SetActive(false);
			}
		}

		// Launch first step!
		m_step = Step.INIT;
		LaunchNextStep();
	}

	//------------------------------------------------------------------------//
	// STEP CONTROL METHODS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the controller of a specific step.
	/// </summary>
	/// <returns>The step controller.</returns>
	/// <param name="_step">Step whose controller we want.</param>
	public ResultsScreenStep GetStep(Step _step) {
		return m_steps[(int)_step];
	}

	/// <summary>
	/// Checks the next step to be displayed and launches it. 
	/// </summary>
	private void LaunchNextStep() {
		// Find out next step
		m_step = CheckNextStep();

		// If we're at the last step, go back to menu!
		if(m_step == Step.FINISHED) {
			GoToMenu();
			return;
		}

		// Launch the target step! We'll receive the OnStepFinished callback when step has finished
		ResultsScreenStep targetStep = m_steps[(int)m_step];
		targetStep.gameObject.SetActive(true);
		targetStep.Launch();
	}

	/// <summary>
	/// Check which step to display next.
	/// </summary>
	/// <returns>The next step to be displayed. Step.FINISHED if none.</returns>
	public Step CheckNextStep() {
		// Just use recursive call
		return CheckNextStep(m_step);
	}

	/// <summary>
	/// Given a step, check which step to display next.
	/// </summary>
	/// <returns>The next step to be displayed. Step.FINISHED if none.</returns>
	/// <param name="_step">Step to be checked.</param>
	public Step CheckNextStep(Step _step) {
		// Increase step index
		_step++;
		ResultsScreenStep targetStep = m_steps[(int)_step];

		// If we're at the last step, we're done!
		if(_step == Step.FINISHED) {
			return Step.FINISHED;	// End of recursivity
		}

		// If the step has no logic assigned, skip it
		else if(targetStep == null) {
			return CheckNextStep(_step);	// Recursive call
		}

		// If step mustn't be displayed, skip it
		else if(!targetStep.MustBeDisplayed()) {
			return CheckNextStep(_step);	// Recursive call
		}

		// This is the next step! Return it
		else {
			return _step;
		}
	}

	/// <summary>
	/// Performed all required actions to go back to the menu.
	/// </summary>
	private void GoToMenu() {
		// Show loading screen
		LoadingScreen.Toggle(true, false);

		// If a new dragon was unlocked, tell the menu to show the dragon unlocked screen first!
		// [AOC] TODO!!
		/*if(m_unlockBar.newDragonUnlocked) {
			GameVars.unlockedDragonSku = m_unlockBar.nextDragonData.def.sku;
		}*/

		// Send FTUX tracking event
		if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.DRAGON_SELECTION)) {
			HDTrackingManager.Instance.Notify_Funnel_FirstUX(FunnelData_FirstUX.Steps._05_continue_clicked);
		}

		// Go back to main menu
		FlowManager.GoToMenu();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The current step has finished.
	/// </summary>
	public void OnStepFinished() {
		// Hide current step
		ResultsScreenStep currentStep = m_steps[(int)m_step];
		if(currentStep != null) currentStep.gameObject.SetActive(false);

		// Just check next step
		LaunchNextStep();
	}
}
