using UnityEngine;

public class CollectibleRandomRewardEntity : CollectibleEntity {
    //
    [SerializeField] private ProbabilitySet m_probability;


    //
    private Reward[] m_rewards;


    //

    protected override void Awake() {
        m_rewards = new Reward[m_probability.numElements];
        base.Awake();
    }

    protected override void OnRewardCreated() {
        for (int i = 0; i < m_probability.numElements; ++i) {
            m_rewards[i] = new Reward();

            switch (m_probability.GetLabel(i)) {
                case "pc":      m_rewards[i].pc     = reward.pc;    break;
                case "coins":   m_rewards[i].coins  = reward.coins; break;
                case "score":   m_rewards[i].score  = reward.score; break;
                case "xp":      m_rewards[i].xp     = reward.xp;    break;
                case "health":  m_rewards[i].health = reward.health;break;
            }
        }
    }

    public override Reward GetOnKillReward(DyingReason _reason) {
        return m_rewards[m_probability.GetWeightedRandomElementIdx()];
	}
}
