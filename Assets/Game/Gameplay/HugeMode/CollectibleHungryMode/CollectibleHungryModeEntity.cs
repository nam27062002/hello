using UnityEngine;

public class CollectibleHungryModeEntity : CollectibleEntity {

    [SerializeField] private string m_onCollectSound;
    PositionEventInfo m_eventInfo = new PositionEventInfo();

    public override Reward GetOnKillReward(DyingReason _reason) {
        m_eventInfo.position = m_machine.position;
        Broadcaster.Broadcast(BroadcastEventType.HUNGRY_MODE_ENTITY_EATEN, m_eventInfo);

        // play the sfx.         
        if ( !string.IsNullOrEmpty(m_onCollectSound) )
            AudioController.Play( m_onCollectSound );

        return reward;
	}
}
