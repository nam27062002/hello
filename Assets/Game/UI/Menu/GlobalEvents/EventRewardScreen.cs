using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventRewardScreen : MonoBehaviour {

	[SerializeField] private GameObject m_introScreen;
	[SerializeField] private GameObject m_globalEventStepScreen;

	[SerializeField] private GlobalEventsProgressBar m_progressBar;

	private GlobalEvent m_event;
	private int m_step;

	void OnEnable() {
		m_introScreen.SetActive(true);
		m_globalEventStepScreen.SetActive(false);

		m_event = GlobalEventManager.currentEvent;

		m_step = 0;

		m_progressBar.RefreshRewards(m_event);
		m_progressBar.RefreshProgress(0);
	}

	public void OnRewardButton() {
		m_introScreen.SetActive(false);
		m_globalEventStepScreen.SetActive(true);

		AdvanceStep();
	}

	public void OnContinueButton() {
		if (m_step == m_event.rewardLevel) {
			m_event.FinishRewardCollection();

			GlobalEventManager.RequestCurrentEventData();
			InstanceManager.menuSceneController.screensController.GoToScreen((int)MenuScreens.DRAGON_SELECTION);
		} else {
			AdvanceStep();
		}
	}

	private void AdvanceStep() {
		if (m_step < m_event.rewardSlots.Count - 1) {
			m_progressBar.RefreshProgress(m_event.rewardSlots[m_step].targetPercentage);
		} else {
			m_progressBar.RefreshProgress(1f);
		}

		m_event.CollectReward(m_step);
		m_step++;
	}
}
