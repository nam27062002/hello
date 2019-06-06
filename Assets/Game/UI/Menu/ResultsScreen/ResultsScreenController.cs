using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class ResultsScreenController : MonoBehaviour {
    //------------------------------------------------------------------//
    // CONSTANTS														//
    //------------------------------------------------------------------//
    public const string NAME = "SC_ResultsScreen";

	// [AOC] ADD THEM TO THE EDITOR AS WELL!
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

		TOURNAMENT_COINS,		// Tournament, gold obtained during the run (same as REWARDS step, but no survival bonus)
		TOURNAMENT_SCORE,		// Tournament, show run score
		TOURNAMENT_LEADERBOARD,	// Tournament, show leaderboard changes
		TOURNAMENT_INVALID_RUN,	// Tournament, run didn't count for the tournament (i.e. "Eat 100 birds as fast as possible" but you died before reaching 100 birds)
		TOURNAMENT_SYNC,		// Tournament, sync with server, apply rewards and do tracking

		LEAGUE_SCORE,			// Special Dragons League, show run score and "new high score" if moving up the ladder
		LEAGUE_LEADERBOARD,		// Special Dragons League, show leaderboard changes
		LEAGUE_SYNC,			// Special Dragons League, sync with server, apply rewards and do tracking

		PAUSE,					// Simple step to add a "Tap To Continue" pause

		COUNT
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private ResultsScreenSummary m_summary = null;
	public ResultsScreenSummary summary {
		get { return m_summary; }
	}

	[SerializeField] private ResultsScreenStep[] m_steps = new ResultsScreenStep[(int)Step.COUNT];

	[Reorderable]
	[HideEnumValues(true, true)]
	[SerializeField] private Step[] m_tournamentStepsSequence = new Step[0];

	[Reorderable]
	[HideEnumValues(true, true)]
	[SerializeField] private Step[] m_defaultStepsSequence = new Step[0];

	[Reorderable]
	[HideEnumValues(true, true)]
	[SerializeField] private Step[] m_specialDragonStepsSequence = new Step[0];

	// Other references
	private ResultsSceneSetup m_scene = null;
	public ResultsSceneSetup scene {
		get { return m_scene; }
	}

	// Internal
	private int m_stepIdx = -1;
	private Step[] m_activeSequence = null;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Make use of properties to easily add test code without getting it all dirty
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

    private long m_totalGf = 0;
    public long totalGf {
        get { return m_totalGf; }
        set { m_totalGf = value; }
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
		m_totalCoins = UsersManager.currentUser.coins - this.coins;		// Coins have been added in real-time, so start the results screen counter with the amount of coins we had before the run
		m_totalPc = UsersManager.currentUser.pc;
        m_totalGf = UsersManager.currentUser.goldenEggFragments;

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

		// Initialize 3D scene
		m_scene.Init();

		// Initialize summary as well
		m_summary.InitSummary();

		// Disable all steps
		for(int i = 0; i < m_steps.Length; ++i) {
			if(m_steps[i] != null) {
				// Start hidden!
				m_steps[i].gameObject.SetActive(false);
			}
		}

		// Choose steps sequence based on current game mode
		switch(SceneController.mode) {
			case GameSceneController.Mode.TOURNAMENT: {
				m_activeSequence = m_tournamentStepsSequence;
			} break;

			case GameSceneController.Mode.SPECIAL_DRAGONS: {
				m_activeSequence = m_specialDragonStepsSequence;
			} break;

			default: {
				m_activeSequence = m_defaultStepsSequence;
			} break;
		}

		// Init those steps that are gonna be used
		for(int i = 0; i < m_activeSequence.Length; ++i) {
			GetStep(m_activeSequence[i]).Init(this, m_activeSequence[i]);
		}

		// Launch first step!
		m_stepIdx = -1;
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
		m_stepIdx = CheckNextStep();

        if (m_stepIdx == (int)Step.INIT) {
            HDTrackingManager.Instance.Notify_LoadingResultsEnd();
        }

		// If we're at the last step, go back to menu!
		if(m_stepIdx >= m_activeSequence.Length) {
			GoToMenu();
			return;
		}

		// Launch the target step! We'll receive the OnStepFinished callback when step has finished
		ResultsScreenStep targetStep = GetStep(m_activeSequence[m_stepIdx]);
		targetStep.gameObject.SetActive(true);
		targetStep.Launch();
	}

	/// <summary>
	/// Check which step to display next.
	/// </summary>
	/// <returns>The next step to be displayed. Step.FINISHED if none.</returns>
	public int CheckNextStep() {
		// Just use recursive call
		return CheckNextStep(m_stepIdx);
	}

	/// <summary>
	/// Given a step, check which step to display next.
	/// </summary>
	/// <returns>The next step to be displayed. Step.FINISHED if none.</returns>
	/// <param name="_stepIdx">Index of the step to be checked.</param>
	public int CheckNextStep(int _stepIdx) {
		// Increase step index
		_stepIdx++;

		// If we're at the last step, we're done!
		if(_stepIdx >= m_activeSequence.Length) {
			return _stepIdx;	// End of recursivity
		}

		// Get target step from current sequence
		ResultsScreenStep targetStep = GetStep(m_activeSequence[_stepIdx]);

		// If the step has no logic assigned, skip it
		if(targetStep == null) {
			return CheckNextStep(_stepIdx);	// Recursive call
		}

		// If step mustn't be displayed, skip it
		else if(!targetStep.MustBeDisplayed()) {
			return CheckNextStep(_stepIdx);	// Recursive call
		}

		// This is the next step! Return it
		else {
			return _stepIdx;
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

		// Tell the menu where to go based on current game mode (or other modifiers)
		switch(GameSceneController.mode) {
			case GameSceneController.Mode.TOURNAMENT: {
				// Unless score was dismissed due to some error, in which case we'll return to the PLAY screen
				ResultsScreenStepTournamentSync syncStep = GetStep(Step.TOURNAMENT_SYNC) as ResultsScreenStepTournamentSync;
				if(syncStep != null && syncStep.hasBeenDismissed) {
					GameVars.menuInitialScreen = MenuScreen.PLAY;
				} else {
					GameVars.menuInitialScreen = MenuScreen.TOURNAMENT_INFO;
				}
			} break;

            case GameSceneController.Mode.SPECIAL_DRAGONS: {
                    GameVars.menuInitialScreen = MenuScreen.LAB_DRAGON_SELECTION;  
            } break;

			default: {
				GameVars.menuInitialScreen = MenuScreen.NONE;	// By setting NONE, default behaviour will apply (dragon selection) (MenuTransitionManager::Start)
			} break;
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
		ResultsScreenStep currentStep = GetStep(m_activeSequence[m_stepIdx]);
		if(currentStep != null) currentStep.gameObject.SetActive(false);

		// Just check next step
		LaunchNextStep();
	}
}
