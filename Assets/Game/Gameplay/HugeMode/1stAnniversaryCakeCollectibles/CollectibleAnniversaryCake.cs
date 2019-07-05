using UnityEngine;

public class CollectibleAnniversaryCake : CollectibleEntity {

    [SerializeField] private string m_onCollectSound;

    public override Reward GetOnKillReward(DyingReason _reason) {
		Messenger.Broadcast<Vector3>(MessengerEvents.ANNIVERSARY_CAKE_SLICE_EATEN, m_machine.position);

        // play the sfx.         
        if ( !string.IsNullOrEmpty(m_onCollectSound) )
            AudioController.Play( m_onCollectSound );

        return reward;
	}
}
