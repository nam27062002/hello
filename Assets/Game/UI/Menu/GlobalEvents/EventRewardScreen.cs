using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventRewardScreen : MonoBehaviour {	
	private enum State {
		Animation = 0,
		OpenNextReward
	}


	[SerializeField] private GameObject m_introScreen;
	[SerializeField] private GameObject m_globalEventStepScreen;
	[SerializeField] private ShowHideAnimator m_topToContinueText;

	[SerializeField] private DragControlRotation m_rewardDragController = null;

	[SerializeField] private GlobalEventsProgressBar m_progressBar;

	private RewardSceneController m_sceneController = null;

	private GlobalEvent m_event;
	private int m_step;

	private State m_state;


	//--------------------------------------------//

	void OnEnable() {
		ValidateReferences();

		m_introScreen.SetActive(true);
		m_globalEventStepScreen.SetActive(false);

		m_event = GlobalEventManager.currentEvent;

		m_step = 0;

		m_progressBar.RefreshRewards(m_event);
		m_progressBar.RefreshProgress(0);

		Messenger.AddListener<Egg>(GameEvents.EGG_OPENED, OnEggCollected);

		m_state = State.OpenNextReward;
	}

	void OnDisable() {
		Messenger.RemoveListener<Egg>(GameEvents.EGG_OPENED, OnEggCollected);
	}

	/// <summary>
	/// Make sure all required references are initialized.
	/// </summary>
	private void ValidateReferences() {
		// 3d scene for this screen
		if (m_sceneController == null) {
			MenuSceneController sceneController = InstanceManager.menuSceneController;
			Debug.Assert(sceneController != null, "This component must be only used in the menu scene!");
			MenuScreenScene menuScene = sceneController.screensController.GetScene((int)MenuScreens.REWARD);
			if (menuScene != null) {
				// Get scene controller and initialize
				m_sceneController = menuScene.GetComponent<RewardSceneController>();
				if (m_sceneController != null) {
					// Initialize
					m_sceneController.InitReferences(m_rewardDragController, null);	// [AOC] TODO!! Assign reward info UI

					// Subscribe to listeners
					m_sceneController.OnAnimFinished.AddListener(OnStateNextReward);
				}
			}
		}
	}

	public void OnRewardButton() {
		m_introScreen.SetActive(false);
		m_globalEventStepScreen.SetActive(true);

		// fill user rewards
		for (int i = m_event.rewardSlots.Count - 1; i >= 0; --i) {
			Metagame.Reward reward = m_event.rewardSlots[i].reward;
			UsersManager.currentUser.rewardStack.Push(reward);
		}

		AdvanceStep();
	}

	public void OnContinueButton() {
		if (m_state == State.OpenNextReward) {
			if (m_step == m_event.rewardLevel) {
				m_event.FinishRewardCollection();	// [AOC] TODO!! Mark event as collected immediately after rewards have been pushed to the stack

				GlobalEventManager.RequestCurrentEventData();
				InstanceManager.menuSceneController.screensController.GoToScreen((int)MenuScreens.DRAGON_SELECTION);
			} else {
				AdvanceStep();
			}
		}
	}

	private void AdvanceStep() {
		if (m_step < m_event.rewardSlots.Count - 1) {
			m_progressBar.RefreshProgress(m_event.rewardSlots[m_step].targetPercentage);
		} else {
			m_progressBar.RefreshProgress(1f);
		}

		m_sceneController.OpenReward();
		m_step++;

		m_state = State.Animation;
	}

	private void LaunchRewardAnimation() {
		// Aux vars
		bool goldenEggCompleted = EggManager.goldenEggCompleted;

		// Show HUD
		InstanceManager.menuSceneController.hud.animator.Show();

		// Initialize and show final panel
		// Delay if duplicate, we need to give enough time for the duplicate animation!

		// TODO  !!!
		/*
		float delay = m_finalPanelDelay;
		if(rewardData.fragments > 0) {
			delay = m_finalPanelDelayWhenFragmentsGiven;
		} else if(rewardData.coins > 0) {
			delay = m_finalPanelDelayWhenCoinsGiven;
		}*/

		// If it's the first time we're getting golden fragments, show info popup
		// TODO  !!!
		/*
		if(rewardData.fragments > 0 && !UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.GOLDEN_FRAGMENTS_INFO)) {
			// Show popup after some extra delay
			UbiBCN.CoroutineManager.DelayedCall(
				() => { 
					PopupManager.OpenPopupInstant(PopupInfoGoldenFragments.PATH);
					UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.GOLDEN_FRAGMENTS_INFO, true);
				},
				delay + 1.5f, 
				false
			);
		}*/

		m_sceneController.OpenReward();
	}


	//--------------------------------------------//
	private void OnStateNextReward() {
		m_state = State.OpenNextReward;
	}

	private void OnEggCollected(Egg _egg) {		
		// Delay to sync with the egg anim
		UbiBCN.CoroutineManager.DelayedCall(LaunchRewardAnimation, 1.75f, false);
	}
}
