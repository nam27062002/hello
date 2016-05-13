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

	[Separator]
	[SerializeField] private Localizer m_highScoreLabel = null;
	[SerializeField] private GameObject m_newHighScoreDeco = null;

	[Separator]
	[SerializeField] private Slider m_levelBar = null;
	[SerializeField] private Localizer m_levelText = null;
	[SerializeField] private Localizer m_dragonNameText = null;

	[Separator]
	[SerializeField] private GameObject m_nextDragonGroup = null;
	[SerializeField] private GameObject m_nextDragonScene3DPrefab = null;

	// Internal
	private int m_levelAnimCount = 0;
	private Tween m_xpBarTween = null;

	private UIScene3D m_nextDragonScene3D = null;

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

		DebugUtils.Assert(m_levelBar != null, "Required field not initialized!");
		DebugUtils.Assert(m_levelText != null, "Required field not initialized!");
		DebugUtils.Assert(m_dragonNameText != null, "Required field not initialized!");

		DebugUtils.Assert(m_nextDragonGroup != null, "Required field not initialized!");
		DebugUtils.Assert(m_nextDragonScene3DPrefab != null, "Required field not initialized!");
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

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// If we got a tween running, kill it immediately
		if(m_xpBarTween != null) {
			m_xpBarTween.Kill(false);
			m_xpBarTween = null;
		}

		// Destroy next dragon 3D scene
		if(m_nextDragonScene3D != null) {
			UIScene3DManager.Remove(m_nextDragonScene3D);
			m_nextDragonScene3D = null;
		}
	}

	/// <summary>
	/// Initislization.
	/// </summary>
	public void Initialize() {
		// set values from score manager
		// Launch number animators
		int survivalBonus = RewardManager.instance.CalculateSurvivalBonus();

		m_scoreAnimator.SetValue(0, (int)RewardManager.score);
		m_coinsAnimator.SetValue(0, (int)(RewardManager.coins + survivalBonus));
		m_bonusCoinsAnimator.SetValue(0, RewardManager.instance.CalculateSurvivalBonus()); //TODO: get bouns coins from Reward Manager

		m_highScoreLabel.Localize(m_highScoreLabel.tid, StringUtils.FormatNumber(UserProfile.highScore));

		m_newHighScoreDeco.SetActive(RewardManager.isHighScore);

		// Set time - format to MM:SS
		GameSceneController game = InstanceManager.GetSceneController<GameSceneController>();
		m_timeLabel.text = TimeUtils.FormatTime(game.elapsedSeconds, TimeUtils.EFormat.DIGITS, 2, TimeUtils.EPrecision.MINUTES);

		// Set dragon name and xp
		// Get updated dragon's data from the dragon manager
		DragonData data = DragonManager.currentDragon;
		m_dragonNameText.Localize(data.def.Get("tidName"));

		// Bar value - animate!
		// [AOC] As usual, animating the XP bar is not obvious (dragon may have leveled up several times during a single game)
		m_levelBar.minValue = 0;
		m_levelBar.maxValue = 1;
		m_levelBar.value = RewardManager.dragonInitialLevelProgress;
		m_levelAnimCount = RewardManager.dragonInitialLevel;
		m_levelText.Localize("TID_LEVEL_ABBR", StringUtils.FormatNumber(m_levelAnimCount + 1));
		LaunchXPBarAnim();

		// Next dragon unlock
		// Don't show if next dragon is already unlocked or if the dragon we played with was already maxed
		DragonData nextDragonData = DragonManager.nextDragon;
		bool show = RewardManager.dragonInitialUnlockProgress < 1f && nextDragonData != null && !nextDragonData.isOwned;
		m_nextDragonGroup.SetActive(show);
		if(show) {
			// Bar: animate!
			Slider nextDragonBar = m_nextDragonGroup.GetComponentInChildren<Slider>();
			if(nextDragonBar) {
				// Initialize bar
				nextDragonBar.minValue = 0;
				nextDragonBar.maxValue = 1;
				nextDragonBar.value = RewardManager.dragonInitialUnlockProgress;

				// Program animation
				LaunchNextDragonBarAnim(nextDragonBar);
			}

			// Next dragon 3D preview
			// Clear any existing 3D scene
			if(m_nextDragonScene3D != null) {
				UIScene3DManager.Remove(m_nextDragonScene3D);
				m_nextDragonScene3D = null;
			}
				
			// Create and initializ a new 3D scene
			m_nextDragonScene3D = UIScene3DManager.CreateFromPrefab<UIScene3D>(m_nextDragonScene3DPrefab);
			MenuDragonLoader dragonLoader = m_nextDragonScene3D.FindComponentRecursive<MenuDragonLoader>();
			if(dragonLoader != null) {
				// Load and pose the dragon
				dragonLoader.LoadDragonPreview(nextDragonData.def.sku);
				dragonLoader.FindComponentRecursive<Animator>().SetTrigger("idle");
			}

			// Initialize the raw image where the dragon will be rendered
			RawImage nextDragonRawImage = m_nextDragonGroup.GetComponentInChildren<RawImage>();
			if(nextDragonRawImage != null) {
				nextDragonRawImage.texture = m_nextDragonScene3D.renderTexture;
				nextDragonRawImage.color = Colors.white;
			}

			// Hide unlock group
			GameObject unlockFXGroup = m_nextDragonGroup.FindObjectRecursive("UnlockFX");
			unlockFXGroup.SetActive(false);
		}
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
			targetDelta, 0.75f
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

	/// <summary>
	/// Launchs the animation of the "unlock next dragon" bar
	/// </summary>
	/// <param name="_bar">The bar to be animated.</param>
	private void LaunchNextDragonBarAnim(Slider _bar) {
		// Aux vars
		bool unlockCheat = false;
		float targetDelta = DragonManager.currentDragon.progression.progressByXp;
		if(unlockCheat) targetDelta = 1f;

		// Launch the tween!
		DOTween.To(
			// Getter function
			() => { 
				return _bar.value; 
			}, 

			// Setter function
			(_newValue) => {
				_bar.value = _newValue;
			},

			// Value and speed
			targetDelta, 0.5f
		)

		// Other setup parameters
			.SetSpeedBased(true)
			.SetEase(Ease.InOutCubic)

			// What to do once the anim has finished?
			.OnComplete(
				() => {
					// If we reached max delta, a dragon has been unlocked!
					if(targetDelta >= 1f || unlockCheat) {
						// Aux vars
						DragonData nextDragonData = DragonManager.nextDragon;
						float speedMult = 1f;	// To easily adjust timings
						GameObject unlockFXGroup = m_nextDragonGroup.FindObjectRecursive("UnlockFX");

						// Prepare for animation
						unlockFXGroup.SetActive(true);
						unlockFXGroup.transform.localScale = Vector3.zero;

						// Animation
						DOTween.Sequence()
							// Pause
							.AppendInterval(0.5f)

							// Scale up
							.Append(unlockFXGroup.transform.DOScale(1f, 0.25f * speedMult).SetEase(Ease.OutBack))

							// Change bar text as well
							.AppendCallback(() => {
								Localizer barText = m_nextDragonGroup.FindComponentRecursive<Localizer>("InfoText");
								if(barText != null) {
									barText.Localize("TID_RESULTS_DRAGON_UNLOCKED", nextDragonData.def.GetLocalized("tidName"));
								}
							})

							// Pause
							.AppendInterval(1f)

							// Scale out
							.Append(unlockFXGroup.transform.DOScale(2f, 0.5f * speedMult).SetEase(Ease.OutCubic))

							// Fade out
							.Join(unlockFXGroup.GetComponent<CanvasGroup>().DOFade(0f, 0.5f * speedMult).SetEase(Ease.OutCubic))

							// Disable object once the sequence is completed
							.OnComplete(() => {
								unlockFXGroup.SetActive(false);
							})

							// Go!!!
							.Play();

						// Update text with the name of the unlocked dragon
						Localizer unlockText = unlockFXGroup.FindComponentRecursive<Localizer>("UnlockText");
						if(unlockText != null) unlockText.Localize("TID_RESULTS_DRAGON_UNLOCKED", nextDragonData.def.GetLocalized("tidName"));

						// Play cool dragon animation!
						/*if(m_nextDragonScene3D != null) {
							Animator anim = m_nextDragonScene3D.FindComponentRecursive<Animator>();
							if(anim != null) {
								anim.SetTrigger("unlocked");
							}
						}*/
					}
				}
			);
	}

	/// <summary>
	/// Go back to menu!
	/// </summary>
	private void GoToMenu() {
		// Update global stats
		UserProfile.gamesPlayed = UserProfile.gamesPlayed + 1;

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
		// Was it the chest reward popup?
		if(_popup.GetComponent<PopupChestReward>() != null) {
			// Go back to menu
			GoToMenu();
		}
	}

	/// <summary>
	/// Go back to the main menu, finalizing all the required stuff in the game scene.
	/// </summary>
	public void OnGoToMenu() {
		// If we found a chest, open the chest reward popup
		if(ChestManager.selectedChest != null && ChestManager.selectedChest.collected) {
			PopupManager.OpenPopupAsync(PopupChestReward.PATH);
		} else {
			GoToMenu();
		}
	}
}
