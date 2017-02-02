using FGOL.Events;
using System;

public class HungryLetterMapMarker : MapMarker {

	private HungryLetter m_hungryLetter;

	void Awake( )
	{
		m_hungryLetter = GetComponent<HungryLetter>();
		Messenger.AddListener<Reward>(GameEvents.LETTER_COLLECTED, OnCollectedHungryLetter);

	}

	void OnDestroy()
	{
		Messenger.RemoveListener<Reward>(GameEvents.LETTER_COLLECTED, OnCollectedHungryLetter);
	}

	protected void OnUpdateMarkerStatus(int mapLevel)	// this was overriding something
	{
		if (mapLevel == 1) {

			bool showMarker = true;
			for (int i = (int)m_hungryLetter.letter - 1; i >= 0; --i) {
				bool isLetterCollected = m_hungryLetter.GetHungryLettersManager ().IsLetterCollected ( (HungryLettersManager.CollectibleLetters)i );
				if (isLetterCollected == false) {
					showMarker = false;
					break;
				}
			}
			// TODO: Recover 
			// ShowIcon (showMarker);
		}
		else
		{
			// TODO: Recover 
			// ShowIcon (mapLevel >= m_mapLevel);
		}
		// TODO: Recover 
		// OnMapUpdate();
	}

	private void OnCollectedHungryLetter( Reward _r )
	{
		// TODO: Recover this
		/*
		if (m_mapMarker != null) {
			int mapLevel = App.Instance.PlayerProgress.MiscItemLevel ("Map" + App.Instance.currentLevel);
			OnUpdateMarkerStatus (mapLevel);
		}
		*/
	}
}